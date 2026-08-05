using Runestone.AesirInspector;
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

#pragma warning disable CS0067 // 事件从未使用过

namespace Runestone.AesirInspector.Editor.Tests
{
    public class MemberDataInheritanceTests
    {
        [Test]
        public void TestSystemObjectMethodIsInheritance()
        {
            var methodInfo = typeof(MethodDataInheritTests).GetRuntimeMethods()
                .First(m => m.Name == "GetHashCode");
            Debug.Log(methodInfo.DeclaringType);
            Debug.Log(methodInfo.ReflectedType);
            Debug.Log(methodInfo.GetBaseDefinition().DeclaringType);
            Assert.IsTrue(methodInfo.IsFromInheritance());
        }

        [Test]
        public void TestFieldIsFromInheritance()
        {
            var fieldsData = UnitTestAnalysisFactory.Default
                .CreateTypeData(typeof(TestClassImplement), UnitTestAnalysisFactory.Default)
                .RuntimeReflectedFieldsData;
            foreach (var fieldData in fieldsData)
            {
                var memberData = (MemberData)fieldData;
                Debug.Log(memberData.IsFromInheritance);
                Assert.IsTrue(memberData.IsFromInheritance);
            }
        }

        [Test]
        public void TestPropertyIsFromInheritance()
        {
            var propertiesData = UnitTestAnalysisFactory.Default
                .CreateTypeData(typeof(TestClassImplement), UnitTestAnalysisFactory.Default)
                .RuntimeReflectedPropertiesData;
            foreach (var propertyData in propertiesData)
            {
                var memberData = (MemberData)propertyData;
                Debug.Log(memberData.IsFromInheritance);
                Assert.IsTrue(memberData.IsFromInheritance);
            }
        }

        [Test]
        public void TestEventIsFromInheritance()
        {
            var eventsData = UnitTestAnalysisFactory.Default
                .CreateTypeData(typeof(TestClassImplement), UnitTestAnalysisFactory.Default)
                .RuntimeReflectedEventsData;
            foreach (var eventData in eventsData)
            {
                var memberData = (MemberData)eventData;
                Debug.Log(memberData.IsFromInheritance);
                Assert.IsTrue(memberData.IsFromInheritance);
            }
        }

        #region Nested type: TestClassBase

        class TestClassBase
        {
            protected int ProtectedIntField;
            public bool PublicBooleanField;
            protected string ProtectedStringProperty { get; set; }
            public float PublicFloatProperty { get; set; }
            protected event Action ProtectedEvent;
            public event Action PublicEvent;
            protected void ProtectedMethod() { }
            public void PublicMethod() { }
        }

        #endregion

        #region Nested type: TestClassImplement

        class TestClassImplement : TestClassBase { }

        #endregion
    }
}
