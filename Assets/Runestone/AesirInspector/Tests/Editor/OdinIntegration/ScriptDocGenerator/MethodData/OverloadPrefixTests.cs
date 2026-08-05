using Runestone.AesirInspector;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// 验证重载方法的 [Overload] 前缀只出现一次，不会因重载数量增加而重复追加。
    /// </summary>
    public class OverloadPrefixTests
    {
        static readonly IAnalysisDataFactory Factory = UnitTestAnalysisFactory.Default;

        /// <summary>
        /// 两个重载方法各自只应有一个 [Overload] 前缀。
        /// </summary>
        [Test]
        public void TwoOverloads_OnlyOnePrefixEach()
        {
            var typeData = Factory.CreateTypeData(typeof(TwoOverloadClass), Factory);
            var methods = typeData.RuntimeReflectedMethodsData
                .Where(m => m.SignatureWithoutParameters.Contains("DoSomething"))
                .ToArray();

            Assert.AreEqual(2, methods.Length, "应有 2 个 DoSomething 重载");

            foreach (var method in methods)
            {
                Assert.IsTrue(method.IsOverloadMethodInDeclaringType, "应标记为重载");
                var prefixCount = CountPrefix(method.Signature, "[Overload]");
                Assert.AreEqual(1, prefixCount,
                    $"方法 '{method.Signature}' 应只有 1 个 [Overload] 前缀，但有 {prefixCount} 个");
            }
        }

        /// <summary>
        /// 三个重载方法各自只应有一个 [Overload] 前缀。
        /// </summary>
        [Test]
        public void ThreeOverloads_OnlyOnePrefixEach()
        {
            var typeData = Factory.CreateTypeData(typeof(ThreeOverloadClass), Factory);
            var methods = typeData.RuntimeReflectedMethodsData
                .Where(m => m.SignatureWithoutParameters.Contains("Execute"))
                .ToArray();

            Assert.AreEqual(3, methods.Length, "应有 3 个 Execute 重载");

            foreach (var method in methods)
            {
                Assert.IsTrue(method.IsOverloadMethodInDeclaringType, "应标记为重载");
                var prefixCount = CountPrefix(method.Signature, "[Overload]");
                Assert.AreEqual(1, prefixCount,
                    $"方法 '{method.Signature}' 应只有 1 个 [Overload] 前缀，但有 {prefixCount} 个");
            }
        }

        /// <summary>
        /// 四个重载方法各自只应有一个 [Overload] 前缀。
        /// </summary>
        [Test]
        public void FourOverloads_OnlyOnePrefixEach()
        {
            var typeData = Factory.CreateTypeData(typeof(FourOverloadClass), Factory);
            var methods = typeData.RuntimeReflectedMethodsData
                .Where(m => m.SignatureWithoutParameters.Contains("Run"))
                .ToArray();

            Assert.AreEqual(4, methods.Length, "应有 4 个 Run 重载");

            foreach (var method in methods)
            {
                Assert.IsTrue(method.IsOverloadMethodInDeclaringType, "应标记为重载");
                var prefixCount = CountPrefix(method.Signature, "[Overload]");
                Assert.AreEqual(1, prefixCount,
                    $"方法 '{method.Signature}' 应只有 1 个 [Overload] 前缀，但有 {prefixCount} 个");
            }
        }

        /// <summary>
        /// 非重载方法不应有 [Overload] 前缀。
        /// </summary>
        [Test]
        public void NonOverloadMethod_NoPrefix()
        {
            var typeData = Factory.CreateTypeData(typeof(NonOverloadClass), Factory);
            var methods = typeData.RuntimeReflectedMethodsData
                .Where(m => m.SignatureWithoutParameters.Contains("UniqueMethod"))
                .ToArray();

            Assert.AreEqual(1, methods.Length);

            Assert.IsFalse(methods[0].IsOverloadMethodInDeclaringType, "不应标记为重载");
            var prefixCount = CountPrefix(methods[0].Signature, "[Overload]");
            Assert.AreEqual(0, prefixCount, "非重载方法不应有 [Overload] 前缀");
        }

        static int CountPrefix(string source, string prefix)
        {
            var count = 0;
            var idx = 0;
            while ((idx = source.IndexOf(prefix, idx)) != -1)
            {
                count++;
                idx += prefix.Length;
            }
            return count;
        }

        #region Test Classes

        class TwoOverloadClass
        {
            public void DoSomething() { }
            public void DoSomething(int count) { }
        }

        class ThreeOverloadClass
        {
            public void Execute() { }
            public void Execute(int count) { }
            public void Execute(string name, int count) { }
        }

        class FourOverloadClass
        {
            public void Run() { }
            public void Run(int count) { }
            public void Run(string name) { }
            public void Run(string name, int count, bool flag) { }
        }

        class NonOverloadClass
        {
            public void UniqueMethod() { }
        }

        #endregion
    }
}
