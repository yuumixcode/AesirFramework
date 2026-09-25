using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 本地安装路径定位器 — 解析 Assets 形态安装（复制 / unitypackage 导入）的 Runestone
    /// 安装根目录，确保 Runestone 可移动到项目的任意文件夹。
    /// <para>
    /// 机制参照 Odin Inspector 的 SirenixAssetPaths（OdinPathLookup.asset 锚点）：每包包根放一个
    /// <see cref="AesirPathLookup" /> 锚点资产，文件夹移动时 .meta GUID 保持不变，依次按
    /// 「默认安装根 → 锚点 GUID 查询 → 锚点类型搜索」三级定位（一级命中即止、逐级变慢）。
    /// </para>
    /// <para>
    /// 结果在静态构造期解析一次，域重载后自动重解析；移动含脚本的 Runestone 目录必然触发域重载，
    /// 各消费端（构建剔除 / 更新器 / Getting Started）下一次访问即拿到新位置。
    /// </para>
    /// <para>
    /// 边界：仅覆盖 Assets 形态安装——UPM（Git URL / 嵌入式）安装的包路径由 Package Manager 固定、
    /// 不可移动、无需锚点；Package Manager 导入的示例位于 <c>Assets/Samples/</c>（Unity 固定路径），
    /// 由消费端直接按前缀匹配。
    /// </para>
    /// </summary>
    internal static class AesirAssetPaths
    {
        /// <summary>默认安装根（项目相对路径）——快路径首查位置。</summary>
        public const string DefaultInstallRoot = "Assets/Runestone";

        /// <summary>AesirArchitecture 包锚点资产的 .meta GUID（文件夹移动后 GUID 不变，定位靠它）。</summary>
        public const string ArchitectureLookupAssetGuid = "d2965568e68354d3ca31d6085ac86865";

        /// <summary>AesirModules 包锚点资产的 .meta GUID。</summary>
        public const string ModulesLookupAssetGuid = "465d45b44c6cf426882248dceeea94a0";

        /// <summary>锚点资产文件名（每包包根一份）。</summary>
        public const string LookupAssetFileName = "AesirPathLookup.asset";

        /// <summary>package.json 中 Aesir 包 id 的公共前缀（锚点定位的包根验证用）。</summary>
        internal const string PackageIdPrefix = "cn.runestone.aesir.";

        static readonly string[] installRoots = ResolveInstallRoots();

        /// <summary>本地安装根列表（项目相对路径，按解析顺序去重；默认根优先）。</summary>
        public static IReadOnlyList<string> InstallRoots => installRoots;

        /// <summary>主安装根（列表首个；备份与提示文案用）。无任何本地安装时回退默认根。</summary>
        public static string PrimaryInstallRoot =>
            installRoots.Length > 0 ? installRoots[0] : DefaultInstallRoot;

        #region 定位链

        /// <summary>
        /// 三级定位：① 默认根存在即收（磁盘判定，零 AssetDatabase 开销）；
        /// ② 锚点 GUID 查询（默认根被移走后靠这一级，O(1)）；
        /// ③ 锚点类型搜索（GUID 被外部工具改写时的兜底，仅在 ② 一无所获时才跑）。
        /// </summary>
        static string[] ResolveInstallRoots()
        {
            var roots = new List<string>();

            // ① 默认根快路径：根下是否有 Aesir 包由消费端扫描 package.json 自然过滤
            if (Directory.Exists(ToAbsolute(DefaultInstallRoot)))
            {
                roots.Add(DefaultInstallRoot);
            }

            // ② 锚点 GUID 定位
            var foundByGuid = CollectFromLookupAsset(
                AssetDatabase.GUIDToAssetPath(ArchitectureLookupAssetGuid), roots);
            foundByGuid |= CollectFromLookupAsset(
                AssetDatabase.GUIDToAssetPath(ModulesLookupAssetGuid), roots);

            // ③ 类型搜索兜底——正常路径（GUID 定位成功）零 FindAssets 开销
            if (!foundByGuid)
            {
                foreach (var guid in AssetDatabase.FindAssets("t:" + nameof(AesirPathLookup)))
                {
                    CollectFromLookupAsset(AssetDatabase.GUIDToAssetPath(guid), roots);
                }
            }

            return roots.ToArray();
        }

        /// <summary>
        /// 从锚点资产路径反推安装根：包根 = 锚点所在目录、安装根 = 包根的父目录；
        /// 包根经 package.json 验证（防止同名目录误判），已收录的根跳过。
        /// </summary>
        /// <returns>是否成功收集到安装根。</returns>
        static bool CollectFromLookupAsset(string lookupAssetPath, List<string> roots)
        {
            var packageRoot = GetPackageRootFromLookupAssetPath(lookupAssetPath);
            if (packageRoot == null || !LooksLikeAesirPackageRoot(packageRoot))
            {
                return false;
            }

            var installRoot = GetParentDirectory(packageRoot);
            if (installRoot == null)
            {
                return false;
            }

            foreach (var existing in roots)
            {
                if (string.Equals(existing, installRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return true; // 与已收录根相同视为成功（GUID 定位有效）
                }
            }

            roots.Add(installRoot);
            return true;
        }

        #endregion

        #region 纯函数（供单元测试）

        /// <summary>锚点资产路径 → 包根（锚点所在目录）；输入空或无目录层级时返回 null。</summary>
        internal static string GetPackageRootFromLookupAssetPath(string lookupAssetPath)
        {
            if (string.IsNullOrEmpty(lookupAssetPath))
            {
                return null;
            }

            var separator = lookupAssetPath.LastIndexOf('/');
            return separator > 0 ? lookupAssetPath.Substring(0, separator) : null;
        }

        /// <summary>取项目相对路径的父目录（保持正斜杠形态）；无父层时返回 null。</summary>
        internal static string GetParentDirectory(string projectRelativePath)
        {
            if (string.IsNullOrEmpty(projectRelativePath))
            {
                return null;
            }

            var separator = projectRelativePath.LastIndexOf('/');
            return separator > 0 ? projectRelativePath.Substring(0, separator) : null;
        }

        /// <summary>目录是否为 Aesir 包根：存在 package.json 且内容含 Aesir 包 id 前缀。</summary>
        internal static bool LooksLikeAesirPackageRoot(string packageRootProjectRelativePath)
        {
            if (string.IsNullOrEmpty(packageRootProjectRelativePath))
            {
                return false;
            }

            return LooksLikeAesirPackageRootAbsolute(ToAbsolute(packageRootProjectRelativePath));
        }

        /// <summary>同 <see cref="LooksLikeAesirPackageRoot" />，接受绝对路径（测试 fixture 用）。</summary>
        internal static bool LooksLikeAesirPackageRootAbsolute(string packageRootAbsolutePath)
        {
            if (string.IsNullOrEmpty(packageRootAbsolutePath))
            {
                return false;
            }

            try
            {
                var pkgJsonPath = Path.Combine(packageRootAbsolutePath, "package.json");
                return File.Exists(pkgJsonPath) && File.ReadAllText(pkgJsonPath).Contains(PackageIdPrefix);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                return false; // 读不了按非 Aesir 包处理（fail-closed）
            }
        }

        /// <summary>
        /// 按锚点资产所在包目录名返回期望 GUID（<see cref="ArchitectureLookupAssetGuid" /> /
        /// <see cref="ModulesLookupAssetGuid" />）；锚点不在已知包根下返回 null。
        /// </summary>
        internal static string GetExpectedLookupAssetGuid(string lookupAssetProjectRelativePath)
        {
            var packageRoot = GetPackageRootFromLookupAssetPath(lookupAssetProjectRelativePath);
            if (packageRoot == null)
            {
                return null;
            }

            var dirName = packageRoot.Substring(packageRoot.LastIndexOf('/') + 1);
            if (string.Equals(dirName, "AesirArchitecture", StringComparison.Ordinal))
            {
                return ArchitectureLookupAssetGuid;
            }

            return string.Equals(dirName, "AesirModules", StringComparison.Ordinal)
                ? ModulesLookupAssetGuid
                : null;
        }

        #endregion

        #region 路径工具

        /// <summary>项目相对路径 → 绝对路径。</summary>
        internal static string ToAbsolute(string projectRelativePath) =>
            Path.GetFullPath(Path.Combine(ProjectRootPath, projectRelativePath));

        /// <summary>Unity 项目根目录（Assets 的上一级）。</summary>
        static string ProjectRootPath =>
            Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;

        #endregion
    }
}
