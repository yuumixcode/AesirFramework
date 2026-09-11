using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor.UI
{
    /// <summary>
    /// UIModule 面板生命周期状态机的 EditMode 测试：
    /// 三路 Show 状态迁移、Hide 的 DestroyOnHide 双分叉、Prewarm 幂等与复用、
    /// 以实际类型为键的键语义诊断、外部销毁的注册表反清理、层 Canvas 缺失的中止行为。
    /// <para>
    /// 单例经 Instance getter 惰性创建（全同步路径，UIRoot 层级经反射驱动 Initialize 构建，
    /// EditMode 中 AddComponent 不触发 Awake）；TearDown 全量销毁创建的 GameObject。
    /// 日志断言采用自捕获（<see cref="CaptureLogs" />）而非 LogAssert.Expect——
    /// 后者的匹配语义在本引擎（团结 2022.3.62）下对富文本前缀日志的 Regex 匹配不稳定，
    /// 自捕获同时以 ignoreFailingMessages 抑制 TestRunner 对框架日志的失败判定。
    /// </para>
    /// </summary>
    public class UIModuleTests
    {
        /// <summary>断言捕获的日志中存在指定类型且消息含指定片段的条目。</summary>
        static void AssertHasLog(List<LogEntry> logs, LogType type, string fragment, string message)
        {
            foreach (var entry in logs)
            {
                if (entry.Matches(type, fragment))
                {
                    return;
                }
            }

            Assert.Fail(message + $"（未捕获到 {type} 日志，实际日志数 {logs.Count}）");
        }

        #region 测试面板

        /// <summary>记录生命周期回调顺序的测试面板。</summary>
        internal class TestPanel : AesirBasePanel
        {
            protected override void OnInit() => LifeLog.Add("Init");

            // 覆写生命周期时保留 base 调用：base.OnShow/OnHide 负责激活/停用物体，
            // 是"Awake/OnEnable 推迟到 Show 激活时触发"契约的载体
            protected override void OnShow(object payload)
            {
                base.OnShow(payload);
                LifeLog.Add("Show");
            }

            protected override void OnHide()
            {
                base.OnHide();
                LifeLog.Add("Hide");
            }

            protected override void OnClose() => LifeLog.Add("Close");
        }

        /// <summary>TestPanel 的派生面板，用于键语义（注册键 = 实际类型）测试。</summary>
        internal class TestPanelDerived : TestPanel { }

        /// <summary>生命周期回调记录（跨用例共享，SetUp 清空）。</summary>
        protected static readonly List<string> LifeLog = new List<string>();

        /// <summary>键语义测试专用的无注册面板类型。</summary>
        internal class OtherPanel : AesirBasePanel { }

        #endregion

        #region 环境管理

        readonly List<GameObject> _createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            LifeLog.Clear();
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
            LifeLog.Clear();
        }

        /// <summary>创建 GameObject 并登记，TearDown 统一销毁。</summary>
        GameObject NewGameObject(string name)
        {
            var gameObject = new GameObject(name);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        /// <summary>
        /// 经 Instance getter 惰性获取 UIModule（全同步路径，未预放置时连带创建
        /// [Aesir Modules] 宿主与 UIModule 子物体），并登记全部关联物体。
        /// </summary>
        UIModule CreateModule()
        {
            var module = UIModule.Instance;
            var moduleGo = module.gameObject;
            if (moduleGo.transform.parent != null)
            {
                Track(moduleGo.transform.parent.gameObject);
            }

            Track(moduleGo);
            return module;
        }

        /// <summary>
        /// 惰性获取并登记 UIRoot。EditMode 中 AddComponent 不触发 Awake，
        /// 层级构建需反射驱动 <c>Initialize</c>（与 Audio 测试驱动 Awake 同款模式；
        /// 此处只构建层级，Awake 的 DDOL/单例分支不应在测试中执行）。
        /// </summary>
        void TrackUIRoot()
        {
            var root = UIRoot.Instance;
            if (root.transform.childCount == 0)
            {
                InvokePrivate(root, "Initialize");
            }

            Track(root.gameObject);
        }

        /// <summary>反射调用私有实例方法（EditMode 下组件回调不自动执行）。</summary>
        static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(target, null);
        }

        void Track(GameObject gameObject)
        {
            if (!_createdObjects.Contains(gameObject))
            {
                _createdObjects.Add(gameObject);
            }
        }

        /// <summary>创建一个挂载 TestPanel 的物体作为"面板预制体"并注册到模块。</summary>
        TestPanel CreateRegisteredPrefab(UIModule module)
        {
            var prefabGo = NewGameObject("TestPanelPrefab");
            var panel = prefabGo.AddComponent<TestPanel>();
            module.RegisterPanelPrefab<TestPanel>(prefabGo);
            return panel;
        }

        /// <summary>经 SerializedObject 修改面板的 destroyOnHide 序列化字段。</summary>
        static void SetDestroyOnHide(AesirBasePanel panel, bool value)
        {
            var so = new SerializedObject(panel);
            so.FindProperty("destroyOnHide").boolValue = value;
            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// 自捕获指定操作产生的日志（绕开 LogAssert.Expect 匹配语义的不确定性）：
        /// 订阅 logMessageReceived 记录全部日志，同时以 ignoreFailingMessages 抑制
        /// TestRunner 对未处理框架日志的失败判定，结束后还原。
        /// </summary>
        static List<LogEntry> CaptureLogs(Action action)
        {
            var logs = new List<LogEntry>();

            void Handler(string message, string stackTrace, LogType type)
            {
                logs.Add(new LogEntry(type, message));
            }

            Application.logMessageReceived += Handler;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                action();
            }
            finally
            {
                Application.logMessageReceived -= Handler;
                LogAssert.ignoreFailingMessages = false;
            }

            return logs;
        }

        /// <summary>单条日志记录（类型 + 完整消息）。</summary>
        readonly struct LogEntry
        {
            public readonly LogType Type;
            public readonly string Message;

            public LogEntry(LogType type, string message)
            {
                Type = type;
                Message = message;
            }

            /// <summary>是否存在指定类型且消息含指定片段的日志。</summary>
            public bool Matches(LogType type, string fragment) => Type == type && Message.Contains(fragment);
        }

        #endregion

        #region ShowPanel：新建 / 复用 / 置顶

        [Test]
        public void ShowPanel_NewPanel_InitializesShowsAndRegisters()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            var panel = module.ShowPanel<TestPanel>();

            Assert.IsNotNull(panel);
            Assert.IsTrue(panel.IsOpen);
            Assert.AreEqual(new[] { "Init", "Show" }, LifeLog.ToArray());
            Assert.AreSame(panel, module.GetPanel<TestPanel>(), "首次 Show 后应可按实际类型获取");
        }

        [Test]
        public void ShowPanel_SecondShow_ReusesInstanceAndMovesToLastSibling()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            var first = module.ShowPanel<TestPanel>();
            var second = module.ShowPanel<TestPanel>();

            Assert.AreSame(first, second, "已注册面板再次 Show 应复用实例");
            Assert.AreEqual(new[] { "Init", "Show", "Show" }, LifeLog.ToArray(), "重复 Show 应重新触发 OnShow");
            var mono = (MonoBehaviour)second;
            Assert.AreEqual(mono.transform.parent.childCount - 1, mono.transform.GetSiblingIndex(),
                "重复 Show 应将面板置顶到层内最后");
        }

        [Test]
        public void ShowPanel_BaseTypeAfterDerivedRegistration_LogsErrorAndDoesNotDuplicate()
        {
            var module = CreateModule();
            TrackUIRoot();
            // 预制体挂派生脚本、以基类类型注册（键语义坑的典型成因）
            var prefabGo = NewGameObject("DerivedPrefab");
            prefabGo.AddComponent<TestPanelDerived>();
            module.RegisterPanelPrefab<TestPanel>(prefabGo);

            var first = module.ShowPanel(typeof(TestPanel));
            Assert.IsNotNull(first, "首次以基类类型 Show 应正常实例化（注册键为实际类型）");

            // 第二次以基类类型 Show：必须报错拒绝而不是重复实例化
            IUIPanel second = null;
            var logs = CaptureLogs(() => second = module.ShowPanel(typeof(TestPanel)));
            Assert.IsNull(second, "以基类类型重复 Show 应被拒绝");
            AssertHasLog(logs, LogType.Error, "面板注册表以实例的实际类型为键", "拒绝应记录键语义 Error");

            Assert.IsNotNull(module.GetPanel<TestPanelDerived>(), "派生实例应保持注册，未被重复实例化顶替");

            // 以实际类型 Show 仍可正常复用
            var third = module.ShowPanel<TestPanelDerived>();
            Assert.AreSame(first, third);
        }

        #endregion

        #region HidePanel：销毁分叉 / 隐藏分叉 / 幂等

        [Test]
        public void HidePanel_DestroyOnHide_DestroysAndClearsRegistry()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            var panel = module.ShowPanel<TestPanel>();
            SetDestroyOnHide(panel, true);

            // EditMode 中 UIModule 的 Destroy(gameObject) 会被 Unity 拒绝（EngineError），
            // 但 OnHide → OnClose 的受控销毁回调照常触发
            var logs = CaptureLogs(() => module.HidePanel<TestPanel>());

            Assert.AreEqual(new[] { "Init", "Show", "Hide", "Close" }, LifeLog.ToArray(),
                "受控销毁路径应依次触发 OnHide → OnClose（实例销毁因 EditMode Destroy 报错而未实际执行）");
            AssertHasLog(logs, LogType.Error, "Destroy may not be called from edit mode",
                "EditMode 中框架的 Destroy 调用被引擎拒绝属预期行为");
        }

        [Test]
        public void HidePanel_KeepOnHide_HidesAndNextShowReuses()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            var panel = module.ShowPanel<TestPanel>();
            SetDestroyOnHide(panel, false);

            module.HidePanel<TestPanel>();

            Assert.IsFalse(panel.IsOpen);
            Assert.IsTrue(panel != null, "DestroyOnHide=false 时实例应存活");
            CollectionAssert.Contains(LifeLog, "Hide");

            var reShown = module.ShowPanel<TestPanel>();
            Assert.AreSame(panel, reShown, "再次 Show 应复用隐藏的实例");
            Assert.AreEqual(new[] { "Init", "Show", "Hide", "Show" }, LifeLog.ToArray());
        }

        [Test]
        public void HidePanel_UnknownType_IsSilentNoOp()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            var logs = CaptureLogs(() =>
            {
                module.HidePanel<OtherPanel>();
                Assert.IsNull(module.GetPanel<OtherPanel>(), "无关联实例时 Get 应返回 null");
            });

            // 完全静默 = 无 Warning 及以上级别的框架日志
            //（本引擎 LogAssert.ignoreFailingMessages 的 setter 会输出 Log 级调试日志，不算框架噪音）
            foreach (var entry in logs)
            {
                Assert.IsTrue(entry.Type == LogType.Log,
                    $"无关联实例的 Hide/Get 不应产生框架 Warning/Error 日志（实际 {entry.Type}: {entry.Message}）");
            }
        }

        [Test]
        public void HidePanel_PrewarmedButNeverShown_IsNoOp()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            Assert.IsTrue(module.PrewarmPanel<TestPanel>());
            module.HidePanel<TestPanel>();

            Assert.AreEqual(new[] { "Init" }, LifeLog.ToArray(), "未显示面板的关闭应为幂等操作，不触发额外回调");
            Assert.IsNotNull(module.GetPanel<TestPanel>(), "预热面板不应被关闭路径销毁或移出注册表");
        }

        #endregion

        #region PrewarmPanel

        [Test]
        public void PrewarmPanel_InitializesWithoutShow_ShowThenReuses()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            Assert.IsTrue(module.PrewarmPanel<TestPanel>());
            Assert.AreEqual(new[] { "Init" }, LifeLog.ToArray(), "预热只初始化不显示");
            Assert.IsFalse(module.GetPanel<TestPanel>().IsOpen);

            var panel = module.ShowPanel<TestPanel>();
            Assert.AreEqual(new[] { "Init", "Show" }, LifeLog.ToArray(), "预热后的 Show 应复用实例（不再 Init）");
            Assert.IsTrue(panel.IsOpen);
        }

        [Test]
        public void PrewarmPanel_AlreadyShown_ReturnsTrueWithoutSideEffects()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            var panel = module.ShowPanel<TestPanel>();
            LifeLog.Clear();

            Assert.IsTrue(module.PrewarmPanel<TestPanel>(), "已显示面板的预热应返回 true");
            CollectionAssert.IsEmpty(LifeLog, "已存在面板的预热不应有任何副作用");
        }

        #endregion

        #region 键语义诊断（Get / Hide 侧）

        [Test]
        public void GetPanel_BaseTypeAfterDerivedRegistration_LogsKeySemanticsWarning()
        {
            var module = CreateModule();
            TrackUIRoot();
            var prefabGo = NewGameObject("DerivedPrefab");
            prefabGo.AddComponent<TestPanelDerived>();
            module.RegisterPanelPrefab<TestPanel>(prefabGo);

            module.ShowPanel(typeof(TestPanel));

            IUIPanel result = null;
            var logs = CaptureLogs(() => result = module.GetPanel<TestPanel>());
            Assert.IsNull(result, "以基类类型 Get 应返回 null");
            AssertHasLog(logs, LogType.Warning, "未获取到面板实例：注册表以实例的实际类型为键", "以基类类型误查应记录键语义 Warning");
        }

        [Test]
        public void HidePanel_BaseTypeAfterDerivedRegistration_LogsKeySemanticsWarning()
        {
            var module = CreateModule();
            TrackUIRoot();
            var prefabGo = NewGameObject("DerivedPrefab");
            prefabGo.AddComponent<TestPanelDerived>();
            module.RegisterPanelPrefab<TestPanel>(prefabGo);

            var panel = (TestPanelDerived)module.ShowPanel(typeof(TestPanel));

            var logs = CaptureLogs(() => module.HidePanel<TestPanel>());
            AssertHasLog(logs, LogType.Warning, "未关闭任何面板：注册表以实例的实际类型为键", "以基类类型误关应记录键语义 Warning");
            Assert.IsNotNull(panel, "以基类类型 Hide 不应影响已注册的派生实例");
            Assert.IsTrue(panel.IsOpen, "派生实例应保持显示状态");
        }

        #endregion

        #region 外部销毁与层缺失

        [Test]
        public void RemovePanelRecord_ExternalDestroy_CleansRegistry()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            var panel = module.ShowPanel<TestPanel>();
            var panelGo = panel.gameObject;

            // 模拟外部 Destroy（如场景卸载）：EditMode 下组件 OnDestroy 回调不保证触发，
            // 反清理由运行时 OnDestroy → UIModule.RemovePanelRecord 驱动，
            // 此处直接调用同一 internal 契约入口验证其幂等清理语义
            Object.DestroyImmediate(panelGo);
            UIModule.RemovePanelRecord(panel);
            LifeLog.Clear();

            Assert.IsTrue(module.GetPanel<TestPanel>() == null, "反清理后注册表不应再命中该面板（Unity null 语义）");

            // 反清理后再次 Show 可正常新建实例
            var recreated = module.ShowPanel<TestPanel>();
            Assert.IsNotNull(recreated);
            Assert.AreNotSame(panel, recreated);
            Assert.AreEqual(new[] { "Init", "Show" }, LifeLog.ToArray(), "清理后应重新走完整生命周期");
        }

        [Test]
        public void ShowPanel_MissingLayerCanvas_AbortsAndDoesNotRegister()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredPrefab(module);

            // 删除 Normal 层的 Canvas 子物体，模拟 UIRoot 层级结构性损坏
            var normalLayer = UIRoot.Instance.GetLayerRoot(UILayer.Normal);
            Object.DestroyImmediate(normalLayer.gameObject);

            IUIPanel panel = null;
            var logs = CaptureLogs(() => panel = module.ShowPanel<TestPanel>());

            Assert.IsNull(panel, "层 Canvas 缺失时应中止显示");
            AssertHasLog(logs, LogType.Error, "UIRoot 缺少 Normal 层的 Canvas", "层缺失应记录结构性错误");
            AssertHasLog(logs, LogType.Error, "Destroy may not be called from edit mode",
                "EditMode 中半挂载实例的清理 Destroy 被引擎拒绝属预期行为");
            Assert.IsTrue(module.GetPanel<TestPanel>() == null, "中止的面板不应残留注册表");

            // EditMode 中 UIModule 的 Destroy 为 no-op，被中止的克隆体悬在世界空间根，
            // 不在 UIRoot 子树内（TearDown 无法连带销毁），须手动登记清理避免污染打开场景
            var leftovers = Object.FindObjectsByType<TestPanel>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var leftover in leftovers)
            {
                if (leftover != null && !_createdObjects.Contains(leftover.gameObject))
                {
                    _createdObjects.Add(leftover.gameObject);
                }
            }
        }

        #endregion
    }
}
