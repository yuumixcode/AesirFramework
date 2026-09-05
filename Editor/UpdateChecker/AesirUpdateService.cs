using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    /// 更新流程与 QFramework PackageKit 同构：远程版本源（此处为 GitHub Releases 而非自建服务器）
    /// → 本地版本记录（此处直接读取包内 package.json）→ 下载 .unitypackage → 先删后导。
    /// 相比 QF 的两处增强：更新前自动备份（用户可能修改过代码）；残留清理按"上次安装清单 − 新版清单"
    /// 精确差集，不误伤用户新增文件。
    /// </para>
    /// </summary>
    public static class AesirUpdateService
    {
        #region 常量

        /// <summary>GitHub 仓库路径（owner/repo）。</summary>
        public const string RepoPath = "yuumixcode/AesirFramework";

        /// <summary>GitHub Releases 最新版 API（降级源之二，未认证限流 60 次/时/IP）。</summary>
        public static readonly string LatestReleaseApiUrl = $"https://api.github.com/repos/{RepoPath}/releases/latest";

        /// <summary>GitHub releases/latest 页面地址（302 到最新 tag，可完全绕开 API 限流）。</summary>
        public static readonly string LatestReleasePageUrl = $"https://github.com/{RepoPath}/releases/latest";

        /// <summary>Releases 网页地址（供用户手动下载 / 查看更新日志）。</summary>
        public static readonly string ReleasesPageUrl = $"https://github.com/{RepoPath}/releases";

        /// <summary>GitHub Release 资产下载地址前缀（资产命名约定见 ReleaseSnapshot.GetUnityPackageUrl）。</summary>
        public static readonly string GitHubDownloadUrlBase = $"https://github.com/{RepoPath}/releases/download";

        /// <summary>jsDelivr CDN 域名（按大陆可达性经验排序；fastly 会 301 跳转到主域名，自动跟随）。</summary>
        public static readonly string[] JsDelivrDomains =
        {
            "cdn.jsdelivr.net", "testingcf.jsdelivr.net", "gcore.jsdelivr.net", "fastly.jsdelivr.net",
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

        /// <summary>package.json 中 Aesir 包 id 的公共前缀。</summary>
        const string PackageIdPrefix = "cn.runestone.aesir.";

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
        /// </summary>
        public sealed class ReleaseSnapshot
        {
            /// <summary>来源描述（如 "jsDelivr (cdn.jsdelivr.net)" / "GitHub API" / "GitHub 重定向"）。</summary>
            public string Source;

            /// <summary>Release 标签名（如 v0.15.0）。</summary>
            public string Tag;

            /// <summary>版本与清单信息；302 重定向路径只有 tag，此字段为 null（更新时跳过残留清理）。</summary>
            public UpdateInfo Info;

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

            /// <summary>各包清单。</summary>
            public PackageEntry[] packages;

            /// <summary>按包目录名查找条目；不存在返回 null。</summary>
            public PackageEntry GetPackage(string dirName) =>
                packages?.FirstOrDefault(p => p != null && p.name == dirName);
        }

        /// <summary>扫描到的本地已安装包。</summary>
        public sealed class InstalledPackage
        {
            /// <summary>包目录名（如 AesirArchitecture）。</summary>
            public string DirName;

            /// <summary>package.json 中的包 id（如 cn.runestone.aesir.architecture）。</summary>
            public string PackageId;

            /// <summary>package.json 中的版本号。</summary>
            public string Version;

            /// <summary>包目录的 Assets 相对路径（如 Assets/Runestone/AesirArchitecture）。</summary>
            public string AssetsPath;
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
        public static List<InstalledPackage> ScanInstalledPackages(
            string installRootRelativePath = InstallRootRelativePath)
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
                if (string.IsNullOrEmpty(name) ||
                    !name.StartsWith(PackageIdPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                results.Add(new InstalledPackage
                {
                    DirName = Path.GetFileName(dir),
                    PackageId = name,
                    Version = version,
                    AssetsPath = ToAssetsRelativePath(dir),
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
                ? full.Substring(projectRootFull.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                : full;
            return relative.Replace(Path.DirectorySeparatorChar, '/');
        }

        #endregion

        #region 版本比较

        /// <summary>
        /// 比较两个语义化版本号（允许 v/V 前缀，缺省段按 0 处理）。
        /// 返回值：a &lt; b 为负，相等为 0，a &gt; b 为正。
        /// </summary>
        public static int CompareVersion(string a, string b)
        {
            var partsA = SplitVersion(a);
            var partsB = SplitVersion(b);
            for (var i = 0; i < 3; i++)
            {
                var cmp = partsA[i].CompareTo(partsB[i]);
                if (cmp != 0)
                {
                    return Math.Sign(cmp);
                }
            }

            return 0;
        }

        static int[] SplitVersion(string version)
        {
            var result = new int[3];
            if (string.IsNullOrEmpty(version))
            {
                return result;
            }

            var segments = version.TrimStart('v', 'V').Split('.');
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
                        return new ReleaseSnapshot { Source = $"jsDelivr ({domain})", Tag = info.tag, Info = info };
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
            return ExtractTagFromLocation(location)
                ?? throw new Exception($"Location 头中未解析到 tag: {location}");
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
        public static async Task<byte[]> DownloadBytesAsync(string url, Action<float> onProgress = null,
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
        public static FilesManifest MergePackageEntry(FilesManifest localManifest, FilesManifest.PackageEntry entry)
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
        public static List<string> ComputeStaleFiles(string[] previousFiles, string[] newFiles, string packageAssetsPath)
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
        /// <param name="label">备份子目录名，须以时间戳开头（格式 yyyyMMdd-HHmmss_...），
        /// 保证 Ordinal 排序即时间序（版本号前缀会打乱 0.9 与 0.14 的次序，故时间戳在前）。</param>
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
                .OrderBy(d => d, StringComparer.Ordinal)
                .ToArray();
            for (var i = 0; i < dirs.Length - keepCount; i++)
            {
                Directory.Delete(dirs[i], true);
            }
        }

        #endregion
    }
}
