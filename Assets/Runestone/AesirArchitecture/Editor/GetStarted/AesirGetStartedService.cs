using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir Getting Started 服务 — 两版 Getting Started 窗口（IMGUI 兜底 / Odin 版）共用的无状态数据层：
    /// 扫描本机安装的 Aesir 包、解析示例清单与场景资产、按教学分组归置示例，并提供跳转动作。
    /// <para>
    /// 包发现覆盖三种安装形态（同名包按此优先级取一）：① 代码导入
    /// <see cref="AesirUpdateService.InstallRootRelativePath" />（复制 / unitypackage，示例位于包内
    /// <c>Samples/</c>）；② 嵌入式包（<c>Packages/</c> 目录下的包源码）；③ UPM Git URL 安装
    /// （包源在 <see cref="PackageCacheRootPath" />）。后两种 UPM 形态的示例须经
    /// Package Manager → Samples 导入到 <c>Assets/Samples/&lt;包显示名&gt;/&lt;版本&gt;/&lt;示例显示名&gt;/</c>，
    /// 未导入的条目仍保留在清单中（<see cref="AesirSampleInfo.IsImported" /> 为 false），
    /// 由窗口引导用户去 Package Manager 导入。
    /// </para>
    /// <para>
    /// 示例元数据（显示名 / 描述 / 顺序 / 档位）以各包 package.json 的 samples 清单为唯一真源，
    /// 本类不做逐示例硬编码登记——新增示例只需更新 package.json，窗口自动跟进。
    /// </para>
    /// </summary>
    public static class AesirGetStartedService
    {
        #region 常量

        /// <summary>Aesir 包的 package.json name 前缀（识别本框架包）。</summary>
        public const string PackageIdPrefix = "cn.runestone.aesir.";

        /// <summary>UPM Git URL 安装的包缓存根目录（项目相对路径，Unity 不导入）。</summary>
        public const string PackageCacheRootPath = "Library/PackageCache";

        /// <summary>UPM 示例导入的落地根目录（Package Manager → Samples → Import 后的位置）。</summary>
        public const string UpmSamplesRootPath = "Assets/Samples";

        /// <summary>package.json samples 清单里路径的 Samples~ 镜像前缀。</summary>
        const string SamplesMirrorPrefix = "Samples~/";

        /// <summary>Unity Package Manager 窗口菜单路径（未导入示例的引导落点）。</summary>
        const string PackageManagerMenuPath = "Window/Package Manager";

        #endregion

        #region 已知包目录

        /// <summary>框架已知包（概览页按此补全「未安装」占位卡片；新增公开包时在此登记）。</summary>
        public sealed class AesirKnownPackage
        {
            /// <summary>包定位描述（概览卡片文案）。</summary>
            public string Description;

            /// <summary>包显示名（概览卡片标题）。</summary>
            public string DisplayName;

            /// <summary>package.json name（包唯一标识）。</summary>
            public string Id;
        }

        /// <summary>已公开发布的 Aesir 包（顺序即概览卡片顺序）。</summary>
        public static readonly AesirKnownPackage[] KnownPackages =
        {
            new AesirKnownPackage
            {
                Id = "cn.runestone.aesir.architecture",
                DisplayName = "Aesir Architecture",
                Description = "渐进式 MVC 架构框架：能力接口组合、Command/Query 读写分发、MiniEvent 轻量事件与 " +
                              "ObservableValue 响应式属性、PlayerLoop 生命周期与帧粒度时间调度。可独立安装。"
            },
            new AesirKnownPackage
            {
                Id = "cn.runestone.aesir.modules",
                DisplayName = "Aesir Modules",
                Description = "功能模块集合：轻量级 UI 框架（面板生命周期与四层 Canvas）、事件模块（订阅者过滤器与 " +
                              "SO 资产化）、音频模块与场景模块。依赖 Aesir Architecture。"
            }
        };

        #endregion

        #region 数据模型

        /// <summary>包安装形态（决定示例根目录的解析方式）。</summary>
        public enum AesirInstallType
        {
            /// <summary>代码导入 Assets/Runestone（复制 / unitypackage），示例位于包内 Samples/。</summary>
            AssetsCopy,

            /// <summary>嵌入式包（Packages/ 目录），示例经 Package Manager 导入到 Assets/Samples/。</summary>
            Embedded,

            /// <summary>UPM Git URL 安装（包源在 Library/PackageCache），示例经 Package Manager 导入到 Assets/Samples/。</summary>
            Upm
        }

        /// <summary>已安装的 Aesir 包信息（扫描产物）。</summary>
        public sealed class AesirPackageInfo
        {
            /// <summary>包定位描述（package.json description）。</summary>
            public string Description;

            /// <summary>包目录名（Assets 副本目录 / 嵌入式目录 / PackageCache 缓存目录）。</summary>
            public string DirName;

            /// <summary>包显示名（概览卡片标题 / UPM 示例导入目录名）。</summary>
            public string DisplayName;

            /// <summary>package.json name（包唯一标识）。</summary>
            public string Id;

            /// <summary>安装形态。</summary>
            public AesirInstallType InstallType;

            /// <summary>
            /// 包根的项目相对路径（仅 <see cref="AesirInstallType.AssetsCopy" /> 形态有值；
            /// 经锚点资产定位，Runestone 可移动到项目任意文件夹——示例路径据此动态拼接，
            /// 不硬编码安装根。嵌入式 / UPM 形态为 null。
            /// </summary>
            public string PackageRootPath;

            /// <summary>示例清单（保持 package.json 声明顺序：渐进式示例即教学顺序）。</summary>
            public List<AesirSampleInfo> Samples = new List<AesirSampleInfo>();

            /// <summary>本地版本号。</summary>
            public string Version;

            /// <summary>已导入且可直接打开场景的示例数。</summary>
            public int ImportedWithSceneCount
            {
                get
                {
                    var count = 0;
                    foreach (var s in Samples)
                    {
                        if (s.IsImported && s.HasScene)
                        {
                            count++;
                        }
                    }

                    return count;
                }
            }
        }

        /// <summary>单个示例信息（元数据来自 package.json samples 清单，路径经扫描解析）。</summary>
        public sealed class AesirSampleInfo
        {
            /// <summary>示例描述（package.json samples.description）。</summary>
            public string Description;

            /// <summary>示例显示名（package.json samples.displayName）。</summary>
            public string DisplayName;

            /// <summary>所属包。</summary>
            public AesirPackageInfo Package;

            /// <summary>包内相对目录（samples.path 去掉 Samples~/ 前缀，如 Counter-Mvc-Quick、Events/02_Filters）。</summary>
            public string RelativeDir;

            /// <summary>示例根目录（项目 Assets 相对路径；未导入为 null）。</summary>
            public string RootPath;

            /// <summary>示例场景（Assets 相对路径；无场景示例为 null）。</summary>
            public string ScenePath;

            /// <summary>示例已导入（根目录存在于 Assets 中）。</summary>
            public bool IsImported => RootPath != null;

            /// <summary>示例含可直接打开的场景。</summary>
            public bool HasScene => ScenePath != null;
        }

        /// <summary>教学分组（示例卡片页的分组小节，组内保持 package.json 声明顺序）。</summary>
        public sealed class SampleGroup
        {
            /// <summary>组内示例。</summary>
            public List<AesirSampleInfo> Samples = new List<AesirSampleInfo>();

            /// <summary>分组标题（如「MVC 计数器 · 渐进式」）。</summary>
            public string Title;
        }

        #endregion

        #region 包扫描

        /// <summary>
        /// 扫描本机安装的 Aesir 包并解析示例。包按 Id 字母序返回
        /// （cn.runestone.aesir.architecture → cn.runestone.aesir.modules，恰为教学顺序）；
        /// 同名包按「Assets 副本 → 嵌入式 → PackageCache」优先级取一。
        /// </summary>
        public static List<AesirPackageInfo> ScanPackages()
        {
            var packages = new List<AesirPackageInfo>();
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;

            // ① Assets 安装：安装根经锚点资产定位（AesirAssetPaths，与更新器同源），
            //    支持 Runestone 移动到项目任意文件夹；多根时首个根（默认根）优先
            var installRoots = AesirAssetPaths.InstallRoots;
            for (var i = 0; i < installRoots.Count; i++)
            {
                CollectPackages(Path.Combine(projectRoot ?? throw new InvalidOperationException(), installRoots[i]), AesirInstallType.AssetsCopy,
                    packages, i > 0, installRoots[i]);
            }

            // ② 嵌入式包：Packages/*/package.json（file: 安装形态不在此列，属可接受的覆盖缺口）
            CollectPackages(Path.Combine(projectRoot ?? throw new InvalidOperationException(), "Packages"), AesirInstallType.Embedded, packages, true);

            // ③ UPM Git 安装：Library/PackageCache/cn.runestone.aesir.*@*/
            CollectPackages(Path.Combine(projectRoot, PackageCacheRootPath), AesirInstallType.Upm, packages,
                true);

            packages.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            return packages;
        }

        /// <summary>
        /// 收集一个根目录下符合 Aesir 包前缀的包（读取 package.json、解析示例清单）。
        /// skipDuplicates 为 true 时同 Id 包跳过——用于低优先级根目录，保证 Assets 副本优先。
        /// installRootRelativePath 为该根的项目相对路径（Assets 形态记录到包上供示例定位，
        /// 其他形态传 null）。
        /// </summary>
        static void CollectPackages(string rootAbsDir,
            AesirInstallType installType,
            List<AesirPackageInfo> into,
            bool skipDuplicates = false,
            string installRootRelativePath = null)
        {
            if (!Directory.Exists(rootAbsDir))
            {
                return;
            }

            foreach (var dir in Directory.GetDirectories(rootAbsDir))
            {
                // PackageCache 根目录按目录名前缀先过滤，避免逐包读 package.json
                var dirName = Path.GetFileName(dir);
                if (installType == AesirInstallType.Upm &&
                    !dirName.StartsWith(PackageIdPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var meta = TryReadPackageMeta(dir);
                if (meta == null || string.IsNullOrEmpty(meta.name) ||
                    !meta.name.StartsWith(PackageIdPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (skipDuplicates && ContainsId(into, meta.name))
                {
                    continue;
                }

                var pkg = new AesirPackageInfo
                {
                    Id = meta.name,
                    DisplayName = string.IsNullOrEmpty(meta.displayName) ? meta.name : meta.displayName,
                    Version = meta.version,
                    Description = meta.description,
                    DirName = dirName,
                    PackageRootPath = installRootRelativePath != null
                        ? $"{installRootRelativePath}/{dirName}"
                        : null,
                    InstallType = installType
                };
                ResolveSamples(pkg, meta);
                into.Add(pkg);
            }
        }

        static bool ContainsId(List<AesirPackageInfo> packages, string id)
        {
            foreach (var p in packages)
            {
                if (p.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 解析包的全部示例：Assets 安装取包内 Samples/&lt;相对目录&gt;（Unity 不导入 Samples~ 镜像，
        /// 可见的 Samples/ 即可运行副本）；UPM / 嵌入式安装取 Assets/Samples/&lt;显示名&gt;/&lt;版本&gt;/&lt;示例显示名&gt;，
        /// 目录不存在（尚未导入）时条目保留并标记未导入。
        /// </summary>
        static void ResolveSamples(AesirPackageInfo pkg, PackageJsonMeta meta)
        {
            if (meta.samples == null)
            {
                return;
            }

            foreach (var s in meta.samples)
            {
                if (s == null || string.IsNullOrEmpty(s.path))
                {
                    continue;
                }

                var relativeDir = s.path.StartsWith(SamplesMirrorPrefix, StringComparison.Ordinal)
                    ? s.path.Substring(SamplesMirrorPrefix.Length)
                    : s.path;

                string rootPath;
                if (pkg.InstallType == AesirInstallType.AssetsCopy)
                {
                    // 包根经锚点定位（Runestone 可移动），包根缺失（异常形态）自然判为未导入
                    rootPath = pkg.PackageRootPath != null
                        ? $"{pkg.PackageRootPath}/Samples/{relativeDir}"
                        : null;
                }
                else
                {
                    rootPath = $"{UpmSamplesRootPath}/{pkg.DisplayName}/{pkg.Version}/{s.displayName}";
                }

                if (!AssetDatabase.IsValidFolder(rootPath))
                {
                    rootPath = null;
                }

                var scenePath = rootPath != null ? FindScene(rootPath) : null;

                pkg.Samples.Add(new AesirSampleInfo
                {
                    Package = pkg,
                    DisplayName = s.displayName,
                    Description = s.description,
                    RelativeDir = relativeDir,
                    RootPath = rootPath,
                    ScenePath = scenePath
                });
            }
        }

        /// <summary>在示例根目录下查找场景资产（多场景时取路径最浅者，保证主场景优先）。</summary>
        static string FindScene(string sampleRootPath)
        {
            var guids = AssetDatabase.FindAssets("t:SceneAsset", new[] { sampleRootPath });
            if (guids.Length == 0)
            {
                return null;
            }

            string bestPath = null;
            var bestDepth = int.MaxValue;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var depth = path.Split('/').Length;
                if (bestPath == null || depth < bestDepth ||
                    (depth == bestDepth && string.CompareOrdinal(path, bestPath) < 0))
                {
                    bestDepth = depth;
                    bestPath = path;
                }
            }

            return bestPath;
        }

        #endregion

        #region 分组与档位

        /// <summary>示例分组标题（渐进式教学顺序：MVC → MVP → 功能演示 → 实战 → 各模块）。</summary>
        public static string GetSampleGroup(AesirSampleInfo sample)
        {
            var name = sample.DisplayName ?? string.Empty;
            if (name.StartsWith("Counter-MVC", StringComparison.OrdinalIgnoreCase))
            {
                return "MVC 计数器 · 渐进式";
            }

            if (name.StartsWith("Counter-MVP", StringComparison.OrdinalIgnoreCase))
            {
                return "MVP 计数器 · 渐进式";
            }

            if (name.StartsWith("Event Module", StringComparison.OrdinalIgnoreCase))
            {
                return "事件模块";
            }

            if (name.StartsWith("Audio Module", StringComparison.OrdinalIgnoreCase))
            {
                return "音频模块";
            }

            if (name.StartsWith("PlaneWar", StringComparison.OrdinalIgnoreCase))
            {
                return "实战示例";
            }

            return "功能演示";
        }

        /// <summary>分组显示顺序（数值小者靠前；未登记分组按首现顺序排在末尾）。</summary>
        static int GetGroupOrder(string groupTitle)
        {
            switch (groupTitle)
            {
                case "MVC 计数器 · 渐进式":
                    return 0;
                case "MVP 计数器 · 渐进式":
                    return 1;
                case "功能演示":
                    return 2;
                case "实战示例":
                    return 3;
                case "事件模块":
                    return 4;
                case "音频模块":
                    return 5;
                default:
                    return 100;
            }
        }

        /// <summary>
        /// 按教学分组归置包内示例（组内保持 package.json 声明顺序；组间按
        /// <see cref="GetGroupOrder" /> 排序，同序号保持首现顺序——插入排序保证稳定性）。
        /// </summary>
        public static List<SampleGroup> GroupSamples(AesirPackageInfo pkg)
        {
            var groups = new List<SampleGroup>();
            var orders = new List<int>();

            foreach (var s in pkg.Samples)
            {
                var title = GetSampleGroup(s);
                var order = GetGroupOrder(title);

                var index = groups.FindIndex(g => g.Title == title);
                if (index < 0)
                {
                    groups.Add(new SampleGroup { Title = title });
                    orders.Add(order);
                    index = groups.Count - 1;
                }

                groups[index].Samples.Add(s);
            }

            for (var i = 1; i < groups.Count; i++)
            for (var j = i; j > 0 && orders[j - 1] > orders[j]; j--)
            {
                (groups[j - 1], groups[j]) = (groups[j], groups[j - 1]);
                (orders[j - 1], orders[j]) = (orders[j], orders[j - 1]);
            }

            return groups;
        }

        /// <summary>档位徽章文本（无档位返回 null）：名称中的 快捷 / 标准 / 严格，或目录序号（01 基础、其余进阶）。</summary>
        public static string GetSampleBadge(AesirSampleInfo sample)
        {
            var name = sample.DisplayName ?? string.Empty;
            if (name.Contains("快捷"))
            {
                return "快捷";
            }

            if (name.Contains("标准"))
            {
                return "标准";
            }

            if (name.Contains("严格"))
            {
                return "严格";
            }

            // 分层示例的目录带父级（如 Events/01_KeyPress），序号取末段目录名判定
            var dir = sample.RelativeDir ?? string.Empty;
            var leaf = dir.Substring(dir.LastIndexOf('/') + 1);
            if (leaf.Length > 1 && char.IsDigit(leaf[0]))
            {
                return leaf.StartsWith("01") ? "基础" : "进阶";
            }

            return null;
        }

        #endregion

        #region 跳转动作

        /// <summary>
        /// 打开示例场景：先保存当前打开的场景一次（无未保存修改时为空操作），再切换到示例场景。
        /// <para>
        /// 当前场景从未保存过（无路径）时由 Unity Save 面板兜底，用户在面板中取消保存则中止切换
        /// （避免丢失未保存的改动）；无场景 / 未导入示例退化为 <see cref="PingSample" /> 定位。
        /// </para>
        /// </summary>
        /// <returns>是否已实际打开示例场景（保存被取消或示例不可打开时为 false，不产生切换）。</returns>
        public static bool OpenSampleScene(AesirSampleInfo sample)
        {
            if (!sample.IsImported || !sample.HasScene)
            {
                PingSample(sample);
                return false;
            }

            if (!EditorSceneManager.SaveOpenScenes())
            {
                return false;
            }

            EditorSceneManager.OpenScene(sample.ScenePath, OpenSceneMode.Single);
            return true;
        }

        /// <summary>在 Project 窗口选中并 Ping 示例根目录文件夹（未导入时打开 Package Manager）。</summary>
        /// <returns>是否定位成功（资产缺失或未导入时为 false）。</returns>
        public static bool PingSample(AesirSampleInfo sample)
        {
            if (!sample.IsImported)
            {
                OpenPackageManager();
                return false;
            }

            var path = sample.RootPath;
            // 文件夹资产为 DefaultAsset，同样可 Ping 定位
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset == null)
            {
                return false;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return true;
        }

        /// <summary>打开 Package Manager（UPM / 嵌入式安装的示例经其 Samples 标签页导入）。</summary>
        public static void OpenPackageManager() =>
            EditorApplication.ExecuteMenuItem(PackageManagerMenuPath);

        /// <summary>定位动作的 Toast 提示文本（告知用户已在 Project 窗口选中示例文件夹）。</summary>
        public static string BuildPingToastMessage(AesirSampleInfo sample) =>
            $"已在 Project 窗口选中示例文件夹：{sample.RootPath}";

        /// <summary>打开场景动作的 Toast 提示文本（告知示例场景已打开、切换前的场景已保存）。</summary>
        public static string BuildOpenSceneToastMessage(AesirSampleInfo sample) =>
            $"已打开示例场景：{sample.ScenePath}，切换前的场景已保存";

        #endregion

        #region package.json 解析

        /// <summary>package.json 反序列化载体（仅取 Getting Started 所需字段）。</summary>
        [Serializable]
        class PackageJsonMeta
        {
            public string name;
            public string displayName;
            public string version;
            public string description;
            public SampleJsonMeta[] samples;
        }

        /// <summary>package.json samples 清单项。</summary>
        [Serializable]
        class SampleJsonMeta
        {
            public string displayName;
            public string description;
            public string path;
        }

        /// <summary>
        /// 读取并解析包目录下的 package.json。文件缺失或损坏时返回 null 并告警跳过：
        /// 扫描根目录（尤其 PackageCache）含大量第三方包，单个损坏的 package.json
        /// 不应阻断整个 Getting Started 体验——这是本类唯一吞异常的位置。
        /// </summary>
        static PackageJsonMeta TryReadPackageMeta(string packageDirAbsPath)
        {
            try
            {
                var jsonPath = Path.Combine(packageDirAbsPath, "package.json");
                if (!File.Exists(jsonPath))
                {
                    return null;
                }

                return JsonUtility.FromJson<PackageJsonMeta>(File.ReadAllText(jsonPath));
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"[Aesir GetStarted] 解析 package.json 失败，已跳过该目录：{packageDirAbsPath}\n{e.Message}");
                return null;
            }
        }

        #endregion
    }
}
