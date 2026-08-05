using Runestone.AesirInspector;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirInspector.Editor.Tests
{
    public class MethodDataOperatorTests
    {
        static readonly MethodInfo[] TestClassMethodInfos = typeof(TestClass).GetRuntimeMethods().ToArray();

        static readonly IMethodData[] TestClassMethodDataArray = TestClassMethodInfos
            .Select(x => UnitTestAnalysisFactory.Default.CreateMethodData(x)).ToArray();

        [Test]
        public void OutputInfoAndData()
        {
            foreach (var methodInfo in TestClassMethodInfos)
            {
                Debug.Log(methodInfo.Name);
            }

            foreach (var methodData in TestClassMethodDataArray)
            {
                var memberData = (IMemberData)methodData;
                Debug.Log(memberData.Name);
            }
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
