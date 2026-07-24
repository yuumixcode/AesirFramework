using UnityEditor;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 自动确保 <c>AESIR_ARCHITECTURE</c> 脚本宏定义符号存在。
    /// <para>
    /// 通过 <see cref="InitializeOnLoadAttribute" /> 在编辑器加载时自动执行，
    /// 供 Aesir 系列其他插件通过 <c>#if AESIR_ARCHITECTURE</c> 检测本架构是否存在。
    /// </para>
    /// </summary>
    [InitializeOnLoad]
    internal static class EnsureAesirArchitectureDefine
    {
        const string Symbol = "AESIR_ARCHITECTURE";

        static EnsureAesirArchitectureDefine()
        {
            ScriptingSymbolUtility.EnsureScriptingDefineSymbol(Symbol);
        }
    }
}
