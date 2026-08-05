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
    /// 支持两种使用方式：
    /// <list type="bullet">
    /// <item><b>场景预放置</b>：将本组件放在场景中的 GameObject 上，<see cref="Instance"/> 会自动发现，
    /// 不调用 <c>DontDestroyOnLoad</c>，实例随场景生命周期销毁。适合多场景叠加加载的开发模式。</item>
    /// <item><b>运行时创建</b>：未在场景中预放置时，首次访问 <see cref="Instance"/> 会自动创建
    /// <c>[Aesir Architecture]</c> GameObject 并调用 <c>DontDestroyOnLoad</c>。</item>
    /// </list>
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
        /// 标记当前实例是否由 <see cref="Instance" /> getter 在运行时创建。
        /// 预放置在场景中的实例此标记为 false，不调用 <see cref="UnityEngine.Object.DontDestroyOnLoad" />。
        /// </summary>
        static bool _createdByRuntime;

        /// <summary>
        /// 获取全局唯一的架构管理器实例
        /// </summary>
        /// <remarks>
        /// 优先在已加载场景中查找预放置的实例；未找到时运行时创建并调用 <c>DontDestroyOnLoad</c>。
        /// </remarks>
        public static AesirArchitecture Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                _instance = FindFirstObjectByType<AesirArchitecture>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 运行时创建，标记后由 Awake 决定是否 DDOL
                _createdByRuntime = true;
                var go = new GameObject("[Aesir Architecture]");
                // AddComponent 在主线程同步执行，Awake 会在 AddComponent 返回前完成，
                // 此时 _createdByRuntime 已被 Awake 消费完毕，可以安全重置。
                // 重置后标志不会残留，避免影响后续 Awake（如 Enter Play Mode 触发的 Domain Reload）。
                _instance = go.AddComponent<AesirArchitecture>();
                _createdByRuntime = false;

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

            // 仅运行时创建的实例使用 DontDestroyOnLoad；场景中预放置的实例保留在场景中
            if (_createdByRuntime)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
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
