#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;

#elif !UNITY_EDITOR && !ODIN_INSPECTOR_EDITOR_ONLY && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using UnityEngine;
#endif

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// RAA 架构标准 ScriptableObject 基类，根据运行环境自动选择序列化方式。
    /// </summary>
    /// <remarks>
    /// 通过条件编译在编译期决定基类，与 <see cref="AesirMonoBehaviour" /> 采用相同的策略：
    /// <list type="bullet">
    ///     <item>编辑器 + 定义了 <c>ODIN_INSPECTOR</c>：继承 <c>SerializedScriptableObject</c>，获得 Odin 序列化能力。</item>
    ///     <item>
    ///     运行时 + 定义了 <c>ODIN_INSPECTOR</c> 且未定义 <c>ODIN_INSPECTOR_EDITOR_ONLY</c>：继承
    ///     <c>SerializedScriptableObject</c>。
    ///     </item>
    ///     <item>其他情况：继承 <c>ScriptableObject</c>，使用 Unity 默认序列化。</item>
    /// </list>
    /// <para>
    /// <c>ODIN_INSPECTOR_EDITOR_ONLY</c> 宏用于在运行时剔除 Odin 序列化（减小包体），
    /// 同时保留编辑器内的 Odin Inspector 体验。
    /// <b>警告</b>：定义该宏后，编辑器分支经 Odin 序列化后端持久化、构建分支回退为裸 Unity 序列化后端——
    /// 编辑器内经 Odin 录入而 Unity 无法原生序列化的数据（字典、私有字段等）在玩家构建中会<b>静默丢失</b>。
    /// 使用该宏前请确认序列化字段均为 Unity 原生可序列化形态。
    /// </para>
    /// </remarks>
    /// <seealso cref="AesirMonoBehaviour" />
    public abstract class AesirScriptableObject :
#if UNITY_EDITOR && ODIN_INSPECTOR
        SerializedScriptableObject
#elif !UNITY_EDITOR && !ODIN_INSPECTOR_EDITOR_ONLY && ODIN_INSPECTOR
        SerializedScriptableObject
#else
        ScriptableObject
#endif
    { }
}
