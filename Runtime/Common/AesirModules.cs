using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Aesir Modules 接入 MonoBehaviour 生命周期的持久化物体对象。
    /// </summary>
    /// <remarks>
    /// 是否加入 DontDestroyOnLoad 场景由序列化字段 <see cref="dontDestroyOnLoad" /> 统一控制，
    /// 场景预放置与运行时创建两种来源共用同一份决策：
    /// <list type="bullet">
    ///     <item><b>默认（勾选）</b>：实例在 <c>Awake</c> 时加入 DontDestroyOnLoad 场景，跨场景持久存在。</item>
    ///     <item>
    ///     <b>取消勾选</b>：实例保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载下的
    ///     生命周期管理。Inspector 会显示警告信息框，运行时亦输出提醒日志。
    ///     </item>
    /// </list>
    /// </remarks>
    [DefaultExecutionOrder(-999)]
    [DisallowMultipleComponent]
    public class AesirModules : AesirMonoBehaviour
    {
        internal const string DontDestroyOnLoadFieldName = nameof(dontDestroyOnLoad);
        static AesirModules _instance;

        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。
        /// </summary>
        /// <remarks>
        /// 默认 true（跨场景持久）。设为 false 时实例保留在所在场景、随场景卸载销毁，
        /// 必须自行处理多场景叠加（Additive）加载下的生命周期管理；
        /// 运行时自动创建的实例恒以默认值 true 创建（AddComponent 同步触发 Awake，无法在创建后修改）。
        /// <para>
        /// Inspector 呈现（字段说明 InfoBox 与关闭警告 InfoBox）由
        /// <c>AesirModulesAttributeProcessor</c> 动态注入，运行时代码不持有任何 Inspector 样式特性。
        /// </para>
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        /// <summary>
        /// 获取全局唯一的架构管理器实例
        /// </summary>
        /// <remarks>
        /// 优先在已加载场景中查找预放置的实例；未找到时运行时创建，
        /// 新实例依据 <see cref="dontDestroyOnLoad" /> 默认值（true）在 Awake 中自动加入 DontDestroyOnLoad 场景。
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
                // 使用 FindAnyObjectByType 而非 FindFirstObjectByType，后者因依赖 InstanceID 排序在 Unity 6 中已废弃
                _instance = FindAnyObjectByType<AesirModules>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 运行时创建；AddComponent 同步触发 Awake，
                // 由 dontDestroyOnLoad 默认值（true）决定自动加入 DDOL 场景
                var go = new GameObject("[Aesir Modules]");
                _instance = go.AddComponent<AesirModules>();
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

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.AesirModulesTag,
                    "dontDestroyOnLoad 已关闭：实例保留在所在场景、随场景卸载销毁，" + "必须自行处理多场景叠加（Additive）加载下的生命周期");
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
