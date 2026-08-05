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
    /// <remarks>
    /// <c>[InitializeOnLoad]</c> 特性使 Unity 在编辑器加载时自动调用此类的静态构造函数，
    /// 静态构造函数通过 <see cref="ScriptingSymbolUtility.EnsureScriptingDefineSymbol"/> 方法
    /// 确保所有构建目标中都存在 <c>AESIR_ARCHITECTURE</c> 宏定义，
    /// 从而使依赖本架构的其他包可以通过条件编译指令在编译期检测架构是否可用。
    /// </remarks>
    /// <seealso cref="ScriptingSymbolUtility"/>
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
