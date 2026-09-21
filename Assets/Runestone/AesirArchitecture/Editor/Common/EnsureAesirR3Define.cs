using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 按需确保 <c>AESIR_R3</c> 脚本宏定义符号 —— 检测到 Cysharp R3 程序集时启用，未检测到时移除。
    /// </summary>
    /// <remarks>
    /// <para>
    /// R3 集成程序集（<c>Runestone.AesirArchitecture.R3</c>）以 <c>AESIR_R3</c> 作为
    /// <c>defineConstraints</c> 守卫：宏未定义时该程序集整体不参与编译，主包不受影响。
    /// 本类负责让这个宏与「项目里到底有没有 R3」保持一致，避免用户手工维护。
    /// </para>
    /// <para>
    /// R3 在 Unity 中的常见安装方式都能被识别：插件 DLL（如 <c>Assets/Plugins/R3/R3.dll</c>）、
    /// NuGetForUnity 安装到 <c>Assets/Packages/</c>、或任意位置的 <c>R3.dll</c>。
    /// 检测不到时移除宏（幂等），因此卸载 R3 后无需手工清理宏。
    /// </para>
    /// </remarks>
    /// <seealso cref="ScriptingSymbolUtility" />
    [InitializeOnLoad]
    internal static class EnsureAesirR3Define
    {
        const string Symbol = "AESIR_R3";

        const string R3AssemblyName = "R3";

        static EnsureAesirR3Define()
        {
            if (HasR3Assembly())
            {
                ScriptingSymbolUtility.EnsureScriptingDefineSymbol(Symbol);
            }
            else
            {
                ScriptingSymbolUtility.RemoveScriptingDefineSymbol(Symbol);
            }
        }

        /// <summary>
        /// 检测项目中是否存在 R3 程序集。
        /// </summary>
        /// <returns>存在返回 <c>true</c>。</returns>
        static bool HasR3Assembly()
        {
            // 已加载的程序集（编辑器启动后插件 DLL 已进入 AppDomain）
            if (AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.GetName().Name == R3AssemblyName))
            {
                return true;
            }

            // 兜底：尚未加载时按预编译程序集清单查找（覆盖 NuGetForUnity 等任意安装位置）
            return CompilationPipeline
                .GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.All)
                .Any(path => string.Equals(
                    System.IO.Path.GetFileNameWithoutExtension(path),
                    R3AssemblyName,
                    StringComparison.Ordinal));
        }
    }
}
