using System;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 计时器调试窗口（IMGUI 兜底版）— 按可调的拉取节奏列出当前注册表中的计时器，
    /// 逐行显示状态、形态、时间源、剩余秒数、进度、执行次数与回调归属。
    /// <para>
    /// 安装了 Odin Inspector 时，菜单入口经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口
    /// （AesirTimerDebugWindowOdin，Odin 风格的概览与表格）；未安装时本窗口为菜单落点。
    /// 两者共用 <see cref="AesirTimerDebugSource" /> 抓取的数据与 <see cref="AesirTimerText" /> 的措辞，
    /// 本类只做 IMGUI 排布——换窗口外观不需要碰数据源，也不需要碰文案。
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

        /// <summary>列宽（表头与数据行共用，保证对齐）。</summary>
        const float StateWidth = 54f;

        const float FormWidth = 86f;
        const float ClockWidth = 92f;
        const float RemainingWidth = 62f;
        const float ProgressWidth = 104f;
        const float ExecutionsWidth = 62f;
        const float LeftWidth = 52f;
        const float AgeWidth = 52f;

        /// <summary>
        /// Odin 版窗口的打开委托（由 Odin 程序集经 [InitializeOnLoadMethod] 注册；
        /// 未安装 Odin Inspector 时为 null，菜单打开本 IMGUI 兜底窗口）。
        /// </summary>
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

        Vector2 _scroll;

        #endregion

        #region 生命周期

        void OnEnable()
        {
            AesirTimerDebuggerSettings.Changed += OnSettingsChanged;
            EditorApplication.update += OnEditorUpdate;
            _source.ResetCadence();
            _source.Refresh();
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            AesirTimerDebuggerSettings.Changed -= OnSettingsChanged;
        }

        /// <summary>拉取节奏变化（切换模式/频率、开关自动刷新）：重置节奏并立即重绘。</summary>
        void OnSettingsChanged()
        {
            _source.ResetCadence();
            Repaint();
        }

        /// <summary>
        /// 编辑器 update 驱动器：Play 状态切换或到达设定节奏时抓取一次，抓取成功才重绘
        /// （未到点的 update 不产生任何重绘，这是限流生效的关键）。
        /// </summary>
        void OnEditorUpdate()
        {
            if (_source.Tick())
            {
                Repaint();
            }
        }

        #endregion

        #region 绘制

        void OnGUI()
        {
            DrawToolbar();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(AesirTimerText.EditModeHint, MessageType.Info);
            }

            DrawStatistics();
            DrawHeader();

            if (_source.Rows.Count == 0)
            {
                EditorGUILayout.HelpBox("当前没有注册中的计时器。", MessageType.None);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (var i = 0; i < _source.Rows.Count; i++)
            {
                DrawRow(_source.Rows[i]);
            }

            if (_source.IsTruncated)
            {
                EditorGUILayout.HelpBox(
                    $"仅显示最紧迫的 {AesirTimerDebugSource.MaxRows} 个，共 {_source.Statistics.Registered} 个计时器。",
                    MessageType.None);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>工具栏：拉取模式、拉取频率、自动刷新开关、立即刷新。</summary>
        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var mode = (AesirTimerRefreshMode)EditorGUILayout.Popup(
                (int)AesirTimerDebuggerSettings.Mode, ModeLabels, EditorStyles.toolbarPopup,
                GUILayout.Width(70));

            if (mode != AesirTimerDebuggerSettings.Mode)
            {
                AesirTimerDebuggerSettings.Mode = mode;
            }

            if (AesirTimerDebuggerSettings.Mode == AesirTimerRefreshMode.Interval)
            {
                GUILayout.Label("间隔", EditorStyles.miniLabel, GUILayout.Width(26));

                var interval = EditorGUILayout.FloatField(AesirTimerDebuggerSettings.IntervalSeconds,
                    EditorStyles.toolbarTextField, GUILayout.Width(42));

                if (!Mathf.Approximately(interval, AesirTimerDebuggerSettings.IntervalSeconds))
                {
                    AesirTimerDebuggerSettings.IntervalSeconds = interval;
                }

                GUILayout.Label("秒", EditorStyles.miniLabel, GUILayout.Width(16));
            }
            else
            {
                GUILayout.Label("每", EditorStyles.miniLabel, GUILayout.Width(16));

                var frames = EditorGUILayout.IntField(AesirTimerDebuggerSettings.FrameInterval,
                    EditorStyles.toolbarTextField, GUILayout.Width(36));

                if (frames != AesirTimerDebuggerSettings.FrameInterval)
                {
                    AesirTimerDebuggerSettings.FrameInterval = frames;
                }

                GUILayout.Label("帧", EditorStyles.miniLabel, GUILayout.Width(16));
            }

            AesirTimerDebuggerSettings.AutoRefresh = GUILayout.Toggle(
                AesirTimerDebuggerSettings.AutoRefresh, "自动刷新", EditorStyles.toolbarButton,
                GUILayout.Width(66));

            if (GUILayout.Button("立即刷新", EditorStyles.toolbarButton, GUILayout.Width(66)))
            {
                _source.ResetCadence();
                _source.Refresh();
                Repaint();
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label(AesirTimerDebuggerSettings.DescribeCadence(), EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>累计统计（编辑模式下恒为零：那里没有数据源）。</summary>
        void DrawStatistics()
        {
            EditorGUILayout.LabelField(AesirTimerText.DescribeStatistics(_source.Statistics),
                EditorStyles.miniLabel);
        }

        /// <summary>表头。</summary>
        void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            Part("状态", StateWidth, EditorStyles.miniBoldLabel);
            Part("形态", FormWidth, EditorStyles.miniBoldLabel);
            Part("时间源", ClockWidth, EditorStyles.miniBoldLabel);
            Part("剩余", RemainingWidth, EditorStyles.miniBoldLabel);
            Part("进度", ProgressWidth, EditorStyles.miniBoldLabel);
            Part("已执行", ExecutionsWidth, EditorStyles.miniBoldLabel);
            Part("还剩", LeftWidth, EditorStyles.miniBoldLabel);
            Part("存活", AgeWidth, EditorStyles.miniBoldLabel);
            GUILayout.Label("回调", EditorStyles.miniBoldLabel);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>数据行：各列限宽 + 进度条 + 回调名占剩余宽度。</summary>
        void DrawRow(in AesirTimerSnapshot info)
        {
            EditorGUILayout.BeginHorizontal();

            var previousColor = GUI.contentColor;
            GUI.contentColor = AesirTimerText.StateColor(info);
            Part(AesirTimerText.DescribeState(info), StateWidth, EditorStyles.miniBoldLabel);
            GUI.contentColor = previousColor;

            Part(AesirTimerText.DescribeForm(info), FormWidth, EditorStyles.miniLabel);
            Part(AesirTimerText.DescribeClock(info), ClockWidth, EditorStyles.miniLabel);
            Part(AesirTimerText.DescribeRemaining(info), RemainingWidth, EditorStyles.miniLabel);

            var barRect = GUILayoutUtility.GetRect(ProgressWidth, EditorGUIUtility.singleLineHeight,
                GUILayout.Width(ProgressWidth));
            EditorGUI.ProgressBar(barRect, AesirTimerText.Progress01(info), AesirTimerText.DescribeProgress(info));

            Part(AesirTimerText.DescribeExecutions(info), ExecutionsWidth, EditorStyles.miniLabel);
            Part(AesirTimerText.DescribeLeftExecutions(info), LeftWidth, EditorStyles.miniLabel);
            Part(AesirTimerText.DescribeAge(info), AgeWidth, EditorStyles.miniLabel);
            GUILayout.Label(AesirTimerText.DescribeCallback(info), EditorStyles.miniLabel,
                GUILayout.MinWidth(120));

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>定宽标签。</summary>
        static void Part(string text, float width, GUIStyle style)
        {
            GUILayout.Label(text, style, GUILayout.Width(width));
        }

        #endregion
    }
}
