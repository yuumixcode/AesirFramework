using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Aesir Architecture 接入 MonoBehaviour 生命周期的持久化物体对象。
    /// </summary>
    [DefaultExecutionOrder(-999)]
    public class AesirArchitecture : AesirMonoBehaviour
    {
        static AesirArchitecture _instance;

        /// <summary>
        /// 获取全局唯一的架构管理器实例
        /// </summary>
        public static AesirArchitecture Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("[Aesir Architecture]").AddComponent<AesirArchitecture>();
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
            if (_instance == this)
            {
                _instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            // 强制启动 Aesir Architecture，在场景的 Awake 之前执行
            _ = Instance;
        }

        /// <summary>
        /// 获取或为架构物体添加指定的组件类型
        /// </summary>
        public static T GetOrAddComponent<T>() where T : MonoBehaviour
        {
            var component = Instance.GetComponent<T>();
            if (component == null)
            {
                component = Instance.gameObject.AddComponent<T>();
            }

            return component;
        }
    }
}
