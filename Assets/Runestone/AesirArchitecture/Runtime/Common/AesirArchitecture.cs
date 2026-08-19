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
    ///     <item>
    ///     <b>场景预放置</b>：将本组件放在场景中的 GameObject 上，<see cref="Instance" /> 会自动发现，
    ///     不调用 <c>DontDestroyOnLoad</c>，实例随场景生命周期销毁。适合多场景叠加加载的开发模式。
    ///     </item>
    ///     <item>
    ///     <b>运行时创建</b>：未在场景中预放置时，首次访问 <see cref="Instance" /> 会自动创建
    ///     <c>[Aesir Architecture]</c> GameObject 并调用 <c>DontDestroyOnLoad</c>。
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    [DefaultExecutionOrder(-999)]
    public class AesirArchitecture : AesirMonoBehaviour
    {
        static AesirArchitecture _instance;

        /// <summary>
        /// 一次性临时标记：通知下一次 <see cref="Awake" /> 调用需要执行 <see cref="UnityEngine.Object.DontDestroyOnLoad" />。
        /// 由 <see cref="Instance" /> getter 在创建实例前置为 true，Awake 消费后立即重置为 false。
        /// </summary>
        static bool _pendingDontDestroyOnLoad;

        /// <summary>
        /// 当前实例是否为预放置在场景中的物体
        /// </summary>
        bool _isPrePlaced;

        /// <summary>
        /// 预放置标记字段名，提供给 Odin Inspector 使用。
        /// </summary>
        public static string IsPrePlacedFieldName => nameof(_isPrePlaced);

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

                _instance = FindAnyObjectByType<AesirArchitecture>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例，则表示需要动态生成，动态生成则默认标记为需要 DontDestroyOnLoad
                _pendingDontDestroyOnLoad = true;
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
            if (_pendingDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
                // Awake 消费标记，立刻重置这个静态变量。
                _pendingDontDestroyOnLoad = false;
            }
            else
            {
                _isPrePlaced = true;
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
        /// <para>此前本类依赖 Unity fake-null 机制隐式救援（退出 Play 时对象销毁，<c>_instance != null</c> 自然变 false），
        /// 属"碰巧正确"而非"按原则正确"——补显式重置使静态重置铁律在框架内无一处例外。</para>
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
            _pendingDontDestroyOnLoad = false;
        }

        #endregion
    }
}
