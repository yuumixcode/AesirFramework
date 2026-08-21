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
    /// 是否加入 DontDestroyOnLoad 场景由序列化字段 <see cref="dontDestroyOnLoad" /> 统一控制，
    /// 场景预放置与运行时创建两种来源共用同一份决策：
    /// <list type="bullet">
    ///     <item>
    ///     <b>默认（勾选）</b>：实例在 <c>Awake</c> 时加入 DontDestroyOnLoad 场景，跨场景持久存在。
    ///     </item>
    ///     <item>
    ///     <b>取消勾选</b>：实例保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载下的
    ///     生命周期管理。Inspector 会显示警告信息框，运行时亦输出提醒日志。
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    [DefaultExecutionOrder(-999)]
    public class AesirArchitecture : AesirMonoBehaviour
    {
        static AesirArchitecture _instance;

        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。
        /// </summary>
        /// <remarks>
        /// 默认 true（跨场景持久）。设为 false 时实例保留在所在场景、随场景卸载销毁，
        /// 必须自行处理多场景叠加（Additive）加载下的生命周期管理。
        /// 运行时自动创建的实例恒以默认值 true 创建（AddComponent 同步触发 Awake，无法在创建后修改）。
        /// <para>
        /// Inspector 呈现（字段说明 InfoBox 与关闭警告 InfoBox）由
        /// <c>AesirArchitectureAttributeProcessor</c> 动态注入，运行时代码不持有任何 Inspector 样式特性。
        /// </para>
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        /// <summary>
        /// DDOL 开关字段名，提供给 Odin Inspector 使用。
        /// </summary>
        public static string DontDestroyOnLoadFieldName => nameof(dontDestroyOnLoad);

        /// <summary>
        /// 获取全局唯一的架构管理器实例
        /// </summary>
        /// <remarks>
        /// 优先在已加载场景中查找预放置的实例；未找到时运行时创建，
        /// 新实例依据 <see cref="dontDestroyOnLoad" /> 默认值（true）在 Awake 中自动加入 DontDestroyOnLoad 场景。
        /// </remarks>
        public static AesirArchitecture Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = FindAnyObjectByType<AesirArchitecture>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 运行时创建；AddComponent 同步触发 Awake，
                // 由 dontDestroyOnLoad 默认值（true）决定自动加入 DDOL 场景
                var go = new GameObject("[Aesir Architecture]");
                _instance = go.AddComponent<AesirArchitecture>();
                return _instance;
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

        #region 生命周期

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
                AesirArchitectureDebug.LogWarning("dontDestroyOnLoad 已关闭：实例保留在所在场景、随场景卸载销毁，" +
                                                  "必须自行处理多场景叠加（Additive）加载下的生命周期");
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
        /// 重置静态字段。关闭 Domain Reload 时由 Unity 在子系统注册阶段自动调用，无需手动调用。
        /// </summary>
        /// <remarks>
        /// 非泛型类按框架铁律在类内声明 <c>[RuntimeInitializeOnLoadMethod]</c> 自重置，
        /// 而非经 <see cref="ResetStaticsAssistant" />（该助手仅服务泛型类——泛型类中的 RIOLM 会被 Unity 静默跳过）。
        /// <para>
        /// 此前本类依赖 Unity fake-null 机制隐式救援（退出 Play 时对象销毁，<c>_instance != null</c> 自然变 false），
        /// 属"碰巧正确"而非"按原则正确"——补显式重置使静态重置铁律在框架内无一处例外。
        /// </para>
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
        }

        #endregion
    }
}
