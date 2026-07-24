using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 在所属 GameObject 销毁时自动移除所有监听。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RemoveListenerOnDestroyTrigger : RemoveListenerTrigger
    {
        /// <summary>
        /// Unity 在销毁此 MonoBehaviour 所在的 GameObject 时自动调用，执行所有已注册监听的移除操作。
        /// </summary>
        void OnDestroy() => RemoveAllListeners();
    }
}
