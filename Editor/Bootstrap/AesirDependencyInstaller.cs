using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Runestone.AesirModules.Editor.Bootstrap
{
    /// <summary>
    /// Aesir Modules 依赖补全器 —— 检测 Aesir Architecture (RAA) 缺失并经 Git URL 引导安装。
    /// </summary>
    /// <remarks>
    /// 本类所在程序集<b>必须零引用</b>（不引用 RAM 核心、RAA、Odin）：Assets 形态安装本包时若
    /// RAA 缺失，RAM 核心 / Editor 程序集因解析不到 <c>Runestone.AesirArchitecture</c> 全部不编译，
    /// 此时本菜单是唯一可用的 Aesir 工具入口，其编译不得依赖任何会失败的程序集。
    /// <para>
    /// UPM 形态安装无需本菜单：本包 package.json 的 dependencies 已声明 RAA 的 Git URL，
    /// Package Manager 安装时自动递归拉取依赖。
    /// </para>
    /// <para>
    /// 安装走 <see cref="Client.Add" />（Git URL），RAA 落在 Packages/ 下以 UPM 形态存在；
    /// RAM 核心 asmdef 按程序集<b>名</b>引用（非 GUID），UPM 形态的 RAA 同名程序集同样能解析，
    /// Assets + UPM 混合形态可正常编译。
    /// </para>
    /// </remarks>
    internal static class AesirDependencyInstaller
    {
        #region 常量

        /// <summary>RAA 的包 id（package.json 的 name；安装检测与包根验证共用）。</summary>
        internal const string ArchitecturePackageId = "cn.runestone.aesir.architecture";

        /// <summary>
        /// 依赖 Git URL 模板，{0} 为 RAA 版本号——锚定 CI subtree split 生成的版本分支，
        /// 与根 README 的 UPM 安装教程同款锚定方式；两包同号发版，分支版本由本包 version 推导。
        /// </summary>
        internal const string DependencyGitUrlTemplate =
            "https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-v{0}";

        /// <summary>本包 package.json 解析失败时的兜底版本（与 package.json 的 version 随发版同步 bump）。</summary>
        internal const string FallbackSelfVersion = "0.26.0";

        /// <summary>菜单路径：Aesir Modules 包专属工具，归 Tools/Aesir/Modules/ 组。</summary>
        public const string MenuPath = "Tools/Aesir/Modules/Install Dependencies";

        /// <summary>
        /// 安装进行中标记（SessionState 键）：安装成功会触发编译域重载，静态字段与后续执行全部中断，
        /// 收尾提示靠该标记在域重载后的 <see cref="ReportInstallResultAfterReload" /> 完成。
        /// </summary>
        const string InstallPendingKey = "Aesir.Modules.DependencyInstaller.InstallPending";

        /// <summary>进行中的安装请求（静态字段仅在同一域内有效，跨域重载由 SessionState 标记接管）。</summary>
        static AddRequest _addRequest;

        #endregion

        #region 菜单入口

        // validate：RAA 缺失时才显示——UPM 形态依赖自动拉取、正常 Assets 安装 RAA 在场，均不出现
        [MenuItem(MenuPath, true)]
        static bool ValidateMenuVisible()
        {
            return !IsArchitectureInstalled();
        }

        // priority 998：Modules 组内最前（组内现有 Script Doc Generator 999、Scene Editor Settings 1000）；
        // 组级 priority 取子项最小值 998，与 Architecture 组（995）差 ≤10 不产生分割线
        [MenuItem(MenuPath, false, 998)]
        static void InstallDependencies()
        {
            if (_addRequest != null && !_addRequest.IsCompleted)
            {
                EditorUtility.DisplayDialog("安装进行中",
                    "上一个 Aesir Architecture 安装请求仍在进行，请稍候。", "确定");
                return;
            }

            var selfVersion = ReadSelfVersion();
            var gitUrl = BuildDependencyGitUrl(selfVersion);
            var message =
                "检测到 Aesir Architecture (RAA) 缺失，AesirModules 核心程序集当前无法编译。\n\n" +
                $"将安装：Aesir Architecture v{selfVersion}\n" +
                $"Git URL：{gitUrl}\n" +
                "安装方式：Package Manager（安装到 Packages/，作为 UPM 包管理）\n" +
                "安装完成后 AesirModules 核心程序集将自动恢复编译。\n" +
                "（需要可访问 GitHub 的网络环境）\n\n" +
                "是否继续安装？";
            if (!EditorUtility.DisplayDialog("安装 Aesir 依赖", message, "安装", "取消"))
            {
                return;
            }

            SessionState.SetBool(InstallPendingKey, true);
            Debug.Log($"[Aesir] 正在通过 Package Manager 安装 Aesir Architecture（{gitUrl}）…");
            _addRequest = Client.Add(gitUrl);
            EditorApplication.update += PollAddRequest;
        }

        /// <summary>
        /// 安装收尾：安装成功触发编译域重载后，由 SessionState 标记唤起本方法给出结果提示；
        /// 安装失败不触发域重载，由 <see cref="PollAddRequest" /> 直接弹窗并清除标记。
        /// </summary>
        [InitializeOnLoadMethod]
        static void ReportInstallResultAfterReload()
        {
            if (!SessionState.GetBool(InstallPendingKey, false))
            {
                return;
            }

            if (!IsArchitectureInstalled())
            {
                return; // 安装尚未生效（域重载早于安装完成），保留标记待下次域加载再查
            }

            SessionState.SetBool(InstallPendingKey, false);
            Debug.Log("[Aesir] Aesir Architecture 安装完成，AesirModules 核心程序集已恢复编译。" +
                      "（该包位于 Packages/ 下，后续版本更新经 Package Manager 移除后重新 Add Git URL）");
        }

        /// <summary>轮询安装请求：失败即时弹窗；成功路径不在此提示（域重载会中断后续执行，转交 SessionState 标记收尾）。</summary>
        static void PollAddRequest()
        {
            if (_addRequest == null || !_addRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= PollAddRequest;

            if (_addRequest.Status == StatusCode.Failure)
            {
                SessionState.SetBool(InstallPendingKey, false);
                EditorUtility.DisplayDialog("安装失败",
                    "Aesir Architecture 安装失败：\n" + _addRequest.Error.message +
                    "\n\n请检查网络（需可访问 GitHub）后重试。", "确定");
            }

            _addRequest = null;
        }

        #endregion

        #region RAA 安装检测

        /// <summary>RAA 是否已安装：UPM 注册（含间接依赖）或 Assets 形态锚点定位，任一命中即视为已安装。</summary>
        internal static bool IsArchitectureInstalled()
        {
            return IsRegisteredInPackageManager() || HasArchitectureLookupAsset();
        }

        /// <summary>UPM 形态检测：包注册列表（含直接与间接依赖）中是否含 RAA。</summary>
        internal static bool IsRegisteredInPackageManager()
        {
            var registered = PackageInfo.GetAllRegisteredPackages();
            if (registered == null)
            {
                return false;
            }

            var names = new string[registered.Length];
            for (var i = 0; i < registered.Length; i++)
            {
                names[i] = registered[i].name;
            }

            return ContainsArchitectureName(names);
        }

        /// <summary>注册包名列表中是否含 RAA（单元测试参数化用）。</summary>
        internal static bool ContainsArchitectureName(string[] packageNames)
        {
            if (packageNames == null)
            {
                return false;
            }

            foreach (var packageName in packageNames)
            {
                if (string.Equals(packageName, ArchitecturePackageId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Assets 形态检测：按文件名定位各包包根的 AesirPathLookup 锚点资产，
        /// 锚点所在目录经 package.json 验证是否为 RAA 包根（定位机制与 RAA 的 AesirAssetPaths 同款，
        /// 支持 Runestone 目录整体移动后的任意位置）。
        /// </summary>
        internal static bool HasArchitectureLookupAsset()
        {
            foreach (var guid in AssetDatabase.FindAssets("AesirPathLookup"))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var packageRoot = GetPackageRootFromAssetPath(assetPath);
                if (packageRoot != null && PackageRootHasArchitectureId(packageRoot))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 纯函数（供单元测试）

        /// <summary>锚点资产路径 → 包根（锚点所在目录）；输入空或无目录层级时返回 null。</summary>
        internal static string GetPackageRootFromAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            var separator = assetPath.LastIndexOf('/');
            return separator > 0 ? assetPath.Substring(0, separator) : null;
        }

        /// <summary>
        /// 目录是否为 RAA 包根：package.json 可解析且顶层 name 字段为 RAA 包 id。
        /// 不用子串匹配——RAM 的 dependencies 也引用着 RAA 的包 id（依赖声明键名），
        /// 全文 Contains 会把 RAM 包根误判为 RAA。
        /// </summary>
        internal static bool PackageRootHasArchitectureId(string packageRootProjectRelativePath)
        {
            if (string.IsNullOrEmpty(packageRootProjectRelativePath))
            {
                return false;
            }

            try
            {
                var pkgJsonPath = ToAbsolute(Path.Combine(packageRootProjectRelativePath, "package.json"));
                if (!File.Exists(pkgJsonPath))
                {
                    return false;
                }

                var manifest = JsonUtility.FromJson<PackageManifest>(File.ReadAllText(pkgJsonPath));
                return string.Equals(manifest.name, ArchitecturePackageId, StringComparison.Ordinal);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException)
            {
                return false; // 读取 / 解析失败按非 RAA 包处理（fail-closed）
            }
        }

        /// <summary>本包版本号 → RAA 版本分支名（两包同号发版，版本由 CI 校验一致）。</summary>
        internal static string BuildDependencyBranchName(string selfVersion)
        {
            return "AesirArchitecture-v" + selfVersion;
        }

        /// <summary>本包版本号 → 依赖 Git URL。</summary>
        internal static string BuildDependencyGitUrl(string selfVersion)
        {
            return string.Format(DependencyGitUrlTemplate, selfVersion);
        }

        /// <summary>
        /// 读本包 package.json 的 version 作为依赖版本号：源文件位于 <c>&lt;包根&gt;/Editor/Bootstrap/</c>，
        /// 经编译期 <see cref="CallerFilePathAttribute" /> 实参定位包根（Assets 与 UPM 形态目录结构一致）；
        /// 定位或解析失败回退 <see cref="FallbackSelfVersion" />。
        /// </summary>
        internal static string ReadSelfVersion([CallerFilePath] string sourceFilePath = "")
        {
            try
            {
                var packageRoot = Path.GetDirectoryName(
                    Path.GetDirectoryName(Path.GetDirectoryName(sourceFilePath)));
                var manifest = JsonUtility.FromJson<PackageManifest>(
                    File.ReadAllText(Path.Combine(packageRoot ?? string.Empty, "package.json")));
                return string.IsNullOrEmpty(manifest.version) ? FallbackSelfVersion : manifest.version;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException)
            {
                return FallbackSelfVersion; // 包根定位 / 读取 / 解析失败的兜底（坏安装形态）
            }
        }

        /// <summary>包 package.json 的最小解析模型（JsonUtility 忽略未声明字段）。</summary>
        [Serializable]
        class PackageManifest
        {
            public string name;
            public string version;
        }

        #endregion

        #region 路径工具

        /// <summary>项目相对路径 → 绝对路径。</summary>
        static string ToAbsolute(string projectRelativePath) =>
            Path.GetFullPath(Path.Combine(ProjectRootPath, projectRelativePath));

        /// <summary>Unity 项目根目录（Assets 的上一级）。</summary>
        static string ProjectRootPath =>
            Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;

        #endregion
    }
}
