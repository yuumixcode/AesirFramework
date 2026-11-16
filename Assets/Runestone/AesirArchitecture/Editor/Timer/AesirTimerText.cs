using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 计时器面板的默认文案与颜色样式：把 <see cref="AesirTimerSnapshot" /> 翻译成中文显示文本。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>它是"样式"，不是"数据"</b>：输入只有抓取结果，输出只有字符串与颜色，不读
    ///     <see cref="AesirTimer" />、不碰注册表、不画控件。因此换一套文案风格（英文 / 表格口径 /
    ///     省略单位）＝另写一个同签证的样式类，数据源（<see cref="AesirTimerDebugSource" />）与
    ///     抓取结构完全不动。
    ///     </para>
    ///     <para>
    ///     调试窗口（Odin 版与 IMGUI 版）、字段绘制器、示例 Inspector 共用这一份措辞，
    ///     避免同一状态在多个面板里说法不同。
    ///     </para>
    ///     <para>
    ///     是否刷新由调用方决定（窗口按 <see cref="AesirTimerDebuggerSettings" /> 的节奏，
    ///     绘制器随重绘），本类不做任何调度。
    ///     </para>
    /// </remarks>
    internal static class AesirTimerText
    {
        /// <summary>字段未持有实例时的占位文案。</summary>
        internal const string EmptyText = "（未创建）";

        /// <summary>非 Play 模式的统一提示（两个窗口共用，保证口径一致）。</summary>
        internal const string EditModeHint =
            "当前不在 Play 模式：计时器只在运行时被 PlayerLoop 推进，编辑模式下这里没有数据源，数据保持为零。";

        internal static readonly Color RunningColor = new Color(0.42f, 0.83f, 0.45f);
        internal static readonly Color PausedColor = new Color(0.95f, 0.76f, 0.30f);
        internal static readonly Color FinishedColor = new Color(0.62f, 0.62f, 0.62f);
        internal static readonly Color CancelledColor = new Color(0.88f, 0.46f, 0.42f);
        internal static readonly Color UninitializedColor = new Color(0.62f, 0.62f, 0.62f);

        /// <summary>状态词：运行中 / 已暂停 / 已到期 / 已取消 / 未初始化。</summary>
        internal static string DescribeState(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            switch (timer.State)
            {
                case AesirTimerState.Running:
                    return "运行中";
                case AesirTimerState.Paused:
                    return "已暂停";
                case AesirTimerState.Finished:
                    return "已到期";
                case AesirTimerState.Cancelled:
                    return "已取消";
                default:
                    return "未初始化（反序列化残留）";
            }
        }

        /// <summary>状态对应的文本颜色（深色主题下可读，浅色主题下仍然可辨）。</summary>
        internal static Color StateColor(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return FinishedColor;
            }

            switch (timer.State)
            {
                case AesirTimerState.Running:
                    return RunningColor;
                case AesirTimerState.Paused:
                    return PausedColor;
                case AesirTimerState.Cancelled:
                    return CancelledColor;
                case AesirTimerState.Finished:
                    return FinishedColor;
                default:
                    return UninitializedColor;
            }
        }

        /// <summary>形态词：单次 / 下一帧 / 重复 ×N / 重复 ∞。</summary>
        internal static string DescribeForm(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            if (timer.NextFrameMode)
            {
                return "下一帧";
            }

            if (timer.TotalTicks < 0)
            {
                return "重复 ∞";
            }

            return timer.TotalTicks <= 1 ? "单次" : $"重复 ×{timer.TotalTicks}";
        }

        /// <summary>时间源词：scaled / unscaled，时间被全局冻结时标注。</summary>
        internal static string DescribeClock(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            if (timer.Unscaled)
            {
                return "unscaled";
            }

            return timer.ClockFrozen ? "scaled · 冻结" : "scaled";
        }

        /// <summary>距下一次回调的剩余秒数；"下一帧"形态没有剩余时间。</summary>
        internal static string DescribeRemaining(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            return timer.NextFrameMode ? "—" : $"{timer.Remaining:0.00}s";
        }

        /// <summary>进度 0 → 1（空引用按 0 处理，交由调用方决定如何展示）。</summary>
        internal static float Progress01(in AesirTimerSnapshot timer) =>
            timer.HasTimer ? timer.Progress : 0f;

        /// <summary>进度百分比文本。</summary>
        internal static string DescribeProgress(in AesirTimerSnapshot timer) =>
            timer.HasTimer ? $"{timer.Progress * 100f:0}%" : "—";

        /// <summary>已执行次数 / 总次数，如 "1 / 3"、"1 / ∞"。</summary>
        internal static string DescribeExecutions(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            if (timer.NextFrameMode)
            {
                return timer.State == AesirTimerState.Finished ? "已执行" : "待执行";
            }

            var total = timer.TotalTicks < 0 ? "∞" : timer.TotalTicks.ToString();
            return $"{timer.ExecutedTicks} / {total}";
        }

        /// <summary>接下来还会执行几次回调；无限重复为 "∞"。</summary>
        internal static string DescribeLeftExecutions(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            return timer.RemainingExecutions < 0 ? "∞" : timer.RemainingExecutions.ToString();
        }

        /// <summary>回调归属；没有回调时给出明确说明而不是空字符串。</summary>
        internal static string DescribeCallback(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            return timer.Callback ?? "无回调";
        }

        /// <summary>存活时长（自创建或最近一次 Restart 起的真实时间）。</summary>
        internal static string DescribeAge(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            return timer.AgeSeconds < 0f ? "—" : $"{timer.AgeSeconds:0.0}s";
        }

        /// <summary>
        /// 单行摘要：状态 + 形态 + 剩余 + 次数 + 回调，供字段绘制器与窗口详情共用。
        /// </summary>
        internal static string DescribeOneLine(in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                return EmptyText;
            }

            return $"{DescribeState(timer)} · {DescribeForm(timer)} · {DescribeClock(timer)}" +
                   $" · 剩余 {DescribeRemaining(timer)} · 次数 {DescribeExecutions(timer)}" +
                   $" · 还剩 {DescribeLeftExecutions(timer)} 次 · {DescribeCallback(timer)}" +
                   $" · 存活 {DescribeAge(timer)}";
        }

        /// <summary>
        /// 统计行：注册中 / 运行中 / 暂停 / 累计创建 / 累计到期 / 累计取消 / 结算帧。
        /// </summary>
        internal static string DescribeStatistics(in AesirTimerStatistics statistics) =>
            $"注册中 {statistics.Registered}　|　运行中 {statistics.Running}" +
            $"　|　暂停 {statistics.Paused}　|　累计创建 {statistics.CreatedTotal}" +
            $"　|　累计到期 {statistics.FinishedTotal}　|　累计取消 {statistics.CancelledTotal}" +
            $"　|　结算帧 {statistics.SettlePasses}";
    }
}
