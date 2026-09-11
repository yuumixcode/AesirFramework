using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="SceneModule" /> 行为层：TryGetLoadablePath 拒绝矩阵（经公共入口）、
    /// 协程失败分支（手动驱动 IEnumerator）、Single 加载失败保留叠加追踪（锁定修复时序）、
    /// SetActiveScene 校验、场景事件广播、重复实例与 DDOL 语义、预设名只读列表。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     加载成功路径依赖 LoadSceneAsync 的真实执行（仅 Play Mode 可用），本套件只覆盖纯逻辑与失败分支；
    ///     预期的 LogError/LogWarning 用 <see cref="LogAssert" /> 显式声明。
    ///     </para>
    ///     <para>
    ///     Addressables 相关用例与 SceneAssetWrapperTests 共用桥隔离约定：SetUp 注销桥、
    ///     TearDown 恢复真实桥（经反射，未装包时自动跳过）。
    ///     </para>
    /// </remarks>
    public class SceneModuleTests
    {
        GameObject _host;
        SceneModule _module;

        [SetUp]
        public void SetUp()
        {
            SceneAssetWrapperAddressablesBridge.Unregister();
            _host = new GameObject("SceneModule_Under_Test");
            _module = _host.AddComponent<SceneModule>();
        }

        [TearDown]
        public void TearDown()
        {
            // EditMode 测试对象必须立即销毁，避免污染当前打开场景
            if (_host != null)
            {
                Object.DestroyImmediate(_host);
            }

            ResetInstanceToNull();
            SceneAssetWrapperAddressablesBridge.Unregister();
            RestoreRealBridgeIfAvailable();
        }

        #region 预设名列表

        [Test]
        public void PresetBootstrapSceneNames_IsReadOnlyWithExpectedEntries()
        {
            Assert.IsInstanceOf<IReadOnlyList<string>>(SceneModule.PresetBootstrapSceneNames,
                "预设名列表应以只读接口暴露");

            var names = SceneModule.PresetBootstrapSceneNames;
            Assert.AreEqual(8, names.Count);
            Assert.AreEqual("Bootstrap", names[0]);
            Assert.AreEqual("bootstrapper", names[names.Count - 1]);
        }

        #endregion

        #region 反射辅助

        static void ResetInstanceToNull()
        {
            var field = typeof(SceneModule).GetField("_instance",
                BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, null);
        }

        static void InvokePrivateAwake(SceneModule module)
        {
            var method = typeof(SceneModule).GetMethod("Awake",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "私有方法 Awake 不存在");
            method.Invoke(module, null);
        }

        static IEnumerator StartPrivateCoroutine(SceneModule module, string methodName, params object[] args)
        {
            var method = typeof(SceneModule).GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, $"私有协程 {methodName} 不存在");
            return (IEnumerator)method.Invoke(module, args);
        }

        static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"私有字段 {fieldName} 不存在");
            field.SetValue(target, value);
        }

        static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"私有字段 {fieldName} 不存在");
            return field.GetValue(target);
        }

        /// <summary>读取静态单例字段 _instance（不走实例反射辅助，避免 null 目标 NRE）。</summary>
        static SceneModule GetInstanceStatic()
        {
            var field = typeof(SceneModule).GetField("_instance",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, "_instance 静态字段不存在");
            return (SceneModule)field.GetValue(null);
        }

        /// <summary>构造仅含路径缓存的 wrapper（不触碰 AssetDatabase，路径不进 BuildSettings 即为 Unsafe）。</summary>
        static SceneAssetWrapper MakeWrapperWithPath(string scenePath)
        {
            var wrapper = new SceneAssetWrapper();
            var field = typeof(SceneAssetWrapper).GetField("scenePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(wrapper, scenePath);
            return wrapper;
        }

        /// <summary>构造仅含地址缓存的 Addressable wrapper。</summary>
        static SceneAssetWrapper MakeAddressableWrapper(string address)
        {
            var wrapper = new SceneAssetWrapper();
            var field = typeof(SceneAssetWrapper).GetField("sceneAddress",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(wrapper, address);
            return wrapper;
        }

        static void RestoreRealBridgeIfAvailable()
        {
            var glueType = Type.GetType(
                "Runestone.AesirModules.Editor.Addressables.SceneAssetWrapperAddressablesEditor, " +
                "Runestone.AesirModules.Editor.Addressables");
            glueType?.GetMethod("Register", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
        }

        #endregion

        #region TryGetLoadablePath 拒绝矩阵（经公共入口，无效引用不会触发协程）

        [Test]
        public void LoadSceneSingle_NullWrapper_InvokesOnlyFailed()
        {
            var completed = false;
            var failed = false;

            // AesirModulesDebug 输出带富文本前缀，纯字符串匹配不上，须用 Regex
            LogAssert.Expect(LogType.Error, new Regex("场景引用为空（SceneAssetWrapper == null）。"));
            _module.LoadSceneSingle((SceneAssetWrapper)null, () => completed = true, () => failed = true);

            Assert.IsFalse(completed, "空引用不得触发完成回调");
            Assert.IsTrue(failed, "空引用应触发失败回调");
        }

        [Test]
        public void LoadSceneSingle_AddressableWrapper_InvokesOnlyFailed()
        {
            var completed = false;
            var failed = false;
            var wrapper = MakeAddressableWrapper("fake-address");

            LogAssert.Expect(LogType.Error, new Regex("为 Addressable 场景，SceneModule 无法加载"));
            _module.LoadSceneSingle(wrapper, () => completed = true, () => failed = true);

            Assert.IsFalse(completed, "Addressable 引用不得触发完成回调");
            Assert.IsTrue(failed, "Addressable 引用应触发失败回调");
        }

        [Test]
        public void LoadSceneSingle_UnsafeWrapper_InvokesOnlyFailed()
        {
            var completed = false;
            var failed = false;
            var wrapper = MakeWrapperWithPath("Assets/Fake/NotInBuild.unity");

            LogAssert.Expect(LogType.Error, new Regex("场景引用无效（不在 BuildSettings）"));
            _module.LoadSceneSingle(wrapper, () => completed = true, () => failed = true);

            Assert.IsFalse(completed);
            Assert.IsTrue(failed, "不在 BuildSettings 的引用应触发失败回调");
        }

        [Test]
        public void LoadSceneAdditive_NullWrapper_InvokesOnlyFailed()
        {
            var failed = false;

            LogAssert.Expect(LogType.Error, new Regex("场景引用为空（SceneAssetWrapper == null）。"));
            _module.LoadSceneAdditive((SceneAssetWrapper)null, null, () => failed = true);

            Assert.IsTrue(failed);
        }

        [Test]
        public void UnloadScene_NullWrapper_InvokesOnlyFailed()
        {
            var failed = false;

            LogAssert.Expect(LogType.Error, new Regex("场景引用为空，无法卸载。"));
            _module.UnloadScene((SceneAssetWrapper)null, null, () => failed = true);

            Assert.IsTrue(failed);
        }

        #endregion

        #region 协程失败分支（手动驱动 IEnumerator，不触发真实加载）

        [Test]
        public void LoadSceneInternal_InvalidPath_InvokesFailedWithoutCompleted()
        {
            var completed = false;
            var failed = false;

            LogAssert.Expect(LogType.Error, new Regex("无效场景路径"));
            var routine = StartPrivateCoroutine(_module, "LoadSceneInternal", "",
                (Action)(() => completed = true), (Action)(() => failed = true), null, LoadSceneMode.Single);

            // 空路径分支在同一次 MoveNext 内完成失败回调并 yield break
            Assert.IsFalse(routine.MoveNext(), "空路径应立即终止协程");

            Assert.IsFalse(completed);
            Assert.IsTrue(failed);
        }

        [Test]
        public void SingleLoad_Failure_KeepsAddedScenePaths()
        {
            // 锁定修复时序：Single 追踪清空只发生在加载成功之后，失败路径必须保留旧追踪
            var addedPaths = (List<string>)GetPrivateField(_module, "_addedScenePaths");
            addedPaths.Add("Assets/Fake/Additive.unity");

            var failed = false;
            LogAssert.Expect(LogType.Error, new Regex("无效场景路径"));
            var routine = StartPrivateCoroutine(_module, "LoadSceneInternal", "", null,
                (Action)(() => failed = true), null, LoadSceneMode.Single);
            routine.MoveNext();

            Assert.IsTrue(failed);
            Assert.AreEqual(1, addedPaths.Count, "Single 加载失败时不得清空叠加追踪");
            Assert.AreEqual("Assets/Fake/Additive.unity", addedPaths[0]);
        }

        [Test]
        public void UnloadSceneInternal_InvalidPath_InvokesFailed()
        {
            var unloaded = false;
            var failed = false;

            LogAssert.Expect(LogType.Error, new Regex("无效场景路径"));
            var routine = StartPrivateCoroutine(_module, "UnloadSceneInternal", "",
                (Action)(() => unloaded = true), (Action)(() => failed = true));

            Assert.IsFalse(routine.MoveNext());
            Assert.IsFalse(unloaded);
            Assert.IsTrue(failed);
        }

        [Test]
        public void UnloadAllAddedScenes_EmptyTracking_InvokesAllUnloadedCallback()
        {
            var allUnloaded = false;

            var routine = StartPrivateCoroutine(_module, "UnloadAllAddedScenesInternal",
                (Action)(() => allUnloaded = true));
            Assert.IsFalse(routine.MoveNext(), "空追踪列表应单次 MoveNext 完成并结束");

            Assert.IsTrue(allUnloaded, "空追踪列表应立即回调 onAllUnloaded");
        }

        #endregion

        #region SetActiveScene

        [Test]
        public void SetActiveScene_NotLoadedPath_ReturnsFalse()
        {
            LogAssert.Expect(LogType.Error, new Regex("场景未加载，无法设为激活场景"));
            Assert.IsFalse(_module.SetActiveScene("Assets/Fake/NotLoaded.unity"));
        }

        [Test]
        public void SetActiveScene_NullWrapper_ReturnsFalse()
        {
            LogAssert.Expect(LogType.Error, new Regex("场景引用无效或场景未加载"));
            Assert.IsFalse(_module.SetActiveScene((SceneAssetWrapper)null));
        }

        [Test]
        public void SetActiveScene_UnsafeWrapper_ReturnsFalse()
        {
            // wrapper 路径不在 BuildSettings 也不可寻址 → TryGetLoadedScene 为 false
            var wrapper = MakeWrapperWithPath("Assets/Fake/NotInBuild.unity");

            LogAssert.Expect(LogType.Error, new Regex("场景引用无效或场景未加载"));
            Assert.IsFalse(_module.SetActiveScene(wrapper));
        }

        #endregion

        #region 场景事件

        [Test]
        public void SceneLoadedEvent_Invoke_ReachesListenerAndAutoRemove()
        {
            var received = 0;
            var handle = _module.SceneLoadedEvent.AddListener(path => received++);

            _module.SceneLoadedEvent.Invoke("Assets/Fake/Scene.unity");
            Assert.AreEqual(1, received);

            handle.Dispose();
            _module.SceneLoadedEvent.Invoke("Assets/Fake/Scene.unity");
            Assert.AreEqual(1, received, "Dispose 后不应再收到事件");
        }

        [Test]
        public void SceneUnloadedEvent_Invoke_ReachesListener()
        {
            var receivedPath = null as string;
            _module.SceneUnloadedEvent.AddListener(path => receivedPath = path);

            _module.SceneUnloadedEvent.Invoke("Assets/Fake/Unloaded.unity");
            Assert.AreEqual("Assets/Fake/Unloaded.unity", receivedPath);
        }

        [Test]
        public void SceneEvents_AreIndependent()
        {
            var loadedCount = 0;
            var unloadedCount = 0;
            _module.SceneLoadedEvent.AddListener(_ => loadedCount++);
            _module.SceneUnloadedEvent.AddListener(_ => unloadedCount++);

            _module.SceneLoadedEvent.Invoke("a.unity");

            Assert.AreEqual(1, loadedCount);
            Assert.AreEqual(0, unloadedCount);
        }

        #endregion

        #region 单例与 DDOL 语义

        [Test]
        public void DuplicateInstance_SecondAwake_DoesNotReplaceInstance()
        {
            // 首个实例挂为非根物体：Awake 不触发 DDOL 分支（EditMode 下避免 DontDestroyOnLoad 场景迁移）
            var parent = new GameObject("SceneModule_Host_Parent");
            _host.transform.SetParent(parent.transform);

            // 反射驱动 Awake（EditMode 不自动执行）
            InvokePrivateAwake(_module);

            var secondHost = new GameObject("SceneModule_Duplicate");
            var second = secondHost.AddComponent<SceneModule>();

            // Destroy 在 Edit Mode 中不被允许，LogAssert 吞掉该错误以驱动分支
            // 错误消息带多行说明（\nDestroying an object in edit mode...），须用 Regex 匹配首行
            LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
            InvokePrivateAwake(second);

            Assert.AreSame(_module, GetInstanceStatic(), "重复实例的 Awake 不得覆盖既有单例");

            Object.DestroyImmediate(secondHost);
            Object.DestroyImmediate(parent);
        }

        [Test]
        public void DontDestroyOnLoadDisabled_EmitsWarning()
        {
            SetPrivateField(_module, "dontDestroyOnLoad", false);

            LogAssert.Expect(LogType.Warning, new Regex("dontDestroyOnLoad 已关闭"));
            InvokePrivateAwake(_module);

            Assert.AreNotEqual("DontDestroyOnLoad", _host.scene.name,
                "关闭 DDOL 时实例不应迁移到 DontDestroyOnLoad 场景");
        }

        [Test]
        public void NonRootObject_FollowsHostNoDDOLNoWarning()
        {
            // 运行时自动创建的子物体：跟随宿主 DDOL 决策，本字段不参与判断——无警告、无 DDOL 调用
            var parent = new GameObject("SceneModule_Host");
            _host.transform.SetParent(parent.transform);
            SetPrivateField(_module, "dontDestroyOnLoad", true);

            InvokePrivateAwake(_module);

            Assert.AreEqual(parent.scene, _module.gameObject.scene, "子物体不应触发 DDOL 场景迁移");
            Object.DestroyImmediate(parent);
        }

        [Test]
        public void ResetStatics_ClearsInstance()
        {
            // 模拟已有实例（直接写静态字段，不驱动 Awake 以避开 DDOL 分支）
            var field = typeof(SceneModule).GetField("_instance",
                BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, _module);

            var method = typeof(SceneModule).GetMethod("ResetStatics",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "SceneModule 应提供 ResetStatics 静态重置（对齐非泛型单例约定）");
            method.Invoke(null, null);

            Assert.IsNull(field.GetValue(null), "ResetStatics 后 _instance 应为 null");
        }

        #endregion
    }
}
