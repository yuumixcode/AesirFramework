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
    /// RAA 框架标准 ScriptableObject 基类，根据运行环境自动选择序列化方式。
    /// </summary>
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
