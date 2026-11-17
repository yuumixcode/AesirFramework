using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 计时器调试窗口（UI Toolkit 版）— 按可调的拉取节奏列出当前注册表中的计时器，
    /// 逐行显示状态、形态、时间源、剩余秒数、进度、执行次数与回调归属。
    /// <para>
    /// 安装 Odin Inspector 且样式开关 <see cref="AesirInspectorStyleSettings.UseOdin" /> 为 true 时，
    /// 菜单经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口；否则打开本 UI Toolkit 窗口。
    /// 两版共用 <see cref="AesirTimerDebugSource" /> 抓取的数据与 <see cref="AesirTimerText" /> 的措辞。
    /// </para>
    /// <para>
    /// 性能取向：拉取（枚举注册表 + 逐行快照）与重绘是两条节奏。重绘由编辑器驱动，
    /// 只读缓存快照；拉取仅在 <see cref="AesirTimerDebuggerSettings" /> 设定的间隔到达时发生，
    /// 因此"打开调试面板就变卡"这件事由刷新频率配置兜住，而不是靠用户少开窗口。
    /// </para>
    /// </summary>
    public class AesirTimerDebugWindow : EditorWindow
    {
        #region 菜单与 Odin 窗口路由

        const string MenuPath = "Tools/Aesir/Architecture/Timer Debugger";
        const string WindowTitle = "Aesir Timer Debugger";

        /// <summary>Odin 版窗口的打开委托（Odin 程序集注册；关闭 Odin 样式时为 null）。</summary>
        public static Action OdinWindowOpener { get; private set; }

        /// <summary>注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。</summary>
        public static void RegisterOdinWindowOpener(Action opener) => OdinWindowOpener = opener;

        // priority 996：紧邻既有 Architecture 组（Samples 995），与 Modules 组（999）保持分割
        [MenuItem(MenuPath, false, 996)]
        static void Open()
        {
            if (AesirInspectorStyleSettings.UseOdin && OdinWindowOpener != null)
            {
                OdinWindowOpener();
                return;
            }

            var window = GetWindow<AesirTimerDebugWindow>(WindowTitle);
            window.minSize = new Vector2(760, 360);
            window.Show();
        }

        #endregion

        #region 状态

        static readonly string[] ModeLabels = { "按时间", "按帧" };

        readonly AesirTimerDebugSource _source = new AesirTimerDebugSource();

        Label _statisticsLabel;
        Label _cadenceLabel;
        VisualElement _rowsContainer;
        VisualElement _emptyHint;
        Label _hintLabel;

        #endregion

        #region 生命周期

        void OnEnable()
        {
            AesirTimerDebuggerSettings.Changed += OnSettingsChanged;
            EditorApplication.update += OnEditorUpdate;
            _source.ResetCadence();
            _source.Refresh();
            BuildUI();
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            AesirTimerDebuggerSettings.Changed -= OnSettingsChanged;
        }

        /// <summary>拉取节奏变化（切换模式/频率、开关自动刷新）：重置节奏并刷新视图。</summary>
        void OnSettingsChanged()
        {
            _source.ResetCadence();
            UpdateCadenceLabel();
            RefreshRows();
        }

        /// <summary>
        /// 编辑器 update 驱动器：Play 状态切换或到达设定节奏时抓取一次，抓取成功才刷新行视图
        /// （未到点的 update 不产生任何视图变更，这是限流生效的关键）。
        /// </summary>
        void OnEditorUpdate()
        {
            if (_source.Tick())
            {
                RefreshRows();
            }
        }

        #endregion

        #region UI

        void BuildUI()
        {
            var root = rootVisualElement;
            root.Clear();

            root.Add(BuildToolbar());

            _hintLabel = new Label(AesirTimerText.EditModeHint);
            _hintLabel.style.whiteSpace = WhiteSpace.Normal;
            _hintLabel.AddToClassList("aesir-timer-hint");
            root.Add(_hintLabel);

            _statisticsLabel = new Label();
            _statisticsLabel.style.whiteSpace = WhiteSpace.Normal;
            root.Add(_statisticsLabel);

            root.Add(BuildHeader());

            _rowsContainer = new ScrollView(ScrollViewMode.Vertical);
            _rowsContainer.style.flexGrow = 1f;
            root.Add(_rowsContainer);

            _emptyHint = new Label("当前没有注册中的计时器。");
            _emptyHint.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(_emptyHint);

            UpdateCadenceLabel();
            RefreshRows();
        }

        VisualElement BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingBottom = 4f;

            var mode = new EnumField("拉取", AesirTimerDebuggerSettings.Mode);
            mode.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is AesirTimerRefreshMode refreshMode)
                {
                    AesirTimerDebuggerSettings.Mode = refreshMode;
                    BuildUI(); // 模式切换会改变工具栏字段（间隔 / 帧），重建
                }
            });
            toolbar.Add(mode);

            if (AesirTimerDebuggerSettings.Mode == AesirTimerRefreshMode.Interval)
            {
                var interval = new FloatField("间隔(秒)") { value = AesirTimerDebuggerSettings.IntervalSeconds };
                interval.RegisterValueChangedCallback(evt =>
                    AesirTimerDebuggerSettings.IntervalSeconds = evt.newValue);
                toolbar.Add(interval);
            }
            else
            {
                var frames = new IntegerField("每(帧)") { value = AesirTimerDebuggerSettings.FrameInterval };
                frames.RegisterValueChangedCallback(evt =>
                    AesirTimerDebuggerSettings.FrameInterval = evt.newValue);
                toolbar.Add(frames);
            }

            var autoRefresh = new Toggle("自动刷新") { value = AesirTimerDebuggerSettings.AutoRefresh };
            autoRefresh.RegisterValueChangedCallback(evt =>
                AesirTimerDebuggerSettings.AutoRefresh = evt.newValue);
            toolbar.Add(autoRefresh);

            toolbar.Add(new Button(ForceRefresh) { text = "立即刷新" });

            _cadenceLabel = new Label();
            _cadenceLabel.style.flexGrow = 1f;
            _cadenceLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            toolbar.Add(_cadenceLabel);

            return toolbar;
        }

        VisualElement BuildHeader()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.Add(new Label("状态") { style = { width = 54 } });
            header.Add(new Label("形态") { style = { width = 86 } });
            header.Add(new Label("时间源") { style = { width = 92 } });
            header.Add(new Label("剩余") { style = { width = 62 } });
            header.Add(new Label("进度") { style = { width = 104 } });
            header.Add(new Label("已执行") { style = { width = 62 } });
            header.Add(new Label("还剩") { style = { width = 52 } });
            header.Add(new Label("存活") { style = { width = 52 } });
            header.Add(new Label("回调"));
            return header;
        }

        void ForceRefresh()
        {
            _source.ResetCadence();
            _source.Refresh();
            UpdateCadenceLabel();
            RefreshRows();
        }

        void UpdateCadenceLabel()
        {
            if (_cadenceLabel != null)
            {
                _cadenceLabel.text = AesirTimerDebuggerSettings.DescribeCadence();
            }
        }

        /// <summary>重建行视图：统计、空提示、逐行快照。编辑模式与 Play 模式切换时同步提示可见性。</summary>
        void RefreshRows()
        {
            if (_rowsContainer == null)
            {
                return;
            }

            _hintLabel.style.display = EditorApplication.isPlaying ? DisplayStyle.None : DisplayStyle.Flex;
            _statisticsLabel.text = AesirTimerText.DescribeStatistics(_source.Statistics);

            _rowsContainer.Clear();
            var rows = _source.Rows;
            _emptyHint.style.display = rows.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;

            for (var i = 0; i < rows.Count; i++)
            {
                _rowsContainer.Add(BuildRow(rows[i]));
            }

            if (_source.IsTruncated)
            {
                _rowsContainer.Add(new Label(
                    $"仅显示最紧迫的 {AesirTimerDebugSource.MaxRows} 个，共 {_source.Statistics.Registered} 个计时器。"));
            }
        }

        static VisualElement BuildRow(in AesirTimerSnapshot info)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            var state = new Label(AesirTimerText.DescribeState(info)) { style = { width = 54 } };
            state.style.color = AesirTimerText.StateColor(info);
            row.Add(state);
            row.Add(new Label(AesirTimerText.DescribeForm(info)) { style = { width = 86 } });
            row.Add(new Label(AesirTimerText.DescribeClock(info)) { style = { width = 92 } });
            row.Add(new Label(AesirTimerText.DescribeRemaining(info)) { style = { width = 62 } });

            var bar = new ProgressBar
            {
                value = AesirTimerText.Progress01(info) * 100f,
                title = AesirTimerText.DescribeProgress(info),
                style = { width = 104 }
            };
            row.Add(bar);

            row.Add(new Label(AesirTimerText.DescribeExecutions(info)) { style = { width = 62 } });
            row.Add(new Label(AesirTimerText.DescribeLeftExecutions(info)) { style = { width = 52 } });
            row.Add(new Label(AesirTimerText.DescribeAge(info)) { style = { width = 52 } });
            row.Add(new Label(AesirTimerText.DescribeCallback(info)));

            return row;
        }

        #endregion
    }
}
