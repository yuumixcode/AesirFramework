using System;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 计时器调试面板的拉取节奏（按时间 / 按帧两种模式）。
    /// </summary>
    internal enum AesirTimerRefreshMode
    {
        /// <summary>按真实时间间隔拉取（与帧率无关，编辑模式与 Play 模式一致）。</summary>
        Interval = 0,

        /// <summary>按编辑器 update 计数拉取（≈ 帧；适合跟随帧节奏观察）。</summary>
        Frames = 1
    }

    /// <summary>
    /// 计时器调试面板的持久化设置：拉取模式与频率、是否自动刷新。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     调试面板只在"拉取"时枚举注册表并重建显示文本，两次拉取之间只绘制缓存行；
    ///     因此这里配置的是<b>状态拉取频率</b>而不是重绘频率——它决定了打开面板时的调试开销。
    ///     默认按 0.5 秒拉取：肉眼看不出延迟，开销约为每帧拉取的 1/30。
    ///     </para>
    ///     <para>
    ///     值经 <see cref="EditorPrefs" /> 持久化，并缓存在静态字段里（域重载后首次访问重新读取），
    ///     避免 60–100 Hz 的 update 路径反复调用原生 <c>EditorPrefs</c>。
    ///     写入时触发 <see cref="Changed" />，已打开的两个窗口据此重置节奏并立即刷新。
    ///     </para>
    /// </remarks>
    internal static class AesirTimerDebuggerSettings
    {
        const string KeyPrefix = "Aesir.TimerDebugger.";
        const string ModeKey = KeyPrefix + "RefreshMode";
        const string IntervalKey = KeyPrefix + "IntervalSeconds";
        const string FramesKey = KeyPrefix + "FrameInterval";
        const string AutoRefreshKey = KeyPrefix + "AutoRefresh";

        /// <summary>时间间隔下限（秒）。低于此值拉取开销与"每帧刷新"没差别，失去限流意义。</summary>
        internal const float MinIntervalSeconds = 0.05f;

        /// <summary>时间间隔上限（秒）。1 fps 级别的数据没有观察价值。</summary>
        internal const float MaxIntervalSeconds = 10f;

        /// <summary>帧间隔下限。0 帧无意义。</summary>
        internal const int MinFrameInterval = 1;

        /// <summary>帧间隔上限。</summary>
        internal const int MaxFrameInterval = 600;

        const float DefaultIntervalSeconds = 0.5f;
        const int DefaultFrameInterval = 10;

        static bool _loaded;
        static AesirTimerRefreshMode _mode;
        static float _intervalSeconds;
        static int _frameInterval;
        static bool _autoRefresh;

        /// <summary>设置发生变化（两个窗口据此重置节奏）。</summary>
        internal static event Action Changed;

        /// <summary>拉取模式。</summary>
        internal static AesirTimerRefreshMode Mode
        {
            get
            {
                EnsureLoaded();
                return _mode;
            }
            set
            {
                EnsureLoaded();

                if (_mode == value)
                {
                    return;
                }

                _mode = value;
                EditorPrefs.SetInt(ModeKey, (int)value);
                Changed?.Invoke();
            }
        }

        /// <summary>按时间模式的间隔（秒），写入前夹取到 [<see cref="MinIntervalSeconds" />, <see cref="MaxIntervalSeconds" />]。</summary>
        internal static float IntervalSeconds
        {
            get
            {
                EnsureLoaded();
                return _intervalSeconds;
            }
            set
            {
                EnsureLoaded();
                value = Mathf.Clamp(value, MinIntervalSeconds, MaxIntervalSeconds);

                if (Mathf.Approximately(_intervalSeconds, value))
                {
                    return;
                }

                _intervalSeconds = value;
                EditorPrefs.SetFloat(IntervalKey, value);
                Changed?.Invoke();
            }
        }

        /// <summary>按帧模式的间隔（编辑器 update 计数），写入前夹取到 [<see cref="MinFrameInterval" />, <see cref="MaxFrameInterval" />]。</summary>
        internal static int FrameInterval
        {
            get
            {
                EnsureLoaded();
                return _frameInterval;
            }
            set
            {
                EnsureLoaded();
                value = Mathf.Clamp(value, MinFrameInterval, MaxFrameInterval);

                if (_frameInterval == value)
                {
                    return;
                }

                _frameInterval = value;
                EditorPrefs.SetInt(FramesKey, value);
                Changed?.Invoke();
            }
        }

        /// <summary>是否自动按节奏拉取（关闭后只在点击「立即刷新」时拉取）。</summary>
        internal static bool AutoRefresh
        {
            get
            {
                EnsureLoaded();
                return _autoRefresh;
            }
            set
            {
                EnsureLoaded();

                if (_autoRefresh == value)
                {
                    return;
                }

                _autoRefresh = value;
                EditorPrefs.SetBool(AutoRefreshKey, value);
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// 当前配置下"两次拉取之间"的说明文本（工具栏右侧提示用）。
        /// </summary>
        internal static string DescribeCadence() =>
            Mode == AesirTimerRefreshMode.Interval
                ? $"每 {IntervalSeconds:0.##}s 拉取"
                : $"每 {FrameInterval} 帧拉取";

        static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            _mode = (AesirTimerRefreshMode)EditorPrefs.GetInt(ModeKey, (int)AesirTimerRefreshMode.Interval);
            _intervalSeconds = Mathf.Clamp(EditorPrefs.GetFloat(IntervalKey, DefaultIntervalSeconds),
                MinIntervalSeconds, MaxIntervalSeconds);
            _frameInterval = Mathf.Clamp(EditorPrefs.GetInt(FramesKey, DefaultFrameInterval),
                MinFrameInterval, MaxFrameInterval);
            _autoRefresh = EditorPrefs.GetBool(AutoRefreshKey, true);
        }
    }
}
