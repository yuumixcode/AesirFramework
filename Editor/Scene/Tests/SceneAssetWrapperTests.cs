using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="SceneAssetWrapper" />：状态机、TryGet 家族、异常语义、构造工厂、
    /// GUID 自愈、BuildSettings 三态与防重复添加、Addressables 桥接行为（经 mock 注册）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 涉及 EditorBuildSettings.scenes 的用例在 SetUp 保存、TearDown 恢复，保证不污染工程配置。
    /// </para>
    /// <para>
    /// Addressables 相关用例按 <see cref="SceneAssetWrapper.AddressablesSupportEnabled" /> 自适应：
    /// 本仓库默认未安装 Addressables 包（SupportEnabled == false 的路径可被确定性验证）；
    /// 安装了包的环境下自动跳过不适用的用例，并补充验证 SupportEnabled == true 的路径。
    /// </para>
    /// </remarks>
    public class SceneAssetWrapperTests
    {
        const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        const string WrapperCsPath = "Assets/Runestone/AesirModules/Runtime/Scene/SceneAssetWrapper.cs";

        /// <summary>本仓库的 BuildSettings 初始状态，TearDown 时恢复。</summary>
        EditorBuildSettingsScene[] _savedScenes;

        [SetUp]
        public void SetUp()
        {
            _savedScenes = EditorBuildSettings.scenes;
            SceneAssetWrapperAddressablesBridge.Unregister();
        }

        [TearDown]
        public void TearDown()
        {
            EditorBuildSettings.scenes = _savedScenes;
            SceneAssetWrapperAddressablesBridge.Unregister();
            RestoreRealBridgeIfAvailable();
        }

        /// <summary>
        /// 装了 Addressables 包时恢复真实桥注册（SetUp/TearDown 的 Unregister 会把它一并清掉），
        /// 保证测试后 Inspector 的 Addressables 功能立即可用。经反射访问，未装包时类型不存在、自动跳过，
        /// 测试程序集对可选的胶水程序集保持零编译期引用。
        /// </summary>
        static void RestoreRealBridgeIfAvailable()
        {
            var glueType = Type.GetType(
                "Runestone.AesirModules.Editor.Addressables.SceneAssetWrapperAddressablesEditor, " +
                "Runestone.AesirModules.Editor.Addressables");
            glueType?.GetMethod("Register", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
        }

        #region 反射辅助

        static void SetPrivateField(SceneAssetWrapper wrapper, string fieldName, object value)
        {
            var field = typeof(SceneAssetWrapper).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"私有字段 {fieldName} 不存在");
            field.SetValue(wrapper, value);
        }

        static object GetPrivateField(SceneAssetWrapper wrapper, string fieldName)
        {
            var field = typeof(SceneAssetWrapper).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"私有字段 {fieldName} 不存在");
            return field.GetValue(wrapper);
        }

        static void InvokePrivate(SceneAssetWrapper wrapper, string methodName)
        {
            var method = typeof(SceneAssetWrapper).GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, $"私有方法 {methodName} 不存在");
            method.Invoke(wrapper, null);
        }

        /// <summary>整体替换 BuildSettings 场景列表（用例内自包含，TearDown 恢复）。</summary>
        static void SetBuildScenes(params (string path, bool enabled)[] scenes)
        {
            EditorBuildSettings.scenes = scenes
                .Select(s => new EditorBuildSettingsScene(s.path, s.enabled))
                .ToArray();
        }

        static EditorBuildSettingsScene FindBuildScene(string path)
        {
            return EditorBuildSettings.scenes.FirstOrDefault(s => s.path == path);
        }

        #endregion

        #region 空引用

        [Test]
        public void EmptyWrapper_StateUnsafeReasonEmpty()
        {
            var wrapper = new SceneAssetWrapper();

            Assert.AreEqual(SceneAssetWrapperState.Unsafe, wrapper.State);
            Assert.AreEqual(SceneAssetWrapperUnsafeReason.Empty, wrapper.UnsafeReason);
        }

        [Test]
        public void EmptyWrapper_AccessorsThrowEmptyException()
        {
            var wrapper = new SceneAssetWrapper();

            Assert.Throws<EmptySceneAssetWrapperException>(() => _ = wrapper.ScenePath);
            Assert.Throws<EmptySceneAssetWrapperException>(() => _ = wrapper.Guid);
            Assert.Throws<EmptySceneAssetWrapperException>(() => _ = wrapper.SceneName);
            Assert.Throws<EmptySceneAssetWrapperException>(() => _ = wrapper.BuildIndex);
            Assert.Throws<EmptySceneAssetWrapperException>(() => _ = wrapper.LoadedScene);
        }

        [Test]
        public void EmptyWrapper_TryGetFamilyAllFalse()
        {
            var wrapper = new SceneAssetWrapper();

            Assert.IsFalse(wrapper.TryGetScenePath(out _));
            Assert.IsFalse(wrapper.TryGetBuildIndex(out _));
            Assert.IsFalse(wrapper.TryGetSceneName(out _));
            Assert.IsFalse(wrapper.TryGetLoadedScene(out _));
            Assert.IsFalse(wrapper.NotInBuildSettings);
            Assert.AreEqual(string.Empty, wrapper.ToString());
        }

        [Test]
        public void EmptyWrapper_EqualitySemantics()
        {
            var left = new SceneAssetWrapper();
            var right = new SceneAssetWrapper();

            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
            Assert.IsTrue(left.Equals(right));
            Assert.IsTrue(left.Equals((object)right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
            Assert.IsFalse(left.Equals(null));
        }

        #endregion

        #region 构造工厂

        [Test]
        public void FromScenePath_NullOrEmpty_ThrowsCreationException()
        {
            Assert.Throws<SceneAssetWrapperCreationException>(() => SceneAssetWrapper.FromScenePath(null));
            Assert.Throws<SceneAssetWrapperCreationException>(() => SceneAssetWrapper.FromScenePath(string.Empty));
        }

        [Test]
        public void FromScenePath_NonExistentPath_ThrowsCreationException()
        {
            Assert.Throws<SceneAssetWrapperCreationException>(
                () => SceneAssetWrapper.FromScenePath("Assets/Not/Exists/Fake.unity"));
        }

        [Test]
        public void FromScenePath_ValidPath_ResolvesGuidAndSceneAsset()
        {
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.AreEqual(SampleScenePath, wrapper.ScenePath);
            Assert.AreEqual("SampleScene", wrapper.SceneName);
            Assert.IsFalse(string.IsNullOrEmpty(wrapper.Guid), "编辑器下应解析出场景资产 GUID");
            Assert.IsNotNull(wrapper.SceneAsset, "编辑器下应解析出 SceneAsset 对象引用");
        }

        [Test]
        public void FromAsset_Null_ThrowsCreationException()
        {
            Assert.Throws<SceneAssetWrapperCreationException>(() => SceneAssetWrapper.FromAsset(null));
        }

        [Test]
        public void FromScenePath_SamePath_WrappersEqual()
        {
            var left = SceneAssetWrapper.FromScenePath(SampleScenePath);
            var right = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.IsTrue(left == right);
            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void FromScenePath_DifferentPath_WrappersNotEqual()
        {
            var left = SceneAssetWrapper.FromScenePath(SampleScenePath);
            var right = new SceneAssetWrapper();

            Assert.IsFalse(left == right);
        }

        [Test]
        public void FromAsset_AndFromScenePath_ProduceEqualWrappers()
        {
            var fromAsset = SceneAssetWrapper.FromAsset(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(SampleScenePath));
            var fromPath = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.IsTrue(fromAsset == fromPath);
        }

        #endregion

        #region BuildSettings 三态

        [Test]
        public void InBuildEnabled_StateRegular()
        {
            SetBuildScenes((SampleScenePath, true));
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.AreEqual(SceneAssetWrapperState.Regular, wrapper.State);
            Assert.AreEqual(SceneAssetWrapperUnsafeReason.None, wrapper.UnsafeReason);
            Assert.IsFalse(wrapper.NotInBuildSettings);
            Assert.GreaterOrEqual(wrapper.BuildIndex, 0);
        }

        [Test]
        public void InBuildDisabled_StateUnsafeWithDisabledFlags()
        {
            SetBuildScenes((SampleScenePath, false));
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            // 运行时语义：被禁用与不在 Build 同样不可加载
            Assert.AreEqual(SceneAssetWrapperState.Unsafe, wrapper.State);
            Assert.AreEqual(SceneAssetWrapperUnsafeReason.NotInBuild, wrapper.UnsafeReason);
            Assert.IsTrue(wrapper.NotInBuildSettings);
            // 编辑器语义：可精确区分"已加入但被禁用"
            Assert.IsTrue(wrapper.DisabledInBuildSettings);
            Assert.IsFalse(wrapper.MissingFromBuild);
            Assert.AreEqual(-1, wrapper.BuildIndex);
        }

        [Test]
        public void NotInBuild_MissingFlagsAndUnsafeState()
        {
            SetBuildScenes();
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.AreEqual(SceneAssetWrapperState.Unsafe, wrapper.State);
            Assert.AreEqual(SceneAssetWrapperUnsafeReason.NotInBuild, wrapper.UnsafeReason);
            Assert.IsTrue(wrapper.NotInBuildSettings);
            Assert.IsTrue(wrapper.MissingFromBuild);
            Assert.IsFalse(wrapper.DisabledInBuildSettings);
        }

        [Test]
        public void AddToBuildSettings_WhenMissing_AddsEnabledEntry()
        {
            SetBuildScenes();
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            InvokePrivate(wrapper, "AddCurrentSceneToBuildSettings");

            var entries = EditorBuildSettings.scenes;
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(SampleScenePath, entries[0].path);
            Assert.IsTrue(entries[0].enabled);
        }

        [Test]
        public void AddToBuildSettings_Twice_NoDuplicateEntries()
        {
            SetBuildScenes();
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            InvokePrivate(wrapper, "AddCurrentSceneToBuildSettings");
            InvokePrivate(wrapper, "AddCurrentSceneToBuildSettings");

            Assert.AreEqual(1, EditorBuildSettings.scenes.Length, "重复添加必须被防重，只保留一个条目");
        }

        [Test]
        public void AddToBuildSettings_WhenDisabled_EnablesInsteadOfDuplicating()
        {
            SetBuildScenes((SampleScenePath, false));
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            InvokePrivate(wrapper, "AddCurrentSceneToBuildSettings");

            Assert.AreEqual(1, EditorBuildSettings.scenes.Length, "已存在禁用条目时不能重复添加");
            Assert.IsTrue(FindBuildScene(SampleScenePath).enabled, "已存在的禁用条目应被启用");
        }

        [Test]
        public void EnableInBuildSettings_EnablesDisabledEntry()
        {
            SetBuildScenes((SampleScenePath, false));
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            InvokePrivate(wrapper, "EnableCurrentSceneInBuildSettings");

            Assert.IsTrue(FindBuildScene(SampleScenePath).enabled);
            Assert.AreEqual(SceneAssetWrapperState.Regular, wrapper.State);
        }

        #endregion

        #region GUID 自愈

        [Test]
        public void GuidSelfHeal_RecoversPathWhenAssetReferenceLost()
        {
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            // 模拟 SceneAsset 对象引用丢失 + 路径缓存过期（场景被移动、或数据经脚本/文本操作产生）
            SetPrivateField(wrapper, "sceneAsset", null);
            SetPrivateField(wrapper, "scenePath", "Assets/Stale/MovedScene.unity");

            Assert.AreEqual(SampleScenePath, wrapper.ScenePath, "应通过序列化的 GUID 自愈回正确路径");
        }

        [Test]
        public void GuidSelfHeal_IgnoresNonSceneAssets()
        {
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            // GUID 解析结果不是 .unity 场景时不允许污染路径缓存
            var csGuid = AssetDatabase.AssetPathToGUID(WrapperCsPath);
            SetPrivateField(wrapper, "sceneAsset", null);
            SetPrivateField(wrapper, "sceneGuid", csGuid);
            SetPrivateField(wrapper, "scenePath", "Assets/Stale/X.unity");

            Assert.AreEqual("Assets/Stale/X.unity", wrapper.ScenePath, "非场景资产的 GUID 不应触发路径恢复");
        }

        #endregion

        #region Addressables 桥接

        [Test]
        public void WithoutBridge_IsAddressableFalseAndStateUnsafe()
        {
            // SetUp 已注销桥（本项目未装 Addressables 时桥本来也不会注册）
            SetBuildScenes();
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.IsFalse(wrapper.IsAddressable);
            Assert.AreEqual(SceneAssetWrapperState.Unsafe, wrapper.State);
        }

        [Test]
        public void FakeBridge_LiveMarksAddressable()
        {
            SceneAssetWrapperAddressablesBridge.Register(
                path => path == SampleScenePath ? "my-address" : null,
                path => "made-address");
            SetBuildScenes();
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.IsTrue(wrapper.IsAddressable, "桥已注册时应以实时核验结果为准");
            Assert.AreEqual(SceneAssetWrapperState.Addressable, wrapper.State);
            Assert.AreEqual(SceneAssetWrapperUnsafeReason.None, wrapper.UnsafeReason);
        }

        [Test]
        public void FakeBridge_Unregistered_FallsBackToCachedAddressData()
        {
            SceneAssetWrapperAddressablesBridge.Register(
                path => path == SampleScenePath ? "my-address" : null,
                path => "made-address");
            SetBuildScenes();
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);
            Assert.IsTrue(wrapper.IsAddressable);

            // 卸载 Addressables（桥消失）后按缓存数据判定——最小惊讶：数据仍在
            SceneAssetWrapperAddressablesBridge.Unregister();

            Assert.IsTrue(wrapper.IsAddressable, "桥消失后应回退到序列化的地址数据");
            Assert.AreEqual(SceneAssetWrapperState.Addressable, wrapper.State);
        }

        [Test]
        public void MakeAddressable_UpdatesCachedAddress()
        {
            SceneAssetWrapperAddressablesBridge.Register(
                path => path == SampleScenePath ? "already" : null,
                path => "made-address");
            SetBuildScenes();

            // 空引用场景：make 委托返回新地址后缓存应被更新
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);
            SetPrivateField(wrapper, "sceneAddress", string.Empty);

            InvokePrivate(wrapper, "AddSceneToAddressables");

            Assert.AreEqual("made-address", GetPrivateField(wrapper, "sceneAddress"));
            Assert.IsTrue(wrapper.IsAddressable);
        }

        [Test]
        public void Address_ThrowsSupportDisabled_WhenPackageAbsent()
        {
            if (SceneAssetWrapper.AddressablesSupportEnabled)
            {
                Assert.Ignore("当前环境安装了 Addressables 包，跳过（该行为在无包环境下验证）");
            }

            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.Throws<AddressablesSupportDisabledException>(() => _ = wrapper.Address);
            Assert.Throws<AddressablesSupportDisabledException>(() => wrapper.TryGetAddress(out _));
        }

        [Test]
        public void Address_ReturnsCachedAddress_WhenPackagePresent()
        {
            if (!SceneAssetWrapper.AddressablesSupportEnabled)
            {
                Assert.Ignore("当前环境未安装 Addressables 包，跳过（该行为在有包环境下验证）");
            }

            SetBuildScenes();
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);
            SetPrivateField(wrapper, "sceneAddress", "cached-address");

            Assert.AreEqual("cached-address", wrapper.Address);
            Assert.IsTrue(wrapper.TryGetAddress(out var address));
            Assert.AreEqual("cached-address", address);
        }

        [Test]
        public void Address_ThrowsNotAddressable_WhenSceneHasNoAddress()
        {
            if (!SceneAssetWrapper.AddressablesSupportEnabled)
            {
                Assert.Ignore("当前环境未安装 Addressables 包，跳过（该行为在有包环境下验证）");
            }

            SetBuildScenes();
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.Throws<SceneNotAddressableException>(() => _ = wrapper.Address);
        }

        #endregion
    }
}
