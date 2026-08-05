using Runestone.AesirInspector;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirInspector.Editor.Tests
{
    public class MethodDataInheritTests
    {
        static readonly MethodInfo[] TestClassImplementMethodInfos = typeof(TestClassImplement)
            .GetRuntimeMethods().ToArray();

        static readonly IMethodData[] TestClassImplementMethodDataArray = TestClassImplementMethodInfos
            .Select(x => UnitTestAnalysisFactory.Default.CreateMethodData(x)).ToArray();

        static readonly MethodInfo[] TestClassAbstractMethodInfos = typeof(TestClassAbstract)
            .GetRuntimeMethods().ToArray();

        static readonly IMethodData[] TestClassAbstractMethodDataArray = TestClassAbstractMethodInfos
            .Select(x => UnitTestAnalysisFactory.Default.CreateMethodData(x)).ToArray();

        [Test]
        public void TestVirtualMethod()
        {
            var methodData = TestClassAbstractMethodDataArray.First(m =>
                ((IMemberData)m).Name == nameof(TestClassAbstract.OverrideVirtualMethod));
            if (methodData.TryAsIMemberData(out var memberData))
            {
                Debug.Log(memberData.IsFromInheritance);
            }

            Debug.Log(methodData.Signature);
            Assert.AreEqual("public virtual void OverrideVirtualMethod()", methodData.Signature);
        }

        [Test]
        public void TestAbstractMethod()
        {
            var methodData = TestClassAbstractMethodDataArray.First(m =>
                ((IMemberData)m).Name == nameof(TestClassAbstract.OverrideAbstractMethod));
            if (methodData.TryAsIMemberData(out var memberData))
            {
                Debug.Log(memberData.IsFromInheritance);
            }

            Debug.Log(methodData.Signature);
            Assert.AreEqual("public abstract void OverrideAbstractMethod()", methodData.Signature);
        }

        [Test]
        public void TestOverrideAbstractMethod()
        {
            var methodData = TestClassImplementMethodDataArray.First(m =>
                ((IMemberData)m).Name == nameof(TestClassImplement.OverrideAbstractMethod));
            if (methodData.TryAsIMemberData(out var memberData))
            {
                Debug.Log(memberData.IsFromInheritance);
            }

            Debug.Log(methodData.Signature);
            Assert.AreEqual("public override void OverrideAbstractMethod()", methodData.Signature);
        }

        [Test]
        public void TestOverrideVirtualMethod()
        {
            var methodData = TestClassImplementMethodDataArray.First(m =>
                ((IMemberData)m).Name == nameof(TestClassImplement.OverrideVirtualMethod));
            if (methodData.TryAsIMemberData(out var memberData))
            {
                Debug.Log(memberData.IsFromInheritance);
            }

            Debug.Log(methodData.Signature);
            Assert.AreEqual("public override void OverrideVirtualMethod()", methodData.Signature);
        }

        [Test]
        public void TestInterfaceMethod()
        {
            var methodData = TestClassImplementMethodDataArray.First(m =>
                ((IMemberData)m).Name == nameof(TestClassImplement.InterfaceMethod));
            if (methodData.TryAsIMemberData(out var memberData))
            {
                Debug.Log(memberData.IsFromInheritance);
            }

            Debug.Log(methodData.Signature);
            Assert.AreEqual("public void InterfaceMethod()", methodData.Signature);
        }

        #region Nested type: IInterface

        public interface IInterface
        {
            void InterfaceMethod();
        }

        #endregion

        #region Nested type: TestClassAbstract

        public abstract class TestClassAbstract
        {
            public virtual void OverrideVirtualMethod() { }
            public abstract void OverrideAbstractMethod();
        }

        #endregion

        #region Nested type: TestClassImplement

        /// <summary>
        /// 测试虚方法，抽象方法，override 方法
        /// </summary>
        public class TestClassImplement : TestClassAbstract, IInterface
        {
            #region IInterface Members

            public void InterfaceMethod()
            {
                Debug.Log("InterfaceMethod");
            }

            #endregion

            public override void OverrideAbstractMethod() { }
            public override void OverrideVirtualMethod() { }
        }

        #endregion
    }
}
