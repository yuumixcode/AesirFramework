using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// GameObject 禁用时自动移除所有监听。挂载此组件后，当 GameObject 被禁用时将执行批量移除操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RemoveListenerOnDisableTrigger : RemoveListenerTrigger
    {
        /// <summary>
        /// Unity 在 GameObject 禁用时调用该方法，自动移除所有已注册的监听。
        /// </summary>
        void OnDisable() => RemoveAllListeners();
    }
}
