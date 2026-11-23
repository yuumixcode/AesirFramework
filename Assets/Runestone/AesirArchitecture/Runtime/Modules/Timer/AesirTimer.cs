using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 计时器门面：以静态工厂方法创建各类计时器，并把调度引擎的测试 / 调试入口委托给
    /// <see cref="AbstractTimer" />。具体形态由 <see cref="AbstractTimer" /> 的子类实现，本类只负责"选哪一种"。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>为什么保留本类</b>：0.33.0 起 <c>AesirTimer.Countdown(...)</c> / <c>Every(...)</c> / <c>NextFrame(...)</c>
    ///     已是公开入口，调用方常以 <c>AesirTimer</c> 声明字段。把它收敛为抽象门面（工厂 + 委托）后，
    ///     所有既有声明与调用零改动，同时把"一个巨类塞满所有形态"拆成
    ///     <see cref="AbstractTimer" /> + 六个职责单一的具体类（OCP / SRP）。
    ///     </para>
    ///     <para>
    ///     创建即开始；返回的实例持有引用可查询进度 / 取消，也可丢弃引用（到期自动出队，不泄漏）。
    ///     各形态的完整语义见对应具体类；时间源、暂停、重开等共有行为见 <see cref="AbstractTimer" />。
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // 一次性延时
    /// AesirTimer.Countdown(3f, () => model.Refresh());
    /// 
    /// // 技能冷却 + UI 进度
    /// AesirTimer _cooldown = AesirTimer.Countdown(5f);
    /// float progress = _cooldown.Progress;
    /// 
    /// // 每 0.5 秒结算一次伤害，共 6 次
    /// AesirTimer.Every(0.5f, () => model.TickDamage(), repeatCount: 6);
    /// 
    /// // 10 秒内每 1 秒触发一次
    /// AesirTimer.Interval(10f, 1f, () => model.Spawn());
    /// 
    /// // 每秒 5 次
    /// AesirTimer.Frequency(5, () => model.Sample());
    /// 
    /// // 测量耗时
    /// var stopwatch = AesirTimer.Stopwatch();
    /// float elapsed = stopwatch.Elapsed;
    /// </code>
    /// </example>
    public abstract class AesirTimer : AbstractTimer
    {
        /// <summary>供具体子类构造器转发共有状态。</summary>
        protected AesirTimer(float duration, int totalTicks, Action onFinished, bool unscaled) : base(
            duration, totalTicks, onFinished, unscaled) { }

        #region 工厂方法

        /// <summary>创建一个倒计时并在同一次调用内开始计时。</summary>
        /// <param name="seconds">倒计时时长（秒）。零或负值在下一次结算到期。</param>
        /// <param name="onFinished">到期回调，可为 null。</param>
        /// <param name="unscaled">true 时不受 <c>Time.timeScale</c> 影响。</param>
        public static CountdownTimer
            Countdown(float seconds, Action onFinished = null, bool unscaled = false) =>
            new CountdownTimer(seconds, onFinished, unscaled);

        /// <summary>创建一个按固定间隔重复的计时器并在同一次调用内开始计时。</summary>
        /// <param name="interval">两次回调之间的间隔（秒）。</param>
        /// <param name="onTick">每次到期执行的回调。</param>
        /// <param name="repeatCount">最多执行次数，省略或负数表示无限，0 表示一次也不执行。</param>
        /// <param name="unscaled">true 时不受 <c>Time.timeScale</c> 影响。</param>
        public static RepeatingTimer Every(float interval,
            Action onTick,
            int repeatCount = -1,
            bool unscaled = false) =>
            new RepeatingTimer(interval, onTick, repeatCount, unscaled);

        /// <summary>创建一个"下一帧执行一次"的计时器。</summary>
        /// <param name="onFinished">下一帧结算时执行的回调。</param>
        public static NextFrameTimer NextFrame(Action onFinished) => new NextFrameTimer(onFinished);

        /// <summary>创建一个从零开始正计时的秒表；不会自动结束，需持有引用读 <see cref="StopwatchTimer.Elapsed" />。</summary>
        /// <param name="unscaled">true 时不受 <c>Time.timeScale</c> 影响。</param>
        public static StopwatchTimer Stopwatch(bool unscaled = false) => new StopwatchTimer(unscaled);

        /// <summary>创建一个区间计时器：总时长内每隔固定间隔触发一次。</summary>
        /// <param name="totalSeconds">总时长（秒），走完即结束。</param>
        /// <param name="intervalSeconds">触发间隔（秒），必须大于 0。</param>
        /// <param name="onInterval">每次间隔到点时执行的回调。</param>
        /// <param name="unscaled">true 时不受 <c>Time.timeScale</c> 影响。</param>
        public static IntervalTimer Interval(float totalSeconds,
            float intervalSeconds,
            Action onInterval,
            bool unscaled = false) =>
            new IntervalTimer(totalSeconds, intervalSeconds, onInterval, unscaled);

        /// <summary>创建一个频率计时器：每秒触发固定次数。</summary>
        /// <param name="ticksPerSecond">每秒触发次数，必须大于 0。</param>
        /// <param name="onTick">每次触发执行的回调。</param>
        /// <param name="unscaled">true 时不受 <c>Time.timeScale</c> 影响。</param>
        public static FrequencyTimer Frequency(int ticksPerSecond, Action onTick, bool unscaled = false) =>
            new FrequencyTimer(ticksPerSecond, onTick, unscaled);

        #endregion

        #region 调度引擎委托（测试与调试窗口入口）

        /// <summary>清空注册表并复位注入标志（域加载与测试用）。</summary>
        internal static void Reset() => AbstractTimer.Reset();

        /// <summary>确定性推进一次结算（测试用）。</summary>
        internal static void TickOnce(float deltaTime, float unscaledDeltaTime) =>
            AbstractTimer.TickOnce(deltaTime, unscaledDeltaTime);

        /// <summary>当前仍在注册表中的实例数量，含已暂停。</summary>
        internal static int RunningCount => AbstractTimer.RunningCount;

        /// <summary>把注册表中的实例复制到缓冲区（调试窗口拉取快照用）。</summary>
        internal static void CopyRunning(List<AbstractTimer> buffer) => AbstractTimer.CopyRunning(buffer);

        /// <summary>累计创建数。</summary>
        internal static int CreatedCount => AbstractTimer.CreatedCount;

        /// <summary>累计正常到期数。</summary>
        internal static int FinishedCount => AbstractTimer.FinishedCount;

        /// <summary>累计取消数。</summary>
        internal static int CancelledCount => AbstractTimer.CancelledCount;

        /// <summary>已执行的结算周期数。</summary>
        internal static int PassCount => AbstractTimer.PassCount;

        #endregion
    }
}
