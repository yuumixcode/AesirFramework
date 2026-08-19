using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 在所属 GameObject 销毁时自动移除所有监听。
    /// </summary>
    /// <remarks>
    /// <c>[DisallowMultipleComponent]</c> 防止在同一 GameObject 上重复挂载本组件。
    /// <para><b>两种使用方式</b>：</para>
    /// <list type="bullet">
    /// <item><b>推荐：编辑阶段提前挂载</b>——订阅时框架经 <c>GetComponent</c> 直接复用，
    /// 避免运行时 <c>AddComponent</c> 的开销。在 Inspector 中手动添加本组件属正常且推荐的做法。</item>
    /// <item><b>兜底：运行时自动挂载</b>——未预挂载时，
    /// <c>RemoveListenerExtensions.RemoveListenerWhenGameObjectOnDestroyed</c> 扩展方法
    /// 会在订阅时自动为本 GameObject 添加本组件（方便性兜底，付一次 <c>AddComponent</c> 开销）。</item>
    /// </list>
    /// <para>两种方式经 <c>[DisallowMultipleComponent]</c> 保证不会重复添加；在 Inspector 中看到本组件属于正常现象。</para>
    /// </remarks>
    /// <seealso cref="RemoveListenerTrigger"/>
    /// <seealso cref="RemoveListenerExtensions"/>
    [DisallowMultipleComponent]
    public sealed class RemoveListenerOnDestroyTrigger : RemoveListenerTrigger
    {
        /// <summary>
        /// Unity 在销毁此 MonoBehaviour 所在的 GameObject 时自动调用，执行所有已注册监听的移除操作。
        /// </summary>
        void OnDestroy() => RemoveAllListeners();
    }
}
