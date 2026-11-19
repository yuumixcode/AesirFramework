using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 计时器面板的数据源：按设定节奏把注册表抓成一批 <see cref="AesirTimerSnapshot" /> 并汇总统计。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>只管抓，不管怎么显示</b>：本类不产出任何文案、颜色或控件，视图（IMGUI 窗口 / Odin 窗口 /
    ///     字段绘制器 / 示例 Inspector）从 <see cref="Rows" /> 与 <see cref="Statistics" /> 自行取数。
    ///     因此"换一套显示样式"只需写新的视图，本类原样复用。
    ///     </para>
    ///     <para>
    ///     <b>编辑模式下没有数据源</b>：计时器只在 Play 模式下被 PlayerLoop 推进，退出 Play 后
    ///     累计计数可能残留在静态字段里（取决于是否发生域重载）。因此 <see cref="Refresh" /> 在
    ///     非 Play 模式直接返回全零、不触碰注册表；<see cref="Tick" /> 在 Play 状态切换的那一刻
    ///     立即重抓一次，让面板在进入 / 退出 Play 的同一帧就换成新口径，而不是等下一个拉取间隔。
    ///     </para>
    ///     <para>
    ///     <b>限流的边界在"拉取"这一侧</b>：<see cref="Tick" /> 只在到达
    ///     <see cref="AesirTimerDebuggerSettings" /> 设定的时间 / 帧间隔时才 <see cref="Refresh" />，
    ///     其余时刻视图只重绘缓存行。因此打开面板的开销与计时器数量、面板重绘频率都无关，
    ///     只与拉取频率相关（默认 0.5 秒一次）。
    ///     </para>
    ///     <para>
    ///     <b>排序与截断</b>：已暂停排后面，其余按剩余时间升序（<c>NextFrame</c> 剩余恒为 0，
    ///     因此"最紧迫"的排在最上面）。超过 <see cref="MaxRows" /> 时只保留前
    ///     <see cref="MaxRows" /> 行并在统计中标出总数——上千个计时器时窗口不至于卡死，
    ///     也不会掩盖真实规模。
    ///     </para>
    /// </remarks>
    internal sealed class AesirTimerDebugSource
    {
        /// <summary>单次拉取最多保留的行数。</summary>
        internal const int MaxRows = 200;

        readonly List<AbstractTimer> _timers = new List<AbstractTimer>();
        readonly List<AesirTimerSnapshot> _rows = new List<AesirTimerSnapshot>();

        int _frameCounter;
        double _nextRefreshTime;

        /// <summary>上一次 <see cref="Tick" /> 观察到的 Play 状态（用于识别进入 / 退出 Play）。</summary>
        bool _wasPlaying;

        /// <summary>缓存的快照行（按紧迫度排序）。</summary>
        internal IReadOnlyList<AesirTimerSnapshot> Rows => _rows;

        /// <summary>最近一次抓取的汇总；编辑模式与从未抓取过时全为零。</summary>
        internal AesirTimerStatistics Statistics { get; private set; }

        /// <summary>是否因行数上限截断了列表。</summary>
        internal bool IsTruncated => Statistics.Registered > _rows.Count;

        /// <summary>
        /// 由 <c>EditorApplication.update</c> 驱动：Play 状态切换时立即重抓，其余时刻按设定节奏拉取。
        /// </summary>
        /// <returns>本次是否真的抓取了（视图据此决定要不要 Repaint）</returns>
        internal bool Tick()
        {
            var playing = EditorApplication.isPlaying;

            // 进入 / 退出 Play：立刻换成新口径，不等下一个拉取间隔
            if (playing != _wasPlaying)
            {
                _wasPlaying = playing;
                ResetCadence();
                Refresh();
                return true;
            }

            // 编辑模式：没有数据源，不枚举、不计时
            if (!playing)
            {
                return false;
            }

            if (!AesirTimerDebuggerSettings.AutoRefresh)
            {
                return false;
            }

            if (AesirTimerDebuggerSettings.Mode == AesirTimerRefreshMode.Frames)
            {
                if (++_frameCounter < AesirTimerDebuggerSettings.FrameInterval)
                {
                    return false;
                }

                _frameCounter = 0;
                Refresh();
                return true;
            }

            var now = EditorApplication.timeSinceStartup;

            if (now < _nextRefreshTime)
            {
                return false;
            }

            _nextRefreshTime = now + AesirTimerDebuggerSettings.IntervalSeconds;
            Refresh();
            return true;
        }

        /// <summary>
        /// 让下一次 <see cref="Tick" /> 立即拉取（切换模式/频率、打开窗口、点「立即刷新」时调用）。
        /// </summary>
        internal void ResetCadence()
        {
            _frameCounter = 0;
            _nextRefreshTime = 0;
        }

        /// <summary>
        /// 立即抓取一次快照并重算统计；非 Play 模式下抓取结果是全零。
        /// </summary>
        internal void Refresh()
        {
            _rows.Clear();
            _timers.Clear();

            if (!EditorApplication.isPlaying)
            {
                Statistics = default;
                return;
            }

            AesirTimer.CopyRunning(_timers);
            _timers.Sort(CompareByUrgency);

            var registered = _timers.Count;
            var running = 0;
            var paused = 0;

            for (var i = 0; i < registered; i++)
            {
                if (_timers[i].IsPaused)
                {
                    paused++;
                }
                else
                {
                    running++;
                }
            }

            var count = Mathf.Min(registered, MaxRows);

            for (var i = 0; i < count; i++)
            {
                _rows.Add(AesirTimerSnapshot.Capture(_timers[i]));
            }

            Statistics = new AesirTimerStatistics(registered, running, paused,
                AesirTimer.CreatedCount, AesirTimer.FinishedCount, AesirTimer.CancelledCount,
                AesirTimer.PassCount);
        }

        /// <summary>
        /// 排序：运行中在前、已暂停在后，组内按剩余时间升序（最紧迫的先看）。
        /// </summary>
        static int CompareByUrgency(AbstractTimer a, AbstractTimer b)
        {
            if (a.IsPaused != b.IsPaused)
            {
                return a.IsPaused ? 1 : -1;
            }

            return a.Remaining.CompareTo(b.Remaining);
        }
    }
}
