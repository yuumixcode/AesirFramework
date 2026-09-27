using System;
using System.Linq;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 自动维护 <c>AESIR_MODULES_UNITASK</c> 脚本宏定义符号——SceneModule 的 UniTask 驱动分支与
    /// UniTask 适配程序集（Runestone.AesirModules.UniTask）均以该宏为编译开关。
    /// <para>
    /// 维护规则：游戏工程以 UPM 包（<c>com.cysharp.unitask</c>）安装 UniTask 时，
    /// 宏由核心程序集与适配程序集的 versionDefines 全权管理（装/卸自动生效），本维护器不干预全局符号；
    /// 其他安装形态（unitypackage / DLL 导入）按「域内是否存在 Cysharp.Threading.Tasks 程序集」
    /// 增删全局符号——存在则补齐，不存在则移除。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 写入宏会触发脚本重编译，而静态构造函数运行在程序集注册 / 域重载期间——
    /// 此时直接发起重编译属于重入，可能使 Unity 走到程序集注册的致命分支。
    /// 故实际同步推迟到 <see cref="EditorApplication.delayCall" />（编辑器空闲首帧）执行；
    /// <see cref="ScriptingSymbolEditorUtility" /> 本身按构建目标逐一比对、仅在符号确实变化时才写入
    /// （无变化则零写入、不触发重编译），此行为不因推迟而改变。
    /// <para>
    /// 边界：工程移除 unitypackage 形态的 UniTask 后，全局宏在下一次域重载才被移除——若先于维护器
    /// 运行触发编译，核心程序集的 UniTask 分支会出现 CS0246，重装 UniTask 或在 Project Settings
    /// 手动移除该宏即可恢复。
    /// </para>
    /// </remarks>
    [InitializeOnLoad]
    internal static class AesirUniTaskDefineKeeper
    {
        /// <summary>UniTask 适配程序集与核心宏分支共用的脚本宏定义符号。</summary>
        internal const string DefineName = "AESIR_MODULES_UNITASK";

        /// <summary>UniTask 的程序集名（asmdef 源码安装与预编译 DLL 安装同名）。</summary>
        internal const string UniTaskAssemblyName = "Cysharp.Threading.Tasks";

        /// <summary>UniTask 的 UPM 包名（该安装形态由 versionDefines 全权管理宏）。</summary>
        internal const string UniTaskUpmPackageName = "com.cysharp.unitask";

        static AesirUniTaskDefineKeeper()
        {
            // 推迟到编辑器空闲首帧再同步宏：静态构造函数运行于程序集注册/域重载期间，此时机写宏（触发重编译）属于重入
            EditorApplication.delayCall += SyncDefine;
        }

        /// <summary>
        /// 全局宏同步决策（纯函数，供测试锁定矩阵）：<see langword="null" /> 表示不干预
        /// （UPM 安装形态，交由 versionDefines 管理）；<see langword="true" /> 表示确保宏存在；
        /// <see langword="false" /> 表示确保宏移除。
        /// </summary>
        internal static bool? DecideDefineAction(bool upmPackageInstalled, bool uniTaskAssemblyLoaded)
        {
            if (upmPackageInstalled)
            {
                return null;
            }

            return uniTaskAssemblyLoaded;
        }

        /// <summary>
        /// 按当前工程状态同步全局宏（延迟调用入口，亦供测试与诊断直接调用）。
        /// </summary>
        internal static void SyncDefine()
        {
            var action = DecideDefineAction(IsUpmPackageInstalled(), IsUniTaskAssemblyLoaded());
            if (action == true)
            {
                ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol(DefineName);
            }
            else if (action == false)
            {
                ScriptingSymbolEditorUtility.RemoveScriptingDefineSymbol(DefineName);
            }
        }

        /// <summary>UniTask 是否以 UPM 包形态安装（该形态宏由 versionDefines 全权管理，不干预全局符号）。</summary>
        internal static bool IsUpmPackageInstalled()
        {
            return PackageInfo.GetAllRegisteredPackages()
                .Any(package => package.name == UniTaskUpmPackageName);
        }

        /// <summary>UniTask 程序集是否已加载进当前域（asmdef 源码安装与预编译 DLL 安装均覆盖）。</summary>
        internal static bool IsUniTaskAssemblyLoaded()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.GetName().Name == UniTaskAssemblyName);
        }
    }
}
