using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 单个计时器的一次只读抓取：只搬数据，不含任何显示文案、颜色或控件。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     它是"数据源"与"显示样式"之间的唯一契约。数据源（<see cref="AesirTimerDebugSource" />）
    ///     只负责按节奏把注册表抓成一批本结构；视图（调试窗口、字段绘制器、示例 Inspector）各自决定
    ///     怎么展示。换一套面板样式时只写新的视图，抓取逻辑不动；新增一个展示字段时只在这里抓一次，
    ///     所有视图立刻可用。
    ///     </para>
    ///     <para>
    ///     <b>回调归属为什么在抓取侧算</b>：Lambda 折叠要读 <c>MethodInfo</c> 并向上找声明类型，
    ///     而抓取默认 0.5 秒一次、重绘每帧多次——放在这里重绘才能保持零反射。折叠只做
    ///     "把编译器生成的 <c>&lt;Outer&gt;b__0_0</c> 换回责任人"的归一化，不属于措辞，故不归样式层。
    ///     </para>
    ///     <para>
    ///     <see cref="default" /> 表示"字段没有持有实例"，与"实例存在但从未被真正构造"
    ///     （<see cref="AesirTimerState.Uninitialized" />）是两件事，用 <see cref="HasTimer" /> 区分。
    ///     </para>
    /// </remarks>
    internal readonly struct AesirTimerSnapshot
    {
        /// <summary>是否真的持有实例；false 时其余字段全为默认值。</summary>
        internal readonly bool HasTimer;

        /// <summary>运行中 / 已暂停 / 已到期 / 已取消 / 未初始化。</summary>
        internal readonly AesirTimerState State;

        /// <summary>是否为"下一帧执行一次"形态。</summary>
        internal readonly bool NextFrameMode;

        /// <summary>时间源是否为 <c>Time.unscaledDeltaTime</c>。</summary>
        internal readonly bool Unscaled;

        /// <summary>使用缩放时间且抓取时 <c>Time.timeScale</c> 为 0（时间被全局冻结）。</summary>
        internal readonly bool ClockFrozen;

        /// <summary>允许执行的总次数，-1 表示无限。</summary>
        internal readonly int TotalTicks;

        /// <summary>已执行的回调次数。</summary>
        internal readonly int ExecutedTicks;

        /// <summary>从此刻起还会回调几次，-1 表示无限。</summary>
        internal readonly int RemainingExecutions;

        /// <summary>构造时给定的时长（重复形态下是两次回调之间的间隔）。</summary>
        internal readonly float Duration;

        /// <summary>距下一次回调的剩余秒数。</summary>
        internal readonly float Remaining;

        /// <summary>当前间隔的进度 0 → 1。</summary>
        internal readonly float Progress;

        /// <summary>自创建（或最近一次 <c>Restart</c>）起的真实秒数，不受 <c>Time.timeScale</c> 影响。</summary>
        internal readonly float AgeSeconds;

        /// <summary>到期回调的归属（如 <c>SkillModel.Tick</c>，Lambda 折叠为 <c>SkillModel.Cast λ</c>），null 表示没有回调。</summary>
        internal readonly string Callback;

        AesirTimerSnapshot(AesirTimer timer)
        {
            HasTimer = true;
            State = timer.State;
            NextFrameMode = timer.IsNextFrameMode;
            Unscaled = timer.UsesUnscaledTime;
            ClockFrozen = !timer.UsesUnscaledTime && EditorApplication.isPlaying &&
                          Mathf.Approximately(Time.timeScale, 0f);
            TotalTicks = timer.TotalTicks;
            ExecutedTicks = timer.ExecutedTicks;
            RemainingExecutions = timer.RemainingExecutions;
            Duration = timer.Duration;
            Remaining = timer.Remaining;
            Progress = Mathf.Clamp01(timer.Progress);
            AgeSeconds = Time.realtimeSinceStartup - timer.CreatedTime;
            Callback = FoldCallbackName(timer.Callback);
        }

        /// <summary>抓取一个实例；null 返回 <see cref="default" />（<see cref="HasTimer" /> 为 false）。</summary>
        internal static AesirTimerSnapshot Capture(AesirTimer timer) =>
            timer == null ? default : new AesirTimerSnapshot(timer);

        /// <summary>
        /// 把回调折叠成"类型.方法"：编译器生成的 Lambda 名字（<c>&lt;Outer&gt;b__0_0</c>）直接展示
        /// 无法定位责任人，换回外层方法名并标 <c>λ</c>；声明类型是显示类（<c>&lt;&gt;c</c> /
        /// <c>&lt;&gt;c__DisplayClassN_M</c>）时向上找到用户类型。
        /// </summary>
        static string FoldCallbackName(System.Action callback)
        {
            if (callback == null)
            {
                return null;
            }

            var method = callback.Method;
            var name = method.Name;

            if (name.Length > 0 && name[0] == '<')
            {
                var close = name.IndexOf('>');

                if (close > 1)
                {
                    name = name.Substring(1, close - 1) + " λ";
                }
            }

            var type = method.DeclaringType;

            while (type != null && type.Name.Length > 0 && type.Name[0] == '<')
            {
                type = type.DeclaringType;
            }

            return type == null ? name : $"{type.Name}.{name}";
        }
    }

    /// <summary>
    /// 一次抓取得到的注册表汇总（存活数量 + 累计计数），编辑模式下恒为全零。
    /// </summary>
    /// <remarks>
    /// 与逐行快照分开是因为两者的消费者不同：工具条 / 统计行只读本结构，
    /// 表格只读行快照；换样式时统计行可以整体重排而无需碰行数据。
    /// </remarks>
    internal readonly struct AesirTimerStatistics
    {
        /// <summary>注册表中的实例总数（含已暂停、含被行数上限截掉的部分）。</summary>
        internal readonly int Registered;

        /// <summary>运行中（非暂停）数量。</summary>
        internal readonly int Running;

        /// <summary>已暂停数量。</summary>
        internal readonly int Paused;

        /// <summary>本次运行时起的累计创建数。</summary>
        internal readonly int CreatedTotal;

        /// <summary>本次运行时起的累计正常到期数。</summary>
        internal readonly int FinishedTotal;

        /// <summary>本次运行时起的累计取消数。</summary>
        internal readonly int CancelledTotal;

        /// <summary>本次运行时起已执行的结算周期数。</summary>
        internal readonly int SettlePasses;

        internal AesirTimerStatistics(int registered, int running, int paused, int createdTotal,
            int finishedTotal, int cancelledTotal, int settlePasses)
        {
            Registered = registered;
            Running = running;
            Paused = paused;
            CreatedTotal = createdTotal;
            FinishedTotal = finishedTotal;
            CancelledTotal = cancelledTotal;
            SettlePasses = settlePasses;
        }
    }
}
