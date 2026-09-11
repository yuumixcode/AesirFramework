using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
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
        public void WithFilter_NullFilter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TestEventArgs().WithFilter(null));
            Assert.Throws<ArgumentNullException>(() => new TestEventArgs().WithFilters(null));
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
    }
}
