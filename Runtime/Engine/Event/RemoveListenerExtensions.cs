using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件监听器自动移除扩展方法类，用于绑定移除操作到 Unity 生命周期
    /// </summary>
    public static class RemoveListenerExtensions
    {
        /// <summary>
        /// 当指定的 MonoBehaviour 所属 GameObject 被销毁时自动移除监听
        /// </summary>
        public static void RemoveListenerWhenGameObjectOnDestroyed(
            this AutoRemoveListenerHandle removeListener,
            MonoBehaviour mono)
        {
            removeListener.RemoveListenerWhenGameObjectOnDestroyed(mono.gameObject);
        }

        /// <summary>
        /// 当指定的 MonoBehaviour 所属 GameObject 被禁用（OnDisable）时自动移除监听
        /// </summary>
        public static void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle removeListener,
            MonoBehaviour mono)
        {
            removeListener.RemoveListenerWhenGameObjectOnDisable(mono.gameObject);
        }

        /// <summary>
        /// 当指定的 GameObject 被销毁时自动移除监听
        /// </summary>
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
        public static void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle removeListener,
            GameObject gameObject)
        {
            var invoker = GetOrAddComponent<RemoveListenerOnDisableTrigger>(gameObject);
            invoker.AddRemoveListenerHandle(removeListener);
        }

        /// <summary>
        /// 当场景卸载时自动移除监听
        /// </summary>
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
