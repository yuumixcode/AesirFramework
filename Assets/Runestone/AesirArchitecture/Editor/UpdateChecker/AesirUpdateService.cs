using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 包自动更新服务 — 供 <see cref="AesirUpdateWindow" /> 调用的无状态工具集。
    /// <para>
    /// 适用场景：用户通过复制 / unitypackage 导入方式将包装在 <see cref="InstallRootRelativePath" />
    /// （Assets/Runestone）下、代码可被修改的非 UPM 安装。经 Package Manager（Git URL）安装的副本
    /// 不位于 Assets 下，本工具扫描不到，应改用 Package Manager 更新。
    /// </para>
    /// <para>
    /// 版本检测面向大陆用户做了多源兜底（unitypackage 下载始终走 GitHub Release 直链）：
    /// ① jsDelivr 多域名拉取仓库内 <see cref="UpdateInfoRelativePath" />（CDN 直达、无限流，
    /// 分支引用有最长约 12 小时的缓存延迟）；② GitHub Releases API（未认证 60 次/时/IP）；③
    /// GitHub releases/latest 的 302 重定向探测（完全绕开 API 限流）。见
    /// <see cref="FetchLatestReleaseSnapshotAsync" />。
    /// </para>
    /// <para>
    /// 更新流程：检测远程版本 → 对比本地版本（直接读取包内 package.json）→ 拉取并展示「本地 → 远程」
    /// 更新日志（<see cref="BuildChangelogDigestAsync" />）→ 确认框二次确认（
    /// <see cref="BuildUpdateConfirmation" />）→ 更新前自动备份（用户可能修改过代码）→
    /// 按"上次安装清单 − 新版清单"精确差集清理残留（不误伤用户新增文件）→
    /// 下载 .unitypackage → 静默导入 → 逐包登记安装清单。
    /// </para>
    /// </summary>
    public static class AesirUpdateService
    {
        #region 常量

        /// <summary>GitHub 仓库路径（owner/repo）。</summary>
        public const string RepoPath = "yuumixcode/AesirFramework";

        /// <summary>GitHub Releases 最新版 API（降级源之二，未认证限流 60 次/时/IP）。</summary>
        public static readonly string LatestReleaseApiUrl =
            $"https://api.github.com/repos/{RepoPath}/releases/latest";

        /// <summary>GitHub releases/latest 页面地址（302 到最新 tag，可完全绕开 API 限流）。</summary>
        public static readonly string LatestReleasePageUrl = $"https://github.com/{RepoPath}/releases/latest";

        /// <summary>Releases 网页地址（供用户手动下载 / 查看更新日志）。</summary>
        public static readonly string ReleasesPageUrl = $"https://github.com/{RepoPath}/releases";

        /// <summary>GitHub Release 资产下载地址前缀（资产命名约定见 ReleaseSnapshot.GetUnityPackageUrl）。</summary>
        public static readonly string GitHubDownloadUrlBase =
            $"https://github.com/{RepoPath}/releases/download";

        /// <summary>jsDelivr CDN 域名（按大陆可达性经验排序；fastly 会 301 跳转到主域名，自动跟随）。</summary>
        public static readonly string[] JsDelivrDomains =
        {
            "cdn.jsdelivr.net", "testingcf.jsdelivr.net", "gcore.jsdelivr.net", "fastly.jsdelivr.net"
        };

        /// <summary>update-info.json 在仓库内的路径（CI 发版后以 [skip ci] 提交回 main）。</summary>
        public const string UpdateInfoRelativePath = ".github/update-info.json";

        /// <summary>包安装根目录（项目相对路径）。</summary>
        public const string InstallRootRelativePath = "Assets/Runestone";

        /// <summary>项目根目录下的更新状态目录名（点前缀，Unity 不导入）。</summary>
        public const string StateDirName = ".aesir";

        /// <summary>项目根目录下的备份目录名（点前缀，Unity 不导入）。</summary>
        public const string BackupDirName = ".aesir-backup";

        /// <summary>本地安装清单文件名（记录每次更新成功后各包的完整文件列表）。</summary>
        public const string ManifestFileName = "installed-manifest.json";

        /// <summary>本地备份保留份数（超出后按时间从旧到新删除）。</summary>
        public const int BackupKeepCount = 3;

        /// <summary>jsDelivr 检测超时（秒）——不可达时通常立刻失败，超时不宜过长。</summary>
        public const int JsDelivrCheckTimeoutSeconds = 5;

        /// <summary>GitHub API / 重定向探测超时（秒）。</summary>
        public const int GitHubCheckTimeoutSeconds = 12;

        /// <summary>unitypackage 下载超时（秒）——大文件慢速连接，给足余量。</summary>
        public const int DownloadTimeoutSeconds = 120;

        /// <summary>包内 CHANGELOG 文件名（Keep a Changelog 格式）。</summary>
        public const string ChangelogFileName = "CHANGELOG.md";

        /// <summary>package.json 中 Aesir 包 id 的公共前缀。</summary>
        const string PackageIdPrefix = "cn.runestone.aesir.";

        /// <summary>
        /// GitHub Raw 内容地址前缀（CHANGELOG 拉取的兜底源；大陆可达性不如 jsDelivr，排在最后）。
        /// </summary>
        public static readonly string GitHubRawUrlBase = $"https://raw.githubusercontent.com/{RepoPath}";

        #endregion

        #region 数据模型

        /// <summary>
        /// update-info.json 结构：版本信息 + 各包文件清单（仓库内文件，jsDelivr / GitHub 均可拉取）。
        /// <para>数组而非 Dictionary — JsonUtility 不支持字典序列化。</para>
        /// </summary>
        [Serializable]
        public sealed class UpdateInfo
        {
            /// <summary>版本号（如 0.15.0）。</summary>
            public string version;

            /// <summary>Release 标签名（如 v0.15.0）。</summary>
            public string tag;

            /// <summary>各包文件清单。</summary>
            public FilesManifest.PackageEntry[] packages;

            /// <summary>按包目录名查找条目；不存在返回 null。</summary>
            public FilesManifest.PackageEntry GetPackage(string dirName) =>
                packages?.FirstOrDefault(p => p != null && p.name == dirName);
        }

        /// <summary>
        /// 一次成功检测的结果快照：来源 + tag +（可能缺失的）清单。
        /// unitypackage 下载地址按命名约定从 tag 构造，不依赖 API 的资产列表。
        /// <para>标记 <see cref="SerializableAttribute" /> — 编辑器窗口字段持有快照时可跨域重载保留。</para>
        /// </summary>
        [Serializable]
        public sealed class ReleaseSnapshot
        {
            /// <summary>版本与清单信息；302 重定向路径只有 tag，此字段为 null（更新时跳过残留清理）。</summary>
            public UpdateInfo Info;

            /// <summary>来源描述（如 "jsDelivr (cdn.jsdelivr.net)" / "GitHub API" / "GitHub 重定向"）。</summary>
            public string Source;

            /// <summary>Release 标签名（如 v0.15.0）。</summary>
            public string Tag;

            /// <summary>
            /// 指定包目录的 unitypackage 下载地址。
            /// 命名约定由 CI 保证：&lt;包目录名&gt;-v&lt;版本&gt;.unitypackage。
            /// </summary>
            public string GetUnityPackageUrl(string dirName) =>
                $"{GitHubDownloadUrlBase}/{Tag}/{dirName}-v{Tag.TrimStart('v')}.unitypackage";
        }

        /// <summary>
        /// files-manifest 结构的本地安装清单（.aesir/installed-manifest.json）。
        /// 与 <see cref="UpdateInfo" /> 共用 <see cref="PackageEntry" />。
        /// </summary>
        [Serializable]
        public sealed class FilesManifest
        {
            /// <summary>各包清单。</summary>
            public PackageEntry[] packages;

            /// <summary>按包目录名查找条目；不存在返回 null。</summary>
            public PackageEntry GetPackage(string dirName) =>
                packages?.FirstOrDefault(p => p != null && p.name == dirName);

            /// <summary>单个包的安装清单。</summary>
            [Serializable]
            public sealed class PackageEntry
            {
                /// <summary>包目录名（如 AesirArchitecture），同时是主键。</summary>
                public string name;

                /// <summary>该清单对应的包版本。</summary>
                public string version;

                /// <summary>包内全部条目的项目相对路径（含目录条目，与 unitypackage 内 pathname 同源）。</summary>
                public string[] files;
            }
        }

        /// <summary>扫描到的本地已安装包。</summary>
        [Serializable]
        public sealed class InstalledPackage
        {
            /// <summary>包目录的 Assets 相对路径（如 Assets/Runestone/AesirArchitecture）。</summary>
            public string AssetsPath;

            /// <summary>包目录名（如 AesirArchitecture）。</summary>
            public string DirName;

            /// <summary>package.json 中的包 id（如 cn.runestone.aesir.architecture）。</summary>
            public string PackageId;

            /// <summary>package.json 中的版本号。</summary>
            public string Version;
        }

        #endregion

        #region 路径

        /// <summary>Unity 项目根目录（Application.dataPath 的上一级）。</summary>
        public static string ProjectRootPath =>
            Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;

        /// <summary>将项目相对路径转为绝对路径。</summary>
        public static string ToAbsolutePath(string projectRelativePath) =>
            Path.GetFullPath(Path.Combine(ProjectRootPath, projectRelativePath));

        #endregion

        #region 本地安装扫描

        /// <summary>
        /// 扫描 <paramref name="installRootRelativePath" /> 下的 Aesir 包安装。
        /// 识别依据：子目录中存在 package.json 且包 id 以 cn.runestone.aesir. 开头。
        /// </summary>
        public static List<InstalledPackage> ScanInstalledPackages(string installRootRelativePath =
            InstallRootRelativePath)
        {
            var results = new List<InstalledPackage>();
            var rootAbs = ToAbsolutePath(installRootRelativePath);
            if (!Directory.Exists(rootAbs))
            {
                return results;
            }

            foreach (var dir in Directory.GetDirectories(rootAbs).OrderBy(p => p, StringComparer.Ordinal))
            {
                var pkgJsonPath = Path.Combine(dir, "package.json");
                if (!File.Exists(pkgJsonPath))
                {
                    continue;
                }

                var (name, version) = ParsePackageJson(pkgJsonPath);
                if (string.IsNullOrEmpty(name) || !name.StartsWith(PackageIdPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                results.Add(new InstalledPackage
                {
                    DirName = Path.GetFileName(dir),
                    PackageId = name,
                    Version = version,
                    AssetsPath = ToAssetsRelativePath(dir)
                });
            }

            return results;
        }

        /// <summary>
        /// 解析 package.json 的 name 与 version 字段。
        /// <para>轻量字段提取（要求自包含，不引入 JSON 库）。</para>
        /// </summary>
        public static (string name, string version) ParsePackageJson(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return (ExtractJsonField(json, "name"), ExtractJsonField(json, "version"));
            }
            catch (Exception)
            {
                return ("", "");
            }
        }

        /// <summary>从 JSON 文本中按字段名提取第一个双引号字符串值（不依赖第三方 JSON 库）。</summary>
        public static string ExtractJsonField(string json, string fieldName)
        {
            var key = "\"" + fieldName + "\"";
            var idx = json.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0)
            {
                return "";
            }

            var colonIdx = json.IndexOf(':', idx + key.Length);
            if (colonIdx < 0)
            {
                return "";
            }

            var quoteStart = json.IndexOf('"', colonIdx + 1);
            if (quoteStart < 0)
            {
                return "";
            }

            var quoteEnd = json.IndexOf('"', quoteStart + 1);
            if (quoteEnd < 0)
            {
                return "";
            }

            return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
        }

        /// <summary>将磁盘绝对路径转为 Assets 相对路径（形如 Assets/Runestone/Xxx）。</summary>
        static string ToAssetsRelativePath(string absolutePath)
        {
            var projectRoot = ProjectRootPath;
            var full = Path.GetFullPath(absolutePath);
            var projectRootFull = Path.GetFullPath(projectRoot);
            var relative = full.StartsWith(projectRootFull, StringComparison.Ordinal)
                ? full.Substring(projectRootFull.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                : full;
            return relative.Replace(Path.DirectorySeparatorChar, '/');
        }

        #endregion

        #region 版本比较

        /// <summary>
        /// 取全部待更新包（本地版本低于远程版本），按包 id 排序保证依赖顺序（Architecture 先于 Modules）。
        /// 远程版本为空时返回空列表。
        /// </summary>
        public static List<InstalledPackage> ComputeOutdatedPackages(List<InstalledPackage> packages,
            string remoteVersion) =>
            string.IsNullOrEmpty(remoteVersion)
                ? new List<InstalledPackage>()
                : packages
                    .Where(p => CompareVersion(p.Version, remoteVersion) < 0)
                    .OrderBy(p => p.PackageId, StringComparer.Ordinal).ToList();

        /// <summary>
        /// 比较两个语义化版本号（允许 v/V 前缀，缺省段按 0 处理）。
        /// 数字段相同时正式版高于预发布版（<c>1.0.0-rc1 &lt; 1.0.0</c>），两侧均为预发布按标识文本排序。
        /// 返回值：a &lt; b 为负，相等为 0，a &gt; b 为正。
        /// </summary>
        public static int CompareVersion(string a, string b)
        {
            var partsA = SplitVersion(a, out var prereleaseA);
            var partsB = SplitVersion(b, out var prereleaseB);
            for (var i = 0; i < 3; i++)
            {
                var cmp = partsA[i].CompareTo(partsB[i]);
                if (cmp != 0)
                {
                    return Math.Sign(cmp);
                }
            }

            if (prereleaseA == prereleaseB)
            {
                return 0;
            }

            // SemVer：预发布版的优先级低于同号正式版（否则装过 rc 后正式版会被误判为"已是最新"）
            if (prereleaseA == null)
            {
                return 1;
            }

            if (prereleaseB == null)
            {
                return -1;
            }

            return string.CompareOrdinal(prereleaseA, prereleaseB);
        }

        static int[] SplitVersion(string version, out string prerelease)
        {
            var result = new int[3];
            prerelease = null;
            if (string.IsNullOrEmpty(version))
            {
                return result;
            }

            var core = version.TrimStart('v', 'V');
            var dashIndex = core.IndexOf('-');
            if (dashIndex >= 0)
            {
                prerelease = core.Substring(dashIndex + 1);
                core = core.Substring(0, dashIndex);
            }

            var segments = core.Split('.');
            for (var i = 0; i < result.Length && i < segments.Length; i++)
            {
                int.TryParse(segments[i], out result[i]);
            }

            return result;
        }

        #endregion

        #region 远程版本检测（jsDelivr → GitHub API → 302 探测）

        /// <summary>
        /// 获取最新 Release 快照（tag + 可能的清单）。按以下顺序兜底，首个成功即返回：
        /// ① jsDelivr 多域名拉取仓库内 update-info.json（大陆友好、无限流，CDN 缓存延迟最长约 12 小时）；
        /// ② GitHub Releases API（未认证 60 次/时/IP）；③ GitHub releases/latest 的 302 重定向探测。
        /// 全部失败时抛出含各源错误明细的异常。
        /// </summary>
        public static async Task<ReleaseSnapshot> FetchLatestReleaseSnapshotAsync()
        {
            var errors = new List<string>();

            foreach (var domain in JsDelivrDomains)
            {
                try
                {
                    var url = $"https://{domain}/gh/{RepoPath}@main/{UpdateInfoRelativePath}";
                    var info = ParseUpdateInfo(await GetTextAsync(url, JsDelivrCheckTimeoutSeconds));
                    if (info?.tag != null)
                    {
                        return new ReleaseSnapshot
                            { Source = $"jsDelivr ({domain})", Tag = info.tag, Info = info };
                    }

                    errors.Add($"jsDelivr ({domain}): 响应中无 tag 字段");
                }
                catch (Exception e)
                {
                    errors.Add($"jsDelivr ({domain}): {e.Message}");
                }
            }

            try
            {
                var json = await GetTextAsync(LatestReleaseApiUrl, GitHubCheckTimeoutSeconds);
                var tag = ExtractJsonField(json, "tag_name");
                if (!string.IsNullOrEmpty(tag))
                {
                    return new ReleaseSnapshot { Source = "GitHub API", Tag = tag };
                }

                errors.Add("GitHub API: 响应中无 tag_name 字段");
            }
            catch (Exception e)
            {
                errors.Add($"GitHub API: {e.Message}");
            }

            try
            {
                var tag = await ProbeLatestTagFromRedirect();
                if (tag != null)
                {
                    return new ReleaseSnapshot { Source = "GitHub 重定向", Tag = tag };
                }

                errors.Add("GitHub 重定向: Location 中未解析到 tag");
            }
            catch (Exception e)
            {
                errors.Add($"GitHub 重定向: {e.Message}");
            }

            throw new Exception("所有更新源均不可用：\n" + string.Join("\n", errors));
        }

        /// <summary>
        /// 解析 update-info.json；内容为空或格式异常时返回 null（调用方按"源不可用"处理）。
        /// </summary>
        public static UpdateInfo ParseUpdateInfo(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                var info = JsonUtility.FromJson<UpdateInfo>(json);
                return info?.tag != null || info?.version != null ? info : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 向 GitHub releases/latest 发起禁止重定向的 HEAD 请求，从 302 Location 中提取最新 tag。
        /// 完全绕开 API 限流（该路径不走 api.github.com）。
        /// </summary>
        public static async Task<string> ProbeLatestTagFromRedirect()
        {
            using var request = UnityWebRequest.Head(LatestReleasePageUrl);
            request.redirectLimit = 0;
            request.timeout = GitHubCheckTimeoutSeconds;
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.responseCode < 300 || request.responseCode >= 400)
            {
                throw new Exception($"预期 302 重定向，实际状态码 {request.responseCode}");
            }

            var location = request.GetResponseHeader("Location");
            return ExtractTagFromLocation(location) ??
                   throw new Exception($"Location 头中未解析到 tag: {location}");
        }

        /// <summary>
        /// 从 releases/latest 重定向地址中提取 tag（如 .../releases/tag/v0.15.0 → v0.15.0）；
        /// 不匹配返回 null。
        /// </summary>
        public static string ExtractTagFromLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            var match = Regex.Match(location, @"releases/tag/([^/?#]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        #endregion

        #region 网络下载

        /// <summary>GET 文本内容（UnityWebRequest，编辑器主线程异步等待）。</summary>
        public static async Task<string> GetTextAsync(string url, int timeoutSeconds = 20)
        {
            using var request = UnityWebRequest.Get(url);
            request.timeout = timeoutSeconds;
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"网络请求失败 ({request.responseCode}): {request.error}");
            }

            return request.downloadHandler.text;
        }

        /// <summary>GET 二进制内容（用于下载 unitypackage），通过回调上报 0~1 下载进度。</summary>
        public static async Task<byte[]> DownloadBytesAsync(string url,
            Action<float> onProgress = null,
            int timeoutSeconds = DownloadTimeoutSeconds)
        {
            using var request = UnityWebRequest.Get(url);
            request.timeout = timeoutSeconds;
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                onProgress?.Invoke(request.downloadProgress);
                await Task.Yield();
            }

            onProgress?.Invoke(1f);
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"下载失败 ({request.responseCode}): {request.error}");
            }

            return request.downloadHandler.data;
        }

        #endregion

        #region 更新日志（CHANGELOG）

        /// <summary>
        /// CHANGELOG 中的一个版本段落（Keep a Changelog 格式的 <c>## [x.y.z] - 日期</c> 小节）。
        /// </summary>
        [Serializable]
        public sealed class ChangelogSection
        {
            /// <summary>版本号（如 0.21.0，无 v 前缀）。</summary>
            public string Version;

            /// <summary>发布日期（标题行 <c>-</c> 后的文本，可为空）。</summary>
            public string Date;

            /// <summary>段落正文（不含标题行；已剔除首尾空行与 <c>---</c> 分隔线）。</summary>
            public string Content;
        }

        /// <summary>CHANGELOG 版本段落标题行：<c>## [0.21.0] - 2026-09-13</c>（日期可缺省）。</summary>
        static readonly Regex ChangelogHeaderRegex =
            new Regex(@"^##\s+\[([^\]]+)\]\s*(?:-\s*(.+?))?\s*$", RegexOptions.Compiled);

        /// <summary>版本号形态判定：纯数字点分段（Unreleased 等非版本标题据此排除）。</summary>
        static readonly Regex VersionNumberRegex = new Regex(@"^\d+(\.\d+)*$", RegexOptions.Compiled);

        /// <summary>
        /// 解析 Keep a Changelog 格式的 markdown，按 <c>## [x.y.z]</c> 切分版本段落（保持文件原顺序：新 → 旧）。
        /// <para>
        /// 非版本标题（<c>## [Unreleased]</c>、<c>## 当前版本</c> 等）不产生段落；遇到任意其他二级标题即结束当前段落。
        /// 输入为空或异常时返回空列表。
        /// </para>
        /// </summary>
        public static List<ChangelogSection> ParseChangelogSections(string markdown)
        {
            var sections = new List<ChangelogSection>();
            if (string.IsNullOrEmpty(markdown))
            {
                return sections;
            }

            ChangelogSection current = null;
            var contentLines = new List<string>();
            using var reader = new StringReader(markdown);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var match = ChangelogHeaderRegex.Match(line);
                if (match.Success)
                {
                    FlushSection();
                    var version = match.Groups[1].Value.Trim();
                    if (VersionNumberRegex.IsMatch(version))
                    {
                        current = new ChangelogSection
                        {
                            Version = version,
                            Date = match.Groups[2].Success ? match.Groups[2].Value.Trim() : ""
                        };
                        contentLines.Clear();
                    }

                    continue;
                }

                // 其他二级标题（如"## 当前版本"）结束当前版本段落
                if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    FlushSection();
                    continue;
                }

                if (current != null)
                {
                    contentLines.Add(line);
                }
            }

            FlushSection();
            return sections;

            void FlushSection()
            {
                if (current == null)
                {
                    return;
                }

                var start = 0;
                var end = contentLines.Count;
                while (start < end && IsBlankOrRule(contentLines[start]))
                {
                    start++;
                }

                while (end > start && IsBlankOrRule(contentLines[end - 1]))
                {
                    end--;
                }

                current.Content = string.Join("\n", contentLines.GetRange(start, end - start));
                sections.Add(current);
                current = null;
            }
        }

        static bool IsBlankOrRule(string line) =>
            string.IsNullOrWhiteSpace(line) || line.Trim() == "---";

        /// <summary>
        /// 从段落列表中筛出位于 (localVersion, remoteVersion] 区间的版本段落（保持原顺序：新 → 旧）。
        /// <paramref name="remoteVersion" /> 为空时不设上限。
        /// </summary>
        public static List<ChangelogSection> CollectNewerSections(
            IReadOnlyList<ChangelogSection> sections, string localVersion, string remoteVersion)
        {
            var result = new List<ChangelogSection>();
            if (sections == null)
            {
                return result;
            }

            foreach (var section in sections)
            {
                if (CompareVersion(section.Version, localVersion) <= 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(remoteVersion) &&
                    CompareVersion(section.Version, remoteVersion) > 0)
                {
                    continue;
                }

                result.Add(section);
            }

            return result;
        }

        /// <summary>将版本段落渲染回可读的 markdown 文本（标题行 + 正文，段落间空行分隔）。</summary>
        public static string RenderChangelogText(IReadOnlyList<ChangelogSection> sections)
        {
            if (sections == null || sections.Count == 0)
            {
                return "";
            }

            var builder = new StringBuilder();
            foreach (var section in sections)
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append("## [").Append(section.Version).Append(']');
                if (!string.IsNullOrEmpty(section.Date))
                {
                    builder.Append(" - ").Append(section.Date);
                }

                builder.Append("\n\n").Append(section.Content).Append('\n');
            }

            return builder.ToString();
        }

        /// <summary>
        /// 拉取指定 Release 版本的包内 CHANGELOG.md（jsDelivr 多域名 → GitHub Raw 兜底，首个成功即返回）。
        /// 全部失败时抛出含各源错误明细的异常。
        /// </summary>
        public static async Task<string> FetchPackageChangelogAsync(string tag, string packageDirName)
        {
            var relativePath = $"{InstallRootRelativePath}/{packageDirName}/{ChangelogFileName}";
            var errors = new List<string>();

            foreach (var domain in JsDelivrDomains)
            {
                try
                {
                    return await GetTextAsync($"https://{domain}/gh/{RepoPath}@{tag}/{relativePath}",
                        JsDelivrCheckTimeoutSeconds);
                }
                catch (Exception e)
                {
                    errors.Add($"jsDelivr ({domain}): {e.Message}");
                }
            }

            try
            {
                return await GetTextAsync($"{GitHubRawUrlBase}/{tag}/{relativePath}",
                    GitHubCheckTimeoutSeconds);
            }
            catch (Exception e)
            {
                errors.Add($"GitHub Raw: {e.Message}");
            }

            throw new Exception("更新日志拉取失败：\n" + string.Join("\n", errors));
        }

        /// <summary>读取本地包内 CHANGELOG.md 全文；文件不存在返回 null。</summary>
        public static string LoadLocalChangelog(string packageAssetsPath)
        {
            var path = ToAbsolutePath(packageAssetsPath + "/" + ChangelogFileName);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        /// <summary>
        /// 生成「本地版本 → 远程版本」的更新日志摘要文本（每包一节，按传入顺序）。
        /// <para>
        /// 远程优先：按 tag 拉取包内 CHANGELOG.md 并提取比本地新的段落；远程拉取失败时回退本地包内
        /// CHANGELOG 的最新段落并标注来源。日志属辅助信息，任何单包失败不影响其余包与其摘要输出。
        /// </para>
        /// </summary>
        public static async Task<string> BuildChangelogDigestAsync(string remoteTag,
            IReadOnlyList<InstalledPackage> outdatedPackages)
        {
            var builder = new StringBuilder();
            foreach (var pkg in outdatedPackages)
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append("【").Append(pkg.DirName).Append("】v").Append(pkg.Version)
                    .Append(" → ").Append(remoteTag).Append('\n');

                string remoteMarkdown = null;
                try
                {
                    remoteMarkdown = await FetchPackageChangelogAsync(remoteTag, pkg.DirName);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Aesir Updater] {pkg.DirName} 远程更新日志拉取失败，回退本地日志。\n{e.Message}");
                }

                if (remoteMarkdown != null)
                {
                    var sections =
                        CollectNewerSections(ParseChangelogSections(remoteMarkdown), pkg.Version, remoteTag);
                    builder.Append(sections.Count > 0 ? RenderChangelogText(sections) : "（无新增版本段落）\n");
                }
                else
                {
                    var localSections = ParseChangelogSections(LoadLocalChangelog(pkg.AssetsPath));
                    if (localSections.Count > 0)
                    {
                        builder.Append("（远程日志拉取失败，以下为本地包内最新段落）\n\n")
                            .Append(RenderChangelogText(new List<ChangelogSection> { localSections[0] }));
                    }
                    else
                    {
                        builder.Append("（更新日志不可用：远程拉取失败，本地包内无 CHANGELOG.md）\n");
                    }
                }
            }

            return builder.ToString();
        }

        #endregion

        #region 清单与残留清理

        /// <summary>本地安装清单的项目相对路径。</summary>
        public static string StateFilePath => StateDirName + "/" + ManifestFileName;

        /// <summary>解析本地清单 JSON；内容为空或格式异常时返回 null（调用方按"无记录"处理）。</summary>
        public static FilesManifest ParseFilesManifest(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                var manifest = JsonUtility.FromJson<FilesManifest>(json);
                return manifest?.packages is { Length: > 0 } ? manifest : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>读取本地安装清单；文件不存在或损坏返回 null。</summary>
        public static FilesManifest LoadLocalManifest()
        {
            var path = ToAbsolutePath(StateFilePath);
            return File.Exists(path) ? ParseFilesManifest(File.ReadAllText(path)) : null;
        }

        /// <summary>写入本地安装清单（自动创建 .aesir 状态目录）。</summary>
        public static void SaveLocalManifest(FilesManifest manifest)
        {
            Directory.CreateDirectory(ToAbsolutePath(StateDirName));
            File.WriteAllText(ToAbsolutePath(StateFilePath), JsonUtility.ToJson(manifest, true));
        }

        /// <summary>
        /// 将远程清单中的一个包条目合并进本地清单（按 name 替换或追加）。
        /// 返回合并后的新清单实例（输入参数不被修改）。
        /// </summary>
        public static FilesManifest MergePackageEntry(FilesManifest localManifest,
            FilesManifest.PackageEntry entry)
        {
            var result = new FilesManifest
            {
                packages = localManifest?.packages != null
                    ? localManifest.packages.ToArray()
                    : Array.Empty<FilesManifest.PackageEntry>()
            };

            var index = Array.FindIndex(result.packages, p => p != null && p.name == entry.name);
            if (index >= 0)
            {
                result.packages[index] = entry;
            }
            else
            {
                Array.Resize(ref result.packages, result.packages.Length + 1);
                result.packages[^1] = entry;
            }

            return result;
        }

        /// <summary>
        /// 计算需要删除的残留条目：上次安装清单中存在、新版清单中不存在、且位于指定包目录内。
        /// <para>
        /// 上次清单为空（首次安装 / 无历史记录）时返回空列表 — 没有历史就无法界定"该删什么"，
        /// 宁可残留也不误删。用户在包内新增的文件不在任何清单中，天然不会被删除。
        /// </para>
        /// </summary>
        public static List<string> ComputeStaleFiles(string[] previousFiles,
            string[] newFiles,
            string packageAssetsPath)
        {
            var stale = new List<string>();
            if (previousFiles == null || previousFiles.Length == 0)
            {
                return stale;
            }

            var current = new HashSet<string>(newFiles ?? Array.Empty<string>(), StringComparer.Ordinal);
            var prefix = packageAssetsPath.TrimEnd('/') + "/";
            foreach (var path in previousFiles)
            {
                if (current.Contains(path) || !path.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                stale.Add(path);
            }

            return stale;
        }

        /// <summary>
        /// 删除残留条目：文件直接删除（连带同名 .meta）；目录仅在其为空时删除。
        /// 返回实际删除的条目数。传入的路径必须是项目相对路径。
        /// </summary>
        public static int DeleteStaleEntries(IEnumerable<string> stalePaths)
        {
            var deleted = 0;
            foreach (var path in stalePaths)
            {
                var abs = ToAbsolutePath(path);
                var metaAbs = abs + ".meta";
                if (File.Exists(abs))
                {
                    File.Delete(abs);
                    if (File.Exists(metaAbs))
                    {
                        File.Delete(metaAbs);
                    }

                    deleted++;
                }
                else if (Directory.Exists(abs) && Directory.GetFileSystemEntries(abs).Length == 0)
                {
                    Directory.Delete(abs);
                    if (File.Exists(metaAbs))
                    {
                        File.Delete(metaAbs);
                    }

                    deleted++;
                }
            }

            return deleted;
        }

        /// <summary>
        /// 自底向上删除指定包目录下的空目录（连带 .meta，不删除包根目录本身）。
        /// 返回删除的目录数。用于残留文件删除后收尾清理空目录。
        /// </summary>
        public static int PruneEmptyDirectories(string packageAssetsPath)
        {
            var rootAbs = ToAbsolutePath(packageAssetsPath);
            if (!Directory.Exists(rootAbs))
            {
                return 0;
            }

            var pruned = 0;
            // 按路径长度降序 = 先处理最深目录；子目录删除后父目录变空，会在同趟内继续被删
            foreach (var dir in Directory.GetDirectories(rootAbs, "*", SearchOption.AllDirectories)
                         .OrderByDescending(d => d.Length))
            {
                if (Directory.GetFileSystemEntries(dir).Length != 0)
                {
                    continue;
                }

                Directory.Delete(dir);
                var meta = dir + ".meta";
                if (File.Exists(meta))
                {
                    File.Delete(meta);
                }

                pruned++;
            }

            return pruned;
        }

        #endregion

        #region 备份

        /// <summary>
        /// 将安装根目录整体复制到备份目录（&lt;backupRoot&gt;/&lt;label&gt;），并裁剪至保留最近
        /// <paramref name="keepCount" /> 份。源目录不存在时返回 null（无安装即无备份）。
        /// </summary>
        /// <param name="label">
        /// 备份子目录名，须以时间戳开头（格式 yyyyMMdd-HHmmss_...），
        /// 保证 Ordinal 排序即时间序（版本号前缀会打乱 0.9 与 0.14 的次序，故时间戳在前）。
        /// </param>
        public static string BackupRunestone(string label,
            string sourceRelativePath = InstallRootRelativePath,
            string backupRootRelativePath = BackupDirName,
            int keepCount = BackupKeepCount)
        {
            var sourceAbs = ToAbsolutePath(sourceRelativePath);
            if (!Directory.Exists(sourceAbs))
            {
                return null;
            }

            var backupRootAbs = ToAbsolutePath(backupRootRelativePath);
            Directory.CreateDirectory(backupRootAbs);

            var destinationAbs = Path.Combine(backupRootAbs, label);
            CopyDirectory(sourceAbs, destinationAbs);
            PruneBackups(backupRootAbs, keepCount);
            return destinationAbs;
        }

        /// <summary>递归复制目录（全量，含隐藏文件；备份追求完整还原能力）。</summary>
        static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
            }
        }

        /// <summary>按目录名的 Ordinal 排序裁剪旧备份（label 以时间戳开头时排序即时间序）。</summary>
        public static void PruneBackups(string backupRootAbsolutePath, int keepCount)
        {
            if (keepCount <= 0 || !Directory.Exists(backupRootAbsolutePath))
            {
                return;
            }

            var dirs = Directory.GetDirectories(backupRootAbsolutePath)
                .OrderBy(d => d, StringComparer.Ordinal).ToArray();
            for (var i = 0; i < dirs.Length - keepCount; i++)
            {
                Directory.Delete(dirs[i], true);
            }
        }

        #endregion

        #region 更新执行

        /// <summary>当前项目根目录是否存在 .git（若是 AesirFramework 开发仓库，执行更新会覆盖本地源码）。</summary>
        public static bool IsGitRepository() => Directory.Exists(ToAbsolutePath(".git"));

        /// <summary>
        /// 构建更新前的确认框文本：逐包列示「本地 v旧 → 远程新」，附备份与覆盖说明；
        /// <paramref name="isGitRepository" /> 为 true 时追加开发仓库警告。
        /// </summary>
        public static string BuildUpdateConfirmation(IReadOnlyList<InstalledPackage> targets,
            string remoteVersion, bool isGitRepository)
        {
            var builder = new StringBuilder();
            builder.Append("即将更新以下包：\n\n");
            foreach (var pkg in targets)
            {
                builder.Append("    ").Append(pkg.DirName)
                    .Append("：v").Append(pkg.Version).Append(" → ").Append(remoteVersion).Append('\n');
            }

            builder.Append('\n')
                .Append("更新前自动备份 ").Append(InstallRootRelativePath).Append(" 至 ")
                .Append(BackupDirName).Append("/（保留最近 ").Append(BackupKeepCount).Append(" 份）。\n")
                .Append("包目录内的本地修改将被 Release 内容覆盖，可从备份还原。");

            if (isGitRepository)
            {
                builder.Append(
                    "\n\n⚠ 检测到当前项目存在 .git 目录。若这是 AesirFramework 开发仓库，更新会覆盖本地源码，强烈建议取消。");
            }

            builder.Append("\n\n确认开始更新？");
            return builder.ToString();
        }

        /// <summary>
        /// 执行更新：整体备份 → 逐包（下载 → 按清单差集清残留 → 静默导入 → 登记安装清单）。
        /// 返回备份目录的绝对路径（无安装源时备份为 null）。
        /// </summary>
        /// <param name="snapshot">检测结果快照；<see cref="ReleaseSnapshot.Info" /> 同时承载新清单，
        /// 302 重定向降级路径无清单，残留清理自动跳过。</param>
        /// <param name="targets">待更新包列表，须已按依赖顺序排列（Architecture 先于 Modules）。</param>
        /// <param name="onProgress">进度回调（阶段描述 + 0~1 进度）。</param>
        public static async Task<string> UpdatePackagesAsync(ReleaseSnapshot snapshot,
            IReadOnlyList<InstalledPackage> targets, Action<string, float> onProgress)
        {
            // 1. 整体备份（一次，覆盖本次全部导入）
            onProgress?.Invoke("备份 Assets/Runestone ...", 0.05f);
            var backupPath = BackupRunestone($"{DateTime.Now:yyyyMMdd-HHmmss}_v{GetMaxVersion(targets)}");

            var localManifest = LoadLocalManifest();

            // 2. 逐包下载导入（targets 已按依赖顺序排列）
            for (var i = 0; i < targets.Count; i++)
            {
                var pkg = targets[i];
                var assetUrl = snapshot.GetUnityPackageUrl(pkg.DirName);
                var assetName = $"{pkg.DirName}-v{snapshot.Tag.TrimStart('v')}.unitypackage";

                var progressBase = 0.1f + 0.8f * i / targets.Count;
                var progressSpan = 0.8f / targets.Count;
                onProgress?.Invoke($"[{pkg.DirName}] 下载 {assetName} ...", progressBase);
                var bytes = await DownloadBytesAsync(assetUrl,
                    p => onProgress?.Invoke($"[{pkg.DirName}] 下载 {assetName} ...",
                        progressBase + progressSpan * p));

                var tempDir = ToAbsolutePath("Temp/AesirUpdate");
                Directory.CreateDirectory(tempDir);
                var tempFile = Path.Combine(tempDir, assetName);
                File.WriteAllBytes(tempFile, bytes);

                onProgress?.Invoke($"[{pkg.DirName}] 导入 {assetName} ...", progressBase + progressSpan * 0.95f);
                AssetDatabase.ImportPackage(tempFile, false);
                File.Delete(tempFile);

                // 导入后先校验再清理：包目录缺失说明导入静默失败（unitypackage 损坏等），
                // 此时跳过残留清理与清单登记（旧清单保留，下次更新的差集依据不丢失），并告警指向备份
                var newEntry = snapshot.Info?.GetPackage(pkg.DirName);
                if (!Directory.Exists(ToAbsolutePath(pkg.AssetsPath)))
                {
                    Debug.LogError($"[Aesir Updater] {pkg.DirName}: 导入后未找到包目录 {pkg.AssetsPath}，" +
                                   $"导入可能已失败。本次未执行残留清理，请从备份恢复：{backupPath}");
                    continue;
                }

                // 残留清理：仅当本次检测带清单且本地存在上次安装清单时有明确删除依据。
                // 清理在导入成功之后执行——导入失败不会造成"旧文件已删、新文件未进"的双失局面
                if (newEntry != null)
                {
                    var stale = ComputeStaleFiles(
                        localManifest?.GetPackage(pkg.DirName)?.files, newEntry.files, pkg.AssetsPath);
                    var deleted = DeleteStaleEntries(stale);
                    PruneEmptyDirectories(pkg.AssetsPath);
                    if (deleted > 0)
                    {
                        Debug.Log($"[Aesir Updater] {pkg.DirName}: 清理 {deleted} 个残留条目");
                    }
                }

                // 3. 逐包登记新清单：更新中途域重载时，已导入包的状态保证正确落盘
                if (newEntry != null)
                {
                    localManifest = MergePackageEntry(localManifest, newEntry);
                    SaveLocalManifest(localManifest);
                }
            }

            return backupPath;
        }

        /// <summary>取包列表中的最高版本号（两包版本由 CI 强制一致，此处仍按最高取值兜底）。</summary>
        static string GetMaxVersion(IEnumerable<InstalledPackage> packages)
        {
            string max = null;
            foreach (var pkg in packages)
            {
                if (max == null || CompareVersion(pkg.Version, max) > 0)
                {
                    max = pkg.Version;
                }
            }

            return max ?? "0.0.0";
        }

        #endregion
    }
}
