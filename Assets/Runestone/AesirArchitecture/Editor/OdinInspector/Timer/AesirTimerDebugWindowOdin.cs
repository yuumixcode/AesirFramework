using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 计时器调试窗口（Odin Inspector 版）— 按可调的拉取节奏列出注册表中的计时器。
    /// <para>
    /// 与 IMGUI 兜底窗口 <see cref="AesirTimerDebugWindow" /> 共用 <see cref="AesirTimerDebugSource" />
    /// 抓取的数据与 <see cref="AesirTimerText" /> 的措辞，本类只做 Odin 侧的行视图模型与工具栏绑定。
    /// 编辑器加载时经 <see cref="RegisterOpener" /> 把打开方式注册到菜单入口；未安装 Odin Inspector
    /// 时本程序集整体不参与编译，菜单自动回退到 IMGUI 窗口。
    /// </para>
    /// <para>
    /// 行视图模型（<see cref="TimerRow" />）在每次抓取后用快照重建并算好显示文本，
    /// 重绘期间零 LINQ、零字符串拼接、零注册表枚举；拉取频率由
    /// <see cref="AesirTimerDebuggerSettings" /> 决定（默认 0.5 秒一次）。
    /// </para>
    /// </summary>
    public class AesirTimerDebugWindowOdin : OdinEditorWindow
    {
        #region 行视图模型

        /// <summary>
        /// 单个计时器的行视图模型：把 <see cref="AesirTimerSnapshot" /> 按本窗口的列布局摊平。
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     摊平（而不是直接序列化快照）是 Odin 列表的绘制要求：一列一个成员才有列宽。
        ///     它属于本窗口的样式——换一套列布局只需换这个视图模型，快照与措辞不动。
        ///     </para>
        ///     <para>
        ///     每个显示字段都要标 <c>[EnableGUI]</c>：列表本身是只读的，Odin 会把它的子项也当作
        ///     不可编辑逐个推入 <c>GUI.enabled = false</c> 绘制（父级 <c>[EnableGUI]</c> 管不到子属性
        ///     各自的绘制作用域），于是行文本被渲染成"禁用灰"。
        ///     </para>
        /// </remarks>
        [Serializable]
        public sealed class TimerRow
        {
            /// <summary>状态词（运行中 / 已暂停）。</summary>
            [HorizontalGroup("Row", Width = 58)]
            [EnableGUI]
            [DisplayAsString(false, 12)]
            [HideLabel]
            public string State;

            /// <summary>形态与时间源，如 "重复 ×3 · unscaled"。</summary>
            [HorizontalGroup("Row", Width = 150)]
            [EnableGUI]
            [DisplayAsString(false, 12)]
            [HideLabel]
            public string Shape;

            /// <summary>剩余 / 已执行 / 还剩 / 存活 的合并文本。</summary>
            [HorizontalGroup("Row", Width = 250)]
            [EnableGUI]
            [DisplayAsString(false, 12)]
            [HideLabel]
            public string Timing;

            /// <summary>当前间隔的进度（0–100，进度条）。</summary>
            [HorizontalGroup("Row", Width = 96)]
            [EnableGUI]
            [ProgressBar(0, 100)]
            [HideLabel]
            public float ProgressPercent;

            /// <summary>回调归属（类型.方法）。</summary>
            [EnableGUI]
            [DisplayAsString(false, 12)]
            [HideLabel]
            public string Callback;

            /// <summary>供 Odin 反序列化使用的空构造。</summary>
            public TimerRow() { }

            /// <summary>由快照填充显示字段。</summary>
            internal TimerRow(in AesirTimerSnapshot info)
            {
                State = AesirTimerText.DescribeState(info);
                Shape = $"{AesirTimerText.DescribeForm(info)} · {AesirTimerText.DescribeClock(info)}";
                Timing = $"剩余 {AesirTimerText.DescribeRemaining(info)}" +
                         $" · 已 {AesirTimerText.DescribeExecutions(info)}" +
                         $" · 还剩 {AesirTimerText.DescribeLeftExecutions(info)}" +
                         $" · 存活 {AesirTimerText.DescribeAge(info)}";
                ProgressPercent = AesirTimerText.Progress01(info) * 100f;
                Callback = AesirTimerText.DescribeCallback(info);
            }
        }

        #endregion

        #region 常量与文案

        const string WindowTitle = "Aesir Timer Debugger";

        /// <summary>非 Play 模式提示（与 IMGUI 版同源，作为 <c>InfoBox</c> 的编译期常量别名）。</summary>
        const string EditModeHint = AesirTimerText.EditModeHint;

        #endregion

        #region 打开方式注册

        /// <summary>编辑器加载时把 Odin 窗口打开方式注册给菜单入口（域重载后静态委托清空，每次重载重新注册）。</summary>
        [InitializeOnLoadMethod]
        static void RegisterOpener() => AesirTimerDebugWindow.RegisterOdinWindowOpener(OpenWindow);

        /// <summary>打开窗口（菜单路由到此）。</summary>
        public static void OpenWindow()
        {
            var window = GetWindow<AesirTimerDebugWindowOdin>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(760, 380);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(980, 520);
            window.Show();
        }

        #endregion

        #region 状态

        readonly AesirTimerDebugSource _source = new AesirTimerDebugSource();

        /// <summary>
        /// 行视图模型（序列化以便域重载后仍有内容，下一次抓取即被覆盖）。
        /// 显示成员由 <see cref="AesirTimerDebugSource" /> 的快照重建，不做双向绑定。
        /// </summary>
        [SerializeField]
        [EnableGUI]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("计时器")]
        [InfoBox("$" + nameof(EditModeHint), InfoMessageType.Info, VisibleIf = nameof(IsNotPlaying))]
        [InfoBox("当前没有注册中的计时器。", InfoMessageType.None, VisibleIf = nameof(IsEmpty))]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = false, IsReadOnly = true,
            ShowItemCount = false, ShowPaging = false, ShowIndexLabels = false)]
        List<TimerRow> _rows = new List<TimerRow>();

        /// <summary>统计行文本（随抓取更新，避免绘制期拼串）。</summary>
        string _statistics = string.Empty;

        #endregion

        #region 工具栏

        /// <summary>拉取模式（写入即持久化，两个窗口同步重置节奏）。</summary>
        [PropertyOrder(-30)]
        [HorizontalGroup("Toolbar", Width = 150)]
        [ShowInInspector]
        [HideLabel]
        [EnumToggleButtons]
        [EnableGUI]
        AesirTimerRefreshMode RefreshMode
        {
            get => AesirTimerDebuggerSettings.Mode;
            set => AesirTimerDebuggerSettings.Mode = value;
        }

        /// <summary>按时间模式的间隔（秒）。</summary>
        [PropertyOrder(-29)]
        [HorizontalGroup("Toolbar", Width = 92)]
        [ShowInInspector]
        [HideLabel]
        [ShowIf(nameof(IsIntervalMode))]
        [EnableGUI]
        [SuffixLabel("秒", false)]
        [PropertyRange(AesirTimerDebuggerSettings.MinIntervalSeconds, AesirTimerDebuggerSettings.MaxIntervalSeconds)]
        float IntervalSeconds
        {
            get => AesirTimerDebuggerSettings.IntervalSeconds;
            set => AesirTimerDebuggerSettings.IntervalSeconds = value;
        }

        /// <summary>按帧模式的间隔（编辑器 update 计数）。</summary>
        [PropertyOrder(-28)]
        [HorizontalGroup("Toolbar", Width = 92)]
        [ShowInInspector]
        [HideLabel]
        [ShowIf(nameof(IsFrameMode))]
        [EnableGUI]
        [SuffixLabel("帧", false)]
        [PropertyRange(AesirTimerDebuggerSettings.MinFrameInterval, AesirTimerDebuggerSettings.MaxFrameInterval)]
        int FrameInterval
        {
            get => AesirTimerDebuggerSettings.FrameInterval;
            set => AesirTimerDebuggerSettings.FrameInterval = value;
        }

        /// <summary>自动按节奏拉取；关闭后只在点「立即刷新」时更新。</summary>
        [PropertyOrder(-27)]
        [HorizontalGroup("Toolbar", Width = 110)]
        [ShowInInspector]
        [EnableGUI]
        bool AutoRefresh
        {
            get => AesirTimerDebuggerSettings.AutoRefresh;
            set => AesirTimerDebuggerSettings.AutoRefresh = value;
        }

        /// <summary>立即抓取一次（不改变自动刷新设置）。</summary>
        [PropertyOrder(-26)]
        [HorizontalGroup("Toolbar", Width = 100)]
        [Button("立即刷新", ButtonSizes.Medium)]
        void RefreshNow()
        {
            _source.ResetCadence();
            _source.Refresh();
            RebuildRows();
        }

        bool IsIntervalMode => AesirTimerDebuggerSettings.Mode == AesirTimerRefreshMode.Interval;

        bool IsFrameMode => AesirTimerDebuggerSettings.Mode == AesirTimerRefreshMode.Frames;

        bool IsNotPlaying => !EditorApplication.isPlaying;

        bool IsEmpty => _rows.Count == 0;

        #endregion

        #region 面板内容

        /// <summary>累计统计（编辑模式下恒为零：那里没有数据源）。</summary>
        [PropertyOrder(-20)]
        [ShowInInspector]
        [EnableGUI]
        [DisplayAsString(false, 12)]
        [HideLabel]
        string Statistics => _statistics;

        #endregion

        #region 生命周期

        protected override void OnEnable()
        {
            base.OnEnable();
            _source.ResetCadence();
            _source.Refresh();
            RebuildRows();

            AesirTimerDebuggerSettings.Changed += OnSettingsChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        protected override void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            AesirTimerDebuggerSettings.Changed -= OnSettingsChanged;
            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void DrawEditor(int index)
        {
            // 域重载后行视图模型可能落后于快照（_rows 为序列化字段，快照不是），数量失配时兜底重建
            if (_rows.Count != _source.Rows.Count)
            {
                RebuildRows();
            }

            base.DrawEditor(index);
        }

        /// <summary>设置变化（切换模式/频率、开关自动刷新）：重置节奏并重绘。</summary>
        void OnSettingsChanged()
        {
            _source.ResetCadence();
            Repaint();
        }

        /// <summary>编辑器 update 驱动器：Play 状态切换或到达设定节奏时抓取快照并重建行。</summary>
        void OnEditorUpdate()
        {
            if (!_source.Tick())
            {
                return;
            }

            RebuildRows();
            Repaint();
        }

        /// <summary>按当前快照重建行视图模型与统计文本。</summary>
        void RebuildRows()
        {
            _rows.Clear();

            for (var i = 0; i < _source.Rows.Count; i++)
            {
                _rows.Add(new TimerRow(_source.Rows[i]));
            }

            _statistics = AesirTimerText.DescribeStatistics(_source.Statistics) +
                          $"　|　{AesirTimerDebuggerSettings.DescribeCadence()}" +
                          (_source.IsTruncated
                              ? $"　|　仅显示最紧迫的 {AesirTimerDebugSource.MaxRows} 个"
                              : string.Empty);
        }

        #endregion
    }
}
