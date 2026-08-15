using Runestone.AesirInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Runestone.AesirInspector.Editor.Tests
{
    public class PropertyDataTests
    {
        [Test]
        [TestCaseSource(nameof(_defaultValueCases))]
        [TestCaseSource(nameof(_accessorCases))]
        public void Signature_MatchesExpected(string propertyName, string expectedSignature)
        {
            var allPropertyData = DefaultValuePropertyDataArray.Concat(AccessorPropertyDataArray);
            var propertyData = allPropertyData.First(p => ((MemberData)p).Name == propertyName);
            Assert.AreEqual(expectedSignature, propertyData.Signature);
        }

        #region Nested type: DefaultValueTestEnum

        enum DefaultValueTestEnum
        {
            A,
            B,
            C
        }

        #endregion

        #region Nested type: DefaultValueTestClass

        class DefaultValueTestClass
        {
            public DefaultValueTestClass() => StringPropertyInitOnCtor = "Hello World";
            public static int StaticIntPropertyWithDefaultValue { get; set; } = 1;
            public static float StaticFloatPropertyWithDefaultValue { get; set; } = 1f;
            public static bool StaticBoolPropertyWithDefaultValue { get; set; } = true;
            public static string StaticStringPropertyWithDefaultValue { get; set; } = "Hello";

            public static DefaultValueTestEnum StaticEnumPropertyWithDefaultValue { get; set; } =
                DefaultValueTestEnum.B;

            public int IntPropertyWithDefaultValue { get; internal set; } = 77;
            public float FloatPropertyWithDefaultValue { get; protected set; } = 77f;
            public bool BoolPropertyWithDefaultValue { get; private set; } = true;
            public string StringPropertyWithDefaultValue { get; set; } = "World";
            public DefaultValueTestEnum EnumPropertyWithDefaultValue { get; set; } = DefaultValueTestEnum.C;
            public string StringPropertyInitOnCtor { get; set; }
        }

        #endregion

        #region Nested type: AccessorTestClass

        class AccessorTestClass
        {
            public int IntPropertyPublicGetPublicSet { get; set; }
            public string StringPropertyPublicGetInternalSet { get; internal set; }
            public float FloatPropertyPublicGetProtectedSet { get; protected set; }
            public bool BoolPropertyPublicGetPrivateSet { get; private set; }
            public int IntPropertyInternalGetPublicSet { internal get; set; }
            public float FloatPropertyProtectedGetPublicSet { protected get; set; }
            public bool BoolPropertyPrivateGetPublicSet { private get; set; }
            public int StaticIntPropertyPublicGetPublicSet { get; set; }
        }

        #endregion

        #region Default value properties

        static readonly PropertyInfo[] DefaultValuePropertyInfos =
            typeof(DefaultValueTestClass).GetRuntimeProperties().ToArray();

        static readonly IPropertyData[] DefaultValuePropertyDataArray = DefaultValuePropertyInfos
            .Select(p => UnitTestAnalysisFactory.Default.CreatePropertyData(p)).ToArray();

        static readonly Dictionary<string, string> DefaultValueSignatureMaps = new Dictionary<string, string>
        {
            {
                nameof(DefaultValueTestClass.StaticIntPropertyWithDefaultValue),
                "public static int StaticIntPropertyWithDefaultValue { get; set; } = 1;"
            },
            {
                nameof(DefaultValueTestClass.StaticFloatPropertyWithDefaultValue),
                "public static float StaticFloatPropertyWithDefaultValue { get; set; } = 1f;"
            },
            {
                nameof(DefaultValueTestClass.StaticBoolPropertyWithDefaultValue),
                "public static bool StaticBoolPropertyWithDefaultValue { get; set; } = true;"
            },
            {
                nameof(DefaultValueTestClass.StaticStringPropertyWithDefaultValue),
                "public static string StaticStringPropertyWithDefaultValue { get; set; } = \"Hello\";"
            },
            {
                nameof(DefaultValueTestClass.StaticEnumPropertyWithDefaultValue),
                "public static PropertyDataTests.DefaultValueTestEnum StaticEnumPropertyWithDefaultValue { get; set; } = DefaultValueTestEnum.B;"
            }
        };

        static IEnumerable _defaultValueCases()
        {
            foreach (var kvp in DefaultValueSignatureMaps)
            {
                yield return new TestCaseData(kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region Accessor properties

        static readonly PropertyInfo[] AccessorPropertyInfos =
            typeof(AccessorTestClass).GetRuntimeProperties().ToArray();

        static readonly IPropertyData[] AccessorPropertyDataArray = AccessorPropertyInfos
            .Select(p => UnitTestAnalysisFactory.Default.CreatePropertyData(p)).ToArray();

        static readonly Dictionary<string, string> AccessorSignatureMaps = new Dictionary<string, string>
        {
            {
                nameof(AccessorTestClass.IntPropertyPublicGetPublicSet),
                "public int IntPropertyPublicGetPublicSet { get; set; }"
            },
            {
                nameof(AccessorTestClass.StringPropertyPublicGetInternalSet),
                "public string StringPropertyPublicGetInternalSet { get; internal set; }"
            },
            {
                nameof(AccessorTestClass.FloatPropertyPublicGetProtectedSet),
                "public float FloatPropertyPublicGetProtectedSet { get; protected set; }"
            },
            {
                nameof(AccessorTestClass.BoolPropertyPublicGetPrivateSet),
                "public bool BoolPropertyPublicGetPrivateSet { get; private set; }"
            },
            {
                nameof(AccessorTestClass.IntPropertyInternalGetPublicSet),
                "public int IntPropertyInternalGetPublicSet { internal get; set; }"
            },
            {
                nameof(AccessorTestClass.FloatPropertyProtectedGetPublicSet),
                "public float FloatPropertyProtectedGetPublicSet { protected get; set; }"
            },
            {
                nameof(AccessorTestClass.BoolPropertyPrivateGetPublicSet),
                "public bool BoolPropertyPrivateGetPublicSet { private get; set; }"
            },
            {
                nameof(AccessorTestClass.StaticIntPropertyPublicGetPublicSet),
                "public int StaticIntPropertyPublicGetPublicSet { get; set; }"
            }
        };

        static IEnumerable _accessorCases()
        {
            foreach (var kvp in AccessorSignatureMaps)
            {
                yield return new TestCaseData(kvp.Key, kvp.Value);
            }
        }

        #endregion
    }
}
