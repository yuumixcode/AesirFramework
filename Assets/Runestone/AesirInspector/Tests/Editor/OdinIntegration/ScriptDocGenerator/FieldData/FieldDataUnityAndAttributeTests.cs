using Runestone.AesirInspector;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

#pragma warning disable CS0618 // 类型或成员已过时

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// 测试 Unity 的类型以及带特性的字段
    /// </summary>
    public class FieldDataUnityAndAttributeTests
    {
        static readonly FieldInfo[] FieldInfos = typeof(TestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] FieldDataArray = FieldInfos
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static IEnumerable _fullDeclarationEqualsCases()
        {
            yield return new TestCaseData(nameof(TestClass.gameObjectField),
                "public GameObject gameObjectField;");
            yield return new TestCaseData(nameof(TestClass.transformField),
                "public Transform transformField;");
            yield return new TestCaseData(nameof(TestClass.rigidbodyField),
                "public Rigidbody rigidbodyField;");
            yield return new TestCaseData(nameof(TestClass.vector3Field), "public Vector3 vector3Field;");
            yield return new TestCaseData(nameof(TestClass.quaternionField), @"[SerializeField]
[UnityEngine.Tooltip(""This is a tooltip"")]
[UnityEngine.Range(0, 100)]
public Quaternion quaternionField;");
        }

        static IEnumerable _fullDeclarationContainsCases()
        {
            yield return new TestCaseData(nameof(TestClass.colorField), @"[UnityEngine.ColorUsage(true, true)]
public Color colorField;");
            yield return new TestCaseData(nameof(TestClass.layerMaskField),
                @"[System.Obsolete(""Use newField instead"")]
public LayerMask layerMaskField;");
        }

        [Test]
        [TestCaseSource(nameof(_fullDeclarationEqualsCases))]
        public void FullDeclarationWithAttributes_MatchesExpected(string fieldName, string expected)
        {
            var fieldData = FieldDataArray.First(f => ((MemberData)f).Name == fieldName);
            Assert.AreEqual(expected, fieldData.FullDeclarationWithAttributes);
        }

        [Test]
        [TestCaseSource(nameof(_fullDeclarationContainsCases))]
        public void FullDeclarationWithAttributes_ContainsExpected(string fieldName, string expected)
        {
            var fieldData = FieldDataArray.First(f => ((MemberData)f).Name == fieldName);
            Assert.IsTrue(fieldData.FullDeclarationWithAttributes.Contains(expected));
        }

        #region Nested type: TestClass

        [Serializable]
        class TestClass
        {
            #region Serialized Fields

            /// <summary>
            /// GameObject 字段
            /// </summary>
            public GameObject gameObjectField;

            /// <summary>
            /// Transform 字段
            /// </summary>
            public Transform transformField;

            /// <summary>
            /// Rigidbody 字段
            /// </summary>
            public Rigidbody rigidbodyField;

            /// <summary>
            /// Vector3 字段
            /// </summary>
            public Vector3 vector3Field = new Vector3(1, 1, 1);

            /// <summary>
            /// Quaternion 字段
            /// </summary>
            [SerializeField]
            [Tooltip("This is a tooltip")]
            [UnityEngine.Range(0, 100)]
            public Quaternion quaternionField = new Quaternion(0, 0, 0, 1);

            /// <summary>
            /// Color 字段
            /// </summary>
            [ColorUsage(true, true)]
            public Color colorField = Color.white;

            /// <summary>
            /// LayerMask 字段
            /// </summary>
            [Obsolete("Use newField instead")]
            public LayerMask layerMaskField;

            #endregion
        }

        #endregion
    }
}
