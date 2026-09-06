using System.Collections.Generic;
using NUnit.Framework;
using Runestone.AesirArchitecture;
using Runestone.AesirModules;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 用于验证 Context 下拉选择逻辑的测试 Context（未标注 <see cref="InternalContextAttribute" />，应可选）。
    /// </summary>
    class BinderSelectableTestContext : AbstractContext<BinderSelectableTestContext>
    {
        protected override void Configure() { }
    }

    /// <summary>
    /// 被标记为框架内部 Context 的测试用例（应被 Binder Context 下拉排除）。
    /// </summary>
    [InternalContext]
    class BinderExcludedTestContext : AbstractContext<BinderExcludedTestContext>
    {
        protected override void Configure() { }
    }

    /// <summary>
    /// Binder「Context 类型」下拉的选择范围测试: <see cref="InternalContextAttribute" /> 标记的 Context 被排除。
    /// </summary>
    public class BinderContextSelectorTests
    {
        GameObject _probe;

        [TearDown]
        public void TearDown()
        {
            if (_probe)
            {
                Object.DestroyImmediate(_probe);
            }
        }

        [Test]
        public void GetContextTypeChoices_ExcludesInternalContexts()
        {
            _probe = new GameObject("BinderContextSelectorProbe");
            var assistant = _probe.AddComponent<BinderAssistant>();
            var choices = assistant.GetContextTypeChoices();
            var values = new HashSet<string>();
            foreach (var item in choices)
            {
                values.Add(item.Value);
            }

            // 未标注的测试 Context 可选
            Assert.That(values, Does.Contain("Runestone.AesirModules.Tests.Editor.BinderSelectableTestContext"));
            // 被标注的测试 Context 被排除
            Assert.That(values, Does.Not.Contain("Runestone.AesirModules.Tests.Editor.BinderExcludedTestContext"));
            // Architecture 示例 Context（[InternalContext] 标注）被排除
            Assert.That(values, Does.Not.Contain("Runestone.AesirArchitecture.Samples.MvcQuick.SampleMvcQuickCounterContext"));
            // Architecture 测试的嵌套 Context（[InternalContext] 标注）被排除
            Assert.That(values,
                Does.Not.Contain("Runestone.AesirArchitecture.Tests.Editor.AbstractContextInitializationTests+ThrowingModelContext"));
        }
    }
}
