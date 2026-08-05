using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 任意场景卸载时自动移除该场景注册的监听。按场景名分桶，场景 A 卸载不会误杀场景 B 的监听。
    /// <para>挂载在 [Aesir Architecture] GameObject 上，通过 <see cref="Instance" /> 访问。</para>
    /// </summary>
    /// <remarks>
    /// 作为全局单例挂载在 <c>[Aesir Architecture]</c> GameObject 上，通过 <see cref="Instance"/> 访问。
    /// <para>
    /// 按场景名分桶管理监听句柄，场景 A 卸载时仅移除场景 A 注册的监听，不会误杀场景 B 的监听。
    /// </para>
    /// <para>
    /// 静态构造函数通过 <c>ResetStaticsAssistant.Register</c> 注册重置回调，
    /// 确保在编辑器关闭 Domain Reload 时静态字段被正确清空。
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

        readonly Dictionary<string, RemoveListenerHandleCollection> _sceneHandles =
            new Dictionary<string, RemoveListenerHandleCollection>();

        static RemoveListenerOnSceneUnloadedTrigger()
        {
            ResetStaticsAssistant.Register(() => _instance = null);
        }

        /// <summary>
        /// 获取全局唯一的场景卸载监听移除器实例
        /// </summary>
        /// <remarks>
        /// 采用懒创建模式：首次访问时通过 <see cref="AesirArchitecture.GetOrAddComponent{T}"/>
        /// 将本组件挂载到 <c>[Aesir Architecture]</c> GameObject 上，复用架构宿主对象。
        /// </remarks>
        public static RemoveListenerOnSceneUnloadedTrigger Instance
        {
            get
            {
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
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 添加监听句柄，使其在注册时所属场景卸载时自动移除
        /// </summary>
        /// <param name="handle">要注册的自动移除监听句柄，封装了目标监听与移除委托</param>
        /// <remarks>
        /// 以调用时 <c>SceneManager.GetActiveScene().name</c> 返回的当前活动场景名作为分桶键，
        /// 将句柄归入该场景的集合。当对应场景卸载时，仅移除该桶中的监听。
        /// </remarks>
        public void AddRemoveListenerHandle(AutoRemoveListenerHandle handle)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (!_sceneHandles.TryGetValue(sceneName, out var collection))
            {
                collection = new RemoveListenerHandleCollection();
                _sceneHandles[sceneName] = collection;
            }

            collection.Add(handle);
        }

        /// <summary>
        /// 当场景卸载时移除该场景下所有已注册的监听。
        /// </summary>
        void OnSceneUnloaded(Scene scene)
        {
            if (_sceneHandles.TryGetValue(scene.name, out var collection))
            {
                collection.RemoveAllListeners();
                _sceneHandles.Remove(scene.name);
            }
        }
    }
}
