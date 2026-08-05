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
    /// RAA 框架标准 MonoBehaviour 基类，根据运行环境自动选择序列化方式。
    /// </summary>
    /// <remarks>
    /// 通过条件编译在编译期决定基类，避免运行时反射开销：
    /// <list type="bullet">
    /// <item>编辑器 + 定义了 <c>ODIN_INSPECTOR</c>：继承 <c>SerializedMonoBehaviour</c>，获得 Odin 序列化能力，编辑器内可使用 Odin Inspector。</item>
    /// <item>运行时 + 定义了 <c>ODIN_INSPECTOR</c> 且未定义 <c>ODIN_INSPECTOR_EDITOR_ONLY</c>：继承 <c>SerializedMonoBehaviour</c>，运行时也使用 Odin 序列化。</item>
    /// <item>其他情况：继承 <c>MonoBehaviour</c>，使用 Unity 默认序列化。</item>
    /// </list>
    /// <para>
    /// <c>ODIN_INSPECTOR_EDITOR_ONLY</c> 宏用于在运行时剔除 Odin 序列化（减小包体），
    /// 同时保留编辑器内的 Odin Inspector 体验。
    /// </para>
    /// </remarks>
    /// <seealso cref="AesirScriptableObject"/>
    public abstract class AesirMonoBehaviour :
#if UNITY_EDITOR && ODIN_INSPECTOR
        SerializedMonoBehaviour
#elif !UNITY_EDITOR && !ODIN_INSPECTOR_EDITOR_ONLY && ODIN_INSPECTOR
SerializedMonoBehaviour
#else
MonoBehaviour
#endif
    { }
}
