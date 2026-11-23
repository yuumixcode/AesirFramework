using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 事件日志窗口（Odin 版）：用 Odin 表格展示公开门面 <see cref="AesirEventDiagnostics" /> 的链路快照。
    /// <para>
    /// 经 <see cref="AesirEventLogWindow.OdinWindowOpener" /> 静态委托路由打开；
    /// 未安装 Odin 或关闭 Aesir Odin 样式时落回 UI Toolkit 版 <see cref="AesirEventLogWindow" />。
    /// 两版信息等价，Odin 版提供表格排序与即时刷新。
    /// </para>
    /// </summary>
    public class AesirEventLogWindowOdin : OdinEditorWindow
    {
        [ShowInInspector]
        [ReadOnly]
        [TableList(AlwaysExpanded = false)]
        [PropertyOrder(0)]
        AesirEventTraceSnapshot[] Traces { get; set; }

        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(1)]
        string Summary { get; set; }

        [ShowInInspector]
        [PropertyOrder(2)]
        bool Enabled
        {
            get => AesirEventDiagnostics.Enabled;
            set => AesirEventDiagnostics.Enabled = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Event Log (Odin)");
            AesirEventDiagnostics.TraceProduced += OnTrace;
            Refresh();
        }

        protected override void OnDisable()
        {
            AesirEventDiagnostics.TraceProduced -= OnTrace;
            base.OnDisable();
        }

        [Button("清空", ButtonSizes.Medium)]
        [PropertyOrder(3)]
        void Clear()
        {
            AesirEventDiagnostics.ClearTraces();
            Refresh();
        }

        [Button("刷新", ButtonSizes.Medium)]
        [PropertyOrder(4)]
        void Refresh()
        {
            Traces = AesirEventDiagnostics.GetRecentTraces();
            Summary = $"链路快照 {Traces.Length} / 容量 {AesirEventDiagnostics.TraceCapacity}";
        }

        void OnTrace(AesirEventTraceSnapshot snapshot) => Refresh();

        [InitializeOnLoadMethod]
        static void RegisterOpener()
        {
            AesirEventLogWindow.RegisterOdinWindowOpener(() =>
            {
                var window = GetWindow<AesirEventLogWindowOdin>();
                window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(760f, 520f);
                window.Show();
            });
        }
    }
}
