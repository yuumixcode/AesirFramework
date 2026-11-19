#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// AesirTimer 用法演示：一次性延时、下一帧、按间隔重复、秒表、区间、频率，
    /// 实例级暂停 / 恢复 / 重开 / 取消，以及"每帧只读查询画进度条"的用法。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     进入 Play 后延时 / 冷却 / 有限重复会自动开始，其余动作在自定义 Inspector 里点按钮
    ///     （或对组件右键，<c>ContextMenu</c> 走同一批方法）触发。
    ///     左上角面板与 Inspector 都只是读 <c>Remaining</c> / <c>Progress</c> /
    ///     <c>RemainingExecutions</c> 这些属性，没有任何驱动代码——这就是"查询式计时"。
    ///     </para>
    ///     <para>
    ///     两者共用 <see cref="CaptureRows" /> 算出的同一份 <see cref="TimerRow" />：
    ///     数据在一处算，样式各自一套（运行时 <c>OnGUI</c> 一套、编辑器 Inspector 一套）。
    ///     这就是"显示样式可以整体替换、数据源不动"的最小示范。
    ///     </para>
    ///     <para>
    ///     把 <c>Tools → Aesir → Architecture → Timer Debugger</c> 打开，可看到同一批实例的运行时快照
    ///     （状态 / 形态 / 时间源 / 剩余 / 进度 / 已执行 / 还剩几次 / 回调归属）。
    ///     </para>
    /// </remarks>
    public sealed class AesirTimerSample : MonoBehaviour
    {
        /// <summary>左上角面板的宽度。</summary>
        const float PanelWidth = 520f;

        /// <summary>运行中 / 就绪可用的状态色。</summary>
        internal static readonly Color ReadyColor = new Color(0.40f, 0.80f, 0.45f);

        /// <summary>暂停 / 冷却中的状态色。</summary>
        internal static readonly Color WaitingColor = new Color(0.95f, 0.76f, 0.30f);

        /// <summary>未创建 / 已结束的状态色。</summary>
        internal static readonly Color IdleColor = new Color(0.62f, 0.62f, 0.62f);

        [Tooltip("一次性延时的时长（秒）")]
        [SerializeField]
        [Min(0.1f)]
        float delaySeconds = 3f;

        [Tooltip("冷却时长（秒）：面板每帧读 Progress 画进度条")]
        [SerializeField]
        [Min(0.1f)]
        float cooldownSeconds = 5f;

        [Tooltip("重复回调的间隔（秒）")]
        [SerializeField]
        [Min(0.1f)]
        float repeatInterval = 1f;

        [Tooltip("重复回调的总次数")]
        [SerializeField]
        [Min(1)]
        int repeatCount = 5;

        /// <summary>纯 C# 冷却模型，演示不继承 MonoBehaviour 的用法。</summary>
        readonly SkillCooldown _dash = new SkillCooldown(4f);

        /// <summary>面板复用的行缓冲（每次绘制前重算，避免每帧分配）。</summary>
        readonly List<TimerRow> _rows = new List<TimerRow>();

        AesirTimer _delay;
        AesirTimer _cooldown;
        AesirTimer _repeats;
        AesirTimer _heartbeat;
        AesirTimer _stopwatch;
        AesirTimer _interval;
        AesirTimer _frequency;
        int _repeatTick;
        int _intervalTick;
        int _frequencyTick;

        void Awake()
        {
            StartDelay();
            StartCooldown();
            StartRepeats();
        }

        #region 触发动作（Inspector 按钮 / 组件右键菜单）

        [ContextMenu("① 开始延时（Countdown）")]
        internal void StartDelay()
        {
            _delay?.Cancel();
            _delay = AesirTimer.Countdown(delaySeconds,
                () => Debug.Log($"[AesirTimer] 延时 {delaySeconds:0.#} 秒到点"));
        }

        [ContextMenu("② 开始冷却（Countdown + Progress）")]
        internal void StartCooldown()
        {
            _cooldown?.Cancel();
            _cooldown = AesirTimer.Countdown(cooldownSeconds, () => Debug.Log("[AesirTimer] 冷却结束"));
        }

        [ContextMenu("③ 每 N 秒触发、共 M 次（Every）")]
        internal void StartRepeats()
        {
            _repeats?.Cancel();
            _repeatTick = 0;

            var total = repeatCount;
            _repeats = AesirTimer.Every(repeatInterval,
                () => Debug.Log($"[AesirTimer] 重复 {++_repeatTick}/{total} 次"), total);
        }

        [ContextMenu("④ 切换无限重复（Every ∞）")]
        internal void ToggleHeartbeat()
        {
            if (IsAlive(_heartbeat))
            {
                _heartbeat.Cancel();
                return;
            }

            _heartbeat = AesirTimer.Every(0.5f, () => Debug.Log("[AesirTimer] 心跳"));
        }

        [ContextMenu("⑤ 下一帧执行一次（NextFrame）")]
        internal void RunNextFrame() =>
            AesirTimer.NextFrame(() => Debug.Log($"[AesirTimer] 下一帧执行，落在第 {Time.frameCount} 帧"));

        [ContextMenu("⑥ 冲刺一次（纯 C# 冷却模型）")]
        internal void TryDash() => Debug.Log(_dash.TryCast()
            ? "[AesirTimer] 冲刺成功"
            : $"[AesirTimer] 冲刺冷却中，还需 {_dash.Remaining:0.00} 秒");

        [ContextMenu("⑦ 秒表开始/读取（Stopwatch）")]
        internal void ToggleStopwatch()
        {
            if (IsAlive(_stopwatch))
            {
                Debug.Log($"[AesirTimer] 秒表已走 {((StopwatchTimer)_stopwatch).Elapsed:0.00} 秒");
                return;
            }

            _stopwatch = AesirTimer.Stopwatch();
        }

        [ContextMenu("⑧ 区间触发：3 秒每 0.5 秒（Interval）")]
        internal void StartInterval()
        {
            _interval?.Cancel();
            _intervalTick = 0;
            _interval = AesirTimer.Interval(3f, 0.5f,
                () => Debug.Log($"[AesirTimer] 区间 {++_intervalTick}/6 次"));
        }

        [ContextMenu("⑨ 频率触发：每秒 4 次（Frequency）")]
        internal void ToggleFrequency()
        {
            if (IsAlive(_frequency))
            {
                _frequency.Cancel();
                return;
            }

            _frequencyTick = 0;
            _frequency = AesirTimer.Frequency(4, () =>
            {
                if (++_frequencyTick % 4 == 0)
                {
                    Debug.Log($"[AesirTimer] 频率累计 {_frequencyTick} 次");
                }
            });
        }

        [ContextMenu("暂停全部（Pause）")]
        internal void PauseAll()
        {
            _delay?.Pause();
            _cooldown?.Pause();
            _repeats?.Pause();
            _heartbeat?.Pause();
            _stopwatch?.Pause();
            _interval?.Pause();
            _frequency?.Pause();
            _dash.Pause();
        }

        [ContextMenu("恢复全部（Resume）")]
        internal void ResumeAll()
        {
            _delay?.Resume();
            _cooldown?.Resume();
            _repeats?.Resume();
            _heartbeat?.Resume();
            _stopwatch?.Resume();
            _interval?.Resume();
            _frequency?.Resume();
            _dash.Resume();
        }

        [ContextMenu("重开冷却（Restart：剩余时间与计数一起复位）")]
        internal void RestartCooldown()
        {
            if (_cooldown == null)
            {
                StartCooldown();
                return;
            }

            _cooldown.Restart();
        }

        [ContextMenu("取消全部（Cancel）")]
        internal void CancelAll()
        {
            _delay?.Cancel();
            _cooldown?.Cancel();
            _repeats?.Cancel();
            _heartbeat?.Cancel();
            _stopwatch?.Cancel();
            _interval?.Cancel();
            _frequency?.Cancel();

            _delay = null;
            _cooldown = null;
            _repeats = null;
            _heartbeat = null;
            _stopwatch = null;
            _interval = null;
            _frequency = null;

            _dash.Reset();
        }

        #endregion

        #region 展示数据（面板与 Inspector 共用）

        /// <summary>
        /// 面板与 Inspector 共用的一行：只含显示所需的值，不含任何绘制代码。
        /// </summary>
        /// <remarks>
        /// 状态词与颜色在这里算一次，两套样式原样渲染——想换一种展示（表格、进度环、世界空间条）
        /// 只需另写一个消费本结构的视图，抓取逻辑（<see cref="CaptureRows" />）不用改。
        /// </remarks>
        internal readonly struct TimerRow
        {
            /// <summary>行标题。</summary>
            internal readonly string Label;

            /// <summary>状态词（运行中 / 已暂停 / 已到期 / 已取消 / 未创建 / 就绪 / 冷却中）。</summary>
            internal readonly string State;

            /// <summary>明细文本（剩余秒数、还剩次数）。</summary>
            internal readonly string Detail;

            /// <summary>进度 0 → 1。</summary>
            internal readonly float Progress;

            /// <summary>状态颜色。</summary>
            internal readonly Color StateColor;

            internal TimerRow(string label, string state, string detail, float progress, Color stateColor)
            {
                Label = label;
                State = state;
                Detail = detail;
                Progress = progress;
                StateColor = stateColor;
            }
        }

        /// <summary>
        /// 把当前持有的计时位抓成行数据（面板 <c>OnGUI</c> 与自定义 Inspector 共用）。
        /// </summary>
        internal void CaptureRows(List<TimerRow> buffer)
        {
            buffer.Clear();
            buffer.Add(DescribeTimer("① 延时 Countdown", _delay));
            buffer.Add(DescribeTimer("② 冷却 Countdown", _cooldown));
            buffer.Add(DescribeTimer("③ 有限重复 Every", _repeats));
            buffer.Add(DescribeTimer("④ 心跳 Every ∞", _heartbeat));
            buffer.Add(DescribeStopwatch("⑦ 秒表 Stopwatch", _stopwatch as StopwatchTimer));
            buffer.Add(DescribeTimer("⑧ 区间 Interval", _interval));
            buffer.Add(DescribeTimer("⑨ 频率 Frequency", _frequency));
            buffer.Add(DescribeCooldown("⑥ 冲刺（纯 C# 模型）", _dash));
        }

        static TimerRow DescribeTimer(string label, AesirTimer timer)
        {
            if (timer == null)
            {
                return new TimerRow(label, "未创建", "由工厂方法创建", 0f, IdleColor);
            }

            string state;
            Color color;

            if (timer.IsCancelled)
            {
                state = "已取消";
                color = IdleColor;
            }
            else if (timer.IsPaused)
            {
                state = "已暂停";
                color = WaitingColor;
            }
            else if (timer.IsFinished)
            {
                state = "已到期";
                color = IdleColor;
            }
            else
            {
                state = "运行中";
                color = ReadyColor;
            }

            var left = timer.RemainingExecutions < 0 ? "∞" : timer.RemainingExecutions.ToString();
            var detail = $"剩余 {timer.Remaining:0.00}s · 还剩 {left} 次";
            return new TimerRow(label, state, detail, timer.Progress, color);
        }

        static TimerRow DescribeStopwatch(string label, StopwatchTimer stopwatch)
        {
            if (stopwatch == null)
            {
                return new TimerRow(label, "未创建", "由 AesirTimer.Stopwatch 创建", 0f, IdleColor);
            }

            var state = stopwatch.IsCancelled ? "已取消" : stopwatch.IsPaused ? "已暂停" : "计时中";
            var color = stopwatch.IsCancelled ? IdleColor : stopwatch.IsPaused ? WaitingColor : ReadyColor;
            return new TimerRow(label, state, $"已走 {stopwatch.Elapsed:0.00}s", 0f, color);
        }

        static TimerRow DescribeCooldown(string label, SkillCooldown cooldown)
        {
            if (cooldown.IsReady)
            {
                return new TimerRow(label, "就绪", "可再次施放", 1f, ReadyColor);
            }

            var state = cooldown.IsPaused ? "冷却中（已暂停）" : "冷却中";
            return new TimerRow(label, state, $"剩余 {cooldown.Remaining:0.00}s",
                cooldown.Progress, WaitingColor);
        }

        #endregion

        #region 运行时只读面板

        void OnGUI()
        {
            CaptureRows(_rows);

            GUILayout.BeginArea(new Rect(10f, 10f, PanelWidth, 210f), GUI.skin.box);
            GUILayout.Label("AesirTimer Sample — 动作在 Play 模式下于 Inspector 点按钮触发");

            for (var i = 0; i < _rows.Count; i++)
            {
                DrawRow(_rows[i]);
            }

            GUILayout.EndArea();
        }

        /// <summary>面板样式：标题 150 + 状态 60 + 进度条 60 + 明细占剩余宽度。</summary>
        static void DrawRow(in TimerRow row)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(row.Label, GUILayout.Width(150f));

            var previousColor = GUI.contentColor;
            GUI.contentColor = row.StateColor;
            GUILayout.Label(row.State, GUILayout.Width(110f));
            GUI.contentColor = previousColor;

            DrawBar(row.Progress);
            GUILayout.Label(row.Detail);
            GUILayout.EndHorizontal();
        }

        static void DrawBar(float progress)
        {
            GUILayout.Box(GUIContent.none, GUILayout.Width(60f), GUILayout.Height(14f));

            var rect = GUILayoutUtility.GetLastRect();
            var fill = new Rect(rect.x + 1f, rect.y + 1f,
                (rect.width - 2f) * Mathf.Clamp01(progress), rect.height - 2f);

            // 用白纹理着色填充：GUI.DrawRect 是 EditorGUI 的成员，运行时 GUI 没有
            GUI.DrawTexture(fill, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f,
                new Color(0.40f, 0.80f, 0.45f), 0f, 0f);
        }

        #endregion

        static bool IsAlive(AesirTimer timer) =>
            timer != null && !timer.IsFinished && !timer.IsCancelled;
    }
}
#endif
