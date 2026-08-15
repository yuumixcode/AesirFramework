using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 在所属 GameObject 销毁时自动移除所有监听。
    /// </summary>
    /// <remarks>
    /// <c>[DisallowMultipleComponent]</c> 防止在同一 GameObject 上重复挂载本组件。
    /// <para>
    /// 通常不直接添加，而是通过 <c>RemoveListenerExtensions.RemoveListenerWhenGameObjectOnDestroyed</c>
    /// 扩展方法间接使用，该扩展方法会自动为本组件查找或添加监听句柄。
    /// </para>
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
