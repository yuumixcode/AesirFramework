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
