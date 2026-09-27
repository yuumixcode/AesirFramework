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
    /// 经 <see cref="ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol" /> 方法
    /// 确保所有构建目标中都存在 <c>AESIR_ARCHITECTURE</c> 宏定义，
    /// 从而使依赖本架构的其他包可以通过条件编译指令在编译期检测架构是否可用。
    /// <para>
    /// 写入宏会触发脚本重编译，而静态构造函数运行在程序集注册 / 域重载期间——
    /// 此时直接发起重编译属于重入，可能使 Unity 走到程序集注册的致命分支。
    /// 故实际写入推迟到 <see cref="EditorApplication.delayCall" />（编辑器空闲首帧）执行；
    /// <see cref="ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol" /> 本身按构建目标逐一比对、
    /// 仅在符号确实缺失时才写入（已存在则零写入、不触发重编译），此行为不因推迟而改变。
    /// </para>
    /// </remarks>
    /// <seealso cref="ScriptingSymbolEditorUtility" />
    [InitializeOnLoad]
    internal static class EnsureAesirArchitectureDefine
    {
        const string Symbol = "AESIR_ARCHITECTURE";

        static EnsureAesirArchitectureDefine()
        {
            // 推迟到编辑器空闲首帧再确保宏：静态构造函数运行于程序集注册/域重载期间，此时机写宏（触发重编译）属于重入
            EditorApplication.delayCall += Ensure;
        }

        static void Ensure()
        {
            ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol(Symbol);
        }
    }
}
