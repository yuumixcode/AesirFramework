using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 事件监视窗口（Odin 版）：用 Odin 表格展示公开 API <see cref="AesirEventDiagnostics" /> 提供的订阅快照。
    /// <para>
    /// 经 <see cref="AesirEventMonitorWindow.OdinWindowOpener" /> 静态委托由菜单路由打开；
    /// 未安装 Odin 或关闭 Aesir Odin 样式时，菜单落回 UI Toolkit 版 <see cref="AesirEventMonitorWindow" />。
    /// 两版消费同一套公开 API，信息等价——Odin 版只提供更好的表格与即时刷新体验。
    /// </para>
    /// </summary>
    public class AesirEventMonitorWindowOdin : OdinEditorWindow
    {
        [ShowInInspector]
        [ReadOnly]
        [TableList(AlwaysExpanded = true)]
        [PropertyOrder(0)]
        AesirEventSubscriptionSnapshot[] Subscriptions { get; set; }

        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(1)]
        string Summary { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Event Monitor (Odin)");
            AesirEventDiagnostics.TraceProduced += OnTrace;
            Refresh();
        }

        protected override void OnDisable()
        {
            AesirEventDiagnostics.TraceProduced -= OnTrace;
            base.OnDisable();
        }

        [Button("刷新", ButtonSizes.Medium)]
        [PropertyOrder(2)]
        void Refresh()
        {
            Subscriptions = AesirEventDiagnostics.GetSubscriptions();
            Summary = $"已注册事件 {Subscriptions.Length} 个 · " +
                      $"诊断{(AesirEventDiagnostics.Enabled ? "开启" : "关闭")}";
        }

        void OnTrace(AesirEventTraceSnapshot snapshot)
        {
            if (snapshot.Kind == AesirEventTraceKind.SubscriberBound ||
                snapshot.Kind == AesirEventTraceKind.SubscriberUnbound)
            {
                Refresh();
            }
        }

        [InitializeOnLoadMethod]
        static void RegisterOpener()
        {
            AesirEventMonitorWindow.RegisterOdinWindowOpener(() =>
            {
                var window = GetWindow<AesirEventMonitorWindowOdin>();
                window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(640f, 480f);
                window.Show();
            });
        }
    }
}
