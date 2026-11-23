using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Runestone.AesirArchitecture;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 事件模块可观测性增强的 EditMode 测试：取消传播语义与公开诊断 API（订阅快照 / 链路环形缓冲）。
    /// <para>
    /// 互不相同的受害者契约：①取消对四档优先级的不同影响；②NonCancellable 免疫；
    /// ③复用实例跨趟复位；④诊断快照准确反映订阅与分发；⑤环形缓冲容量与清空。
    /// </para>
    /// </summary>
    public class EventModuleDiagnosticsTests
    {
        static readonly FieldInfo ModuleInstanceField =
            typeof(EventModule).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);

        EventModule _module;
        int _priorCapacity;
        bool _priorEnabled;

        [SetUp]
        public void SetUp()
        {
            _module = new GameObject("TestEventModuleDiag").AddComponent<EventModule>();
            ModuleInstanceField.SetValue(null, _module);
            _priorEnabled = AesirEventDiagnostics.Enabled;
            _priorCapacity = AesirEventDiagnostics.TraceCapacity;
            AesirEventDiagnostics.Enabled = true;
            AesirEventDiagnostics.ClearTraces();
        }

        [TearDown]
        public void TearDown()
        {
            AesirEventDiagnostics.Enabled = _priorEnabled;
            AesirEventDiagnostics.TraceCapacity = _priorCapacity;
            AesirEventDiagnostics.ClearTraces();
            ModuleInstanceField.SetValue(null, null);
            if (_module != null)
            {
                Object.DestroyImmediate(_module.gameObject);
            }
        }

        [Serializable]
        class ProbeArgs : AesirEventArgs { }

        #region 取消传播

        [Test]
        public void Cancel_SkipsMedium_ButFirstAndLastStillReceive()
        {
            var hits = new List<string>();
            var subscriber = new object();
            var handles = new List<AutoRemoveListenerHandle>
            {
                EventModule.AddListener<ProbeArgs>(subscriber, e => hits.Add("First"),
                    SubscriberPriority.First),
                EventModule.AddListener<ProbeArgs>(subscriber, e =>
                {
                    hits.Add("High");
                    e.Cancel(subscriber);
                }, SubscriberPriority.High),
                EventModule.AddListener<ProbeArgs>(subscriber, e => hits.Add("Medium"),
                    SubscriberPriority.Medium),
                EventModule.AddListener<ProbeArgs>(subscriber, e => hits.Add("Last"), SubscriberPriority.Last)
            };

            new ProbeArgs().Invoke(subscriber);

            Assert.AreEqual(new[] { "First", "High", "Last" }, hits,
                "取消后 High 档剩余与 Medium 档被跳过，First/Last 不受影响");
            foreach (var handle in handles)
            {
                handle.Dispose();
            }
        }

        [Test]
        public void Cancel_RecordsCanceller()
        {
            var subscriber = new object();
            var args = new ProbeArgs();
            var handle = EventModule.AddListener<ProbeArgs>(subscriber, e => e.Cancel(subscriber));
            args.Invoke(subscriber);

            Assert.IsTrue(args.IsCancelled);
            Assert.AreSame(subscriber, args.CancelledBy);
            handle.Dispose();
        }

        [Test]
        public void NonCancellable_DeclaredFirst_IgnoresCancel()
        {
            var hits = new List<string>();
            var subscriber = new object();
            var args = new ProbeArgs().NonCancellable();
            var handles = new List<AutoRemoveListenerHandle>
            {
                EventModule.AddListener<ProbeArgs>(subscriber, e =>
                {
                    hits.Add("First");
                    e.Cancel(subscriber);
                }, SubscriberPriority.First),
                EventModule.AddListener<ProbeArgs>(subscriber, e => hits.Add("Medium"),
                    SubscriberPriority.Medium)
            };

            args.Invoke(subscriber);

            Assert.AreEqual(new[] { "First", "Medium" }, hits, "预先声明不可取消时，Cancel 应被忽略");
            Assert.IsFalse(args.IsCancelled);
            foreach (var handle in handles)
            {
                handle.Dispose();
            }
        }

        [Test]
        public void ReusedEventArgs_ResetCancellationEachTopLevelDispatch()
        {
            var subscriber = new object();
            var args = new ProbeArgs();
            var handle = EventModule.AddListener<ProbeArgs>(subscriber, e => e.Cancel(subscriber));

            args.Invoke(subscriber);
            Assert.IsTrue(args.IsCancelled, "第一趟被取消");

            // 移除会取消的订阅者，改用只读订阅者复用同一实例
            handle.Dispose();
            var observed = false;
            var handle2 = EventModule.AddListener<ProbeArgs>(subscriber, e =>
            {
                observed = e.IsCancelled;
            });
            args.Invoke(subscriber);

            Assert.IsFalse(args.IsCancelled, "第二趟分发前应复位取消状态");
            Assert.IsFalse(observed, "订阅者读到的应是复位后的状态");
            handle2.Dispose();
        }

        #endregion

        #region 公开诊断 API

        [Test]
        public void GetSubscriptions_ReflectsRegisteredEvent()
        {
            var subscriber = new object();
            var handle = EventModule.AddListener<ProbeArgs>(subscriber, e => { });

            var subs = AesirEventDiagnostics.GetSubscriptions();

            Assert.AreEqual(1, subs.Length);
            Assert.AreEqual(typeof(ProbeArgs), subs[0].EventType);
            Assert.AreEqual(1, subs[0].SubscriberCount);
            handle.Dispose();
        }

        [Test]
        public void GetSubscribers_ReportsPriorityAndAliveState()
        {
            var subscriber = new object();
            var handle = EventModule.AddListener<ProbeArgs>(subscriber, e => { }, SubscriberPriority.Last);

            var list = AesirEventDiagnostics.GetSubscribers(typeof(ProbeArgs));

            Assert.AreEqual(1, list.Length);
            Assert.AreEqual(SubscriberPriority.Last, list[0].Priority);
            Assert.IsTrue(list[0].IsAlive);
            handle.Dispose();
        }

        [Test]
        public void GetSubscriberCount_And_HasSubscribers_MatchRegistry()
        {
            Assert.IsFalse(AesirEventDiagnostics.HasSubscribers(typeof(ProbeArgs)));
            var subscriber = new object();
            var handle = EventModule.AddListener<ProbeArgs>(subscriber, e => { });

            Assert.AreEqual(1, AesirEventDiagnostics.GetSubscriberCount(typeof(ProbeArgs)));
            Assert.IsTrue(AesirEventDiagnostics.HasSubscribers(typeof(ProbeArgs)));
            handle.Dispose();
            Assert.AreEqual(0, AesirEventDiagnostics.GetSubscriberCount(typeof(ProbeArgs)));
        }

        [Test]
        public void RecordRaised_TraceReflectsActualEventArgsType()
        {
            var subscriber = new object();
            var handle = EventModule.AddListener<ProbeArgs>(subscriber, e => { });

            // AddListener 自身会产生 SubscriberBound 快照；清空后只留本次分发的 Raised 快照
            AesirEventDiagnostics.ClearTraces();
            new ProbeArgs().Invoke(subscriber);

            var traces = AesirEventDiagnostics.GetRecentTraces();

            Assert.AreEqual(1, traces.Length);
            Assert.AreEqual(AesirEventTraceKind.Raised, traces[0].Kind);
            Assert.AreEqual(typeof(ProbeArgs), traces[0].EventType,
                "非泛型 Invoke() 也应报告运行期实际事件类型，而非基类 AesirEventArgs");
            Assert.AreEqual(1, traces[0].InvokedCount);
            Assert.AreEqual(1, traces[0].TotalSubscriberCount);
            handle.Dispose();
        }

        [Test]
        public void TraceProduced_PushesRaisedSnapshot()
        {
            AesirEventTraceSnapshot received = default;
            var got = false;
            var subscriber = new object();
            var handle = EventModule.AddListener<ProbeArgs>(subscriber, e => { });

            Action<AesirEventTraceSnapshot> handler = t =>
            {
                received = t;
                got = true;
            };
            AesirEventDiagnostics.TraceProduced += handler;
            new ProbeArgs().Invoke(subscriber);
            AesirEventDiagnostics.TraceProduced -= handler;

            Assert.IsTrue(got);
            Assert.AreEqual(AesirEventTraceKind.Raised, received.Kind);
            handle.Dispose();
        }

        [Test]
        public void TraceCapacity_LimitsRingBuffer_AndClearEmpties()
        {
            AesirEventDiagnostics.TraceCapacity = 3;
            var subscriber = new object();
            var handle = EventModule.AddListener<ProbeArgs>(subscriber, e => { });

            for (var i = 0; i < 5; i++)
            {
                new ProbeArgs().Invoke(subscriber);
            }

            Assert.AreEqual(3, AesirEventDiagnostics.TraceCount, "环形缓冲应硬限制在容量上限");
            AesirEventDiagnostics.ClearTraces();
            Assert.AreEqual(0, AesirEventDiagnostics.TraceCount);
            handle.Dispose();
        }

        #endregion
    }
}
