using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// GameObject 禁用时自动移除所有监听。挂载此组件后，当 GameObject 被禁用时将执行批量移除操作。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="RemoveListenerOnDestroyTrigger"/> 不同，本组件在 <c>OnDisable</c> 而非 <c>OnDestroy</c> 时触发移除。
    /// 适用于 UI 面板等频繁启用/禁用的对象：每次禁用时移除监听，避免对象池回收后仍保留无效监听导致空引用。
    /// </remarks>
    /// <seealso cref="RemoveListenerTrigger"/>
    /// <seealso cref="RemoveListenerOnDestroyTrigger"/>
    [DisallowMultipleComponent]
    public sealed class RemoveListenerOnDisableTrigger : RemoveListenerTrigger
    {
        /// <summary>
        /// Unity 在 GameObject 禁用时调用该方法，自动移除所有已注册的监听。
        /// </summary>
        void OnDisable() => RemoveAllListeners();
    }
}
