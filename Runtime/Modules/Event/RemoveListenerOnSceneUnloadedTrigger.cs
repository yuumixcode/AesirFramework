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
    /// 相比按场景名分桶，句柄分桶保证：不同路径下的同名场景各持唯一句柄、互不共享桶；
    /// 场景卸载后重新加载会获得新句柄，不存在旧桶残留。
    /// <para>
    /// 通过 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c>
    /// 在每次域加载时重置静态单例字段，确保在编辑器关闭 Domain Reload 时不残留上一次 Play 会话的旧引用。
    /// </para>
    /// <para>
    /// 在 <c>Awake</c> 中订阅 <c>SceneManager.sceneUnloaded</c>，在 <c>OnDestroy</c> 中取消订阅，
    /// 避免组件销毁后仍接收场景卸载事件。
    /// </para>
    /// </remarks>
    /// <seealso cref="RemoveListenerExtensions" />
    /// <seealso cref="RemoveListenerHandleCollection" />
    [DisallowMultipleComponent]
    public sealed class RemoveListenerOnSceneUnloadedTrigger : AesirMonoBehaviour
    {
        static RemoveListenerOnSceneUnloadedTrigger _instance;

        readonly Dictionary<int, RemoveListenerHandleCollection> _sceneHandles =
            new Dictionary<int, RemoveListenerHandleCollection>();

        /// <summary>
        /// 获取全局唯一的场景卸载监听移除器实例
        /// </summary>
        /// <remarks>
        /// 优先在已加载场景中查找预放置的实例；未找到时通过 <see cref="AesirArchitecture.GetOrAddComponent{T}" />
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
                // 含未激活对象：未激活的预放置实例不被 Awake 赋值，Exclude 会让它被判为不存在而重复创建（Inspector 配置随之失效）
                _instance = FindAnyObjectByType<RemoveListenerOnSceneUnloadedTrigger>(FindObjectsInactive.Include);
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
            if (_instance != null && _instance != this)
            {
                // 重复实例只销毁自身，避免重复订阅场景卸载事件造成分桶分裂
                Destroy(this);
                return;
            }

            // 单例范式同 AesirArchitecture / MonoLifecycleProxy：Awake 即写入静态缓存完成判重；
            // Instance getter 的 FindAnyObjectByType 仅作销毁后重发现的兜底路径
            _instance = this;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDestroy()
        {
            ClearState();
        }

        /// <summary>
        /// 重置所有实例状态：逐桶执行全部句柄的移除回调、清空场景句柄桶、取消订阅场景事件
        /// </summary>
        /// <remarks>
        /// 由 <see cref="OnDestroy" /> 调用。宿主关闭 DDOL 随所在场景卸载销毁时，其余已加载场景的桶中句柄
        /// 必须逐一执行移除（与 <see cref="RemoveListenerOnDestroyTrigger" /> / <see cref="RemoveListenerOnDisableTrigger" />
        /// 的终止语义对称），否则对应监听会永久残留在目标事件上。
        /// </remarks>
        void ClearState()
        {
            foreach (var collection in _sceneHandles.Values)
            {
                collection.RemoveAllListeners();
            }

            _sceneHandles.Clear();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 域加载时重置静态单例引用，兼容关闭 Domain Reload 的 Play 模式设置
        /// </summary>
        /// <remarks>
        /// 由 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c> 自动触发，无需手动调用。
        /// 仅清静态引用即可：SubsystemRegistration 时机上，上一 Play 会话的实例已随场景销毁
        /// （Unity fake-null 使 <c>_instance != null</c> 恒不成立），运行期清理实际由
        /// <see cref="OnDestroy" /> → <see cref="ClearState" /> 承担。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
        }

        /// <summary>
        /// 添加监听句柄，使其在当前活动场景卸载时自动移除
        /// </summary>
        /// <param name="handle">要注册的自动移除监听句柄，封装了目标监听与移除委托</param>
        /// <remarks>
        /// 以调用时的 <c>SceneManager.GetActiveScene()</c> 作为分桶依据。
        /// additive 多场景流程中活动场景不一定是监听者实际所在场景，此时请改用
        /// <see cref="AddRemoveListenerHandle(Scene, AutoRemoveListenerHandle)" /> 显式指定归属场景。
        /// </remarks>
        public void AddRemoveListenerHandle(AutoRemoveListenerHandle handle)
        {
            AddRemoveListenerHandle(SceneManager.GetActiveScene(), handle);
        }

        /// <summary>
        /// 添加监听句柄，使其在指定场景卸载时自动移除
        /// </summary>
        /// <param name="scene">监听归属的场景，按其 <see cref="Scene.handle" /> 分桶</param>
        /// <param name="handle">要注册的自动移除监听句柄，封装了目标监听与移除委托</param>
        /// <remarks>
        /// 以 <paramref name="scene" /> 的 <see cref="Scene.handle" /> 作为分桶键，将句柄归入该场景的集合。
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
