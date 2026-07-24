using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 任意场景卸载时自动移除该场景注册的监听。按场景名分桶，场景 A 卸载不会误杀场景 B 的监听。
    /// <para>挂载在 [Aesir Architecture] GameObject 上，通过 <see cref="Instance" /> 访问。</para>
    /// </summary>
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
