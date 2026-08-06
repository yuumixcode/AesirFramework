namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Mono 生命周期事件类型，涵盖 Unity 原生生命周期回调和自定义 PlayerLoop 阶段。
    /// </summary>
    /// <remarks>
    /// 枚举值按 Unity 执行顺序排列，订阅者可监听任意阶段的事件。
    /// <para>
    /// 不包含 Awake / OnEnable / OnDisable / OnDestroy / Start 事件——因为
    /// <see cref="MonoLifecycleProxy"/> 是挂载在 DontDestroyOnLoad GameObject 上的懒创建单例，
    /// 这些回调仅在代理自身创建或应用退出时触发，外部无法有效订阅。
    /// </para>
    /// <para>
    /// <see cref="BeforeUpdate"/> 和 <see cref="AfterUpdate"/> 由
    /// <see cref="AesirArchitecturePlayerLoop"/> 驱动，分别对应每帧 Update 之前和 PostLateUpdate 之后。
    /// 其余事件由 <see cref="MonoLifecycleProxy"/> 在对应 Unity 回调中直接触发。
    /// </para>
    /// </remarks>
    /// <seealso cref="MonoLifecycleProxy"/>
    public enum MonoLifecycleEvent
    {
        /// <summary>
        /// MonoBehaviour.FixedUpdate — 物理帧
        /// </summary>
        /// <remarks>
        /// 常见场景：Rigidbody 位移、物理射线检测累积、固定时间步长的力学计算。
        /// </remarks>
        FixedUpdate = 0,

        /// <summary>
        /// 自定义 PlayerLoop 阶段：在 Update 之前执行
        /// </summary>
        /// <remarks>
        /// 由 <see cref="AesirArchitecturePlayerLoop"/> 的
        /// <see cref="AesirArchitectureLifecyclePhase.BeforeUpdate"/> 阶段驱动。
        /// <para>常见场景：输入采样、帧前状态快照、在所有 Update 逻辑之前执行的高优先级预处理。</para>
        /// </remarks>
        BeforeUpdate = 1,

        /// <summary>
        /// MonoBehaviour.Update — 每帧逻辑更新
        /// </summary>
        /// <remarks>
        /// 常见场景：游戏逻辑更新、输入处理、状态机推进、计时器递减。
        /// </remarks>
        Update = 2,

        /// <summary>
        /// MonoBehaviour.LateUpdate — 每帧后处理
        /// </summary>
        /// <remarks>
        /// 常见场景：相机跟随目标、动画后处理、在所有 Update 完成后读取最终位置。
        /// </remarks>
        LateUpdate = 3,

        /// <summary>
        /// 自定义 PlayerLoop 阶段：在 PostLateUpdate 之后执行
        /// </summary>
        /// <remarks>
        /// 由 <see cref="AesirArchitecturePlayerLoop"/> 的
        /// <see cref="AesirArchitectureLifecyclePhase.AfterUpdate"/> 阶段驱动。
        /// <para>常见场景：帧结束状态快照、性能采样、延迟队列执行、读取当前帧所有模块的最终状态。</para>
        /// </remarks>
        AfterUpdate = 4,

        /// <summary>
        /// MonoBehaviour.OnApplicationFocus — 应用获得或失去焦点
        /// </summary>
        /// <remarks>
        /// 常见场景：失去焦点时暂停音频和动画、获得焦点时恢复；桌面端窗口最小化时降低渲染频率。
        /// <para>回调中可通过 <c>Application.isFocused</c> 判断当前焦点状态。</para>
        /// </remarks>
        OnApplicationFocus = 5,

        /// <summary>
        /// MonoBehaviour.OnApplicationPause — 应用被系统暂停或恢复
        /// </summary>
        /// <remarks>
        /// 常见场景：移动端切后台时保存游戏进度、暂停网络请求并断开服务器连接、恢复时重新登录。
        /// <para>回调中可通过 <c>Application.isPaused</c> 判断当前暂停状态。</para>
        /// </remarks>
        OnApplicationPause = 6,

        /// <summary>
        /// MonoBehaviour.OnApplicationQuit — 应用退出
        /// </summary>
        /// <remarks>
        /// 常见场景：保存游戏进度到本地、断开服务器连接、释放非托管资源、写入日志。
        /// <para>仅在编辑器中退出 Play Mode 或独立构建应用退出时触发一次。</para>
        /// </remarks>
        OnApplicationQuit = 7
    }
}
