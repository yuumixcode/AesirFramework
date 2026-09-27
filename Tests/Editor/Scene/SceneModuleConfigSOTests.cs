using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor.SceneModuleConfig
{
    /// <summary>
    /// <see cref="SceneModuleConfigSO" /> 单例解析链与 SceneModule 配置消费的 EditMode 测试：
    /// 单例缓存、加载器优先于 Resources、重复注册 fail-fast、注销恢复 Resources 兜底、
    /// 加载器返回 null 落内存默认实例、启动场景兜底回退（实例序列化字段优先于配置）。
    /// <para>
    /// 依赖仓库内 Resources 兜底资产（编辑器初始化器 <c>SceneModuleConfigAssetInitializer</c> 自动创建，
    /// 缺失时先刷新编辑器）；静态状态经反射在 SetUp/TearDown 重置，保证用例互不污染。
    /// 命名空间段取 <c>SceneModuleConfig</c> 而非 <c>Scene</c>——后者会遮蔽兄弟测试类裸引用的
    /// <see cref="UnityEngine.SceneManagement.Scene" /> 类型（命名空间成员优先于 using 导入的 CS0118）。
    /// </para>
    /// </summary>
    public class SceneModuleConfigSOTests
    {
        const BindingFlags StaticFlags = BindingFlags.NonPublic | BindingFlags.Static;

        readonly List<Object> _createdObjects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            ResetConfigStatics();
            ResetSceneModuleStatic();
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
            SceneModuleConfigSO.UnregisterConfigLoader();
            ResetConfigStatics();
            ResetSceneModuleStatic();
        }

        [Test]
        public void Instance_ResolvesFromResourcesAndCaches()
        {
            var resourceConfig = LoadResourceConfig();

            var first = SceneModuleConfigSO.Instance;
            var second = SceneModuleConfigSO.Instance;

            Assert.AreSame(resourceConfig, first, "未注册加载器时 Instance 应经 Resources 兜底解析为仓库资产");
            Assert.AreSame(first, second, "Instance 应缓存首次解析结果");
        }

        [Test]
        public void RegisterConfigLoader_LoaderWinsOverResources()
        {
            var custom = ScriptableObject.CreateInstance<SceneModuleConfigSO>();
            _createdObjects.Add(custom);
            SceneModuleConfigSO.RegisterConfigLoader(() => custom);

            var resolved = SceneModuleConfigSO.Instance;

            Assert.AreSame(custom, resolved, "已注册加载器时 Instance 应只经加载器解析，不走 Resources");
        }

        [Test]
        public void RegisterConfigLoader_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SceneModuleConfigSO.RegisterConfigLoader(null));
        }

        [Test]
        public void RegisterConfigLoader_Duplicate_ThrowsInvalidOperationException()
        {
            SceneModuleConfigSO.RegisterConfigLoader(() => null);

            Assert.Throws<InvalidOperationException>(
                () => SceneModuleConfigSO.RegisterConfigLoader(() => null),
                "重复注册加载器视为初始化错误，应 fail-fast");
        }

        [Test]
        public void RegisterConfigLoader_ReturningNull_FallsBackToDefaultInstance()
        {
            var calls = 0;
            SceneModuleConfigSO.RegisterConfigLoader(() =>
            {
                calls++;
                return null;
            });

            var resolved = SceneModuleConfigSO.Instance;

            Assert.AreEqual(1, calls, "加载器应被调用一次");
            Assert.IsNotNull(resolved, "加载器返回 null 时应落内存默认实例");
            Assert.IsFalse(EditorUtility.IsPersistent(resolved), "内存默认实例不持久化为资产");
        }

        [Test]
        public void UnregisterConfigLoader_RestoresResourcesFallbackAndAllowsReRegister()
        {
            SceneModuleConfigSO.RegisterConfigLoader(() => null);
            Assert.IsNotNull(SceneModuleConfigSO.Instance);

            SceneModuleConfigSO.UnregisterConfigLoader();

            Assert.AreSame(LoadResourceConfig(), SceneModuleConfigSO.Instance, "注销后应重新按 Resources 兜底解析");
            Assert.DoesNotThrow(() => SceneModuleConfigSO.RegisterConfigLoader(() => null), "注销应清空加载器，允许重新注册");
        }

        [Test]
        public void CreateDefault_ReturnsInstanceWithDefaultValues()
        {
            var config = SceneModuleConfigSO.CreateDefault();
            _createdObjects.Add(config);

            Assert.AreEqual(0.9f, config.progressCap, "默认配置的进度归一化上限应为 0.9");
            Assert.IsNull(config.bootstrapScene, "默认配置不应携带启动场景");
        }

        [Test]
        public void BootstrapSceneAssetWrapper_FallsBackToConfigWhenInstanceFieldUnset()
        {
            var configScene = new SceneAssetWrapper();
            var config = ScriptableObject.CreateInstance<SceneModuleConfigSO>();
            _createdObjects.Add(config);
            config.bootstrapScene = configScene;
            SceneModuleConfigSO.RegisterConfigLoader(() => config);

            CreateAndRegisterModule("ConfigFallbackModule");

            Assert.AreSame(configScene, SceneModule.BootstrapSceneAssetWrapper,
                "实例序列化字段未赋值时应回退读取配置资产的全局启动场景");
        }

        [Test]
        public void BootstrapSceneAssetWrapper_InstanceFieldWinsOverConfig()
        {
            var configScene = new SceneAssetWrapper();
            var config = ScriptableObject.CreateInstance<SceneModuleConfigSO>();
            _createdObjects.Add(config);
            config.bootstrapScene = configScene;
            SceneModuleConfigSO.RegisterConfigLoader(() => config);

            var module = CreateAndRegisterModule("InstanceFieldModule");
            var instanceScene = new SceneAssetWrapper();
            SetPrivateBootstrapScene(module, instanceScene);

            Assert.AreSame(instanceScene, SceneModule.BootstrapSceneAssetWrapper,
                "实例序列化字段非 null 时应优先于配置资产");
        }

        [Test]
        public void BootstrapSceneAssetWrapper_ReturnsNullWhenNothingConfigured()
        {
            var config = ScriptableObject.CreateInstance<SceneModuleConfigSO>();
            _createdObjects.Add(config);
            SceneModuleConfigSO.RegisterConfigLoader(() => config);

            CreateAndRegisterModule("UnsetModule");

            Assert.IsNull(SceneModule.BootstrapSceneAssetWrapper, "实例字段与配置均未配置时应返回 null");
        }

        [Test]
        public void BootstrapSceneAssetWrapper_UsesResourcesFallbackWhenNoLoader()
        {
            var resourceConfig = LoadResourceConfig();

            CreateAndRegisterModule("ResourceFallbackModule");

            Assert.AreSame(resourceConfig.bootstrapScene, SceneModule.BootstrapSceneAssetWrapper,
                "无加载器时应读取 Resources 兜底资产的全局启动场景");
        }

        /// <summary>加载仓库内 Resources 兜底资产（编辑器初始化器保证存在）。</summary>
        static SceneModuleConfigSO LoadResourceConfig()
        {
            var config = Resources.Load<SceneModuleConfigSO>(SceneModuleConfigSO.ResourcePath);
            Assert.IsNotNull(config,
                $"Resources 兜底资产缺失（{SceneModuleConfigSO.ResourcePath}），请先刷新编辑器让初始化器创建");
            return config;
        }

        /// <summary>
        /// 创建挂载在宿主下的 SceneModule 并登记为单例（非根物体避开 DDOL 分支；
        /// 直接写 _instance 静态字段，避开 Instance getter 的场景搜索与自动创建路径）。
        /// </summary>
        SceneModule CreateAndRegisterModule(string name)
        {
            var host = new GameObject(name + "_Host");
            _createdObjects.Add(host);
            var child = new GameObject(name);
            child.transform.SetParent(host.transform, false);
            _createdObjects.Add(child);
            var module = child.AddComponent<SceneModule>();

            var instanceField = typeof(SceneModule).GetField("_instance", StaticFlags);
            Assert.IsNotNull(instanceField, "SceneModule._instance 静态字段不存在");
            instanceField.SetValue(null, module);
            return module;
        }

        static void SetPrivateBootstrapScene(SceneModule module, SceneAssetWrapper value)
        {
            var field = typeof(SceneModule).GetField("bootstrapScene", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "私有字段 bootstrapScene 不存在");
            field.SetValue(module, value);
        }

        static void ResetConfigStatics()
        {
            var type = typeof(SceneModuleConfigSO);
            type.GetField("_instance", StaticFlags)?.SetValue(null, null);
            type.GetField("_configLoader", StaticFlags)?.SetValue(null, null);
        }

        static void ResetSceneModuleStatic()
        {
            typeof(SceneModule).GetField("_instance", StaticFlags)?.SetValue(null, null);
        }
    }
}
