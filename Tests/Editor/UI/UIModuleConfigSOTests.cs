using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor.UI
{
    /// <summary>
    /// <see cref="UIModuleConfigSO" /> 单例解析链与 UIModule 配置初始化的 EditMode 测试：
    /// 单例缓存、加载器优先于 Resources、重复注册 fail-fast、注销恢复 Resources 兜底、
    /// 加载器返回 null 落内存默认实例、UIModule 蒙版模式初值取自配置。
    /// <para>
    /// 依赖仓库内 Resources 兜底资产（编辑器初始化器 <c>UIModuleConfigAssetInitializer</c> 自动创建，
    /// 缺失时先刷新编辑器）；静态状态经反射在 SetUp/TearDown 重置，保证用例互不污染。
    /// </para>
    /// </summary>
    public class UIModuleConfigSOTests
    {
        const BindingFlags StaticFlags = BindingFlags.NonPublic | BindingFlags.Static;

        readonly List<Object> _createdObjects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            ResetConfigStatics();
            ResetUIModuleStatic();
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
            UIModuleConfigSO.UnregisterConfigLoader();
            ResetConfigStatics();
            ResetUIModuleStatic();
        }

        [Test]
        public void Instance_ResolvesFromResourcesAndCaches()
        {
            var resourceConfig = LoadResourceConfig();

            var first = UIModuleConfigSO.Instance;
            var second = UIModuleConfigSO.Instance;

            Assert.AreSame(resourceConfig, first, "未注册加载器时 Instance 应经 Resources 兜底解析为仓库资产");
            Assert.AreSame(first, second, "Instance 应缓存首次解析结果");
        }

        [Test]
        public void RegisterConfigLoader_LoaderWinsOverResources()
        {
            var custom = ScriptableObject.CreateInstance<UIModuleConfigSO>();
            _createdObjects.Add(custom);
            UIModuleConfigSO.RegisterConfigLoader(() => custom);

            var resolved = UIModuleConfigSO.Instance;

            Assert.AreSame(custom, resolved, "已注册加载器时 Instance 应只经加载器解析，不走 Resources");
        }

        [Test]
        public void RegisterConfigLoader_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => UIModuleConfigSO.RegisterConfigLoader(null));
        }

        [Test]
        public void RegisterConfigLoader_Duplicate_ThrowsInvalidOperationException()
        {
            UIModuleConfigSO.RegisterConfigLoader(() => null);

            Assert.Throws<InvalidOperationException>(
                () => UIModuleConfigSO.RegisterConfigLoader(() => null),
                "重复注册加载器视为初始化错误，应 fail-fast");
        }

        [Test]
        public void RegisterConfigLoader_ReturningNull_FallsBackToDefaultInstance()
        {
            var calls = 0;
            UIModuleConfigSO.RegisterConfigLoader(() =>
            {
                calls++;
                return null;
            });

            var resolved = UIModuleConfigSO.Instance;

            Assert.AreEqual(1, calls, "加载器应被调用一次");
            Assert.IsNotNull(resolved, "加载器返回 null 时应落内存默认实例");
            Assert.IsFalse(EditorUtility.IsPersistent(resolved), "内存默认实例不持久化为资产");
        }

        [Test]
        public void UnregisterConfigLoader_RestoresResourcesFallbackAndAllowsReRegister()
        {
            UIModuleConfigSO.RegisterConfigLoader(() => null);
            Assert.IsNotNull(UIModuleConfigSO.Instance);

            UIModuleConfigSO.UnregisterConfigLoader();

            Assert.AreSame(LoadResourceConfig(), UIModuleConfigSO.Instance, "注销后应重新按 Resources 兜底解析");
            Assert.DoesNotThrow(() => UIModuleConfigSO.RegisterConfigLoader(() => null), "注销应清空加载器，允许重新注册");
        }

        [Test]
        public void CreateDefault_ReturnsInstanceWithDefaultMaskMode()
        {
            var config = UIModuleConfigSO.CreateDefault();
            _createdObjects.Add(config);

            Assert.AreEqual(UIMaskMode.Single, config.maskMode, "默认配置的蒙版模式应为单遮");
        }

        [Test]
        public void UIModule_MaskMode_InitializesFromConfigLoader()
        {
            var config = ScriptableObject.CreateInstance<UIModuleConfigSO>();
            _createdObjects.Add(config);
            config.maskMode = UIMaskMode.Stacked;
            UIModuleConfigSO.RegisterConfigLoader(() => config);

            var module = CreateModule("StackedConfigModule");

            Assert.AreEqual(UIMaskMode.Stacked, module.MaskMode, "首次访问 MaskMode 应从配置读取初值");
        }

        [Test]
        public void UIModule_MaskMode_UsesResourcesFallbackWhenNoLoader()
        {
            var resourceConfig = LoadResourceConfig();

            var module = CreateModule("ResourceFallbackModule");

            Assert.AreEqual(resourceConfig.maskMode, module.MaskMode, "无加载器时首次访问应读取 Resources 兜底资产");
        }

        [Test]
        public void UIModule_MaskModeSetter_OverridesWithoutRewritingConfigAsset()
        {
            var resourceConfig = LoadResourceConfig();
            var resourceMaskMode = resourceConfig.maskMode;
            var module = CreateModule("SetterModule");

            module.MaskMode = UIMaskMode.Stacked;

            Assert.AreEqual(UIMaskMode.Stacked, module.MaskMode, "运行时切换应覆盖内存值");
            Assert.AreEqual(resourceMaskMode, resourceConfig.maskMode, "运行时切换不改写配置资产");
        }

        /// <summary>加载仓库内 Resources 兜底资产（编辑器初始化器保证存在）。</summary>
        static UIModuleConfigSO LoadResourceConfig()
        {
            var config = Resources.Load<UIModuleConfigSO>(UIModuleConfigSO.ResourcePath);
            Assert.IsNotNull(config,
                $"Resources 兜底资产缺失（{UIModuleConfigSO.ResourcePath}），请先刷新编辑器让初始化器创建");
            return config;
        }

        /// <summary>创建挂载在宿主下的 UIModule（非根物体，避开 DDOL 分支的编辑模式场景迁移）。</summary>
        UIModule CreateModule(string name)
        {
            var host = new GameObject(name + "_Host");
            _createdObjects.Add(host);
            var child = new GameObject(name);
            child.transform.SetParent(host.transform, false);
            _createdObjects.Add(child);
            return child.AddComponent<UIModule>();
        }

        static void ResetConfigStatics()
        {
            var type = typeof(UIModuleConfigSO);
            type.GetField("_instance", StaticFlags)?.SetValue(null, null);
            type.GetField("_configLoader", StaticFlags)?.SetValue(null, null);
        }

        static void ResetUIModuleStatic()
        {
            typeof(UIModule).GetField("_instance", StaticFlags)?.SetValue(null, null);
        }
    }
}
