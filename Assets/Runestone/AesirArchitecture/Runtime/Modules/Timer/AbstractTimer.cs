using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 计时器抽象基类：承载框架级调度引擎（注册表、PlayerLoop 注入、两阶段结算与累计统计）
    /// 与所有计时器共有的状态、查询、生命周期控制；具体计时形态由子类经
    /// <see cref="Advance" /> 与 <see cref="Rearm" /> 两个模板钩子定义。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>为什么拆出本类</b>：早期单一 <c>AesirTimer</c> 用一个类同时表达倒计时、重复、下一帧三种形态，
    ///     形态判断（<c>_nextFrameMode</c> / <c>_totalTicks</c>）与调度引擎、状态查询挤在一起，新增形态只能继续
    ///     往同一个类里加分支。本类用<see href="https://en.wikipedia.org/wiki/Template_method_pattern">模板方法</see>
    ///     把"何时推进、何时到期"的引擎固定下来，把"怎么推进"下沉给子类（OCP 对扩展开放）。
    ///     设计思路参考了开源项目 <see href="https://github.com/adammyhre/Unity-Improved-Timers">adammyhre/Unity-Improved-Timers</see>
    ///     的抽象基类 + 具体 Timer 分层，但结算引擎与语义仍是本框架自有实现。
    ///     </para>
    ///     <para>
    ///     <b>引用类型</b>：框架需要"替你推进"并且"你随时能查"，可变结构体在这里会失效——注册表推进的是副本，
    ///     调用方读到的仍是旧值；引用类型让注册表与调用方操作同一个实例。
    ///     </para>
    ///     <para>
    ///     <b>暂停的两层语义</b>：全局暂停由 <c>Time.timeScale = 0</c> 表达（<c>unscaled</c> 实例不受影响）；
    ///     <see cref="Pause" /> / <see cref="Resume" /> 是实例级暂停。两者可叠加，暂停期间
    ///     <see cref="Remaining" /> 与 <see cref="Progress" /> 一并冻结。
    ///     </para>
    ///     <para>
    ///     <b>注入方式</b>：经 <see cref="AesirPlayerLoop" /> 的 <see cref="AesirLifecyclePhase.BeforeUpdate" />
    ///     钩子推进，<c>order</c> 取 <see cref="int.MinValue" />——计时结算先于任何用户钩子，也复用其
    ///     <see cref="AesirPlayerLoop.EnsureInjected" /> 自愈能力。
    ///     </para>
    ///     <para>
    ///     <b>症状与代价</b>：帧粒度结算，误差在 ±1 帧内。每个计时器一次堆分配（Gen0，生命周期短），
    ///     框架不做对象池。单次到期形态卡顿帧只回调一次（不补发），避免"越慢越忙"的雪崩。
    ///     </para>
    /// </remarks>
    public abstract class AbstractTimer : IDisposable
    {
        /// <summary>计时结算钩子的 order 值：先于任何用户 BeforeUpdate 钩子执行。</summary>
        const int SettleOrder = int.MinValue;

        /// <summary><see cref="TotalTicks" /> 取此值表示无限次执行。</summary>
        const int InfiniteTicks = -1;

        #region 静态调度引擎

        /// <summary>尚未到期（含已暂停）的计时器，按创建顺序排列（保证同帧多个到期按创建顺序回调）。</summary>
        static readonly List<AbstractTimer> Running = new List<AbstractTimer>();

        /// <summary>本次结算中已判定到期、待执行回调的计时器，复用集合以维持稳态零分配。</summary>
        static readonly List<AbstractTimer> DueBuffer = new List<AbstractTimer>();

        /// <summary>是否已把结算钩子注册到 <see cref="AesirPlayerLoop" />（幂等标志）。</summary>
        static bool _scheduled;

        /// <summary>结算钩子的注销句柄，仅用于 <see cref="Reset" /> 时退订。</summary>
        static AutoRemoveListenerHandle _hook;

        /// <summary>已开始的结算周期数。用于"下一帧"形态判定落在哪一次结算。</summary>
        static int _passId;

        /// <summary>累计创建数（调试窗口统计用，<see cref="Reset" /> 归零）。</summary>
        static int _createdCount;

        /// <summary>累计正常到期数（调试窗口统计用）。</summary>
        static int _finishedCount;

        /// <summary>累计取消数（调试窗口统计用）。</summary>
        static int _cancelledCount;

        #endregion

        #region 实例共有状态

        /// <summary>到期回调，可为 null（仅查询进度、不做事）。</summary>
        readonly Action _onFinished;

        /// <summary>时间源：true = <c>Time.unscaledDeltaTime</c>。</summary>
        readonly bool _unscaled;

        /// <summary>构造时给定的时长；重复形态下是两次回调的间隔，区间形态下是总时长。</summary>
        readonly float _duration;

        /// <summary>允许执行的总次数，<see cref="InfiniteTicks" /> 表示无限。单次形态恒为 1。</summary>
        readonly int _totalTicks;

        /// <summary>倒计时剩余（重复形态为距下一次回调的剩余；区间形态为距总时长结束的剩余）。</summary>
        float _remaining;

        /// <summary>已执行的回调次数。</summary>
        int _executedTicks;

        /// <summary>创建（或 <see cref="Restart" /> / <see cref="Resume" />）时所处的结算周期号，仅"下一帧"形态使用。</summary>
        int _createdPassId;

        /// <summary>创建（或 <see cref="Restart" />）时的帧号，调试窗口用来展示存活时长。</summary>
        int _createdFrame;

        /// <summary>创建时的 <c>Time.realtimeSinceStartup</c>，与 <see cref="_createdFrame" /> 配对展示存活时长。</summary>
        float _createdTime;

        /// <summary>是否在 <see cref="Running" /> 中。注册状态的单一真源，杜绝重复注册。</summary>
        bool _registered;

        /// <summary>是否已正常到期（有限次数已用尽，或区间形态总时长结束）。</summary>
        bool _finished;

        /// <summary>是否被取消且尚未到期（与 <see cref="_finished" /> 互斥）。</summary>
        bool _cancelled;

        /// <summary>是否被 <see cref="Pause" /> 挂起。挂起期间仍在注册表中，但结算跳过推进与执行。</summary>
        bool _paused;

        #endregion

        #region 构造与模板钩子

        /// <summary>
        /// 由子类构造器调用，初始化共有状态并立即入队（<paramref name="totalTicks" /> 为 0 时创建即结束、不入队）。
        /// </summary>
        /// <param name="duration">时长（秒）。重复形态为间隔，区间形态为总时长；"下一帧"与秒表传 0。</param>
        /// <param name="totalTicks">允许执行次数，负数表示无限，0 表示一次也不执行。</param>
        /// <param name="onFinished">每次到期执行的回调，可为 null。</param>
        /// <param name="unscaled">是否使用不受 <c>Time.timeScale</c> 影响的时间源。</param>
        protected AbstractTimer(float duration, int totalTicks, Action onFinished, bool unscaled)
        {
            _duration = duration > 0f ? duration : 0f;
            _remaining = _duration;
            _totalTicks = totalTicks < 0 ? InfiniteTicks : totalTicks;
            _onFinished = onFinished;
            _unscaled = unscaled;
            _createdPassId = _passId;
            _createdFrame = Time.frameCount;
            _createdTime = Time.realtimeSinceStartup;
            _createdCount++;

            if (_totalTicks == 0)
            {
                _finished = true;
                return;
            }

            EnsureScheduled();
            Register();
        }

        /// <summary>
        /// 模板钩子：推进一个结算周期，返回本次是否到期（需要触发一次回调）。
        /// </summary>
        /// <remarks>
        /// 子类在此改写 <see cref="RemainingValue" /> 等自有状态；需要"直接结束但不触发回调"时调用
        /// <see cref="Finish" />。返回值只决定是否触发回调，是否留在注册表由 <see cref="Rearm" /> 决定。
        /// </remarks>
        /// <param name="deltaTime">受 <c>Time.timeScale</c> 影响的帧增量。</param>
        /// <param name="unscaledDeltaTime">不受 <c>Time.timeScale</c> 影响的帧增量。</param>
        protected abstract bool Advance(float deltaTime, float unscaledDeltaTime);

        /// <summary>
        /// 模板钩子：一次到期回调记账后调用，返回 true 表示重新装填、继续留在注册表，false 表示完成、出队。
        /// </summary>
        /// <remarks>
        /// 有限次数形态可直接返回 <see cref="RescheduleOrFinish" />；无限形态恒返回 true；
        /// 需要自定义装填（如区间形态重置下一触发点）的子类在此处理后再决定是否继续。
        /// </remarks>
        protected abstract bool Rearm();

        #endregion

        #region 子类辅助

        /// <summary>供子类读写的倒计时剩余值。</summary>
        protected float RemainingValue
        {
            get => _remaining;
            set => _remaining = value;
        }

        /// <summary>供子类读取的构造时长（重复形态为间隔，区间形态为总时长）。</summary>
        protected float DurationValue => _duration;

        /// <summary>供子类读取的时间源标志。</summary>
        protected bool UsesUnscaled => _unscaled;

        /// <summary>供子类读取的当前结算周期号（"下一帧"形态判定用）。</summary>
        protected static int CurrentPassId => _passId;

        /// <summary>供子类读写的创建周期号基准（"下一帧"形态判定落在哪一次结算）。</summary>
        protected int CreatedPassId
        {
            get => _createdPassId;
            set => _createdPassId = value;
        }

        /// <summary>按本实例的时间源从两个帧增量中挑选正确的一个。</summary>
        protected float SelectDelta(float deltaTime, float unscaledDeltaTime) =>
            _unscaled ? unscaledDeltaTime : deltaTime;

        /// <summary>有限次数用尽则标记完成并返回 false，否则返回 true（留在注册表）。</summary>
        protected bool RescheduleOrFinish()
        {
            if (_totalTicks >= 0 && _executedTicks >= _totalTicks)
            {
                Finish();
                return false;
            }

            return true;
        }

        /// <summary>直接标记完成（不触发回调），用于"总时长结束但不等同一次 interval 触发"等场景。</summary>
        protected void Finish() => MarkFinished();

        #endregion

        #region 状态查询

        /// <summary>构造时给定的时长（秒）。"下一帧"与秒表恒为 0；重复形态为间隔，区间形态为总时长。</summary>
        public float Duration => _duration;

        /// <summary>
        /// 剩余秒数。重复形态为距下一次回调的剩余，区间形态为距总时长结束的剩余；到期或取消后保持当时值。
        /// 秒表覆写为已走过的秒数。
        /// </summary>
        public virtual float Remaining => _remaining;

        /// <summary>
        /// 已走完的比例 0 → 1，可直接用于进度条。重复形态为"当前间隔"的进度，区间形态为总时长进度。
        /// </summary>
        /// <remarks>
        /// 正常到期返回 1；<see cref="Duration" /> 为 0 的实例在到期前返回 0（避免 0/0 产生 NaN）；
        /// 被暂停 / 取消时冻结在当时的值。秒表覆写为 0（无总时长概念）。
        /// </remarks>
        public virtual float Progress
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

        /// <summary>是否已正常到期（有限次数用尽，或区间形态总时长结束）。被取消的实例恒为 false。</summary>
        public bool IsFinished => _finished;

        /// <summary>是否被 <see cref="Cancel" /> 或 <see cref="Dispose" /> 取消且尚未到期。</summary>
        /// <remarks>已到期的计时器调用 <see cref="Cancel" /> 不改变任何状态。</remarks>
        public bool IsCancelled => _cancelled;

        /// <summary>是否处于实例级暂停。</summary>
        public bool IsPaused => _paused;

        /// <summary>还会执行几次回调：无限返回 -1，已到期或已取消返回 0。</summary>
        /// <remarks>该值表示"从此刻起还会回调几次"，不含已经执行的次数。秒表与区间形态按需覆写。</remarks>
        public virtual int RemainingExecutions
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
        /// 重新开始计时并重新入队：剩余时间复位为 <see cref="Duration" />，执行计数与暂停状态一并复位。
        /// </summary>
        /// <remarks>
        /// 子类覆写时应先调用 <c>base.Restart()</c> 再复位各自的额外状态（如区间的下一触发点、秒表的累计值）。
        /// 已入队时不会重复注册；暂停中的实例经本方法恢复为运行态；totalTicks 为 0 的实例保持结束、不入队。
        /// </remarks>
        public virtual void Restart()
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

        /// <summary>挂起计时：保留在注册表中、保留剩余时间与执行计数，但不再推进、不再回调。</summary>
        public void Pause()
        {
            if (_paused || _finished || _cancelled)
            {
                return;
            }

            _paused = true;
        }

        /// <summary>从 <see cref="Pause" /> 恢复：剩余时间从暂停时刻继续。</summary>
        /// <remarks>
        /// 同时把周期号基准对齐到当前值——"下一帧"形态恢复后要等恢复后的第一次结算才执行，
        /// 不会因暂停期间周期号前进而"恢复即到期"；其余形态不读该字段，对齐无副作用。
        /// </remarks>
        public void Resume()
        {
            if (!_paused)
            {
                return;
            }

            _paused = false;
            _createdPassId = _passId;
        }

        /// <summary>取消计时：移出结算列表、不再触发回调，状态转为 <see cref="IsCancelled" />。</summary>
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

        /// <summary>等同于 <see cref="Cancel" />，供 <c>using</c> 与句柄风格调用。</summary>
        public void Dispose() => Cancel();

        #endregion

        #region 驱动与注册表

        /// <summary>域加载时清空静态状态。与 <see cref="AesirPlayerLoop" /> 的 Reset 同阶段、互不依赖。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize() => Reset();

        /// <summary>清空注册表并复位注入标志（域加载与测试用）。</summary>
        /// <remarks>
        /// 同时把存活实例标记为"未入队"并对齐周期号 / 清除暂停态：关闭域重载时托管对象会跨 Play 保留，
        /// 不复位会让这些实例的 <see cref="Restart" /> 因"已注册"而静默失效、"下一帧"实例永远等不到自己的周期。
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

            _hook.Dispose();
            _scheduled = false;
            _passId = 0;
            _createdCount = 0;
            _finishedCount = 0;
            _cancelledCount = 0;
        }

        /// <summary>在汇编加载完成后注册结算钩子（必须晚于 SubsystemRegistration，否则会被一并清掉）。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void InitializeAfterAssembliesLoaded() => EnsureScheduled();

        static void EnsureScheduled()
        {
            if (_scheduled)
            {
                return;
            }

            _scheduled = true;
            _hook = AesirPlayerLoop.Register(AesirLifecyclePhase.BeforeUpdate, OnSettle, SettleOrder);
        }

        static void OnSettle() => TickOnce(Time.deltaTime, Time.unscaledDeltaTime);

        /// <summary>
        /// 推进一次结算：先逐实例推进并就地压实注册表（结算阶段不执行回调），再统一执行到期回调。
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     <b>两阶段分离</b>：回调内新建或取消计时器无需挂起队列——新实例落到 <see cref="Running" /> 尾部，
        ///     本次结算已遍历不到，自然在下一帧生效。
        ///     </para>
        ///     <para>
        ///     <b>单趟扫描 + 就地压实</b>：未到期 / 已暂停的实例原地前移，保持 O(n)、零分配、且保序。
        ///     </para>
        ///     <para>
        ///     子类可在 <see cref="Advance" /> 内调用 <see cref="Finish" /> 直接结束而不触发回调；引擎据此出队。
        ///     回调异常按框架约定直接向上传播（fail-fast）；<c>finally</c> 保证缓冲被清空。
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

                    if (timer._paused)
                    {
                        Running[write++] = timer;
                        continue;
                    }

                    var due = timer.Advance(deltaTime, unscaledDeltaTime);

                    // 子类直接结束（如区间形态总时长耗尽）：出队且不触发回调
                    if (timer._finished)
                    {
                        continue;
                    }

                    if (!due)
                    {
                        Running[write++] = timer;
                        continue;
                    }

                    timer._executedTicks++;
                    DueBuffer.Add(timer);

                    if (timer.Rearm())
                    {
                        Running[write++] = timer;
                    }
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

        /// <summary>标记到期并移出注册状态。</summary>
        void MarkFinished()
        {
            _finished = true;
            _registered = false;
            _finishedCount++;
        }

        /// <summary>入队，幂等（已入队时为空操作）。</summary>
        void Register()
        {
            if (_registered)
            {
                return;
            }

            _registered = true;
            Running.Add(this);
        }

        /// <summary>当前仍在注册表中的实例数量，含已暂停（测试与调试窗口用）。</summary>
        internal static int RunningCount => Running.Count;

        #endregion

        #region 调试读取面

        /// <summary>当前状态（编辑器调试窗口与检视面板分类用）。</summary>
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

        /// <summary>是否为"下一帧执行一次"形态（调试窗口展示用，具体形态覆写）。</summary>
        internal virtual bool IsNextFrameMode => false;

        /// <summary>时间源是否为 <c>Time.unscaledDeltaTime</c>（调试窗口展示用）。</summary>
        internal bool UsesUnscaledTime => _unscaled;

        /// <summary>允许执行的总次数，-1 表示无限（调试窗口展示用）。</summary>
        internal int TotalTicks => _totalTicks;

        /// <summary>已执行的回调次数（调试窗口展示用）。</summary>
        internal int ExecutedTicks => _executedTicks;

        /// <summary>创建（或最近一次 <see cref="Restart" />）的帧号（调试窗口展示用）。</summary>
        internal int CreatedFrame => _createdFrame;

        /// <summary>创建时的真实时间（调试窗口展示存活时长用）。</summary>
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

        /// <summary>把注册表中的实例复制到缓冲区（调试窗口拉取快照用）。</summary>
        internal static void CopyRunning(List<AbstractTimer> buffer)
        {
            buffer.Clear();
            buffer.AddRange(Running);
        }

        #endregion
    }
}
