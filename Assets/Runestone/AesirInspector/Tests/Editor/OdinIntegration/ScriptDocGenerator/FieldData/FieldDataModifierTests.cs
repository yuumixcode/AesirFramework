using Runestone.AesirInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

// ReSharper disable UnusedMember.Local

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// 测试字段的不同修饰符（复合关键字和访问修饰符）
    /// </summary>
    public class FieldDataModifierTests
    {
        [Test]
        [TestCaseSource(nameof(_compositeKeywordCases))]
        [TestCaseSource(nameof(_accessModifierCases))]
        public void Signature_MatchesExpected(string fieldName, string expectedSignature)
        {
            var allFieldData = CompositeKeywordFieldData.Concat(AccessModifierFieldData);
            var fieldData = allFieldData.First(f => ((MemberData)f).Name == fieldName);
            Assert.AreEqual(expectedSignature, fieldData.Signature);
        }

        #region Nested type: CompositeKeywordTestClass

        class CompositeKeywordTestClass
        {
            /// <summary>
            /// 常量字段
            /// </summary>
            public const bool CONST_FIELD = true;

            /// <summary>
            /// 静态只读字段
            /// </summary>
            public static readonly bool StaticReadOnlyField;

            /// <summary>
            /// 静态字段
            /// </summary>
            public static bool StaticField;

            /// <summary>
            /// 只读字段
            /// </summary>
            public readonly bool ReadOnlyField;
        }

        #endregion

        #region Nested type: AccessModifierTestClass

        class AccessModifierTestClass
        {
            /// <summary>
            /// 私有字段
            /// </summary>
            int _privateField;

            /// <summary>
            /// 内部字段
            /// </summary>
            internal int InternalField;

            /// <summary>
            /// 私有受保护字段
            /// </summary>
            private protected int PrivateProtectedField;

            /// <summary>
            /// 受保护字段
            /// </summary>
            protected int ProtectedField;

            /// <summary>
            /// 受保护内部字段
            /// </summary>
            protected internal int ProtectedInternalField;

            /// <summary>
            /// 公共字段
            /// </summary>
            public int PublicField;
        }

        #endregion

        #region Composite keyword fields

        static readonly FieldInfo[] CompositeKeywordFields =
            typeof(CompositeKeywordTestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] CompositeKeywordFieldData = CompositeKeywordFields
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static readonly Dictionary<string, string> CompositeKeywordSignatureMaps =
            new Dictionary<string, string>
            {
                { "CONST_FIELD", "public const bool CONST_FIELD = true;" },
                { "StaticReadOnlyField", "public static readonly bool StaticReadOnlyField;" },
                { "StaticField", "public static bool StaticField;" },
                { "ReadOnlyField", "public readonly bool ReadOnlyField;" }
            };

        static IEnumerable _compositeKeywordCases()
        {
            foreach (var kvp in CompositeKeywordSignatureMaps)
            {
                yield return new TestCaseData(kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region Access modifier fields

        static readonly FieldInfo[] AccessModifierFields =
            typeof(AccessModifierTestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] AccessModifierFieldData = AccessModifierFields
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static readonly Dictionary<string, string> AccessModifierSignatureMaps =
            new Dictionary<string, string>
            {
                { "_privateField", "private int _privateField;" },
                { "InternalField", "internal int InternalField;" },
                { "PrivateProtectedField", "private protected int PrivateProtectedField;" },
                { "ProtectedField", "protected int ProtectedField;" },
                { "ProtectedInternalField", "protected internal int ProtectedInternalField;" },
                { "PublicField", "public int PublicField;" }
            };

        static IEnumerable _accessModifierCases()
        {
            foreach (var kvp in AccessModifierSignatureMaps)
            {
                yield return new TestCaseData(kvp.Key, kvp.Value);
            }
        }

        #endregion
    }
}
