using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件监听器自动移除扩展方法类，用于绑定移除操作到 Unity 生命周期
    /// </summary>
    /// <remarks>
    /// 提供一组扩展方法，将 <see cref="AutoRemoveListenerHandle"/> 绑定到 Unity 生命周期事件
    /// （OnDestroy / OnDisable / SceneUnloaded），实现监听的自动清理，
    /// 避免因忘记手动移除监听而导致的内存泄漏。
    /// </remarks>
    public static class RemoveListenerExtensions
    {
        /// <summary>
        /// 当指定的 MonoBehaviour 所属 GameObject 被销毁时自动移除监听
        /// </summary>
        /// <param name="removeListener">要绑定到销毁事件的监听句柄</param>
        /// <param name="mono">监听生命周期所依附的 MonoBehaviour</param>
        public static void RemoveListenerWhenGameObjectOnDestroyed(
            this AutoRemoveListenerHandle removeListener,
            MonoBehaviour mono)
        {
            removeListener.RemoveListenerWhenGameObjectOnDestroyed(mono.gameObject);
        }

        /// <summary>
        /// 当指定的 MonoBehaviour 所属 GameObject 被禁用（OnDisable）时自动移除监听
        /// </summary>
        /// <param name="removeListener">要绑定到禁用事件的监听句柄</param>
        /// <param name="mono">监听生命周期所依附的 MonoBehaviour</param>
        public static void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle removeListener,
            MonoBehaviour mono)
        {
            removeListener.RemoveListenerWhenGameObjectOnDisable(mono.gameObject);
        }

        /// <summary>
        /// 当指定的 GameObject 被销毁时自动移除监听
        /// </summary>
        /// <param name="removeListener">要绑定到销毁事件的监听句柄</param>
        /// <param name="gameObject">监听生命周期所依附的 GameObject</param>
        public static void RemoveListenerWhenGameObjectOnDestroyed(
            this AutoRemoveListenerHandle removeListener,
            GameObject gameObject)
        {
            var invoker = GetOrAddComponent<RemoveListenerOnDestroyTrigger>(gameObject);
            invoker.AddRemoveListenerHandle(removeListener);
        }

        /// <summary>
        /// 当指定的 GameObject 被禁用（OnDisable）时自动移除监听
        /// </summary>
        /// <param name="removeListener">要绑定到禁用事件的监听句柄</param>
        /// <param name="gameObject">监听生命周期所依附的 GameObject</param>
        public static void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle removeListener,
            GameObject gameObject)
        {
            var invoker = GetOrAddComponent<RemoveListenerOnDisableTrigger>(gameObject);
            invoker.AddRemoveListenerHandle(removeListener);
        }

        /// <summary>
        /// 当场景卸载时自动移除监听
        /// </summary>
        /// <param name="removeListener">要绑定到场景卸载事件的监听句柄</param>
        /// <remarks>
        /// 使用 <see cref="RemoveListenerOnSceneUnloadedTrigger.Instance"/> 单例进行管理，
        /// 该单例按场景名分桶存储监听句柄，在对应场景卸载时批量移除该场景下的所有监听，
        /// 避免全局遍历带来的性能开销。
        /// </remarks>
        public static void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle removeListener)
        {
            RemoveListenerOnSceneUnloadedTrigger.Instance.AddRemoveListenerHandle(removeListener);
        }

        static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            var invoker = gameObject.GetComponent<T>();
            if (invoker == null)
            {
                invoker = gameObject.AddComponent<T>();
            }

            return invoker;
        }
    }
}
