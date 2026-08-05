using Runestone.AesirInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// 测试常量和静态字段的默认值
    /// </summary>
    public class FieldDataDefaultValueTests
    {
        [Test]
        [TestCaseSource(nameof(_constCases))]
        [TestCaseSource(nameof(_staticCases))]
        public void Signature_MatchesExpected(string fieldName, string expectedSignature)
        {
            var allFieldData = ConstTestFieldData.Concat(StaticTestFieldData);
            var fieldData = allFieldData.First(f => ((MemberData)f).Name == fieldName);
            Assert.AreEqual(expectedSignature, fieldData.Signature);
        }

        /// <summary>
        /// decimal 类型可以编写 const，但是反射时，const 会变成静态只读
        /// </summary>
        [Test]
        public void Signature_DecimalConstField_IsNotConst()
        {
            const string fieldName = nameof(ConstTestClass.DECIMAL_CONST_FIELD);
            var fieldData = ConstTestFieldData.First(f => ((MemberData)f).Name == fieldName);
            Assert.AreNotEqual(ConstSignatureMaps[fieldName], fieldData.Signature);
        }

        #region Nested type: ConstTestEnum

        enum ConstTestEnum
        {
            Value1,
            Value2,
            Value3
        }

        #endregion

        #region Nested type: ConstTestClass

        class ConstTestClass
        {
            /// <summary>
            /// 字符串常量字段
            /// </summary>
            public const string STRING_CONST_FIELD = "Hello, World!";

            /// <summary>
            /// 整型常量字段
            /// </summary>
            public const int INT_CONST_FIELD = 2147483647;

            /// <summary>
            /// 单精度浮点型常量字段
            /// </summary>
            public const float FLOAT_CONST_FIELD = 3.14159f;

            /// <summary>
            /// 布尔常量字段
            /// </summary>
            public const bool BOOLEAN_CONST_FIELD = true;

            /// <summary>
            /// 字符常量字段
            /// </summary>
            public const char CHAR_CONST_FIELD = 'A';

            /// <summary>
            /// 字节常量字段
            /// </summary>
            public const byte BYTE_CONST_FIELD = 255;

            /// <summary>
            /// 有符号字节常量字段
            /// </summary>
            public const sbyte SBYTE_CONST_FIELD = -128;

            /// <summary>
            /// 短整型常量字段
            /// </summary>
            public const short SHORT_CONST_FIELD = 32767;

            /// <summary>
            /// 无符号短整型常量字段
            /// </summary>
            public const ushort USHORT_CONST_FIELD = 65535;

            /// <summary>
            /// 长整型常量字段
            /// </summary>
            public const long LONG_CONST_FIELD = 9223372036854775807L;

            /// <summary>
            /// 无符号长整型常量字段
            /// </summary>
            public const ulong ULONG_CONST_FIELD = 18446744073709551615ul;

            /// <summary>
            /// 无符号整型常量字段
            /// </summary>
            public const uint UINT_CONST_FIELD = 4294967295u;

            /// <summary>
            /// 双精度浮点型常量字段
            /// </summary>
            public const double DOUBLE_CONST_FIELD = 2.71828182845904d;

            /// <summary>
            /// 十进制常量字段
            /// </summary>
            public const decimal DECIMAL_CONST_FIELD = 123.456m;

            /// <summary>
            /// 枚举常量字段
            /// </summary>
            public const ScriptDocGeneratorTestEnum ENUM_CONST_FIELD = ScriptDocGeneratorTestEnum.Value1;

            /// <summary>
            /// 嵌套类的枚举常量字段
            /// </summary>
            public const ConstTestEnum NESTED_ENUM_CONST_FIELD = ConstTestEnum.Value3;
        }

        #endregion

        #region Nested type: StaticTestEnum

        enum StaticTestEnum
        {
            Value1,
            Value2,
            Value3
        }

        #endregion

        #region Nested type: StaticTestClass

        class StaticTestClass
        {
            /// <summary>
            /// 字符串静态字段
            /// </summary>
            public static string StringStaticField = "Hello, World!";

            /// <summary>
            /// 整型静态字段
            /// </summary>
            public static int INTStaticField = 2147483647;

            /// <summary>
            /// 单精度浮点型静态字段
            /// </summary>
            public static float FloatStaticField = 3.14159f;

            /// <summary>
            /// 布尔静态字段
            /// </summary>
            public static bool BooleanStaticField = true;

            /// <summary>
            /// 字符静态字段
            /// </summary>
            public static char CharStaticField = 'A';

            /// <summary>
            /// 字节静态字段
            /// </summary>
            public static byte ByteStaticField = 255;

            /// <summary>
            /// 有符号字节静态字段
            /// </summary>
            public static sbyte SbyteStaticField = -128;

            /// <summary>
            /// 短整型静态字段
            /// </summary>
            public static short ShortStaticField = 32767;

            /// <summary>
            /// 无符号短整型静态字段
            /// </summary>
            public static ushort UshortStaticField = 65535;

            /// <summary>
            /// 长整型静态字段
            /// </summary>
            public static long LongStaticField = 9223372036854775807L;

            /// <summary>
            /// 无符号长整型静态字段
            /// </summary>
            public static ulong UlongStaticField = 18446744073709551615ul;

            /// <summary>
            /// 无符号整型静态字段
            /// </summary>
            public static uint UintStaticField = 4294967295u;

            /// <summary>
            /// 双精度浮点型静态字段
            /// </summary>
            public static double DoubleStaticField = 2.71828182845904d;

            /// <summary>
            /// 十进制静态字段
            /// </summary>
            public static decimal DecimalStaticField = 123.456m;

            /// <summary>
            /// 枚举静态字段
            /// </summary>
            public static ScriptDocGeneratorTestEnum EnumStaticField = ScriptDocGeneratorTestEnum.Value2;

            /// <summary>
            /// 嵌套类的枚举静态字段
            /// </summary>
            public static StaticTestEnum NestedEnumStaticField = StaticTestEnum.Value3;
        }

        #endregion

        #region Const fields

        static readonly FieldInfo[] ConstTestFields = typeof(ConstTestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] ConstTestFieldData = ConstTestFields
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static readonly Dictionary<string, string> ConstSignatureMaps = new Dictionary<string, string>
        {
            {
                nameof(ConstTestClass.STRING_CONST_FIELD),
                "public const string " + nameof(ConstTestClass.STRING_CONST_FIELD) + " = \"Hello, World!\";"
            },
            {
                nameof(ConstTestClass.INT_CONST_FIELD),
                "public const int " + nameof(ConstTestClass.INT_CONST_FIELD) + " = 2147483647;"
            },
            {
                nameof(ConstTestClass.FLOAT_CONST_FIELD),
                "public const float " + nameof(ConstTestClass.FLOAT_CONST_FIELD) + " = 3.14159f;"
            },
            {
                nameof(ConstTestClass.BOOLEAN_CONST_FIELD),
                "public const bool " + nameof(ConstTestClass.BOOLEAN_CONST_FIELD) + " = true;"
            },
            {
                nameof(ConstTestClass.CHAR_CONST_FIELD),
                "public const char " + nameof(ConstTestClass.CHAR_CONST_FIELD) + " = 'A';"
            },
            {
                nameof(ConstTestClass.BYTE_CONST_FIELD),
                "public const byte " + nameof(ConstTestClass.BYTE_CONST_FIELD) + " = 255;"
            },
            {
                nameof(ConstTestClass.SBYTE_CONST_FIELD),
                "public const sbyte " + nameof(ConstTestClass.SBYTE_CONST_FIELD) + " = -128;"
            },
            {
                nameof(ConstTestClass.SHORT_CONST_FIELD),
                "public const short " + nameof(ConstTestClass.SHORT_CONST_FIELD) + " = 32767;"
            },
            {
                nameof(ConstTestClass.USHORT_CONST_FIELD),
                "public const ushort " + nameof(ConstTestClass.USHORT_CONST_FIELD) + " = 65535;"
            },
            {
                nameof(ConstTestClass.LONG_CONST_FIELD),
                "public const long " + nameof(ConstTestClass.LONG_CONST_FIELD) + " = 9223372036854775807L;"
            },
            {
                nameof(ConstTestClass.ULONG_CONST_FIELD),
                "public const ulong " + nameof(ConstTestClass.ULONG_CONST_FIELD) +
                " = 18446744073709551615ul;"
            },
            {
                nameof(ConstTestClass.UINT_CONST_FIELD),
                "public const uint " + nameof(ConstTestClass.UINT_CONST_FIELD) + " = 4294967295u;"
            },
            {
                nameof(ConstTestClass.DOUBLE_CONST_FIELD),
                "public const double " + nameof(ConstTestClass.DOUBLE_CONST_FIELD) + " = 2.71828182845904d;"
            },
            {
                nameof(ConstTestClass.ENUM_CONST_FIELD),
                "public const " + nameof(ScriptDocGeneratorTestEnum) + " " +
                nameof(ConstTestClass.ENUM_CONST_FIELD) + " = " + nameof(ScriptDocGeneratorTestEnum) +
                ".Value1;"
            },
            {
                nameof(ConstTestClass.NESTED_ENUM_CONST_FIELD),
                "public const " + nameof(FieldDataDefaultValueTests) + "." + nameof(ConstTestEnum) + " " +
                nameof(ConstTestClass.NESTED_ENUM_CONST_FIELD) + " = " + nameof(ConstTestEnum) + ".Value3;"
            },
            {
                nameof(ConstTestClass.DECIMAL_CONST_FIELD),
                "public const decimal " + nameof(ConstTestClass.DECIMAL_CONST_FIELD) + " = 123.456m;"
            }
        };

        static IEnumerable _constCases()
        {
            foreach (var kvp in ConstSignatureMaps)
            {
                if (kvp.Key == nameof(ConstTestClass.DECIMAL_CONST_FIELD))
                {
                    continue;
                }

                yield return new TestCaseData(kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region Static fields

        static readonly FieldInfo[] StaticTestFields = typeof(StaticTestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] StaticTestFieldData = StaticTestFields
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static readonly Dictionary<string, string> StaticSignatureMaps = new Dictionary<string, string>
        {
            {
                nameof(StaticTestClass.StringStaticField),
                "public static string " + nameof(StaticTestClass.StringStaticField) + " = \"Hello, World!\";"
            },
            {
                nameof(StaticTestClass.INTStaticField),
                "public static int " + nameof(StaticTestClass.INTStaticField) + " = 2147483647;"
            },
            {
                nameof(StaticTestClass.FloatStaticField),
                "public static float " + nameof(StaticTestClass.FloatStaticField) + " = 3.14159f;"
            },
            {
                nameof(StaticTestClass.BooleanStaticField),
                "public static bool " + nameof(StaticTestClass.BooleanStaticField) + " = true;"
            },
            {
                nameof(StaticTestClass.CharStaticField),
                "public static char " + nameof(StaticTestClass.CharStaticField) + " = 'A';"
            },
            {
                nameof(StaticTestClass.ByteStaticField),
                "public static byte " + nameof(StaticTestClass.ByteStaticField) + " = 255;"
            },
            {
                nameof(StaticTestClass.SbyteStaticField),
                "public static sbyte " + nameof(StaticTestClass.SbyteStaticField) + " = -128;"
            },
            {
                nameof(StaticTestClass.ShortStaticField),
                "public static short " + nameof(StaticTestClass.ShortStaticField) + " = 32767;"
            },
            {
                nameof(StaticTestClass.UshortStaticField),
                "public static ushort " + nameof(StaticTestClass.UshortStaticField) + " = 65535;"
            },
            {
                nameof(StaticTestClass.LongStaticField),
                "public static long " + nameof(StaticTestClass.LongStaticField) + " = 9223372036854775807L;"
            },
            {
                nameof(StaticTestClass.UlongStaticField),
                "public static ulong " + nameof(StaticTestClass.UlongStaticField) +
                " = 18446744073709551615ul;"
            },
            {
                nameof(StaticTestClass.UintStaticField),
                "public static uint " + nameof(StaticTestClass.UintStaticField) + " = 4294967295u;"
            },
            {
                nameof(StaticTestClass.DoubleStaticField),
                "public static double " + nameof(StaticTestClass.DoubleStaticField) + " = 2.71828182845904d;"
            },
            {
                nameof(StaticTestClass.DecimalStaticField),
                "public static decimal " + nameof(StaticTestClass.DecimalStaticField) + " = 123.456m;"
            },
            {
                nameof(StaticTestClass.EnumStaticField),
                "public static " + nameof(ScriptDocGeneratorTestEnum) + " " +
                nameof(StaticTestClass.EnumStaticField) + " = " + nameof(ScriptDocGeneratorTestEnum) +
                ".Value2;"
            },
            {
                nameof(StaticTestClass.NestedEnumStaticField),
                "public static " + nameof(FieldDataDefaultValueTests) + "." + nameof(StaticTestEnum) + " " +
                nameof(StaticTestClass.NestedEnumStaticField) + " = " + nameof(StaticTestEnum) + ".Value3;"
            }
        };

        static IEnumerable _staticCases()
        {
            foreach (var kvp in StaticSignatureMaps)
            {
                yield return new TestCaseData(kvp.Key, kvp.Value);
            }
        }

        #endregion
    }
}
