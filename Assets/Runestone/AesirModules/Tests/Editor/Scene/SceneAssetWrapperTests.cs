using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="SceneAssetWrapper" />：状态机、TryGet 家族、异常语义、构造工厂、
    /// GUID 自愈、BuildSettings 三态与防重复添加、Addressables 桥接行为（经 mock 注册）。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     涉及 EditorBuildSettings.scenes 的用例在 SetUp 保存、TearDown 恢复，保证不污染工程配置。
    ///     </para>
    ///     <para>
    ///     测试场景由 SetUp 准备（宿主工程缺失时从包内最小场景夹具临时复制一份，TearDown 按"谁创建谁删除"还原）——
    ///     测试须能随包进入任意工程，不能假设宿主工程存在某个场景。涉及的资产路径一律按文件名定位，
    ///     不写死 Assets 相对路径（随包安装形态不同：Assets 安装 / Packages 安装）。
    ///     </para>
    ///     <para>
    ///     Addressables 相关用例按 <see cref="SceneAssetWrapper.AddressablesSupportEnabled" /> 自适应：
    ///     本仓库默认未安装 Addressables 包（SupportEnabled == false 的路径可被确定性验证）；
    ///     安装了包的环境下自动跳过不适用的用例，并补充验证 SupportEnabled == true 的路径。
    ///     </para>
    /// </remarks>
    public class SceneAssetWrapperTests
    {
        /// <summary>
        /// 测试场景路径。宿主工程可能已有该场景（Unity 默认模板路径），也可能已被删除（消费工程）；
        /// 测试不得依赖任一情形——缺失时由 <see cref="EnsureTestSceneExists" /> 临时提供。
        /// </summary>
        const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

        static string _wrapperCsPath;
        static string _minimalSceneFixturePath;

        /// <summary>测试前的 BuildSettings 状态，TearDown 时恢复。</summary>
        EditorBuildSettingsScene[] _savedScenes;

        /// <summary>测试场景资产是否由本次 SetUp 创建（宿主工程原本没有时才为 true，TearDown 据此还原）。</summary>
        bool _createdTestScene;

        /// <summary>本次 SetUp 创建的最高层目录（父级已存在时为 null），TearDown 删除它即回收其下全部新建子目录。</summary>
        string _createdTestSceneFolder;

        /// <summary>
        /// SceneAssetWrapper 源码路径，用于取一个"非场景资产"的 GUID。按文件名定位，不写死 Assets 相对路径。
        /// </summary>
        static string WrapperCsPath => _wrapperCsPath ??= ResolveAssetPath("SceneAssetWrapper", "MonoScript");

        /// <summary>
        /// 最小场景夹具路径——复用 PlayMode 场景套件的最小 .unity 夹具（同属包内测试资产，随包分发），
        /// 作为测试场景的复制源。
        /// </summary>
        static string MinimalSceneFixturePath =>
            _minimalSceneFixturePath ??= ResolveAssetPath("SceneModulePlayTestA", "Scene");

        [SetUp]
        public void SetUp()
        {
            _savedScenes = EditorBuildSettings.scenes;
            SceneAssetWrapperAddressablesBridge.Unregister();
            EnsureTestSceneExists();
        }

        [TearDown]
        public void TearDown()
        {
            EditorBuildSettings.scenes = _savedScenes;
            SceneAssetWrapperAddressablesBridge.Unregister();
            RestoreRealBridgeIfAvailable();
            RemoveTestSceneIfCreated();
        }

        #region 测试场景准备 / 还原

        /// <summary>
        /// 确保测试场景资产存在。宿主工程已有则直接使用（测试全程不写盘）；缺失则从包内最小场景夹具复制一份。
        /// </summary>
        /// <remarks>
        /// 不就地新建场景：编辑器在"当前打开场景未命名且未保存"时拒绝追加式新建
        /// （<c>InvalidOperationException: Cannot create a new scene additively with an untitled scene unsaved</c>，
        /// batchmode 与"新建未保存场景"下必现），而 Single 模式新建会关掉宿主当前打开的场景。
        /// 复制夹具是纯资产操作，不触碰任何已打开场景。
        /// </remarks>
        void EnsureTestSceneExists()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SampleScenePath) != null)
            {
                return;
            }

            _createdTestSceneFolder = CreateFolderIfMissing(Path.GetDirectoryName(SampleScenePath).Replace('\\', '/'));
            AssetDatabase.CopyAsset(MinimalSceneFixturePath, SampleScenePath);
            _createdTestScene = true;
        }

        /// <summary>删除本次创建的测试场景（含本次创建的空目录）；宿主工程原有场景一律不动。</summary>
        void RemoveTestSceneIfCreated()
        {
            if (!_createdTestScene)
            {
                return;
            }

            AssetDatabase.DeleteAsset(SampleScenePath);
            if (_createdTestSceneFolder != null)
            {
                AssetDatabase.DeleteAsset(_createdTestSceneFolder);
            }

            _createdTestScene = false;
            _createdTestSceneFolder = null;
        }

        /// <summary>
        /// 逐级创建资产目录，返回本次创建的最高层目录路径（父级均已存在时返回 null）——
        /// TearDown 删除该目录即连同其下新建的子目录一并回收，不留空文件夹。
        /// </summary>
        static string CreateFolderIfMissing(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return null;
            }

            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            var topmost = CreateFolderIfMissing(parent) ?? folder;
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
            return topmost;
        }

        /// <summary>按文件名在 AssetDatabase 中定位资产（随包安装形态自适应）。</summary>
        static string ResolveAssetPath(string assetName, string filterType)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{filterType} {assetName}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileNameWithoutExtension(path), assetName, StringComparison.Ordinal))
                {
                    return path;
                }
            }

            Assert.Fail($"未找到资产：{assetName}（类型 {filterType}）");
            return null;
        }

        #endregion

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
                .Select(s => new EditorBuildSettingsScene(s.path, s.enabled)).ToArray();
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
            Assert.Throws<SceneAssetWrapperCreationException>(() =>
                SceneAssetWrapper.FromScenePath(string.Empty));
        }

        [Test]
        public void FromScenePath_NonExistentPath_ThrowsCreationException()
        {
            Assert.Throws<SceneAssetWrapperCreationException>(() =>
                SceneAssetWrapper.FromScenePath("Assets/Not/Exists/Fake.unity"));
        }

        [Test]
        public void FromScenePath_ValidPath_ResolvesGuidAndSceneAsset()
        {
            var wrapper = SceneAssetWrapper.FromScenePath(SampleScenePath);

            Assert.AreEqual(SampleScenePath, wrapper.ScenePath);
            Assert.AreEqual(Path.GetFileNameWithoutExtension(SampleScenePath), wrapper.SceneName);
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
                path => path == SampleScenePath ? "my-address" : null, path => "made-address");
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
                path => path == SampleScenePath ? "my-address" : null, path => "made-address");
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
            SceneAssetWrapperAddressablesBridge.Register(path => path == SampleScenePath ? "already" : null,
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
