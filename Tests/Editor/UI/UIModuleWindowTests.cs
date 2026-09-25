using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor.UI
{
    /// <summary>
    /// UIModule 窗口（Canvas 根 UI）生命周期状态机与蒙版机制的 EditMode 测试：
    /// 挂载与接线（UIRoot 直下 / 相机 / sortingOrder / UI 层递归）、Close 双分叉、Prewarm 幂等、
    /// 键语义诊断、跨契约互斥（Panel ↔ Window 入口）、根缺 Canvas 中止、外部销毁反清理；
    /// 蒙版：单遮重算与同序 tie、叠遮独立跟随、closeOnMaskClick 经 Mask Button、无 Mask 子物体无操作。
    /// <para>
    /// 环境管理与日志断言沿用 <see cref="UIModuleTests" /> 的同款模式（自捕获日志 / 反射驱动层级构建）。
    /// </para>
    /// </summary>
    public class UIModuleWindowTests
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

        #region 测试窗口

        /// <summary>记录生命周期回调顺序的测试窗口。</summary>
        internal class TestWindow : AesirBaseWindow
        {
            protected override void OnInit() => LifeLog.Add("Init");

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

        /// <summary>TestWindow 的派生窗口，用于键语义（注册键 = 实际类型）测试。</summary>
        internal class TestWindowDerived : TestWindow { }

        /// <summary>键语义测试专用的无注册窗口类型。</summary>
        internal class OtherWindow : AesirBaseWindow { }

        /// <summary>蒙版叠加测试的第二窗口类型（与 TestWindow 分桶注册）。</summary>
        internal class WindowB : AesirBaseWindow { }

        /// <summary>记录 OnInit 时物体激活状态的测试窗口（生命周期推迟契约）。</summary>
        internal class LifecycleStateWindow : AesirBaseWindow
        {
            public static bool ActiveSelfDuringInit;

            protected override void OnInit() => ActiveSelfDuringInit = gameObject.activeSelf;
        }

        /// <summary>键语义/互斥测试专用的面板类型（非窗口）。</summary>
        internal class PlainPanel : AesirBasePanel { }

        /// <summary>生命周期回调记录（跨用例共享，SetUp 清空）。</summary>
        protected static readonly List<string> LifeLog = new List<string>();

        #endregion

        #region 环境管理

        readonly List<GameObject> _createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            LifeLog.Clear();
            LifecycleStateWindow.ActiveSelfDuringInit = false;
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
        /// 经 Instance getter 惰性获取 UIModule（与 <see cref="UIModuleTests" /> 同款），
        /// 并登记 [Aesir Modules] 宿主与 UIModule 子物体。
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
        /// 层级构建需反射驱动 <c>Initialize</c>（与 <see cref="UIModuleTests" /> 同款模式）。
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

        /// <summary>按 Canvas 根约定创建窗口"预制体"并注册到模块（可声明排序与是否带蒙版子物体）。</summary>
        TestWindow CreateRegisteredWindowPrefab(UIModule module,
            int sortingOrder = 500,
            bool withMask = true,
            bool withMaskButton = true)
        {
            var prefabGo = NewGameObject("TestWindowPrefab");
            prefabGo.AddComponent<Canvas>();
            prefabGo.AddComponent<CanvasScaler>();
            prefabGo.AddComponent<GraphicRaycaster>();
            var window = prefabGo.AddComponent<TestWindow>();
            if (sortingOrder != 500)
            {
                SetSortingOrder(window, sortingOrder);
            }

            if (withMask)
            {
                AddMaskChild(prefabGo, withMaskButton);
            }

            UIModule.RegisterWindowPrefab<TestWindow>(prefabGo);
            return window;
        }

        /// <summary>按 Canvas 根约定创建 WindowB"预制体"（蒙版叠加测试的第二窗口）并注册到模块。</summary>
        WindowB CreateRegisteredWindowBPrefab(UIModule module, int sortingOrder)
        {
            var prefabGo = NewGameObject("WindowBPrefab");
            prefabGo.AddComponent<Canvas>();
            prefabGo.AddComponent<CanvasScaler>();
            prefabGo.AddComponent<GraphicRaycaster>();
            var window = prefabGo.AddComponent<WindowB>();
            SetSortingOrder(window, sortingOrder);
            SetDestroyOnHide(window, false);
            AddMaskChild(prefabGo, true);
            UIModule.RegisterWindowPrefab<WindowB>(prefabGo);
            return window;
        }

        /// <summary>为窗口物体添加蒙版子物体（约定名 Mask，全屏拉伸 Image + 可选 Button）。</summary>
        static GameObject AddMaskChild(GameObject parent, bool withButton = true)
        {
            // GameObject 构造携带 RectTransform（普通 GameObject 只有 Transform，蒙版须为 UI 物体）
            var maskGo = new GameObject("Mask", typeof(RectTransform));
            maskGo.transform.SetParent(parent.transform, false);
            var rectTransform = (RectTransform)maskGo.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            maskGo.AddComponent<Image>();
            if (withButton)
            {
                maskGo.AddComponent<Button>();
            }

            return maskGo;
        }

        /// <summary>读取窗口实例的蒙版子物体激活状态（无 Mask 子物体返回 null）。</summary>
        static bool? IsMaskActive(GameObject windowGo)
        {
            var maskTransform = windowGo.transform.Find("Mask");
            return maskTransform == null ? null : maskTransform.gameObject.activeSelf;
        }

        /// <summary>经 SerializedObject 修改窗口的 sortingOrder 序列化字段。</summary>
        static void SetSortingOrder(AesirBaseWindow window, int value)
        {
            var so = new SerializedObject(window);
            so.FindProperty("sortingOrder").intValue = value;
            so.ApplyModifiedProperties();
        }

        /// <summary>经 SerializedObject 修改窗口的 destroyOnHide 序列化字段。</summary>
        static void SetDestroyOnHide(AesirBaseWindow window, bool value)
        {
            var so = new SerializedObject(window);
            so.FindProperty("destroyOnHide").boolValue = value;
            so.ApplyModifiedProperties();
        }

        /// <summary>经 SerializedObject 修改窗口的 closeOnMaskClick 序列化字段。</summary>
        static void SetCloseOnMaskClick(AesirBaseWindow window, bool value)
        {
            var so = new SerializedObject(window);
            so.FindProperty("closeOnMaskClick").boolValue = value;
            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// 自捕获指定操作产生的日志（绕开 LogAssert.Expect 匹配语义的不确定性）。
        /// 与 <see cref="UIModuleTests" /> 同款模式。
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

        #region OpenWindow：挂载 / 接线 / 生命周期

        [Test]
        public void OpenWindow_NewWindow_InitializesShowsAndRegisters()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var window = module.OpenWindow<TestWindow>();

            Assert.IsNotNull(window);
            Assert.IsTrue(window.IsOpen);
            Assert.AreEqual(new[] { "Init", "Show" }, LifeLog.ToArray());
            Assert.AreSame(window, UIModule.GetWindow<TestWindow>(), "首次 Open 后应可按实际类型获取");
        }

        [Test]
        public void OpenWindow_AttachesDirectlyUnderUIRoot_AndWiresCanvas()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module, 650);

            var window = module.OpenWindow<TestWindow>();
            var mono = (MonoBehaviour)window;

            Assert.AreSame(mono.transform.parent, UIRoot.Instance.transform,
                "窗口应直接挂载在 UIRoot 下（不经四层 Canvas）");
            var canvas = mono.GetComponent<Canvas>();
            Assert.AreEqual(RenderMode.ScreenSpaceCamera, canvas.renderMode, "渲染模式应统一接线为 ScreenSpaceCamera");
            Assert.AreSame(UIRoot.Instance.UICamera, canvas.worldCamera, "窗口相机应接线 UIRoot 的 UICamera");
            Assert.AreEqual(650, canvas.sortingOrder, "sortingOrder 应取窗口声明值");
            Assert.AreEqual(5, mono.gameObject.layer, "窗口根应被递归设置到 UI 层");
            Assert.AreEqual(5, mono.transform.Find("Mask").gameObject.layer, "UI 层递归应覆盖 Mask 子物体");
        }

        [Test]
        public void OpenWindow_SecondOpen_ReusesInstanceAndMovesToLastSibling()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var first = module.OpenWindow<TestWindow>();
            var second = module.OpenWindow<TestWindow>();

            Assert.AreSame(first, second, "已注册窗口再次 Open 应复用实例");
            Assert.AreEqual(new[] { "Init", "Show", "Show" }, LifeLog.ToArray(), "重复 Open 应重新触发 OnShow");
            var mono = (MonoBehaviour)second;
            Assert.AreEqual(mono.transform.parent.childCount - 1, mono.transform.GetSiblingIndex(),
                "重复 Open 应将窗口置顶到 UIRoot 下最后");
        }

        [Test]
        public void OpenWindow_BaseTypeAfterDerivedRegistration_LogsErrorAndDoesNotDuplicate()
        {
            var module = CreateModule();
            TrackUIRoot();
            // 预制体挂派生脚本、以基类类型注册（键语义坑的典型成因）
            var prefabGo = NewGameObject("DerivedWindowPrefab");
            prefabGo.AddComponent<Canvas>();
            prefabGo.AddComponent<TestWindowDerived>();
            UIModule.RegisterWindowPrefab<TestWindow>(prefabGo);

            var first = module.OpenWindow(typeof(TestWindow));
            Assert.IsNotNull(first, "首次以基类类型 Open 应正常实例化（注册键为实际类型）");

            // 第二次以基类类型 Open：必须报错拒绝而不是重复实例化
            IUIWindow second = null;
            var logs = CaptureLogs(() => second = module.OpenWindow(typeof(TestWindow)));
            Assert.IsNull(second, "以基类类型重复 Open 应被拒绝");
            AssertHasLog(logs, LogType.Error, "窗口注册表以实例的实际类型为键", "拒绝应记录键语义 Error");

            Assert.IsNotNull(UIModule.GetWindow<TestWindowDerived>(), "派生实例应保持注册，未被重复实例化顶替");

            // 以实际类型 Open 仍可正常复用
            var third = module.OpenWindow<TestWindowDerived>();
            Assert.AreSame(first, third);
        }

        [Test]
        public void OpenWindow_WindowInactiveUntilShow_LifecycleDeferred()
        {
            var module = CreateModule();
            TrackUIRoot();
            var prefabGo = NewGameObject("LifecycleWindowPrefab");
            prefabGo.AddComponent<Canvas>();
            prefabGo.AddComponent<CanvasScaler>();
            prefabGo.AddComponent<GraphicRaycaster>();
            prefabGo.AddComponent<LifecycleStateWindow>();
            UIModule.RegisterWindowPrefab<LifecycleStateWindow>(prefabGo);

            var window = module.OpenWindow<LifecycleStateWindow>();

            Assert.IsFalse(LifecycleStateWindow.ActiveSelfDuringInit,
                "OnInit 执行时窗口物体应为停用状态（Awake/OnEnable 推迟到 Show 激活才触发，与面板同一契约）");
            Assert.IsTrue(((MonoBehaviour)window).gameObject.activeSelf, "Show 完成后窗口物体应已激活");
        }

        #endregion

        #region CloseWindow：销毁分叉 / 隐藏分叉 / 幂等

        [Test]
        public void CloseWindow_DestroyOnHide_DestroysAndClearsRegistry()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var window = module.OpenWindow<TestWindow>();
            SetDestroyOnHide(window, true);

            // EditMode 中 UIModule 的 Destroy(gameObject) 会被 Unity 拒绝（EngineError），
            // 但 OnHide → OnClose 的受控销毁回调照常触发
            var logs = CaptureLogs(() => module.CloseWindow<TestWindow>());

            Assert.AreEqual(new[] { "Init", "Show", "Hide", "Close" }, LifeLog.ToArray(),
                "受控销毁路径应依次触发 OnHide → OnClose（实例销毁因 EditMode Destroy 报错而未实际执行）");
            AssertHasLog(logs, LogType.Error, "Destroy may not be called from edit mode",
                "EditMode 中框架的 Destroy 调用被引擎拒绝属预期行为");
            Assert.IsNull(UIModule.GetWindow<TestWindow>(), "销毁分叉应清理注册表");
        }

        [Test]
        public void CloseWindow_KeepOnHide_HidesAndNextOpenReuses()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var window = module.OpenWindow<TestWindow>();
            SetDestroyOnHide(window, false);

            module.CloseWindow<TestWindow>();

            Assert.IsFalse(window.IsOpen);
            Assert.IsTrue(window != null, "DestroyOnHide=false 时实例应存活");
            CollectionAssert.Contains(LifeLog, "Hide");

            var reOpened = module.OpenWindow<TestWindow>();
            Assert.AreSame(window, reOpened, "再次 Open 应复用隐藏的实例");
            Assert.AreEqual(new[] { "Init", "Show", "Hide", "Show" }, LifeLog.ToArray());
        }

        [Test]
        public void CloseWindow_UnknownType_IsSilentNoOp()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var logs = CaptureLogs(() =>
            {
                module.CloseWindow<OtherWindow>();
                Assert.IsNull(UIModule.GetWindow<OtherWindow>(), "无关联实例时 Get 应返回 null");
            });

            // 完全静默 = 无 Warning 及以上级别的框架日志
            foreach (var entry in logs)
            {
                Assert.IsTrue(entry.Type == LogType.Log,
                    $"无关联实例的 Close/Get 不应产生框架 Warning/Error 日志（实际 {entry.Type}: {entry.Message}）");
            }
        }

        [Test]
        public void CloseWindow_PrewarmedButNeverShown_IsNoOp()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            Assert.IsTrue(UIModule.PrewarmWindow<TestWindow>());
            module.CloseWindow<TestWindow>();

            Assert.AreEqual(new[] { "Init" }, LifeLog.ToArray(), "未显示窗口的关闭应为幂等操作，不触发额外回调");
            Assert.IsNotNull(UIModule.GetWindow<TestWindow>(), "预热窗口不应被关闭路径销毁或移出注册表");
        }

        #endregion

        #region PrewarmWindow

        [Test]
        public void PrewarmWindow_InitializesWithoutShow_OpenThenReuses()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            Assert.IsTrue(UIModule.PrewarmWindow<TestWindow>());
            Assert.AreEqual(new[] { "Init" }, LifeLog.ToArray(), "预热只初始化不显示");
            Assert.IsFalse(UIModule.GetWindow<TestWindow>().IsOpen);

            var window = module.OpenWindow<TestWindow>();
            Assert.AreEqual(new[] { "Init", "Show" }, LifeLog.ToArray(), "预热后的 Open 应复用实例（不再 Init）");
            Assert.IsTrue(window.IsOpen);
        }

        [Test]
        public void PrewarmWindow_AlreadyShown_ReturnsTrueWithoutSideEffects()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var window = module.OpenWindow<TestWindow>();
            LifeLog.Clear();

            Assert.IsTrue(UIModule.PrewarmWindow<TestWindow>(), "已显示窗口的预热应返回 true");
            CollectionAssert.IsEmpty(LifeLog, "已存在窗口的预热不应有任何副作用");
        }

        #endregion

        #region 键语义诊断（Get / Close 侧）

        [Test]
        public void GetWindow_BaseTypeAfterDerivedRegistration_LogsKeySemanticsWarning()
        {
            var module = CreateModule();
            TrackUIRoot();
            var prefabGo = NewGameObject("DerivedWindowPrefab");
            prefabGo.AddComponent<Canvas>();
            prefabGo.AddComponent<TestWindowDerived>();
            UIModule.RegisterWindowPrefab<TestWindow>(prefabGo);

            module.OpenWindow(typeof(TestWindow));

            IUIWindow result = null;
            var logs = CaptureLogs(() => result = UIModule.GetWindow<TestWindow>());
            Assert.IsNull(result, "以基类类型 Get 应返回 null");
            AssertHasLog(logs, LogType.Warning, "未获取到窗口实例：注册表以实例的实际类型为键", "以基类类型误查应记录键语义 Warning");
        }

        [Test]
        public void CloseWindow_BaseTypeAfterDerivedRegistration_LogsKeySemanticsWarning()
        {
            var module = CreateModule();
            TrackUIRoot();
            var prefabGo = NewGameObject("DerivedWindowPrefab");
            prefabGo.AddComponent<Canvas>();
            prefabGo.AddComponent<TestWindowDerived>();
            UIModule.RegisterWindowPrefab<TestWindow>(prefabGo);

            var window = (TestWindowDerived)module.OpenWindow(typeof(TestWindow));

            var logs = CaptureLogs(() => module.CloseWindow<TestWindow>());
            AssertHasLog(logs, LogType.Warning, "未关闭任何窗口：注册表以实例的实际类型为键", "以基类类型误关应记录键语义 Warning");
            Assert.IsNotNull(window, "以基类类型 Close 不应影响已注册的派生实例");
            Assert.IsTrue(window.IsOpen, "派生实例应保持打开状态");
        }

        #endregion

        #region 跨契约互斥（Panel ↔ Window 入口）

        [Test]
        public void OpenWindow_PanelType_LogsErrorAndDoesNotInstantiate()
        {
            var module = CreateModule();
            TrackUIRoot();
            var prefabGo = NewGameObject("PlainPanelPrefab");
            prefabGo.AddComponent<PlainPanel>();
            module.RegisterPanelPrefab<PlainPanel>(prefabGo);

            IUIWindow window = null;
            var logs = CaptureLogs(() => window = module.OpenWindow(typeof(PlainPanel)));

            Assert.IsNull(window, "面板类型误入窗口入口应被拒绝");
            AssertHasLog(logs, LogType.Error, "是面板（实现了 IUIPanel）", "拒绝应指向面板入口 ShowPanel");
            Assert.IsNull(module.GetWindow(typeof(PlainPanel)), "被拒绝的调用不应残留窗口注册表");
        }

        [Test]
        public void ShowPanel_WindowType_LogsErrorAndDoesNotInstantiate()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            IUIPanel panel = null;
            var logs = CaptureLogs(() => panel = module.ShowPanel(typeof(TestWindow)));

            Assert.IsNull(panel, "窗口类型误入面板入口应被拒绝");
            AssertHasLog(logs, LogType.Error, "是窗口（实现了 IUIWindow）", "拒绝应指向窗口入口 OpenWindow");
        }

        [Test]
        public void PrewarmPanel_WindowType_And_PrewarmWindow_PanelType_BothRejected()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);
            var prefabGo = NewGameObject("PlainPanelPrefab");
            prefabGo.AddComponent<PlainPanel>();
            module.RegisterPanelPrefab<PlainPanel>(prefabGo);

            var logs = CaptureLogs(() =>
            {
                Assert.IsFalse(module.PrewarmPanel(typeof(TestWindow)), "窗口类型误入面板预热入口应被拒绝");
                Assert.IsFalse(module.PrewarmWindow(typeof(PlainPanel)), "面板类型误入窗口预热入口应被拒绝");
            });

            AssertHasLog(logs, LogType.Error, "是窗口（实现了 IUIWindow）", "面板预热入口的拒绝日志");
            AssertHasLog(logs, LogType.Error, "是面板（实现了 IUIPanel）", "窗口预热入口的拒绝日志");
        }

        #endregion

        #region 外部销毁与根缺 Canvas 中止

        [Test]
        public void RemoveWindowRecord_ExternalDestroy_CleansRegistry()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var window = module.OpenWindow<TestWindow>();
            var windowGo = ((MonoBehaviour)window).gameObject;

            // 模拟外部 Destroy（如场景卸载）：EditMode 下组件 OnDestroy 回调不保证触发，
            // 反清理由运行时 OnDestroy → UIModule.RemoveWindowRecord 驱动，
            // 此处直接调用同一 internal 契约入口验证其幂等清理语义
            Object.DestroyImmediate(windowGo);
            UIModule.RemoveWindowRecord(window);
            LifeLog.Clear();

            Assert.IsTrue(UIModule.GetWindow<TestWindow>() == null, "反清理后注册表不应再命中该窗口（Unity null 语义）");

            // 反清理后再次 Open 可正常新建实例
            var recreated = module.OpenWindow<TestWindow>();
            Assert.IsNotNull(recreated);
            Assert.AreNotSame(window, recreated);
            Assert.AreEqual(new[] { "Init", "Show" }, LifeLog.ToArray(), "清理后应重新走完整生命周期");
        }

        [Test]
        public void OpenWindow_MissingCanvasRoot_AbortsAndDoesNotRegister()
        {
            var module = CreateModule();
            TrackUIRoot();
            // 违反 Canvas 根约定的预制体：根节点无 Canvas 组件
            var prefabGo = NewGameObject("NoCanvasWindowPrefab");
            prefabGo.AddComponent<TestWindow>();
            UIModule.RegisterWindowPrefab<TestWindow>(prefabGo);

            IUIWindow window = null;
            var logs = CaptureLogs(() => window = module.OpenWindow<TestWindow>());

            Assert.IsNull(window, "根缺 Canvas 时应中止打开");
            AssertHasLog(logs, LogType.Error, "根节点缺少 Canvas 组件", "根缺 Canvas 应记录结构性错误");
            AssertHasLog(logs, LogType.Error, "Destroy may not be called from edit mode",
                "EditMode 中半挂载实例的清理 Destroy 被引擎拒绝属预期行为");
            Assert.IsNull(UIModule.GetWindow<TestWindow>(), "中止的窗口不应残留注册表");

            // EditMode 中 UIModule 的 Destroy 为 no-op，被中止的克隆体悬在世界空间根，
            // 不在 UIRoot 子树内（TearDown 无法连带销毁），须手动登记清理避免污染打开场景
            var leftovers = Object.FindObjectsByType<TestWindow>(
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

        #region 蒙版机制：单遮 / 叠遮 / 点击

        [Test]
        public void Mask_SingleMode_OnlyTopmostVisibleWindowOwnsMask()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module, 500);
            CreateRegisteredWindowBPrefab(module, 600);
            module.MaskMode = UIMaskMode.Single;

            var windowA = module.OpenWindow<TestWindow>();
            var windowB = module.OpenWindow<WindowB>();
            var goA = ((MonoBehaviour)windowA).gameObject;
            var goB = ((MonoBehaviour)windowB).gameObject;

            Assert.IsFalse(IsMaskActive(goA).Value, "单遮模式下最高层是 WindowB(600)，A 的蒙版应关闭");
            Assert.IsTrue(IsMaskActive(goB).Value, "单遮模式下最高层可见窗口的蒙版应开启");

            // 关闭 B（destroyOnHide=false）→ 蒙版应回落到 A
            module.CloseWindow<WindowB>();
            Assert.IsTrue(IsMaskActive(goA).Value, "B 关闭后蒙版应回落到 A");

            // A 再关闭 → 无可见窗口，蒙版全部关闭
            SetDestroyOnHide(windowA, false);
            module.CloseWindow<TestWindow>();
            Assert.IsFalse(IsMaskActive(goA).Value, "全部窗口关闭后蒙版应关闭");
        }

        [Test]
        public void Mask_SingleMode_SameOrder_TieBrokenByLaterOpened()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module, 500);
            CreateRegisteredWindowBPrefab(module, 500);
            module.MaskMode = UIMaskMode.Single;

            var windowA = module.OpenWindow<TestWindow>();
            var windowB = module.OpenWindow<WindowB>();
            var goA = ((MonoBehaviour)windowA).gameObject;
            var goB = ((MonoBehaviour)windowB).gameObject;

            Assert.IsFalse(IsMaskActive(goA).Value, "同 sortingOrder 时后打开的 B（sibling 靠后）应拥有蒙版");
            Assert.IsTrue(IsMaskActive(goB).Value, "同 sortingOrder 时后打开的窗口为最高层");

            module.CloseWindow<WindowB>();
            Assert.IsTrue(IsMaskActive(goA).Value, "B 关闭后蒙版应回落到 A");
        }

        [Test]
        public void Mask_StackedMode_EachWindowMaskFollowsItsOwnVisibility()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module, 500);
            CreateRegisteredWindowBPrefab(module, 600);
            module.MaskMode = UIMaskMode.Stacked;

            var windowA = module.OpenWindow<TestWindow>();
            var windowB = module.OpenWindow<WindowB>();
            var goA = ((MonoBehaviour)windowA).gameObject;
            var goB = ((MonoBehaviour)windowB).gameObject;

            Assert.IsTrue(IsMaskActive(goA).Value, "叠遮模式下每个打开窗口的蒙版独立生效");
            Assert.IsTrue(IsMaskActive(goB).Value, "叠遮模式下每个打开窗口的蒙版独立生效");

            // 运行时切换到单遮：立即重算，只有最高层 B 持有蒙版
            module.MaskMode = UIMaskMode.Single;
            Assert.IsFalse(IsMaskActive(goA).Value, "切回单遮后 A 的蒙版应立即关闭");
            Assert.IsTrue(IsMaskActive(goB).Value, "切回单遮后只有最高层 B 持有蒙版");

            // 再切回叠遮：各自跟随 IsOpen
            module.MaskMode = UIMaskMode.Stacked;
            Assert.IsTrue(IsMaskActive(goA).Value, "叠遮模式下 A 的蒙版应恢复");
            Assert.IsTrue(IsMaskActive(goB).Value, "叠遮模式下 B 的蒙版应保持");

            module.CloseWindow<WindowB>();
            Assert.IsFalse(IsMaskActive(goB).Value, "B 关闭后 B 的蒙版应关闭");
            Assert.IsTrue(IsMaskActive(goA).Value, "叠遮模式下 A 的蒙版不受 B 关闭影响");
        }

        [Test]
        public void Mask_ClickOnMaskButton_ClosesWindowWhenCloseOnMaskClick()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var window = module.OpenWindow<TestWindow>();
            SetDestroyOnHide(window, false);
            SetCloseOnMaskClick(window, true);

            var maskButton = ((MonoBehaviour)window).transform.Find("Mask").GetComponent<Button>();
            Assert.IsNotNull(maskButton, "蒙版子物体应已接线 Button（Initialize 时挂接 onClick）");
            maskButton.onClick.Invoke();

            Assert.IsFalse(window.IsOpen, "点击蒙版应经 closeOnMaskClick 关闭窗口");
            Assert.IsFalse(((MonoBehaviour)window).gameObject.activeSelf, "隐藏分叉应停用窗口物体");
        }

        [Test]
        public void Mask_ClickOnMaskButton_ByDefaultDoesNotCloseWindow()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module);

            var window = module.OpenWindow<TestWindow>();
            SetDestroyOnHide(window, false);

            var maskButton = ((MonoBehaviour)window).transform.Find("Mask").GetComponent<Button>();
            maskButton.onClick.Invoke();

            Assert.IsTrue(window.IsOpen, "closeOnMaskClick 默认关闭，点击蒙版不应关闭窗口");
        }

        [Test]
        public void Mask_NoMaskChild_IsSilentNoOp()
        {
            var module = CreateModule();
            TrackUIRoot();
            CreateRegisteredWindowPrefab(module, 500, withMask: false);

            TestWindow window = null;
            var logs = CaptureLogs(() => window = module.OpenWindow<TestWindow>());

            Assert.IsNotNull(window, "无 Mask 子物体不应影响窗口打开");
            Assert.IsNull(IsMaskActive(window.gameObject), "无 Mask 子物体时蒙版查询为空");
            Assert.IsTrue(window.IsOpen, "窗口应处于打开状态");

            // 关闭（隐藏分叉）无蒙版窗口：全程无框架 Warning/Error 日志
            SetDestroyOnHide(window, false);
            var closeLogs = CaptureLogs(() => module.CloseWindow<TestWindow>());
            foreach (var entry in closeLogs)
            {
                Assert.IsTrue(entry.Type == LogType.Log,
                    $"无蒙版窗口的开关不应产生框架 Warning/Error 日志（实际 {entry.Type}: {entry.Message}）");
            }
        }

        #endregion
    }
}
