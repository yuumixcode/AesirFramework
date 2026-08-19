using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// GameObject 禁用时自动移除所有监听。挂载此组件后，当 GameObject 被禁用时将执行批量移除操作。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="RemoveListenerOnDestroyTrigger"/> 不同，本组件在 <c>OnDisable</c> 而非 <c>OnDestroy</c> 时触发移除。
    /// 适用于 UI 面板等频繁启用/禁用的对象：每次禁用时移除监听，避免对象池回收后仍保留无效监听导致空引用。
    /// <para><b>两种使用方式</b>：</para>
    /// <list type="bullet">
    /// <item><b>推荐：编辑阶段提前挂载</b>——订阅时框架经 <c>GetComponent</c> 直接复用，
    /// 避免运行时 <c>AddComponent</c> 的开销。</item>
    /// <item><b>兜底：运行时自动挂载</b>——未预挂载时，
    /// <c>RemoveListenerExtensions.RemoveListenerWhenGameObjectOnDisable</c> 扩展方法
    /// 会在订阅时自动为本 GameObject 添加本组件（方便性兜底，付一次 <c>AddComponent</c> 开销）。</item>
    /// </list>
    /// <para>两种方式经 <c>[DisallowMultipleComponent]</c> 保证不会重复添加。</para>
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
