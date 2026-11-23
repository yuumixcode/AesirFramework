#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Collections.Generic;
using System.Text;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules.Samples.Events.Diagnostics
{
    /// <summary>
    /// 诊断示例：演示全部监控数据都来自公开门面 <see cref="AesirEventDiagnostics" />——
    /// 没装 Odin 也能据此自建监视面板；同时演示"退订方无句柄"时用按对象退订 API。
    /// <para>
    /// 操作：Space = 发布事件；D = 精确退订 ListenerA 对该事件的订阅；
    /// F = 批量退订 ListenerB 的全部订阅；R = 恢复两位订阅者。
    /// HUD 实时显示订阅树（拉取式查询）与最近链路快照（推送 + 环形缓冲）。
    /// 也可打开 Tools → Aesir → Modules → Event Monitor / Event Log 对照同样的数据。
    /// </para>
    /// </summary>
    [AddComponentMenu("")]
    public class DiagnosticsDemo : MonoBehaviour
    {
        readonly Listener _listenerA = new Listener("ListenerA");
        readonly Listener _listenerB = new Listener("ListenerB");
        readonly StringBuilder _sb = new StringBuilder();

        int _sequence;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                new PingEventArgs { sequence = ++_sequence }.Invoke(this);
            }

            // 退订方无句柄：按事件类型精确退订某个对象对该事件的订阅
            if (Input.GetKeyDown(KeyCode.D))
            {
                EventModule.RemoveListener<PingEventArgs>(_listenerA);
                RefreshHud();
            }

            // 退订方无句柄：批量退订某个对象的全部订阅
            if (Input.GetKeyDown(KeyCode.F))
            {
                EventModule.RemoveListener(_listenerB);
                RefreshHud();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _listenerA.Subscribe(this);
                _listenerB.Subscribe(this);
                RefreshHud();
            }
        }

        void OnEnable()
        {
            _listenerA.Subscribe(this);
            _listenerB.Subscribe(this);

            // 推送式：订阅公开事件，实时感知链路快照（自建面板的核心入口）
            AesirEventDiagnostics.TraceProduced += OnTrace;

            AesirEventDiagnostics.Enabled = true;
            AesirEventDiagnostics.ClearTraces();
            RefreshHud();
        }

        void OnDisable()
        {
            AesirEventDiagnostics.TraceProduced -= OnTrace;
            _listenerA.Unsubscribe();
            _listenerB.Unsubscribe();
        }

        void OnTrace(AesirEventTraceSnapshot snapshot)
        {
            // 收到新快照即刷新（与内置 Event Log 窗口消费的是同一条公开数据流）
            RefreshHud();
        }

        void RefreshHud()
        {
            _sb.Clear();
            _sb.AppendLine("=== 公开诊断 API 演示（零 Odin 依赖）===");
            _sb.AppendLine("Space=发布  D=精确退A  F=批量退B  R=恢复  （对照 Tools/Aesir/Modules 窗口）");
            _sb.AppendLine();

            // 拉取式：订阅树查询
            _sb.AppendLine("【订阅树 GetSubscriptions / GetSubscribers】");
            var subscriptions = AesirEventDiagnostics.GetSubscriptions();
            if (subscriptions.Length == 0)
            {
                _sb.AppendLine("  （无订阅）");
            }

            foreach (var sub in subscriptions)
            {
                _sb.AppendLine($"  {sub.EventName}: {sub.SubscriberCount} 个订阅者");
                foreach (var s in AesirEventDiagnostics.GetSubscribers(sub.EventType))
                {
                    var alive = s.IsAlive ? "存活" : "已销毁";
                    _sb.AppendLine($"    · {s.SubscriberName} → {s.MemberName} [{s.Priority}] {alive}");
                }
            }

            _sb.AppendLine();

            // 推送式：环形缓冲的最近链路
            _sb.AppendLine("【最近链路 GetRecentTraces（最新在前）】");
            var traces = AesirEventDiagnostics.GetRecentTraces(6);
            if (traces.Length == 0)
            {
                _sb.AppendLine("  （暂无链路，按 Space 发布）");
            }

            foreach (var t in traces)
            {
                var time = t.Timestamp.ToString("HH:mm:ss");
                if (t.Kind == AesirEventTraceKind.Raised)
                {
                    _sb.AppendLine(
                        $"  [{time}] 触发 {t.EventName}：命中 {t.InvokedCount}/{t.TotalSubscriberCount}" +
                        $"  {t.ExecutionMs:F2}ms{(t.Cancelled ? " [已取消]" : "")}");
                }
                else
                {
                    var verb = t.Kind == AesirEventTraceKind.SubscriberBound ? "绑定" : "解绑";
                    _sb.AppendLine($"  [{time}] {verb} {t.EventName}：{t.ActorName}");
                }
            }

            DiagnosticsHud.Render(_sb.ToString());
        }

        /// <summary>示例事件参数：sequence 为发布序号，fired 记录本次被哪些订阅者命中。</summary>
        sealed class PingEventArgs : AesirEventArgs
        {
            public readonly List<string> fired = new List<string>();
            public int sequence;
        }

        /// <summary>订阅者：持有脚本订阅句柄，命中时把自己的名字写进事件载荷。</summary>
        sealed class Listener
        {
            public readonly string Name;
            AutoRemoveListenerHandle _handle;

            public Listener(string name) => Name = name;

            public void Subscribe(object subscriber)
            {
                _handle = EventModule.AddListener<PingEventArgs>(subscriber,
                    e => e.fired.Add($"{Name}(seq={e.sequence})"));
            }

            public void Unsubscribe() => _handle.Dispose();
        }
    }
}
#endif
