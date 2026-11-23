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
    /// 性能监控（executionMsLimit）、AesirEventArgsSO 资产化与按对象退订 API。
    /// <para>
    /// 测试自建 EventModule 实例，TearDown 全量销毁创建的 GameObject / 资产 / 附加场景，
    /// 避免 refresh 时污染打开的场景。
    /// </para>
    /// </summary>
    public class EventModuleTests
    {
        #region 非泛型订阅（按参数实例推断类型）

        [Test]
        public void AddListener_NonGeneric_RoutesByRuntimeType()
        {
            var subscriber = new object();
            var hits = 0;
            var handle = EventModule.AddListener(subscriber, new TestArgsA(), e => hits++);

            new TestArgsA().Invoke(subscriber);
            new TestArgsB().Invoke(subscriber);

            Assert.AreEqual(1, hits, "非泛型订阅应按参数实例的运行期类型路由，不收其他类型");
            handle.Dispose();
        }

        #endregion

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

        /// <summary>EventModule 的私有静态单例字段（测试显式接管，避免依赖 Instance 恰好回落到本用例实例）。</summary>
        static readonly FieldInfo ModuleInstanceField =
            typeof(EventModule).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);

        [SetUp]
        public void SetUp()
        {
            _module = NewGameObject("TestEventModule").AddComponent<EventModule>();

            // EditMode 下 AddComponent 不触发 Awake，静态单例不会被赋值；而用例经静态门面
            // （EventModule.AddListener / AesirEventArgs.Invoke）操作、又断言实例字段（Bindings 等），
            // 若宿主工程里存在预放置的 EventModule，Instance 会解析到别人 —— 显式接管以消除该环境耦合。
            ModuleInstanceField.SetValue(null, _module);
            Assert.AreSame(_module, EventModule.Instance, "测试应独占静态单例");
        }

        [TearDown]
        public void TearDown()
        {
            ModuleInstanceField.SetValue(null, null);

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

        #region 单例查找与静态重置（07 号报告修复项的行为锁定）

        [Test]
        public void Instance_FindsInactivePreplacedModule()
        {
            // Unity 不对未激活物体调用 Awake，"先禁用、用到时再启用"式预放置的模块若被 Exclude 漏掉，
            // 会被判为不存在而重复创建（Inspector 配置静默失效）。Instance 必须含未激活对象查找。
            ModuleInstanceField.SetValue(null, null);

            // SetUp 创建的模块此刻仍活在场景里，查找会先命中它（命中顺序不确定）——先销毁，
            // 使本用例场景内只剩待查的未激活预放置实例，断言才真正指向"含未激活对象查找"这一契约。
            Object.DestroyImmediate(_module);
            var inactive = NewGameObject("InactiveEventModule");
            inactive.SetActive(false);
            var preplaced = inactive.AddComponent<EventModule>();

            Assert.AreSame(preplaced, EventModule.Instance, "未激活的预放置实例应被 Instance 找到而非另建一个");
        }

        [Test]
        public void DontDestroyOnLoad_DefaultsToTrue()
        {
            var probe = NewGameObject("DdolProbe").AddComponent<EventModule>();
            var so = new SerializedObject(probe);
            var prop = so.FindProperty("dontDestroyOnLoad");

            Assert.IsNotNull(prop, "dontDestroyOnLoad 字段应存在且可序列化");
            Assert.IsTrue(prop.boolValue, "默认 true：预放置根物体模块随场景卸载不再消失（跨场景持久）");
        }

        [Test]
        public void ResetStatics_ClearsInstance()
        {
            var reset = typeof(EventModule).GetMethod("ResetStatics",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(reset, "非泛型单例按铁律在类内声明 RuntimeInitializeOnLoadMethod 自重置");

            reset.Invoke(null, null);

            Assert.IsNull(ModuleInstanceField.GetValue(null), "静态重置后 _instance 应为 null");
        }

        #endregion

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

            Assert.IsFalse(_module.Bindings.ContainsKey(typeof(TestEventArgs)), "死绑定应已从注册表移除");
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

            Assert.IsFalse(_module.Bindings.ContainsKey(typeof(TestEventArgs)));
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
        public void ReentrantDispatch_SameArgsInstance_RejectedFailFast()
        {
            // 分发约定：同一参数实例在分发期间不得被再次发布——低概率场景以 fail-fast 拒绝
            // 替代逐订阅者 Sender 校正兜底。跨实例的事件链安全由本区域其余用例锁定；
            // 拒绝后实例不被污染（下一趟可正常发布）由 ReusedEventArgs_ResetCancellationEachTopLevelDispatch 锁定
            var publisher = NewGameObject("SharedArgsPublisher");
            var shadowSender = NewGameObject("ShadowSender");
            var received = new List<object>();
            var shared = new TestEventArgs();

            EventModule.AddListener<TestEventArgs>(publisher, e => e.Invoke(shadowSender));
            EventModule.AddListener<TestEventArgs>(_module.gameObject, e => received.Add(e.Sender));

            var outerSender = NewGameObject("OuterSender");
            LogAssert.Expect(LogType.Error, new Regex("禁止重入发布同一实例"));
            shared.Invoke(outerSender);

            Assert.AreEqual(1, received.Count, "同实例重入应被拒绝，只有外层分发命中");
            Assert.AreSame(outerSender, received[0], "拒绝后外层 Sender 不应被内层发布者覆写");
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

        [Test]
        public void Dispatch_FullPriorityOrder()
        {
            var calls = new List<string>();
            var go = NewGameObject("PriorityOrder");

            var h1 = EventModule.AddListener<TestEventArgs>(go, e => calls.Add("first"),
                SubscriberPriority.First);
            var h2 = EventModule.AddListener<TestEventArgs>(go, e => calls.Add("high"),
                SubscriberPriority.High);
            var h3 = EventModule.AddListener<TestEventArgs>(go, e => calls.Add("medium"),
                SubscriberPriority.Medium);
            var h4 = EventModule.AddListener<TestEventArgs>(go, e => calls.Add("last"),
                SubscriberPriority.Last);

            new TestEventArgs().Invoke(_module.gameObject);

            Assert.AreEqual(new[] { "first", "high", "medium", "last" }, calls.ToArray(),
                "应按 First→High→Medium→Last 全序执行");

            h1.Dispose();
            h2.Dispose();
            h3.Dispose();
            h4.Dispose();
        }

        [Test]
        public void Dispatch_PriorityTiers_RegisteredOutOfOrder_StillSorted()
        {
            // 注册期有序插入的插入路径（顺序注册只走追加路径，由上一用例覆盖）：
            // 乱序注册 + 同档交错，仍须全序执行且同档按注册顺序
            var go = NewGameObject("OutOfOrder");
            var calls = new List<string>();
            EventModule.AddListener<TestEventArgs>(go, e => calls.Add("last"), SubscriberPriority.Last);
            EventModule.AddListener<TestEventArgs>(go, e => calls.Add("medium1"), SubscriberPriority.Medium);
            EventModule.AddListener<TestEventArgs>(go, e => calls.Add("first"), SubscriberPriority.First);
            EventModule.AddListener<TestEventArgs>(go, e => calls.Add("high"), SubscriberPriority.High);
            EventModule.AddListener<TestEventArgs>(go, e => calls.Add("medium2"), SubscriberPriority.Medium);

            new TestEventArgs().Invoke(go);

            Assert.AreEqual(new[] { "first", "high", "medium1", "medium2", "last" }, calls.ToArray(),
                "乱序注册仍应按优先级全序执行，同档按注册顺序");
        }

        #endregion

        #region 按对象退订（无句柄外部解绑）

        [Test]
        public void RemoveListener_OfEventType_RemovesOnlyThatTypeForSubscriber()
        {
            var go = NewGameObject("RemoveByType");
            var aHits = 0;
            var bHits = 0;
            EventModule.AddListener<TestArgsA>(go, e => aHits++);
            EventModule.AddListener<TestArgsB>(go, e => bHits++);

            // 退订方无句柄：只退 TestArgsA，TestArgsB 应保留
            EventModule.RemoveListener<TestArgsA>(go);

            new TestArgsA().Invoke(_module.gameObject);
            new TestArgsB().Invoke(_module.gameObject);

            Assert.AreEqual(0, aHits, "指定事件类型的订阅应被移除");
            Assert.AreEqual(1, bHits, "其它事件类型的订阅不受影响");
        }

        [Test]
        public void RemoveListener_OfSubscriber_RemovesAllItsSubscriptions()
        {
            var go = NewGameObject("RemoveAll");
            var hits = 0;
            var otherHits = 0;
            EventModule.AddListener<TestArgsA>(go, e => hits++);
            EventModule.AddListener<TestArgsB>(go, e => hits++);
            EventModule.AddListener<TestArgsA>(_module.gameObject, e => otherHits++);

            // 退订方无句柄：批量退该对象全部订阅
            EventModule.RemoveListener(go);

            new TestArgsA().Invoke(_module.gameObject);
            new TestArgsB().Invoke(_module.gameObject);

            Assert.AreEqual(0, hits, "该对象的全部订阅应被移除");
            Assert.AreEqual(1, otherHits, "其它对象的订阅不受影响");
        }

        #endregion
    }
}
