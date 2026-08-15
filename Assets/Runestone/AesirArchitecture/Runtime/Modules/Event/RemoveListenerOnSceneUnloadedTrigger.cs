using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 任意场景卸载时自动移除该场景注册的监听。按场景句柄（<see cref="Scene.handle" />）分桶，
    /// 场景 A 卸载不会误杀场景 B 的监听。
    /// <para>挂载在 [Aesir Architecture] GameObject 上，通过 <see cref="Instance" /> 访问。</para>
    /// </summary>
    /// <remarks>
    /// 作为全局单例挂载在 <c>[Aesir Architecture]</c> GameObject 上，通过 <see cref="Instance"/> 访问。
    /// <para>
    /// 按场景句柄（<see cref="Scene.handle"/>）分桶管理监听句柄，场景 A 卸载时仅移除场景 A 注册的监听，
    /// 不会误杀场景 B 的监听。相比按场景名分桶，句柄分桶保证：不同路径下的同名场景各持唯一句柄、互不共享桶；
    /// 场景卸载后重新加载会获得新句柄，不存在旧桶残留。
    /// </para>
    /// <para>
    /// 通过 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c>
    /// 在每次域加载时重置静态单例字段，确保在编辑器关闭 Domain Reload 时不残留上一次 Play 会话的旧引用。
    /// </para>
    /// <para>
    /// 在 <c>Awake</c> 中订阅 <c>SceneManager.sceneUnloaded</c>，在 <c>OnDestroy</c> 中取消订阅，
    /// 避免组件销毁后仍接收场景卸载事件。
    /// </para>
    /// </remarks>
    /// <seealso cref="RemoveListenerExtensions"/>
    /// <seealso cref="RemoveListenerHandleCollection"/>
    [DisallowMultipleComponent]
    public sealed class RemoveListenerOnSceneUnloadedTrigger : AesirMonoBehaviour
    {
        static RemoveListenerOnSceneUnloadedTrigger _instance;

        readonly Dictionary<int, RemoveListenerHandleCollection> _sceneHandles =
            new Dictionary<int, RemoveListenerHandleCollection>();

        /// <summary>
        /// 重置所有实例状态：清空场景句柄桶、取消订阅场景事件
        /// </summary>
        /// <remarks>
        /// 由 <see cref="ResetStatics" /> 和 <see cref="OnDestroy" /> 内部调用，
        /// 确保无论域重载还是组件销毁，都走同一条完整重置路径。
        /// </remarks>
        void Reset()
        {
            _sceneHandles.Clear();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 域加载时重置静态单例，兼容关闭 Domain Reload 的 Play 模式设置
        /// </summary>
        /// <remarks>
        /// 由 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c> 自动触发，无需手动调用。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            if (_instance != null)
            {
                _instance.Reset();
            }

            _instance = null;
        }

        /// <summary>
        /// 获取全局唯一的场景卸载监听移除器实例
        /// </summary>
        /// <remarks>
        /// 优先在已加载场景中查找预放置的实例；未找到时通过 <see cref="AesirArchitecture.GetOrAddComponent{T}"/>
        /// 挂载到 <c>[Aesir Architecture]</c> GameObject 上，复用架构宿主对象。
        /// </remarks>
        public static RemoveListenerOnSceneUnloadedTrigger Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                _instance = FindAnyObjectByType<RemoveListenerOnSceneUnloadedTrigger>();
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = AesirArchitecture.GetOrAddComponent<RemoveListenerOnSceneUnloadedTrigger>();
                return _instance;
            }
        }

        void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDestroy()
        {
            Reset();
        }

        /// <summary>
        /// 添加监听句柄，使其在当前活动场景卸载时自动移除
        /// </summary>
        /// <param name="handle">要注册的自动移除监听句柄，封装了目标监听与移除委托</param>
        /// <remarks>
        /// 以调用时的 <c>SceneManager.GetActiveScene()</c> 作为分桶依据。
        /// additive 多场景流程中活动场景不一定是监听者实际所在场景，此时请改用
        /// <see cref="AddRemoveListenerHandle(Scene, AutoRemoveListenerHandle)"/> 显式指定归属场景。
        /// </remarks>
        public void AddRemoveListenerHandle(AutoRemoveListenerHandle handle)
        {
            AddRemoveListenerHandle(SceneManager.GetActiveScene(), handle);
        }

        /// <summary>
        /// 添加监听句柄，使其在指定场景卸载时自动移除
        /// </summary>
        /// <param name="scene">监听归属的场景，按其 <see cref="Scene.handle"/> 分桶</param>
        /// <param name="handle">要注册的自动移除监听句柄，封装了目标监听与移除委托</param>
        /// <remarks>
        /// 以 <paramref name="scene"/> 的 <see cref="Scene.handle"/> 作为分桶键，将句柄归入该场景的集合。
        /// 当对应场景卸载时，仅移除该桶中的监听。additive 多场景流程中应传入监听者实际所在场景，
        /// 避免误入活动场景的桶导致监听被提前移除或永不清理。
        /// </remarks>
        public void AddRemoveListenerHandle(Scene scene, AutoRemoveListenerHandle handle)
        {
            var sceneHandle = scene.handle;
            if (!_sceneHandles.TryGetValue(sceneHandle, out var collection))
            {
                collection = new RemoveListenerHandleCollection();
                _sceneHandles[sceneHandle] = collection;
            }

            collection.Add(handle);
        }

        /// <summary>
        /// 当场景卸载时移除该场景下所有已注册的监听。
        /// </summary>
        void OnSceneUnloaded(Scene scene)
        {
            if (_sceneHandles.TryGetValue(scene.handle, out var collection))
            {
                collection.RemoveAllListeners();
                _sceneHandles.Remove(scene.handle);
            }
        }
    }
}
