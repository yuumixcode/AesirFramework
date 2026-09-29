using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Runestone.AesirModules.ScriptDocGenerator;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class MethodDataOperatorTests
    {
        static readonly MethodInfo[] TestClassMethodInfos = typeof(TestClass).GetRuntimeMethods().ToArray();

        static readonly IMethodData[] TestClassMethodDataArray = TestClassMethodInfos
            .Select(x => UnitTestAnalysisFactory.Default.CreateMethodData(x)).ToArray();

        [Test]
        public void Analysis_ProducesMemberDataForEveryMethod()
        {
            // 原为"只打日志不断言"的用例（对缺陷完全不敏感）——改为锁定"每个方法都被解析出成员数据"
            Assert.IsNotEmpty(TestClassMethodInfos, "前置：测试类应有方法可解析");
            Assert.AreEqual(TestClassMethodInfos.Length, TestClassMethodDataArray.Length,
                "每个 MethodInfo 都应产出一条 IMethodData");

            foreach (var methodData in TestClassMethodDataArray)
            {
                var memberData = (IMemberData)methodData;
                Assert.IsNotNull(memberData, "成员数据不应为 null");
                Assert.IsNotEmpty(memberData.Name, "成员名不应为空");
            }

            CollectionAssert.Contains(
                TestClassMethodDataArray.Select(x => ((IMemberData)x).Name).ToArray(),
                "op_Addition",
                "运算符方法也应作为成员参与分析");
        }

        [Test]
        public void TestOperatorAdd()
        {
            var methodData = TestClassMethodDataArray.First(m => ((IMemberData)m).Name == "op_Addition");
            Debug.Log(methodData.Signature);
            Assert.AreEqual(
                "public static MethodDataOperatorTests.TestClass operator +(MethodDataOperatorTests.TestClass a, MethodDataOperatorTests.TestClass b)",
                methodData.Signature);
        }

        [Test]
        public void TestOperatorSub()
        {
            var methodData = TestClassMethodDataArray.First(m => ((IMemberData)m).Name == "op_Subtraction");
            Debug.Log(methodData.Signature);
            Assert.AreEqual(
                "public static MethodDataOperatorTests.TestClass operator -(MethodDataOperatorTests.TestClass a, MethodDataOperatorTests.TestClass b)",
                methodData.Signature);
        }

        [Test]
        public void TestOperatorMul()
        {
            var methodData = TestClassMethodDataArray.First(m => ((IMemberData)m).Name == "op_Multiply");
            Debug.Log(methodData.Signature);
            Assert.AreEqual(
                "public static MethodDataOperatorTests.TestClass operator *(MethodDataOperatorTests.TestClass a, MethodDataOperatorTests.TestClass b)",
                methodData.Signature);
        }

        [Test]
        public void TestOperatorDiv()
        {
            var methodData = TestClassMethodDataArray.First(m => ((IMemberData)m).Name == "op_Division");
            Debug.Log(methodData.Signature);
            Assert.AreEqual(
                "public static MethodDataOperatorTests.TestClass operator /(MethodDataOperatorTests.TestClass a, MethodDataOperatorTests.TestClass b)",
                methodData.Signature);
        }

        [Test]
        public void TestOperatorMod()
        {
            var methodData = TestClassMethodDataArray.First(m => ((IMemberData)m).Name == "op_Modulus");
            Debug.Log(methodData.Signature);
            Assert.AreEqual(
                "public static MethodDataOperatorTests.TestClass operator %(MethodDataOperatorTests.TestClass a, MethodDataOperatorTests.TestClass b)",
                methodData.Signature);
        }

        [Test]
        public void TestOperatorImplicit()
        {
            var methodData = TestClassMethodDataArray.First(m => ((IMemberData)m).Name == "op_Implicit");
            Debug.Log(methodData.Signature);
            Assert.AreEqual("public static implicit operator MethodDataOperatorTests.TestClass(int a)",
                methodData.Signature);
        }

        [Test]
        public void TestOperatorExplicit()
        {
            var methodData = TestClassMethodDataArray.First(m => ((IMemberData)m).Name == "op_Explicit");
            Debug.Log(methodData.Signature);
            Assert.AreEqual("public static explicit operator float(MethodDataOperatorTests.TestClass a)",
                methodData.Signature);
        }

        #region Nested type: TestClass

        public class TestClass
        {
            public static TestClass operator +(TestClass a, TestClass b) => new TestClass();
            public static TestClass operator -(TestClass a, TestClass b) => new TestClass();
            public static TestClass operator *(TestClass a, TestClass b) => new TestClass();
            public static TestClass operator /(TestClass a, TestClass b) => new TestClass();
            public static TestClass operator %(TestClass a, TestClass b) => new TestClass();
            public static implicit operator TestClass(int a) => new TestClass();
            public static explicit operator float(TestClass a) => 1f;
        }

        #endregion
    }
}
