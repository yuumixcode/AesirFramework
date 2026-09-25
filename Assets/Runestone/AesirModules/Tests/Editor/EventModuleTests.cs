using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using Runestone.AesirArchitecture;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// Event Module 分发增强的 EditMode 测试：
    /// 订阅者过滤器（ISubscriberFilter + 五个内建过滤器）、死引用清理、
    /// 性能监控（executionMsLimit）、AesirEventArgsSO 资产化与 [AesirListener] AllowMultiple。
    /// <para>
    /// 测试自建 EventModule 实例，TearDown 全量销毁创建的 GameObject / 资产 / 附加场景，
    /// 避免 refresh 时污染打开的场景。
    /// </para>
    /// </summary>
    public class EventModuleTests
    {
        #region 测试事件类型

        [Serializable]
        class TestEventArgs : AesirEventArgs { }

        [Serializable]
        class TestArgsA : AesirEventArgs { }

        [Serializable]
        class TestArgsB : AesirEventArgs { }

        /// <summary>仅对指定订阅者抛异常的过滤器，验证过滤器异常隔离。</summary>
        class ThrowForSubscriberFilter : ISubscriberFilter
        {
            readonly object _target;

            public ThrowForSubscriberFilter(object target) => _target = target;

            public bool ShouldReceive(AesirEventArgs eventArgs,
                object subscriber,
                SubscriberPriority priority)
            {
                if (ReferenceEquals(subscriber, _target))
                {
                    throw new InvalidOperationException("boom");
                }

                return true;
            }
        }

        #endregion

        #region 环境管理

        EventModule _module;

        readonly List<GameObject> _createdObjects = new List<GameObject>();
        readonly List<Object> _createdAssets = new List<Object>();
        Scene _previewScene;

        [SetUp]
        public void SetUp()
        {
            _module = NewGameObject("TestEventModule").AddComponent<EventModule>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _createdObjects)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _createdObjects.Clear();

            foreach (var asset in _createdAssets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }

            _createdAssets.Clear();

            if (_previewScene.IsValid())
            {
                EditorSceneManager.ClosePreviewScene(_previewScene);
            }
        }

        /// <summary>创建 GameObject 并登记，TearDown 统一销毁。</summary>
        GameObject NewGameObject(string name)
        {
            var gameObject = new GameObject(name);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        #endregion

        #region WithFilter / WithFilters 链式 API

        [Test]
        public void WithFilter_AddsFilter_AndReturnsSelfForChaining()
        {
            var eventArgs = new TestEventArgs();
            var filter = new WithTag("Player");

            var result = eventArgs.WithFilter(filter);

            Assert.AreSame(eventArgs, result);
            Assert.AreEqual(1, eventArgs.Filters.Count);
            Assert.AreSame(filter, eventArgs.Filters[0]);
        }

        [Test]
        public void WithFilters_AddsAllFilters()
        {
            var eventArgs = new TestEventArgs();

            eventArgs.WithFilters(new WithTag("A"), new WithPriority(SubscriberPriority.Last));

            Assert.AreEqual(2, eventArgs.Filters.Count);
        }

        [Test]
        public void WithTag_NullFilter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TestEventArgs().WithFilter(null));
            Assert.Throws<ArgumentNullException>(() => new TestEventArgs().WithFilters(null));
        }

        [Test]
        public void WithTag_EmptyTag_ThrowsArgumentException()
        {
            // CompareTag("") 语义无意义——按 fail-fast 在构造期拒绝，而非留到分发期被隔离捕获
            Assert.Throws<ArgumentException>(() => new WithTag(""));
        }

        [Test]
        public void Filters_DefaultNull_UntilFirstFilterAdded()
        {
            Assert.IsNull(new TestEventArgs().Filters);
        }

        #endregion

        #region 内建过滤器

        [Test]
        public void WithTag_OnlyDeliversToTaggedSubscriber()
        {
            var tagged = NewGameObject("Tagged");
            tagged.tag = "Player";
            var untagged = NewGameObject("Untagged");

            var taggedHits = 0;
            var untaggedHits = 0;
            EventModule.AddListener<TestEventArgs>(tagged, e => taggedHits++);
            EventModule.AddListener<TestEventArgs>(untagged, e => untaggedHits++);

            new TestEventArgs().WithFilter(new WithTag("Player")).Invoke(_module.gameObject);

            Assert.AreEqual(1, taggedHits);
            Assert.AreEqual(0, untaggedHits);
        }

        [Test]
        public void WithPriority_OnlyDeliversToMatchingTier()
        {
            var mediumHits = 0;
            var lastHits = 0;
            var subscriber = NewGameObject("Subscriber");
            EventModule.AddListener<TestEventArgs>(subscriber, e => mediumHits++, SubscriberPriority.Medium);
            EventModule.AddListener<TestEventArgs>(subscriber, e => lastHits++, SubscriberPriority.Last);

            new TestEventArgs().WithFilter(new WithPriority(SubscriberPriority.Medium))
                .Invoke(_module.gameObject);

            Assert.AreEqual(1, mediumHits);
            Assert.AreEqual(0, lastHits);
        }

        [Test]
        public void SameSceneAsEmitter_DeliversToSameSceneSubscriber()
        {
            var emitter = NewGameObject("Emitter");
            var subscriber = NewGameObject("SameSceneSubscriber");
            var hits = 0;
            EventModule.AddListener<TestEventArgs>(subscriber, e => hits++);

            new TestEventArgs().WithFilter(new SameSceneAsEmitter()).Invoke(emitter);

            Assert.AreEqual(1, hits);
        }

        [Test]
        public void SameSceneAsEmitter_BlocksSubscriberInOtherScene()
        {
            var emitter = NewGameObject("Emitter");
            var subscriber = NewGameObject("CrossSceneSubscriber");
            var hits = 0;
            EventModule.AddListener<TestEventArgs>(subscriber, e => hits++);

            // 用预览场景承载订阅者：不依赖当前打开场景的保存状态，也不产生层级/磁盘污染
            _previewScene = EditorSceneManager.NewPreviewScene();
            SceneManager.MoveGameObjectToScene(subscriber, _previewScene);

            new TestEventArgs().WithFilter(new SameSceneAsEmitter()).Invoke(emitter);

            Assert.AreEqual(0, hits);
        }

        [Test]
        public void SameSceneAsEmitter_NonSceneEmitter_BlocksEveryone()
        {
            var subscriber = NewGameObject("Subscriber");
            var hits = 0;
            EventModule.AddListener<TestEventArgs>(subscriber, e => hits++);

            // 发布者为纯 C# 对象，无法解析场景 → fail-closed
            new TestEventArgs().WithFilter(new SameSceneAsEmitter()).Invoke(new object());

            Assert.AreEqual(0, hits);
        }

        [Test]
        public void OnlySelf_DeliversToSelfChildAndParent()
        {
            var emitter = NewGameObject("Emitter");
            var child = NewGameObject("Child");
            child.transform.SetParent(emitter.transform);
            var parent = NewGameObject("Parent");
            emitter.transform.SetParent(parent.transform);
            var unrelated = NewGameObject("Unrelated");

            var childHits = 0;
            var parentHits = 0;
            var unrelatedHits = 0;
            EventModule.AddListener<TestEventArgs>(child, e => childHits++);
            EventModule.AddListener<TestEventArgs>(parent, e => parentHits++);
            EventModule.AddListener<TestEventArgs>(unrelated, e => unrelatedHits++);

            new TestEventArgs().WithFilter(new OnlySelf()).Invoke(emitter);

            Assert.AreEqual(1, childHits, "子树订阅者应收到");
            Assert.AreEqual(1, parentHits, "父级链订阅者应收到");
            Assert.AreEqual(0, unrelatedHits, "无关订阅者应被拦截");
        }

        [Test]
        public void InsideCollider2D_OnlyDeliversToSubscribersInsideCollider()
        {
            var emitter = NewGameObject("Emitter");
            var collider = emitter.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(3f, 3f);

            var inside = NewGameObject("Inside");
            inside.transform.position = new Vector3(0.5f, 0.5f);
            var outside = NewGameObject("Outside");
            outside.transform.position = new Vector3(10f, 10f);

            var insideHits = 0;
            var outsideHits = 0;
            EventModule.AddListener<TestEventArgs>(inside, e => insideHits++);
            EventModule.AddListener<TestEventArgs>(outside, e => outsideHits++);

            Physics2D.SyncTransforms();
            new TestEventArgs().WithFilter(new InsideCollider2D()).Invoke(emitter);

            Assert.AreEqual(1, insideHits);
            Assert.AreEqual(0, outsideHits);
        }

        [Test]
        public void ThrowingFilter_Isolated_OtherSubscribersStillReceive()
        {
            var blocked = NewGameObject("Blocked");
            var normal = NewGameObject("Normal");

            var normalHits = 0;
            EventModule.AddListener<TestEventArgs>(blocked, e => Assert.Fail("不应收到事件"));
            EventModule.AddListener<TestEventArgs>(normal, e => normalHits++);

            LogAssert.Expect(LogType.Error, new Regex("事件分发异常"));
            new TestEventArgs().WithFilter(new ThrowForSubscriberFilter(blocked)).Invoke(_module.gameObject);

            Assert.AreEqual(1, normalHits, "过滤器异常不应中断其他订阅者");
        }

        #endregion

        #region 死引用清理

        [Test]
        public void DeadSubscriber_IsRemovedFromRegistry_OnPublish()
        {
            var subscriber = NewGameObject("DeadSubscriber");
            EventModule.AddListener<TestEventArgs>(subscriber, e => Assert.Fail("已销毁订阅者不应收到事件"));

            Object.DestroyImmediate(subscriber);

            LogAssert.Expect(LogType.Warning, new Regex("已清理 1 个已销毁订阅者"));
            new TestEventArgs().Invoke(_module.gameObject);

            var key = AesirEventUtility.GetEventBindingKey<TestEventArgs>();
            Assert.IsFalse(_module.DynamicBindings.ContainsKey(key), "死绑定应已从注册表移除");
        }

        [Test]
        public void DeadSubscriber_SecondPublish_DoesNotWarnAgain()
        {
            var subscriber = NewGameObject("DeadSubscriber");
            EventModule.AddListener<TestEventArgs>(subscriber, e => { });

            Object.DestroyImmediate(subscriber);

            LogAssert.Expect(LogType.Warning, new Regex("已清理 1 个已销毁订阅者"));
            new TestEventArgs().Invoke(_module.gameObject);

            // 第二次发布：注册表已清理，不应再次警告
            new TestEventArgs().Invoke(_module.gameObject);

            var key = AesirEventUtility.GetEventBindingKey<TestEventArgs>();
            Assert.IsFalse(_module.DynamicBindings.ContainsKey(key));
        }

        class AttrDeadSubscriber : MonoBehaviour
        {
            [AesirListener(typeof(TestEventArgs))]
            void OnTest(TestEventArgs e) { }
        }

        [Test]
        public void DeadSubscriber_AttributeTrack_IsRemovedFromRegistry()
        {
            var subscriber = NewGameObject("AttrDead").AddComponent<AttrDeadSubscriber>();
            EventModule.AddListener(subscriber);

            var key = AesirEventUtility.GetEventBindingKey<TestEventArgs>();
            Assert.IsTrue(_module.AttributeBindings.ContainsKey(key), "前置：Attribute 轨应有绑定");

            Object.DestroyImmediate(subscriber.gameObject);

            LogAssert.Expect(LogType.Warning, new Regex("已清理 1 个已销毁订阅者"));
            new TestEventArgs().Invoke(_module.gameObject);

            Assert.IsFalse(_module.AttributeBindings.ContainsKey(key), "Attribute 轨死绑定应被清理");
        }

        #endregion

        #region 性能监控

        [Test]
        public void ExecutionMsLimit_WarnsWhenDispatchExceedsThreshold()
        {
            var serialized = new SerializedObject(_module);
            serialized.FindProperty("executionMsLimit").floatValue = 0.001f;
            serialized.ApplyModifiedProperties();

            var hits = 0;
            EventModule.AddListener<TestEventArgs>(_module.gameObject, e =>
            {
                hits++;
                Thread.Sleep(2);
            });

            LogAssert.Expect(LogType.Warning, new Regex("超过阈值"));
            new TestEventArgs().Invoke(_module.gameObject);

            Assert.AreEqual(1, hits, "超阈值告警不应影响正常分发");
        }

        [Test]
        public void ExecutionMsLimit_Zero_DisablesMonitoring()
        {
            var hits = 0;
            EventModule.AddListener<TestEventArgs>(_module.gameObject, e => hits++);

            // 默认 0 关闭监控：快速分发不应产生任何告警
            new TestEventArgs().Invoke(_module.gameObject);

            Assert.AreEqual(1, hits);
        }

        [Test]
        public void Dispatch_ThousandSubscribers_CompletesWithinGenerousBudget()
        {
            var go = NewGameObject("Perf");
            for (var i = 0; i < 1000; i++)
            {
                EventModule.AddListener<TestEventArgs>(go, e => { });
            }

            var stopwatch = Stopwatch.StartNew();
            new TestEventArgs().Invoke(_module.gameObject);
            stopwatch.Stop();

            // 信息性软门槛：实测约 0.6ms，门槛放大千倍仅作量级回归防线（CI 波动不脆）
            Assert.Less(stopwatch.Elapsed.TotalMilliseconds, 2000,
                $"1000 订阅者单次分发耗时不应出现量级回退（实际 {stopwatch.Elapsed.TotalMilliseconds:F2}ms）");
        }

        #endregion

        #region AesirEventArgsSO 资产化

        [Test]
        public void AesirEventArgsSO_Raise_DeliversConfiguredArgs_AssetAsSender()
        {
            var asset = ScriptableObject.CreateInstance<AesirEventArgsSO>();
            _createdAssets.Add(asset);

            var serialized = new SerializedObject(asset);
            serialized.FindProperty("eventArgs").managedReferenceValue = new TestEventArgs();
            serialized.ApplyModifiedProperties();

            AesirEventArgs received = null;
            EventModule.AddListener<TestEventArgs>(asset, e => received = e);

            asset.Raise();

            Assert.IsNotNull(received, "配置的参数应被投递");
            Assert.AreSame(asset, received.Sender, "发布者应为 SO 资产本身");
        }

        [Test]
        public void AesirEventArgsSO_RaiseWithoutArgs_LogsError()
        {
            var asset = ScriptableObject.CreateInstance<AesirEventArgsSO>();
            _createdAssets.Add(asset);

            LogAssert.Expect(LogType.Error, new Regex("未配置事件参数"));
            asset.Raise();
        }

        #endregion

        #region [AesirListener] AllowMultiple

        class MultiListenerSubscriber
        {
            public int Hits;

            [AesirListener(typeof(TestArgsA))]
            [AesirListener(typeof(TestArgsB))]
            void OnAny(AesirEventArgs e)
            {
                Hits++;
            }
        }

        [Test]
        public void AesirListener_AllowMultiple_OneMethodListensToBothEventTypes()
        {
            var subscriber = new MultiListenerSubscriber();
            EventModule.AddListener(subscriber);

            new TestArgsA().Invoke(_module.gameObject);
            new TestArgsB().Invoke(_module.gameObject);

            Assert.AreEqual(2, subscriber.Hits, "同一方法的多特性订阅应分别命中两种事件");

            EventModule.RemoveListener(subscriber);
        }

        #endregion

        #region 性能特征（反射优化锁定）

        [Test]
        public void GetEventBindingKey_Cached_SameStringInstancePerType()
        {
            var key1 = AesirEventUtility.GetEventBindingKey(new TestEventArgs());
            var key2 = AesirEventUtility.GetEventBindingKey(new TestEventArgs());
            var key3 = AesirEventUtility.GetEventBindingKey<TestEventArgs>();

            Assert.AreSame(key1, key2, "同类型应复用缓存的键实例（热路径零字符串分配）");
            Assert.AreSame(key2, key3, "实例版与泛型版应命中同一缓存");
            Assert.AreEqual(typeof(TestEventArgs).AssemblyQualifiedName, key1);
            Assert.AreNotSame(key1, AesirEventUtility.GetEventBindingKey<TestArgsA>(), "不同事件类型的键应各自缓存");
        }

        class PerfTarget
        {
            public long Counter;

            public void Handle(TestEventArgs e)
            {
                Counter++;
            }
        }

        [Test]
        public void CompiledDelegate_InvokeIsFasterThanReflectionInvoke()
        {
            const int iterations = 200_000;
            var target = new PerfTarget();
            var method = typeof(PerfTarget).GetMethod(nameof(PerfTarget.Handle),
                BindingFlags.Instance | BindingFlags.Public);
            var args = new object[] { new TestEventArgs() };

            // 走生产路径：StaticBindingInfo 注册期的表达式树编译委托
            var binding = new StaticBindingInfo(AesirEventUtility.GetEventBindingKey<TestEventArgs>(), method,
                target, SubscriberPriority.Medium);

            // 预热 JIT，避免首次编译/解析成本计入
            for (var i = 0; i < 1000; i++)
            {
                binding.Invoke(args);
                method.Invoke(target, args);
            }

            target.Counter = 0;
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                binding.Invoke(args);
            }

            stopwatch.Stop();
            var compiledMs = stopwatch.Elapsed.TotalMilliseconds;
            Assert.AreEqual(iterations, target.Counter, "编译委托应正确调用目标方法");

            stopwatch.Restart();
            for (var i = 0; i < iterations; i++)
            {
                method.Invoke(target, args);
            }

            stopwatch.Stop();
            var reflectionMs = stopwatch.Elapsed.TotalMilliseconds;
            Assert.AreEqual(iterations * 2, target.Counter, "反射调用应正确调用目标方法");
            Assert.Less(compiledMs, reflectionMs,
                $"表达式树编译委托应快于 MethodInfo.Invoke（compiled={compiledMs:F2}ms, " +
                $"reflection={reflectionMs:F2}ms）");
        }

        class InferredSubscriber
        {
            public int Hits;

            [AesirListener]
            void OnTest(TestEventArgs e)
            {
                Hits++;
            }
        }

        [Test]
        public void Bind_InfersEventType_FromFirstParameter()
        {
            var subscriber = new InferredSubscriber();
            EventModule.AddListener(subscriber);

            new TestEventArgs().Invoke(_module.gameObject);

            Assert.AreEqual(1, subscriber.Hits, "未显式指定类型时应从方法首参推断事件类型");

            EventModule.RemoveListener(subscriber);
        }

        #endregion

        #region 重入分发（回调内同步发布事件）

        [Test]
        public void ReentrantDispatch_DifferentEventType_OuterRemainingSubscribersReceiveOuterArgs()
        {
            var publisher = NewGameObject("ReentrantPublisher");
            var innerHits = 0;
            var outerHits = 0;
            EventModule.AddListener<TestArgsA>(publisher, e => innerHits++);

            // 先注册者回调内发布异型事件；后注册者必须仍收到外层 TestEventArgs
            EventModule.AddListener<TestEventArgs>(publisher, e =>
            {
                new TestArgsA().Invoke(_module.gameObject);
            });
            EventModule.AddListener<TestEventArgs>(_module.gameObject, e => outerHits++);

            new TestEventArgs().Invoke(_module.gameObject);

            Assert.AreEqual(1, innerHits, "内层事件应被正常分发");
            Assert.AreEqual(1, outerHits, "重入覆写共享参数数组时此处为 0（外层参数被内层替换）");
        }

        [Test]
        public void ReentrantDispatch_SameEventType_OuterSenderPreserved()
        {
            var publisher = NewGameObject("SameTypePublisher");
            var receivedSenders = new List<object>();
            var depth = 0;
            EventModule.AddListener<TestEventArgs>(publisher, e =>
            {
                depth++;
                if (depth == 1)
                {
                    // 内层再发一次同型事件（不同发布者），depth 守卫防无限递归
                    new TestEventArgs().Invoke(_module.gameObject);
                }
            });
            EventModule.AddListener<TestEventArgs>(_module.gameObject, e => receivedSenders.Add(e.Sender));

            var outerSender = NewGameObject("OuterSender");
            new TestEventArgs().Invoke(outerSender);

            Assert.AreEqual(2, receivedSenders.Count, "外层与内层各命中一次");
            Assert.AreSame(_module.gameObject, receivedSenders[0], "首次命中来自内层分发");
            Assert.AreSame(outerSender, receivedSenders[1], "外层继续分发的订阅者应收到外层事件参数（重入覆写时此处会变成内层发布者）");
        }

        [Test]
        public void ReentrantDispatch_ThreeLevels_EachLevelReceivesOwnArgs()
        {
            var go = NewGameObject("ThreeLevel");
            var tail = NewGameObject("Tail");
            var received = new List<AesirEventArgs>();
            var tailReceived = new List<AesirEventArgs>();
            var argsOuter = new TestEventArgs();
            var argsMid = new TestArgsA();
            var argsInner = new TestArgsB();

            EventModule.AddListener<TestArgsB>(go, e => received.Add(e));
            EventModule.AddListener<TestArgsA>(go, e =>
            {
                received.Add(e);
                argsInner.Invoke(_module.gameObject);
            });
            EventModule.AddListener<TestEventArgs>(go, e =>
            {
                received.Add(e);
                argsMid.Invoke(_module.gameObject);
            });

            // 每层各挂一个"尾随"订阅者：重入覆写时尾随者收到的会是错误层的参数
            EventModule.AddListener<TestArgsA>(tail, e => tailReceived.Add(e));
            EventModule.AddListener<TestEventArgs>(tail, e => tailReceived.Add(e));

            argsOuter.Invoke(_module.gameObject);

            Assert.AreEqual(3, received.Count);
            Assert.AreSame(argsOuter, received[0]);
            Assert.AreSame(argsMid, received[1]);
            Assert.AreSame(argsInner, received[2]);
            Assert.AreEqual(2, tailReceived.Count, "两个尾随订阅者都应被命中");
            // 尾随订阅者的命中顺序：内层分发（argsMid）先于外层循环继续（argsOuter）
            Assert.AreSame(argsMid, tailReceived[0]);
            Assert.AreSame(argsOuter, tailReceived[1]);
        }

        #endregion

        #region 快照语义（分发中退订/注册）

        [Test]
        public void UnsubscribeDuringDispatch_SnapshotSemantics_LaterSubscribersStillReceive()
        {
            var go = NewGameObject("Unsub");
            var first = 0;
            var second = 0;
            var third = 0;
            var secondHandle = default(AutoRemoveListenerHandle);
            EventModule.AddListener<TestEventArgs>(go, e =>
            {
                first++;
                secondHandle.Dispose();
            });
            secondHandle = EventModule.AddListener<TestEventArgs>(go, e => second++);
            EventModule.AddListener<TestEventArgs>(go, e => third++);

            new TestEventArgs().Invoke(_module.gameObject);

            Assert.AreEqual(1, first);
            Assert.AreEqual(1, second, "快照语义（对齐原生多播委托）：本趟已开始，被移除的监听仍执行一次");
            Assert.AreEqual(1, third, "退订后续订阅者不应导致其被跳过或迭代越界");

            // 第二次分发：退订已生效
            new TestEventArgs().Invoke(_module.gameObject);
            Assert.AreEqual(2, first);
            Assert.AreEqual(1, second, "第二趟起退订生效");
            Assert.AreEqual(2, third);
        }

        [Test]
        public void RegisterDuringDispatch_NewSubscriberNotCalledThisRound()
        {
            var go = NewGameObject("Reg");
            var lateHits = 0;
            EventModule.AddListener<TestEventArgs>(go, e =>
            {
                EventModule.AddListener<TestEventArgs>(go, _ => lateHits++);
            });

            new TestEventArgs().Invoke(_module.gameObject);
            Assert.AreEqual(0, lateHits, "分发中新增的订阅者本趟不应生效（快照语义）");

            new TestEventArgs().Invoke(_module.gameObject);
            Assert.AreEqual(1, lateHits, "下一趟起新增订阅者正常生效");
        }

        #endregion

        #region 稳定排序（Priority 主键 + 注册序号次键）

        [Test]
        public void Dispatch_SamePriority_ExecutesInRegistrationOrder()
        {
            var go = NewGameObject("Order");
            var order = new List<int>();
            EventModule.AddListener<TestEventArgs>(go, e => order.Add(1));
            EventModule.AddListener<TestEventArgs>(go, e => order.Add(2));
            EventModule.AddListener<TestEventArgs>(go, e => order.Add(3));

            new TestEventArgs().Invoke(_module.gameObject);

            Assert.AreEqual(new[] { 1, 2, 3 }, order.ToArray(), "同优先级应按注册顺序稳定执行");
        }

        class CrossTrackAttrSubscriber
        {
            readonly List<string> _calls;

            public CrossTrackAttrSubscriber(List<string> calls) => _calls = calls;

            [AesirListener(typeof(TestEventArgs), SubscriberPriority.High)]
            void OnHigh(TestEventArgs e) => _calls.Add("attr-high");

            [AesirListener(typeof(TestEventArgs), SubscriberPriority.Last)]
            void OnLast(TestEventArgs e) => _calls.Add("attr-last");
        }

        [Test]
        public void Dispatch_CrossTrack_FullPriorityOrder()
        {
            var calls = new List<string>();
            var attrSubscriber = new CrossTrackAttrSubscriber(calls);
            EventModule.AddListener(attrSubscriber);

            var go = NewGameObject("Dyn");
            EventModule.AddListener<TestEventArgs>(go, e => calls.Add("dyn-first"), SubscriberPriority.First);
            EventModule.AddListener<TestEventArgs>(go, e => calls.Add("dyn-medium"),
                SubscriberPriority.Medium);

            new TestEventArgs().Invoke(_module.gameObject);

            Assert.AreEqual(new[] { "dyn-first", "attr-high", "dyn-medium", "attr-last" }, calls.ToArray(),
                "Attribute+Dynamic 合并后应按 First→High→Medium→Last 全序执行");

            EventModule.RemoveListener(attrSubscriber);
        }

        #endregion
    }
}
