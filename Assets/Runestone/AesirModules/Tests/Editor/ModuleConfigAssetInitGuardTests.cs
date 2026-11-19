using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using Runestone.AesirModules.Editor;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 静态守护：两个模块配置资产初始化器不得注册 <see cref="InitializeOnLoadMethodAttribute" />。
    /// 包导入即写用户工程是隐式副作用，配置资产落盘必须是显式选择（菜单
    /// <c>Tools/Aesir/Modules/Create Module Config Assets</c>）。
    /// </summary>
    public class ModuleConfigAssetInitGuardTests
    {
        [Test]
        public void UIModuleConfigAssetInitializer_HasNoInitializeOnLoadMethod()
        {
            Assert.IsFalse(HasInitializeOnLoadMethod(typeof(UIModuleConfigAssetInitializer)),
                "UI 配置资产不得在包导入时自动创建");
        }

        [Test]
        public void SceneModuleConfigAssetInitializer_HasNoInitializeOnLoadMethod()
        {
            Assert.IsFalse(HasInitializeOnLoadMethod(typeof(SceneModuleConfigAssetInitializer)),
                "场景配置资产不得在包导入时自动创建");
        }

        static bool HasInitializeOnLoadMethod(Type type) =>
            type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(method => method.IsDefined(typeof(InitializeOnLoadMethodAttribute), false));
    }
}
