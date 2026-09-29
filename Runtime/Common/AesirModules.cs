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
    ///     <item><b>默认（勾选）</b>：实例在 <c>Awake</c> 时加入 DontDestroyOnLoad 场景，跨场景持久存在——仅根物体生效。</item>
    ///     <item>
    ///     <b>取消勾选</b>：实例保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载下的
    ///     生命周期管理。Inspector 会显示警告信息框提示。
    ///     </item>
    /// </list>
    /// </remarks>
    [DefaultExecutionOrder(-999)]
    [DisallowMultipleComponent]
    public class AesirModules : AesirMonoBehaviour
    {
        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。
        /// </summary>
        /// <remarks>
        /// 默认 true（跨场景持久）。设为 false 时实例保留在所在场景、随场景卸载销毁，
        /// 必须自行处理多场景叠加（Additive）加载下的生命周期管理；
        /// 运行时自动创建的实例恒以默认值 true 创建（AddComponent 同步触发 Awake，无法在创建后修改）。
        /// <para>
        /// 仅在本物体为根物体时生效：以子物体形式存在时 Unity 会拒绝加入 DDOL 并输出告警，
        /// 宿主随场景卸载将连带销毁其下挂载的全部运行期模块。
        /// </para>
        /// <para>
        /// Inspector 呈现（字段说明 InfoBox 与关闭警告 InfoBox）由
        /// <c>AesirModulesAttributeProcessor</c> 动态注入，运行时代码不持有任何 Inspector 样式特性。
        /// </para>
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        internal const string DontDestroyOnLoadFieldName = nameof(dontDestroyOnLoad);
        static AesirModules _instance;

        /// <summary>
        /// 兼容 Enter Play Mode（跳过域重载）的静态状态重置。
        /// </summary>
        /// <remarks>
        /// 显式清空而非依赖 <c>Instance</c> getter 的隐式 fake-null 重置（已被包内判定为废弃机制）。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
        }

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
                // 含未激活对象：未激活的预放置实例不被 Awake 赋值，Exclude 会让它被判为不存在而重复创建（Inspector 配置随之失效）
                _instance = FindAnyObjectByType<AesirModules>(FindObjectsInactive.Include);
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

            // 非根物体（作为子物体被创建）时加入 DDOL 会被 Unity 拒绝并告警，
            // 仅根物体生效——此时宿主随场景卸载，其下挂载的全部运行期模块一并销毁
            if (dontDestroyOnLoad && transform.root == transform)
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
        /// <remarks>
        /// 已存在同名子物体但缺少目标组件时就地补齐组件，而不是另建一个同名子物体——
        /// <see cref="Transform.Find" /> 始终命中<b>最先</b>的同名子物体，若空壳不被认领，
        /// 每次调用都会再堆一个，使"获取或创建"失去幂等性（与 <c>UIRoot.EnsurePresetLayers</c>
        /// 对"同名但缺 Canvas 的子物体"就地补齐的口径一致）。
        /// </remarks>
        public static T GetOrAddChild<T>() where T : MonoBehaviour
        {
            var childName = typeof(T).Name;
            var child = Instance.transform.Find(childName);
            if (child != null)
            {
                var existing = child.GetComponent<T>();
                return existing != null ? existing : child.gameObject.AddComponent<T>();
            }

            var childGo = new GameObject(childName);
            childGo.transform.SetParent(Instance.transform, false);
            return childGo.AddComponent<T>();
        }
    }
}
