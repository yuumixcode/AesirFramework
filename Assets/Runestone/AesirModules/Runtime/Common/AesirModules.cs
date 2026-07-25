using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Aesir Modules 接入 MonoBehaviour 生命周期的持久化物体对象。
    /// </summary>
    [DefaultExecutionOrder(-999)]
    [DisallowMultipleComponent]
    public class AesirModules : AesirMonoBehaviour
    {
        static AesirModules _instance;

        /// <summary>
        /// 获取全局唯一的架构管理器实例
        /// </summary>
        public static AesirModules Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("[Aesir Modules]").AddComponent<AesirModules>();
                }

                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (_instance != null && _instance == this)
            {
                _instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            // 强制启动 Aesir Modules，在场景的 Awake 之前执行
            _ = Instance;
        }

        /// <summary>
        /// 获取或为架构物体创建子物体并添加指定组件
        /// </summary>
        public static T GetOrAddChild<T>() where T : MonoBehaviour
        {
            var childName = typeof(T).Name;
            var child = Instance.transform.Find(childName);
            if (child != null)
            {
                var existing = child.GetComponent<T>();
                if (existing != null)
                {
                    return existing;
                }
            }

            var childGo = new GameObject(childName);
            childGo.transform.SetParent(Instance.transform, false);
            return childGo.AddComponent<T>();
        }
    }
}
