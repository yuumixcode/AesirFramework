using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Aesir Architecture 接入 MonoBehaviour 生命周期的持久化物体对象。
    /// </summary>
    /// <remarks>
    /// 通过 <c>[DefaultExecutionOrder(-999)]</c> 确保在场景中其他 MonoBehaviour 的 <c>Awake</c> 之前执行，
    /// 使架构基础设施先于业务逻辑完成初始化。
    /// <para>
    /// 通过 <c>[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]</c> 标记的 <see cref="Bootstrap"/> 方法
    /// 在场景加载前自动创建实例，无需在场景中手动放置。
    /// </para>
    /// <para>
    /// 构建时调用 <c>DontDestroyOnLoad</c> 使架构管理器在场景切换时持久存在，
    /// 避免每次进入新场景时重复创建。
    /// </para>
    /// <para>
    /// <see cref="GetOrAddComponent{T}"/> 提供在架构 GameObject 上挂载全局单例组件的便捷方法，
    /// 供框架内各子系统（如事件监听移除器）复用同一宿主对象。
    /// </para>
    /// </remarks>
    [DefaultExecutionOrder(-999)]
    public class AesirArchitecture : AesirMonoBehaviour
    {
        static AesirArchitecture _instance;

        /// <summary>
        /// 获取全局唯一的架构管理器实例
        /// </summary>
        /// <remarks>
        /// 采用懒创建模式：首次访问时自动创建 <c>[Aesir Architecture]</c> GameObject 并挂载本组件。
        /// 在 <c>Awake</c> 中进行单例保护，若已存在实例则销毁重复的 GameObject，
        /// 确保全局仅保留一个实例。
        /// </remarks>
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
        /// <typeparam name="T">要获取或添加的组件类型，必须继承自 <c>MonoBehaviour</c></typeparam>
        /// <returns>架构 GameObject 上已存在或新添加的组件实例</returns>
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
