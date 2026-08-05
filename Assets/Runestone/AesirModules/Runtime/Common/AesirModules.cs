using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Aesir Modules 接入 MonoBehaviour 生命周期的持久化物体对象。
    /// </summary>
    /// <remarks>
    /// 支持两种使用方式：
    /// <list type="bullet">
    /// <item><b>场景预放置</b>：将本组件放在场景中的 GameObject 上，<see cref="Instance"/> 会自动发现，
    /// 不调用 <c>DontDestroyOnLoad</c>，实例随场景生命周期销毁。适合多场景叠加加载的开发模式。</item>
    /// <item><b>运行时创建</b>：未在场景中预放置时，首次访问 <see cref="Instance"/> 会自动创建
    /// <c>[Aesir Modules]</c> GameObject 并调用 <c>DontDestroyOnLoad</c>。</item>
    /// </list>
    /// </remarks>
    [DefaultExecutionOrder(-999)]
    [DisallowMultipleComponent]
    public class AesirModules : AesirMonoBehaviour
    {
        static AesirModules _instance;

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
        public static AesirModules Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                _instance = FindFirstObjectByType<AesirModules>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 运行时创建，标记后由 Awake 决定是否 DDOL
                _createdByRuntime = true;
                var go = new GameObject("[Aesir Modules]");
                // AddComponent 在主线程同步执行，Awake 会在 AddComponent 返回前完成，
                // 此时 _createdByRuntime 已被 Awake 消费完毕，可以安全重置。
                // 重置后标志不会残留，避免影响后续 Awake（如 Enter Play Mode 触发的 Domain Reload）。
                _instance = go.AddComponent<AesirModules>();
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
            if (_instance != null && _instance == this)
            {
                _instance = null;
            }
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
