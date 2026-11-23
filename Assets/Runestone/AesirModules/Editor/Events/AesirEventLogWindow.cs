using System;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 事件日志窗口（UI Toolkit 版）：实时展示事件链路快照（触发 / 绑定 / 解绑）。
    /// <para>
    /// 只消费公开门面 <see cref="AesirEventDiagnostics" /> 的环形缓冲与 <see cref="AesirEventDiagnostics.TraceProduced" />
    /// 推送，无内部访问。环形缓冲有硬容量上限，避免长时间 Play 时日志无限增长吃内存。
    /// </para>
    /// </summary>
    public class AesirEventLogWindow : EditorWindow
    {
        const string MenuPath = "Tools/Aesir/Modules/Event Log";
        Toggle _enabledToggle;

        ScrollView _list;
        bool _refreshRequested;
        Label _summary;

        /// <summary>Odin 版窗口的打开委托。</summary>
        public static Action OdinWindowOpener { get; private set; }

        void OnEnable()
        {
            AesirEventDiagnostics.TraceProduced += OnTrace;
            BuildUI();
            Refresh();
        }

        void OnDisable()
        {
            AesirEventDiagnostics.TraceProduced -= OnTrace;
            EditorApplication.update -= Tick;
        }

        /// <summary>由 Odin 程序集在域加载期注册打开方式。</summary>
        public static void RegisterOdinWindowOpener(Action opener) => OdinWindowOpener = opener;

        [MenuItem(MenuPath)]
        static void Open()
        {
            if (AesirInspectorStyleSettings.UseOdin && OdinWindowOpener != null)
            {
                OdinWindowOpener();
                return;
            }

            var window = GetWindow<AesirEventLogWindow>();
            window.titleContent = new GUIContent("Event Log");
            window.minSize = new Vector2(560f, 360f);
            window.Show();
        }

        void OnTrace(AesirEventTraceSnapshot snapshot)
        {
            _refreshRequested = true;
        }

        void Tick()
        {
            if (!_refreshRequested)
            {
                return;
            }

            _refreshRequested = false;
            Refresh();
        }

        void BuildUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 10f;
            root.style.paddingRight = 10f;
            root.style.paddingTop = 8f;

            var toolbar = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, marginBottom = 6f, alignItems = Align.Center }
            };
            _summary = new Label
            {
                style = { flexGrow = 1f, unityFontStyleAndWeight = FontStyle.Bold }
            };
            toolbar.Add(_summary);

            _enabledToggle = new Toggle("采集")
            {
                value = AesirEventDiagnostics.Enabled,
                tooltip = "关闭后不再产生新的链路快照"
            };
            _enabledToggle.RegisterValueChangedCallback(evt => AesirEventDiagnostics.Enabled = evt.newValue);
            toolbar.Add(_enabledToggle);

            toolbar.Add(new Button(() =>
            {
                AesirEventDiagnostics.ClearTraces();
                Refresh();
            }) { text = "清空" });

            toolbar.Add(new Button(Refresh) { text = "刷新" });
            root.Add(toolbar);

            _list = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1f } };
            root.Add(_list);

            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        void Refresh()
        {
            if (_list == null)
            {
                return;
            }

            _list.Clear();
            var traces = AesirEventDiagnostics.GetRecentTraces();
            _summary.text = $"链路快照 {traces.Length} / 容量 {AesirEventDiagnostics.TraceCapacity}";

            if (traces.Length == 0)
            {
                _list.Add(EventDiagnosticsWindowHelper.Empty("暂无事件链路记录。触发任意事件或绑定订阅者后显示。"));
                return;
            }

            foreach (var trace in traces)
            {
                _list.Add(BuildTraceRow(trace));
            }
        }

        static VisualElement BuildTraceRow(AesirEventTraceSnapshot trace)
        {
            var group = new VisualElement { style = { marginBottom = 5f } };
            var (tag, color) = DescribeKind(trace.Kind);
            var time = trace.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff");

            var head = EventDiagnosticsWindowHelper.Row(
                $"{time}  [{tag}]  {trace.EventName}  ·  {trace.ActorName}", color);
            head.style.unityFontStyleAndWeight = FontStyle.Bold;
            group.Add(head);

            if (trace.Kind == AesirEventTraceKind.Raised)
            {
                var cancelled = trace.Cancelled ? "  〔已取消〕" : string.Empty;
                group.Add(EventDiagnosticsWindowHelper.Row(
                    $"    命中 {trace.InvokedCount}/{trace.TotalSubscriberCount} 个订阅者" +
                    $"  ·  耗时 {trace.ExecutionMs:F2}ms  ·  过滤器 {trace.FilterCount}{cancelled}",
                    trace.Cancelled
                        ? EventDiagnosticsWindowHelper.WarnColor
                        : EventDiagnosticsWindowHelper.MutedColor));
            }
            // Bound / Unbound 快照的明细已在头部显示（事件名 + 参与者），无需额外行

            return group;
        }

        static (string, Color) DescribeKind(AesirEventTraceKind kind)
        {
            switch (kind)
            {
                case AesirEventTraceKind.Raised:
                    return ("触发", EventDiagnosticsWindowHelper.OkColor);
                case AesirEventTraceKind.SubscriberBound:
                    return ("绑定", new Color(0.45f, 0.75f, 1f));
                case AesirEventTraceKind.SubscriberUnbound:
                    return ("解绑", new Color(1f, 0.60f, 0.60f));
                default:
                    return (kind.ToString(), Color.white);
            }
        }
    }
}
