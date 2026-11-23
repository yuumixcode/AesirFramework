using System;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 事件监视窗口（UI Toolkit 版）：以只读方式展示"哪些事件当前有哪些订阅者"。
    /// <para>
    /// 数据全部来自公开门面 <see cref="AesirEventDiagnostics" />，本窗口无任何内部注册表访问——
    /// 未安装 Odin 时本窗口功能完整；装有 Odin 且开启 Aesir Odin 样式时菜单路由到 Odin 版窗口。
    /// </para>
    /// </summary>
    public class AesirEventMonitorWindow : EditorWindow
    {
        const string MenuPath = "Tools/Aesir/Modules/Event Monitor";

        ScrollView _list;
        bool _refreshRequested;
        Label _summary;

        /// <summary>Odin 版窗口的打开委托（Odin 程序集注册；关闭 Odin 样式时为 null）。</summary>
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

            var window = GetWindow<AesirEventMonitorWindow>();
            window.titleContent = new GUIContent("Event Monitor");
            window.minSize = new Vector2(480f, 360f);
            window.Show();
        }

        void OnTrace(AesirEventTraceSnapshot snapshot)
        {
            // 绑定关系变化才需要刷新订阅树；事件触发不改变订阅集合
            if (snapshot.Kind == AesirEventTraceKind.SubscriberBound ||
                snapshot.Kind == AesirEventTraceKind.SubscriberUnbound)
            {
                _refreshRequested = true;
            }
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
                style = { flexDirection = FlexDirection.Row, marginBottom = 6f }
            };
            _summary = new Label
            {
                style = { flexGrow = 1f, unityFontStyleAndWeight = FontStyle.Bold }
            };
            toolbar.Add(_summary);
            toolbar.Add(new Button(Refresh) { text = "刷新" });
            root.Add(toolbar);

            _list = new ScrollView(ScrollViewMode.Vertical)
            {
                style = { flexGrow = 1f }
            };
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
            var subscriptions = AesirEventDiagnostics.GetSubscriptions();
            _summary.text =
                $"已注册事件 {subscriptions.Length} 个 · 诊断{(AesirEventDiagnostics.Enabled ? "开启" : "关闭")}";

            if (subscriptions.Length == 0)
            {
                _list.Add(EventDiagnosticsWindowHelper.Empty("当前没有任何事件订阅。"));
                return;
            }

            foreach (var sub in subscriptions)
            {
                var group = new VisualElement
                {
                    style = { marginBottom = 6f }
                };
                group.Add(EventDiagnosticsWindowHelper.Header(
                    $"{sub.EventName}   〔订阅者 {sub.SubscriberCount}〕"));

                var subscribers = AesirEventDiagnostics.GetSubscribers(sub.EventType);
                if (subscribers.Length == 0)
                {
                    group.Add(EventDiagnosticsWindowHelper.Row("  （无订阅者明细）",
                        EventDiagnosticsWindowHelper.MutedColor));
                }

                foreach (var s in subscribers)
                {
                    var alive = s.IsAlive ? string.Empty : "  〔已销毁〕";
                    var color = s.IsAlive ? Color.white : EventDiagnosticsWindowHelper.WarnColor;
                    group.Add(EventDiagnosticsWindowHelper.Row(
                        $"  · {s.SubscriberName} → {s.MemberName}  [{s.Priority}]{alive}", color));
                }

                _list.Add(group);
            }
        }
    }
}
