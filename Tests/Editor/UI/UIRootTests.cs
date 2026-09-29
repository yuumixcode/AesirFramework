using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.UI
{
    /// <summary>
    /// <see cref="UIRoot" /> 静态重置的 EditMode 守护用例。
    /// </summary>
    /// <remarks>
    /// 重复实例的销毁粒度无法在 EditMode 下断言：该模式下 <c>Destroy</c> 被引擎拒绝
    /// （"Destroy may not be called from edit mode"，两种粒度都是 no-op），故那条守护放在 PlayMode
    /// （<c>UIRootPlayModeTests</c>）。静态重置则是 EditMode 可观测的。
    /// </remarks>
    public class UIRootTests
    {
        [TearDown]
        public void TearDown()
        {
            InvokeResetStatics();
        }

        [Test]
        public void ResetStatics_ClearsCreateInputModuleRegistration()
        {
            Action<GameObject> hook = _ => { };
            UIRoot.CreateInputModule = hook;

            InvokeResetStatics();

            Assert.IsNull(UIRoot.CreateInputModule,
                "域加载期重置必须清空输入模块注册；注册方 InputSystemModuleHook 在 BeforeSceneLoad 会重新注册，故清空不丢注册");
        }

        static void InvokeResetStatics()
        {
            typeof(UIRoot).GetMethod("ResetStatics", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, null);
        }
    }
}
