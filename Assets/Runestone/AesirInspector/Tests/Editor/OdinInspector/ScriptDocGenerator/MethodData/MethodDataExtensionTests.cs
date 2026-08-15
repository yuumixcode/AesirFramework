using Runestone.AesirInspector;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirInspector.Editor.Tests
{
    public class MethodDataExtensionTests
    {
        [Test]
        public void TestStaticMethod()
        {
            var methodData = UnitTestAnalysisFactory.Default.CreateMethodData(
                typeof(TestStaticExtension).GetRuntimeMethod(nameof(TestStaticExtension.StaticMethod),
                    new[] { typeof(TestClass) }));
            Debug.Log(methodData.Signature);
            Assert.AreEqual("[Ext] public static int StaticMethod(this MethodDataExtensionTests.TestClass t)",
                methodData.Signature);
        }

        #region Nested type: TestClass

        public class TestClass { }

        #endregion
    }

    public static class TestStaticExtension
    {
        public static int StaticMethod(this MethodDataExtensionTests.TestClass t) => 0;
    }
}
