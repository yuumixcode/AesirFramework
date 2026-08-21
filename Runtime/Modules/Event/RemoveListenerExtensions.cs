using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件监听器自动移除扩展方法类，用于绑定移除操作到 Unity 生命周期
    /// </summary>
    /// <remarks>
    /// 提供一组扩展方法，将 <see cref="AutoRemoveListenerHandle" /> 绑定到 Unity 生命周期事件
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
        /// 当当前活动场景卸载时自动移除监听
        /// </summary>
        /// <param name="removeListener">要绑定到场景卸载事件的监听句柄</param>
        /// <remarks>
        /// 使用 <see cref="RemoveListenerOnSceneUnloadedTrigger.Instance" /> 单例进行管理，
        /// 该单例按场景句柄分桶存储监听句柄，在对应场景卸载时批量移除该场景下的所有监听，
        /// 避免全局遍历带来的性能开销。
        /// <para>
        /// additive 多场景流程中活动场景不一定是监听者实际所在场景，
        /// 此时请改用 <see cref="RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle, Scene)" />
        /// 或 <see cref="RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle, MonoBehaviour)" /> 显式指定归属场景。
        /// </para>
        /// </remarks>
        public static void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle removeListener)
        {
            RemoveListenerOnSceneUnloadedTrigger.Instance.AddRemoveListenerHandle(removeListener);
        }

        /// <summary>
        /// 当指定场景卸载时自动移除监听
        /// </summary>
        /// <param name="removeListener">要绑定到场景卸载事件的监听句柄</param>
        /// <param name="scene">监听归属的场景，卸载该场景时移除监听</param>
        /// <remarks>
        /// 按指定场景的 <see cref="Scene.handle" /> 分桶，适合 additive 多场景流程：
        /// 显式传入监听者实际所在的场景，避免无参版本按活动场景归桶导致的误清理。
        /// </remarks>
        public static void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle removeListener,
            Scene scene)
        {
            RemoveListenerOnSceneUnloadedTrigger.Instance.AddRemoveListenerHandle(scene, removeListener);
        }

        /// <summary>
        /// 当监听者所在 GameObject 所属的场景卸载时自动移除监听
        /// </summary>
        /// <param name="removeListener">要绑定到场景卸载事件的监听句柄</param>
        /// <param name="mono">监听者，按其 GameObject 所在场景归桶</param>
        /// <remarks>
        /// 以 <paramref name="mono" /> 所在 GameObject 的场景作为归属场景分桶，
        /// 适合 additive 多场景流程：即使当前活动场景并非监听者所在场景，也能正确归桶。
        /// </remarks>
        public static void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle removeListener,
            MonoBehaviour mono)
        {
            RemoveListenerOnSceneUnloadedTrigger.Instance.AddRemoveListenerHandle(mono.gameObject.scene,
                removeListener);
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
