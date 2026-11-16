using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 框架驱动的计时器：创建即开始，到期自动出队或按间隔重复，可随时查询剩余时间、进度与剩余次数。
    /// <para>
    /// 定位是"拿不到每帧回调的地方"的计时手段——纯 C# 的 Model / Service / Command 不需要
    /// MonoBehaviour、也不需要协程即可延时、冷却、按间隔重复。
    /// 能拿到每帧回调的地方（<c>MonoBehaviour.Update</c> 或 <see cref="MonoLifecycleProxy" />）
    /// 应直接用一个 float 字段累减，不必创建计时器。
    /// </para>
    /// <para>
    /// <b>每个计时器是一个独立实例</b>：时长、剩余时间、时间源、已执行次数、到期回调都挂在实例字段上，
    /// 注册表只保存"哪些实例还活着"用于每帧推进。因此调用方可以持有引用来查询
    /// <see cref="Progress" />（供 UI 进度条使用），也可以在创建后直接丢弃引用（框架代持至到期）。
    /// </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>为什么是引用类型而不是值类型</b>：框架需要"替你推进"并且"你随时能查"。
    ///     可变结构体在这里会失效——注册表持有的是副本，推进的是副本，调用方读到的仍是旧值；
    ///     引用类型让注册表与调用方操作同一个实例。
    ///     </para>
    ///     <para>
    ///     <b>为什么没有 <c>Start</c> / <c>Tick</c></b>：工厂方法即开始，不存在"创建了但没跑"的中间态；
    ///     不自带 <c>Tick</c> 是为了不与已有的 Update 路径重复。
    ///     </para>
    ///     <para>
    ///     <b>暂停的两层语义</b>：全局暂停由 <c>Time.timeScale = 0</c> 表达（默认时间源跟随缩放，
    ///     <c>unscaled</c> 实例不受影响）；<see cref="Pause" /> / <see cref="Resume" /> 是实例级暂停，
    ///     用于"只有这一个计时器该停"的场景（过场动画期间冻结某个冷却、或 <c>unscaled</c> 实例的单独挂起）。
    ///     两者可叠加：暂停期间 <see cref="Remaining" /> 与 <see cref="Progress" /> 一并冻结。
    ///     </para>
    ///     <para>
    ///     <b>注入方式</b>：经 <see cref="AesirPlayerLoop" /> 的 <see cref="AesirLifecyclePhase.BeforeUpdate" />
    ///     钩子推进，<c>order</c> 取 <see cref="int.MinValue" />——计时结算先于任何用户钩子，
    ///     语义是本帧先结算计时再跑业务逻辑；该 <c>order</c> 也是用户无法抢占的稳定锚点。
    ///     复用 <see cref="AesirPlayerLoop" /> 而非自行调用 <c>PlayerLoop.SetPlayerLoop</c>，
    ///     可直接获得 <see cref="AesirPlayerLoop.EnsureInjected" /> 的自愈能力（第三方 SDK 覆盖
    ///     PlayerLoop 后不会让计时器静默失效）。
    ///     </para>
    ///     <para>
    ///     <b>症状与代价</b>：帧粒度结算，误差在 ±1 帧内，不做亚帧精度保证，也不做绝对时间校准。
    ///     每个计时器一次堆分配（生命周期短，落在 Gen0）；框架不做对象池，需要复用同一冷却位时用
    ///     <see cref="Restart" />。重复形态不补发——一帧卡顿跨越多个间隔时只回调一次（避免回调雪崩），
    ///     之后的节奏从该次结算重新起算。
    ///     </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 一次性延时
    /// AesirTimer.Countdown(3f, () => model.Refresh());
    ///
    /// // 技能冷却 + UI 进度（View 每帧读 Progress，无需驱动）
    /// _cooldown = AesirTimer.Countdown(5f);
    /// float progress = _cooldown.Progress;
    ///
    /// // 每 0.5 秒结算一次伤害，共 6 次（repeatCount 省略即无限）
    /// AesirTimer.Every(0.5f, () => model.TickDamage(), repeatCount: 6);
    ///
    /// // 下一帧执行一次
    /// AesirTimer.NextFrame(() => Rebuild());
    /// </code>
    /// </example>
    public sealed class AesirTimer : IDisposable
    {
        /// <summary>
        /// 计时结算钩子的 order 值：先于任何用户 BeforeUpdate 钩子执行。
        /// </summary>
        const int SettleOrder = int.MinValue;

        /// <summary>
        /// <see cref="TotalTicks" /> 取此值表示无限次执行（<see cref="RemainingExecutions" /> 随之返回 -1）。
        /// </summary>
        const int InfiniteTicks = -1;

        #region 静态注册表

        /// <summary>
        /// 尚未到期（含已暂停）的计时器，按创建顺序排列（保证同帧多个到期按创建顺序回调）。
        /// </summary>
        static readonly List<AesirTimer> Running = new List<AesirTimer>();

        /// <summary>
        /// 本次结算中已判定到期、待执行回调的计时器，复用集合以维持稳态零分配。
        /// </summary>
        static readonly List<AesirTimer> DueBuffer = new List<AesirTimer>();

        /// <summary>
        /// 是否已把结算钩子注册到 <see cref="AesirPlayerLoop" />（幂等标志）。
        /// </summary>
        static bool _scheduled;

        /// <summary>
        /// 结算钩子的注销句柄。仅用于 <see cref="Reset" /> 时退订，避免复位标志后重复注册同一钩子
        /// （重复注册会让每帧结算执行两次，计时器以双倍速度走完）。
        /// </summary>
        static AutoRemoveListenerHandle _hook;

        /// <summary>
        /// 已开始的结算周期数。用于判定 <see cref="NextFrame" /> 的"下一帧"落在哪一次结算。
        /// </summary>
        static int _passId;

        /// <summary>累计创建数（调试窗口统计用，<see cref="Reset" /> 归零）。</summary>
        static int _createdCount;

        /// <summary>累计正常到期数（调试窗口统计用）。</summary>
        static int _finishedCount;

        /// <summary>累计取消数（调试窗口统计用）。</summary>
        static int _cancelledCount;

        #endregion

        #region 实例状态

        /// <summary>
        /// 到期回调，可为 null（仅查询进度、不做事）。
        /// </summary>
        readonly Action _onFinished;

        /// <summary>
        /// 时间源：true = <c>Time.unscaledDeltaTime</c>（不受 <c>Time.timeScale</c> 影响）。
        /// </summary>
        readonly bool _unscaled;

        /// <summary>
        /// 是否为 <see cref="NextFrame" /> 形态（跳过时间累减，按结算周期判定）。
        /// </summary>
        readonly bool _nextFrameMode;

        /// <summary>
        /// 构造时给定的时长，创建后不再变化（需要不同时长时重建实例）。
        /// 重复形态下它是"两次回调之间的间隔"。
        /// </summary>
        readonly float _duration;

        /// <summary>
        /// 允许执行的总次数，<see cref="InfiniteTicks" /> 表示无限。单次形态恒为 1。
        /// </summary>
        readonly int _totalTicks;

        /// <summary>
        /// 剩余秒数，<see cref="NextFrame" /> 形态恒为 0。重复形态下是"距下一次回调"的剩余。
        /// </summary>
        float _remaining;

        /// <summary>
        /// 创建（或 <see cref="Restart" />）时所处的结算周期号，仅 <see cref="NextFrame" /> 形态使用。
        /// </summary>
        int _createdPassId;

        /// <summary>
        /// 已执行的回调次数。
        /// </summary>
        int _executedTicks;

        /// <summary>
        /// 创建（或 <see cref="Restart" />）时的帧号，调试窗口用来展示存活时长。
        /// </summary>
        int _createdFrame;

        /// <summary>
        /// 创建（或 <see cref="Restart" />）时的 <c>Time.realtimeSinceStartup</c>，
        /// 与 <see cref="_createdFrame" /> 配对展示存活时长（不受 <c>Time.timeScale</c> 影响）。
        /// </summary>
        float _createdTime;

        /// <summary>
        /// 是否在 <see cref="Running" /> 中。注册状态的单一真源，杜绝重复注册。
        /// </summary>
        bool _registered;

        /// <summary>
        /// 是否已正常到期（有限次数已用尽）。
        /// </summary>
        bool _finished;

        /// <summary>
        /// 是否被取消且尚未到期（与 <see cref="_finished" /> 互斥）。
        /// </summary>
        bool _cancelled;

        /// <summary>
        /// 是否被 <see cref="Pause" /> 挂起。挂起期间仍在注册表中，但结算跳过推进与执行。
        /// </summary>
        bool _paused;

        /// <summary>
        /// 创建计时器并立即入队，同时确保结算钩子已注入。
        /// </summary>
        AesirTimer(float seconds, int totalTicks, Action onFinished, bool unscaled, bool nextFrameMode)
        {
            _duration = seconds > 0f ? seconds : 0f;
            _remaining = _duration;
            _totalTicks = totalTicks;
            _onFinished = onFinished;
            _unscaled = unscaled;
            _nextFrameMode = nextFrameMode;
            _createdPassId = _passId;
            _createdFrame = Time.frameCount;
            _createdTime = Time.realtimeSinceStartup;
            _createdCount++;

            // 允许执行 0 次：创建即结束，不入队，也不能被 Restart 拉起
            if (_totalTicks == 0)
            {
                _finished = true;
                return;
            }

            EnsureScheduled();
            Register();
        }

        #endregion

        #region 工厂方法

        /// <summary>
        /// 创建一个倒计时并在同一次调用内开始计时。
        /// </summary>
        /// <param name="seconds">倒计时时长（秒）。零或负值立即处于到期状态，在下一次结算中回调。</param>
        /// <param name="onFinished">到期回调，可为 null。</param>
        /// <param name="unscaled">
        /// true 时使用 <c>Time.unscaledDeltaTime</c>（<c>Time.timeScale = 0</c> 仍继续计时），
        /// 用于暂停菜单、UI 动画等不受游戏速度影响的场景。
        /// </param>
        /// <returns>计时器实例，持有引用可查询进度或在到期前取消</returns>
        /// <remarks>
        /// 调用即产生副作用：立即注册到框架结算列表并确保 PlayerLoop 注入完成。
        /// 返回的实例即使被丢弃也不会泄漏——到期后自动出队，只是失去 <see cref="Cancel" /> 的能力。
        /// </remarks>
        public static AesirTimer Countdown(float seconds, Action onFinished = null, bool unscaled = false) =>
            new AesirTimer(seconds, 1, onFinished, unscaled, false);

        /// <summary>
        /// 创建一个"每 <paramref name="interval" /> 秒执行一次"的重复计时器。
        /// </summary>
        /// <param name="interval">两次回调之间的间隔（秒）。零或负值表示每次结算都回调。</param>
        /// <param name="onTick">每次到期时执行的回调</param>
        /// <param name="repeatCount">
        /// 最多执行几次：省略或负数表示无限重复（需自行 <see cref="Cancel" />），
        /// 0 表示一次也不执行（创建即结束）。有限次数用尽后自动出队，状态转为 <see cref="IsFinished" />。
        /// </param>
        /// <param name="unscaled">同 <see cref="Countdown" />：true 时不受 <c>Time.timeScale</c> 影响。</param>
        /// <returns>计时器实例，可查询进度与 <see cref="RemainingExecutions" /></returns>
        /// <remarks>
        ///     <para>
        ///     首次回调发生在 <paramref name="interval" /> 之后（与 <see cref="Countdown" /> 一致），
        ///     不会在创建当帧立即执行。
        ///     </para>
        ///     <para>
        ///     每个结算周期最多回调一次：一帧卡顿跨越多个间隔时不补发，
        ///     <see cref="Remaining" /> 从该次结算重新起算（避免卡顿后的回调雪崩）。
        ///     </para>
        ///     <para>
        ///     无限重复是"永远不结束"的实例，持有方必须能在合适的时机 <see cref="Cancel" />，
        ///     否则等于把计时器当成每帧逻辑用——那种需求请用 <see cref="MonoLifecycleProxy" />。
        ///     </para>
        /// </remarks>
        public static AesirTimer Every(float interval, Action onTick, int repeatCount = InfiniteTicks,
            bool unscaled = false) =>
            new AesirTimer(interval, repeatCount < 0 ? InfiniteTicks : repeatCount, onTick, unscaled, false);

        /// <summary>
        /// 创建一个"下一帧执行一次"的计时器。
        /// </summary>
        /// <param name="onFinished">下一帧结算时执行的回调</param>
        /// <returns>计时器实例（<see cref="Duration" /> 恒为 0，与倒计时共用同一套查询与取消接口）</returns>
        /// <remarks>
        ///     <para>
        ///     语义是"此后第一次结算"，而不是"当前帧剩下的时间里某一次结算"：创建发生在一次结算之后
        ///     （例如 <c>MonoBehaviour.Update</c>、协程、或另一个计时器的到期回调中）时，回调落在下一帧；
        ///     创建发生在当帧结算之前（例如 <c>AfterAssembliesLoaded</c>）时，回调落在当帧的结算里。
        ///     </para>
        ///     <para>
        ///     "下一帧"是按结算周期计数的，与 <c>Time.timeScale</c> 无关，因此不接受 <c>unscaled</c> 参数。
        ///     </para>
        /// </remarks>
        public static AesirTimer NextFrame(Action onFinished) =>
            new AesirTimer(0f, 1, onFinished, false, true);

        #endregion

        #region 状态查询

        /// <summary>
        /// 构造时给定的时长（秒）。<see cref="NextFrame" /> 形态恒为 0；重复形态下是两次回调的间隔。
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// 剩余秒数（重复形态下为距下一次回调的剩余）。到期或被取消后保持当时的值（到期为 0）。
        /// <see cref="NextFrame" /> 形态恒为 0。
        /// </summary>
        public float Remaining => _remaining;

        /// <summary>
        /// 已走完的比例，0 → 1，可直接用于进度条。重复形态下是"当前间隔"的进度。
        /// </summary>
        /// <remarks>
        /// 正常到期返回 1；<see cref="Duration" /> 为 0 的实例在到期前返回 0（避免 0/0 产生 NaN）；
        /// 被暂停时冻结在暂停时刻的值；被取消后冻结在取消时刻的值。
        /// </remarks>
        public float Progress
        {
            get
            {
                if (_duration <= 0f)
                {
                    return _finished ? 1f : 0f;
                }

                return Mathf.Clamp01(1f - _remaining / _duration);
            }
        }

        /// <summary>
        /// 是否已正常到期（有限次数已用尽）。被取消的计时器恒为 false（与 <see cref="IsCancelled" /> 互斥）。
        /// </summary>
        public bool IsFinished => _finished;

        /// <summary>
        /// 是否被 <see cref="Cancel" /> 或 <see cref="Dispose" /> 取消且尚未到期。
        /// </summary>
        /// <remarks>
        /// 已到期的计时器调用 <see cref="Cancel" /> 不会改变任何状态：同一次结算中"到期"先于"取消"发生，
        /// 已判定到期的回调仍会执行。
        /// </remarks>
        public bool IsCancelled => _cancelled;

        /// <summary>
        /// 是否处于实例级暂停（<see cref="Pause" /> 之后、<see cref="Resume" /> 或 <see cref="Restart" /> 之前）。
        /// </summary>
        public bool IsPaused => _paused;

        /// <summary>
        /// 还会执行几次回调：无限重复返回 -1，已到期或已取消返回 0。
        /// </summary>
        /// <remarks>
        /// 单次形态（<see cref="Countdown" /> / <see cref="NextFrame" />）在触发前恒为 1。
        /// 该值表示"从此刻起还会回调几次"，不包含已经执行的次数。
        /// </remarks>
        public int RemainingExecutions
        {
            get
            {
                if (_finished || _cancelled)
                {
                    return 0;
                }

                return _totalTicks < 0 ? InfiniteTicks : Mathf.Max(0, _totalTicks - _executedTicks);
            }
        }

        #endregion

        #region 生命周期控制

        /// <summary>
        /// 重新开始计时并重新入队：剩余时间复位为 <see cref="Duration" />，执行计数与暂停状态一并复位，
        /// <see cref="IsFinished" /> 与 <see cref="IsCancelled" /> 清零。
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     用于"同一个冷却位反复使用"——UI 或 Model 已经持有该实例，不必换新引用。
        ///     需要不同时长时直接新建实例（框架不做对象池）。
        ///     </para>
        ///     <para>
        ///     <see cref="NextFrame" /> 形态调用本方法等价于重新挂起一次"下一帧执行"；
        ///     重复形态则从第一次执行重新开始计数。重复调用安全：已入队时不会重复注册。
        ///     暂停中的实例经本方法恢复为运行态（重新开始语义优先于暂停）。
        ///     </para>
        /// </remarks>
        public void Restart()
        {
            _remaining = _duration;
            _finished = false;
            _cancelled = false;
            _paused = false;
            _executedTicks = 0;
            _createdPassId = _passId;
            _createdFrame = Time.frameCount;
            _createdTime = Time.realtimeSinceStartup;

            if (_totalTicks == 0)
            {
                _finished = true;
                return;
            }

            Register();
        }

        /// <summary>
        /// 挂起计时：保留在注册表中、保留剩余时间与执行计数，但不再推进、不再回调。
        /// </summary>
        /// <remarks>
        /// 重复调用安全；已到期或已取消的实例调用本方法无效果。
        /// 与 <c>Time.timeScale = 0</c> 的区别是只影响这一个实例，且对 <c>unscaled</c> 实例同样有效。
        /// </remarks>
        public void Pause()
        {
            if (_paused || _finished || _cancelled)
            {
                return;
            }

            _paused = true;
        }

        /// <summary>
        /// 从 <see cref="Pause" /> 恢复：剩余时间从暂停时刻继续。
        /// </summary>
        /// <remarks>
        /// 未处于暂停时为空操作。<see cref="NextFrame" /> 形态恢复后重新挂起"下一帧"：
        /// 恢复后的第一次结算才执行，不会因为暂停期间周期号前进而立刻到期。
        /// </remarks>
        public void Resume()
        {
            if (!_paused)
            {
                return;
            }

            _paused = false;

            if (_nextFrameMode)
            {
                _createdPassId = _passId;
            }
        }

        /// <summary>
        /// 取消计时：移出结算列表、不再触发回调，状态转为 <see cref="IsCancelled" />。
        /// </summary>
        /// <remarks>
        /// 重复调用安全。已到期的计时器调用本方法无效果（见 <see cref="IsCancelled" />）。
        /// 取消后仍可用 <see cref="Restart" /> 重新拉起。
        /// </remarks>
        public void Cancel()
        {
            if (_finished || _cancelled)
            {
                return;
            }

            _cancelled = true;
            _registered = false;
            _cancelledCount++;
            Running.Remove(this);
        }

        /// <summary>
        /// 等同于 <see cref="Cancel" />，供 <c>using</c> 与句柄风格调用。
        /// </summary>
        /// <remarks>
        /// 正常到期的计时器无需释放（到期已自动出队），因此 Dispose 只对"未到期就放弃"的场景有意义。
        /// </remarks>
        public void Dispose() => Cancel();

        #endregion

        #region 驱动与注册表

        /// <summary>
        /// 域加载时清空静态状态。与 <see cref="AesirPlayerLoop" /> 的 Reset 处于同一阶段，
        /// 两者互不依赖，因此同阶段内的执行顺序不影响结果。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize() => Reset();

        /// <summary>
        /// 清空注册表并复位注入标志（域加载与测试用）。
        /// </summary>
        /// <remarks>
        /// 同时把存活实例标记为"未入队"：关闭域重载时托管对象会跨 Play 保留，
        /// 若不复位标志，这些实例的 <see cref="Restart" /> 会因"已注册"而静默失效。
        /// 周期号一并复位到当前值，避免跨会话的 <see cref="NextFrame" /> 实例永远等不到自己的周期；
        /// 暂停态同样复位，否则被挂起的残留实例在下次会话里永远不推进。
        /// </remarks>
        internal static void Reset()
        {
            for (var i = 0; i < Running.Count; i++)
            {
                Running[i]._registered = false;
                Running[i]._createdPassId = 0;
                Running[i]._paused = false;
            }

            Running.Clear();
            DueBuffer.Clear();

            // AesirPlayerLoop 可能已被其自身的 Reset 清空，此处退订在那种情况下是空操作
            _hook.Dispose();
            _scheduled = false;
            _passId = 0;
            _createdCount = 0;
            _finishedCount = 0;
            _cancelledCount = 0;
        }

        /// <summary>
        /// 在汇编加载完成后注册结算钩子。
        /// </summary>
        /// <remarks>
        /// 必须晚于 <see cref="RuntimeInitializeLoadType.SubsystemRegistration" />：
        /// <see cref="AesirPlayerLoop" /> 在该阶段执行 <c>Reset</c> 清空全部钩子，
        /// 若同阶段注册会被一并清掉。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void InitializeAfterAssembliesLoaded() => EnsureScheduled();

        /// <summary>
        /// 幂等注册结算钩子；公开 API 入口也会调用，覆盖"比 AfterAssembliesLoaded 更早使用"的情况。
        /// </summary>
        static void EnsureScheduled()
        {
            if (_scheduled)
            {
                return;
            }

            _scheduled = true;
            _hook = AesirPlayerLoop.Register(AesirLifecyclePhase.BeforeUpdate, OnSettle, SettleOrder);
        }

        /// <summary>
        /// PlayerLoop 钩子入口：按 Unity 的两种增量时间推进所有计时器。
        /// </summary>
        static void OnSettle() => TickOnce(Time.deltaTime, Time.unscaledDeltaTime);

        /// <summary>
        /// 推进一次结算：先判定到期（并就地压实注册表），再统一执行到期回调。
        /// </summary>
        /// <param name="deltaTime">受 <c>Time.timeScale</c> 影响的帧增量</param>
        /// <param name="unscaledDeltaTime">不受 <c>Time.timeScale</c> 影响的帧增量</param>
        /// <remarks>
        ///     <para>
        ///     <b>两阶段分离</b>：结算阶段不执行任何用户代码，执行阶段不遍历注册表。
        ///     因此回调内新建或取消计时器都无需挂起队列——新实例落到 <see cref="Running" /> 尾部，
        ///     本次结算已经遍历不到它，自然在下一帧生效。
        ///     </para>
        ///     <para>
        ///     <b>单趟扫描 + 就地压实</b>：未到期与已暂停的实例原地前移，本次到期实例推入
        ///     <see cref="DueBuffer" />，最后一并截断。相比逐项 <c>RemoveAt</c>（O(n²)）或每帧复制整表，
        ///     本实现保持 O(n)、零分配、且保序。
        ///     </para>
        ///     <para>
        ///     回调异常按框架约定直接向上传播（fail-fast）；<c>finally</c> 保证待执行缓冲被清空，
        ///     否则异常会让同批到期项在下一帧被重复执行。
        ///     </para>
        /// </remarks>
        internal static void TickOnce(float deltaTime, float unscaledDeltaTime)
        {
            _passId++;

            if (Running.Count > 0)
            {
                var write = 0;

                for (var i = 0; i < Running.Count; i++)
                {
                    var timer = Running[i];

                    // 暂停：留在注册表、跳过推进与执行，位置不变
                    if (timer._paused)
                    {
                        Running[write++] = timer;
                        continue;
                    }

                    if (timer._nextFrameMode)
                    {
                        // 落在"创建（或恢复）之后开始的第一次结算"上
                        if (_passId <= timer._createdPassId)
                        {
                            Running[write++] = timer;
                            continue;
                        }
                    }
                    else
                    {
                        timer._remaining -= timer._unscaled ? unscaledDeltaTime : deltaTime;

                        if (timer._remaining > 0f)
                        {
                            Running[write++] = timer;
                            continue;
                        }

                        timer._remaining = 0f;
                    }

                    // 到期：执行一次；有限次数用尽则出队，否则重新装填并留到下一次结算
                    if (timer.ConsumeDueTick())
                    {
                        Running[write++] = timer;
                    }

                    DueBuffer.Add(timer);
                }

                if (write < Running.Count)
                {
                    Running.RemoveRange(write, Running.Count - write);
                }
            }

            if (DueBuffer.Count == 0)
            {
                return;
            }

            try
            {
                for (var i = 0; i < DueBuffer.Count; i++)
                {
                    DueBuffer[i]._onFinished?.Invoke();
                }
            }
            finally
            {
                DueBuffer.Clear();
            }
        }

        /// <summary>
        /// 记账一次到期执行，并决定该实例是否继续留在注册表。
        /// </summary>
        /// <returns>true = 还有下一次执行（保留在 <see cref="Running" />），false = 已到期出队</returns>
        /// <remarks>
        /// 重新装填用 <c>_remaining = _duration</c> 而不是 <c>+=</c>：卡顿帧不补发，避免一次结算
        /// 连续回调多次把调用方拖进"越慢越忙"的正反馈。
        /// </remarks>
        bool ConsumeDueTick()
        {
            _executedTicks++;

            if (_totalTicks >= 0 && _executedTicks >= _totalTicks)
            {
                MarkFinished();
                return false;
            }

            _remaining = _duration;
            _createdPassId = _passId;
            return true;
        }

        /// <summary>
        /// 标记到期并移出注册状态。
        /// </summary>
        void MarkFinished()
        {
            _finished = true;
            _registered = false;
            _finishedCount++;
        }

        /// <summary>
        /// 入队，幂等（已入队时为空操作）。
        /// </summary>
        void Register()
        {
            if (_registered)
            {
                return;
            }

            _registered = true;
            Running.Add(this);
        }

        /// <summary>
        /// 当前仍在注册表中的实例数量，含已暂停（测试与调试窗口用）。
        /// </summary>
        internal static int RunningCount => Running.Count;

        #endregion

        #region 调试读取面

        /// <summary>
        /// 当前状态（编辑器调试窗口与检视面板分类用）。
        /// </summary>
        internal AesirTimerState State
        {
            get
            {
                if (_finished)
                {
                    return AesirTimerState.Finished;
                }

                if (_cancelled)
                {
                    return AesirTimerState.Cancelled;
                }

                if (_totalTicks == 0)
                {
                    return AesirTimerState.Uninitialized;
                }

                return _paused ? AesirTimerState.Paused : AesirTimerState.Running;
            }
        }

        /// <summary>是否为 <see cref="NextFrame" /> 形态（调试窗口展示用）。</summary>
        internal bool IsNextFrameMode => _nextFrameMode;

        /// <summary>时间源是否为 <c>Time.unscaledDeltaTime</c>（调试窗口展示用）。</summary>
        internal bool UsesUnscaledTime => _unscaled;

        /// <summary>允许执行的总次数，-1 表示无限（调试窗口展示用）。</summary>
        internal int TotalTicks => _totalTicks;

        /// <summary>已执行的回调次数（调试窗口展示用）。</summary>
        internal int ExecutedTicks => _executedTicks;

        /// <summary>创建（或最近一次 <see cref="Restart" />）的帧号（调试窗口展示用）。</summary>
        internal int CreatedFrame => _createdFrame;

        /// <summary>创建（或最近一次 <see cref="Restart" />）时的真实时间（调试窗口展示存活时长用）。</summary>
        internal float CreatedTime => _createdTime;

        /// <summary>到期回调（调试窗口展示回调归属用，可为 null）。</summary>
        internal Action Callback => _onFinished;

        /// <summary>累计创建数（调试窗口统计用）。</summary>
        internal static int CreatedCount => _createdCount;

        /// <summary>累计正常到期数（调试窗口统计用）。</summary>
        internal static int FinishedCount => _finishedCount;

        /// <summary>累计取消数（调试窗口统计用）。</summary>
        internal static int CancelledCount => _cancelledCount;

        /// <summary>已执行的结算周期数（调试窗口统计用）。</summary>
        internal static int PassCount => _passId;

        /// <summary>
        /// 把注册表中的实例复制到调用方缓冲区（调试窗口拉取快照用，复制后逐帧读取不再触碰注册表）。
        /// </summary>
        /// <param name="buffer">目标缓冲区，会先被 <c>Clear</c></param>
        internal static void CopyRunning(List<AesirTimer> buffer)
        {
            buffer.Clear();
            buffer.AddRange(Running);
        }

        #endregion
    }
}
