using Runestone.AesirInspector;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Runestone.AesirInspector.Editor.Tests
{
    public class FieldDataSignatureTests
    {
        [Test]
        [TestCaseSource(nameof(_instanceCases))]
        [TestCaseSource(nameof(_collectionCases))]
        [TestCaseSource(nameof(_delegateCases))]
        [TestCaseSource(nameof(_specialTypeCases))]
        public void Signature_MatchesExpected(string fieldName, string expectedSignature)
        {
            var allFieldData = InstanceFieldDataArray.Concat(CollectionFieldDataArray)
                .Concat(DelegateFieldDataArray).Concat(SpecialTypeFieldDataArray);
            var fieldData = allFieldData.First(f => ((MemberData)f).Name == fieldName);
            Assert.AreEqual(expectedSignature, fieldData.Signature);
        }

        #region Nested type: InstanceTestEnum

        enum InstanceTestEnum
        {
            Value1,
            Value2
        }

        #endregion

        #region Nested type: InstanceTestClass

        class InstanceTestClass
        {
            public bool BooleanField;
            public byte ByteField;
            public char CharField;
            public decimal DecimalField;
            public double DoubleField;
            public ScriptDocGeneratorTestEnum EnumField;
            public float FloatField;
            public int IntField;
            public long LongField;
            public InstanceTestEnum NestedEnumField;
            public sbyte SbyteField;
            public short ShortField;
            public string StringField;
            public uint UintField;
            public ulong UlongField;
            public ushort UshortField;
        }

        #endregion

        #region Nested type: CollectionTestClass

        class CollectionTestClass
        {
            #region 数组类型

            /// <summary>
            /// 数组字段
            /// </summary>
            public int[] ArrayField;

            /// <summary>
            /// 多维数组字段
            /// </summary>
            public int[,] MultiArrayField;

            /// <summary>
            /// 交错数组字段
            /// </summary>
            public int[][] JaggedArrayField;

            #endregion

            #region 泛型集合类型

            /// <summary>
            /// 列表字段
            /// </summary>
            public List<string> ListField;

            /// <summary>
            /// 字典字段
            /// </summary>
            public Dictionary<string, int> DictionaryField;

            /// <summary>
            /// 集合字段
            /// </summary>
            public HashSet<string> HashSetField;

            /// <summary>
            /// 有序字典字段
            /// </summary>
            public SortedDictionary<string, int> SortedDictionaryField;

            /// <summary>
            /// 有序列表字段
            /// </summary>
            public SortedList<string, int> SortedListField;

            /// <summary>
            /// 堆栈字段
            /// </summary>
            public Stack<string> StackField;

            /// <summary>
            /// 队列字段
            /// </summary>
            public Queue<int> QueueField;

            /// <summary>
            /// 链表字段
            /// </summary>
            public LinkedList<string> LinkedListField;

            #endregion

            #region 非泛型集合类型

            /// <summary>
            /// ArrayList字段
            /// </summary>
            public ArrayList ArrayListField;

            /// <summary>
            /// Hashtable字段
            /// </summary>
            public Hashtable HashtableField;

            #endregion

            #region 只读和并发集合类型

            /// <summary>
            /// 只读列表字段
            /// </summary>
            public IReadOnlyList<string> ReadOnlyListField;

            /// <summary>
            /// 只读字典字段
            /// </summary>
            public IReadOnlyDictionary<string, int> ReadOnlyDictionaryField;

            /// <summary>
            /// 并发字典字段
            /// </summary>
            public ConcurrentDictionary<string, int> ConcurrentDictionaryField;

            #endregion
        }

        #endregion

        #region Nested type: DelegateTestClass

        class DelegateTestClass
        {
            /// <summary>
            /// Action 字段
            /// </summary>
            public Action ActionField;

            /// <summary>
            /// Action 带参数字段
            /// </summary>
            public Action<int, string> ActionWithParamsField;

            /// <summary>
            /// Comparison 字段
            /// </summary>
            public Comparison<string> ComparisonField;

            /// <summary>
            /// Func 带参数字段
            /// </summary>
            public Func<int, string, bool> FuncWithParamsField;

            /// <summary>
            /// Predicate 字段
            /// </summary>
            public Predicate<int> PredicateField;
        }

        #endregion

        #region Nested type: SpecialTypeITestInterface

        /// <summary>
        /// 测试接口
        /// </summary>
        public interface SpecialTypeITestInterface { }

        #endregion

        #region Nested type: SpecialTypeTestAbstractClass

        /// <summary>
        /// 测试抽象类
        /// </summary>
        public abstract class SpecialTypeTestAbstractClass
        {
            public abstract void AbstractMethod();
        }

        #endregion

        #region Nested type: SpecialTypeTestClass

        public class SpecialTypeTestClass
        {
            /// <summary>
            /// 抽象类字段
            /// </summary>
            public SpecialTypeTestAbstractClass AbstractField;

            /// <summary>
            /// 动态字段
            /// </summary>
            public dynamic DynamicField;

            /// <summary>
            /// 接口字段
            /// </summary>
            public SpecialTypeITestInterface InterfaceField;

            /// <summary>
            /// 可空字段
            /// </summary>
            public int? NullableField = null;
        }

        #endregion

        #region Instance type fields

        static readonly FieldInfo[] InstanceFieldInfos =
            typeof(InstanceTestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] InstanceFieldDataArray = InstanceFieldInfos
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static IEnumerable _instanceCases()
        {
            yield return new TestCaseData(nameof(InstanceTestClass.StringField),
                "public string StringField;");
            yield return new TestCaseData(nameof(InstanceTestClass.IntField), "public int IntField;");
            yield return new TestCaseData(nameof(InstanceTestClass.FloatField), "public float FloatField;");
            yield return new TestCaseData(nameof(InstanceTestClass.BooleanField),
                "public bool BooleanField;");
            yield return new TestCaseData(nameof(InstanceTestClass.CharField), "public char CharField;");
            yield return new TestCaseData(nameof(InstanceTestClass.ByteField), "public byte ByteField;");
            yield return new TestCaseData(nameof(InstanceTestClass.SbyteField), "public sbyte SbyteField;");
            yield return new TestCaseData(nameof(InstanceTestClass.ShortField), "public short ShortField;");
            yield return new TestCaseData(nameof(InstanceTestClass.UshortField),
                "public ushort UshortField;");
            yield return new TestCaseData(nameof(InstanceTestClass.LongField), "public long LongField;");
            yield return new TestCaseData(nameof(InstanceTestClass.UlongField), "public ulong UlongField;");
            yield return new TestCaseData(nameof(InstanceTestClass.UintField), "public uint UintField;");
            yield return new TestCaseData(nameof(InstanceTestClass.DoubleField),
                "public double DoubleField;");
            yield return new TestCaseData(nameof(InstanceTestClass.DecimalField),
                "public decimal DecimalField;");
            yield return new TestCaseData(nameof(InstanceTestClass.EnumField),
                "public ScriptDocGeneratorTestEnum EnumField;");
            yield return new TestCaseData(nameof(InstanceTestClass.NestedEnumField),
                "public FieldDataSignatureTests.InstanceTestEnum NestedEnumField;");
        }

        #endregion

        #region Collection type fields

        static readonly FieldInfo[] CollectionFieldInfos =
            typeof(CollectionTestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] CollectionFieldDataArray = CollectionFieldInfos
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static readonly Dictionary<string, string> CollectionSignatureMaps = new Dictionary<string, string>
        {
            { nameof(CollectionTestClass.ArrayField), "public int[] ArrayField;" },
            { nameof(CollectionTestClass.MultiArrayField), "public int[,] MultiArrayField;" },
            { nameof(CollectionTestClass.JaggedArrayField), "public int[][] JaggedArrayField;" },
            { nameof(CollectionTestClass.ListField), "public List<string> ListField;" },
            {
                nameof(CollectionTestClass.DictionaryField), "public Dictionary<string, int> DictionaryField;"
            },
            { nameof(CollectionTestClass.HashSetField), "public HashSet<string> HashSetField;" },
            {
                nameof(CollectionTestClass.SortedDictionaryField),
                "public SortedDictionary<string, int> SortedDictionaryField;"
            },
            {
                nameof(CollectionTestClass.SortedListField), "public SortedList<string, int> SortedListField;"
            },
            { nameof(CollectionTestClass.StackField), "public Stack<string> StackField;" },
            { nameof(CollectionTestClass.QueueField), "public Queue<int> QueueField;" },
            { nameof(CollectionTestClass.LinkedListField), "public LinkedList<string> LinkedListField;" },
            { nameof(CollectionTestClass.ArrayListField), "public ArrayList ArrayListField;" },
            { nameof(CollectionTestClass.HashtableField), "public Hashtable HashtableField;" },
            {
                nameof(CollectionTestClass.ReadOnlyListField),
                "public IReadOnlyList<string> ReadOnlyListField;"
            },
            {
                nameof(CollectionTestClass.ReadOnlyDictionaryField),
                "public IReadOnlyDictionary<string, int> ReadOnlyDictionaryField;"
            },
            {
                nameof(CollectionTestClass.ConcurrentDictionaryField),
                "public ConcurrentDictionary<string, int> ConcurrentDictionaryField;"
            }
        };

        static IEnumerable _collectionCases()
        {
            foreach (var kvp in CollectionSignatureMaps)
            {
                yield return new TestCaseData(kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region Delegate type fields

        static readonly FieldInfo[] DelegateFieldInfos =
            typeof(DelegateTestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] DelegateFieldDataArray = DelegateFieldInfos
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static readonly Dictionary<string, string> DelegateSignatureMaps = new Dictionary<string, string>
        {
            { "ActionField", "public Action ActionField;" },
            { "ActionWithParamsField", "public Action<int, string> ActionWithParamsField;" },
            { "FuncWithParamsField", "public Func<int, string, bool> FuncWithParamsField;" },
            { "PredicateField", "public Predicate<int> PredicateField;" },
            { "ComparisonField", "public Comparison<string> ComparisonField;" }
        };

        static IEnumerable _delegateCases()
        {
            foreach (var kvp in DelegateSignatureMaps)
            {
                yield return new TestCaseData(kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region Special type fields

        static readonly FieldInfo[] SpecialTypeFieldInfos =
            typeof(SpecialTypeTestClass).GetRuntimeFields().ToArray();

        static readonly IFieldData[] SpecialTypeFieldDataArray = SpecialTypeFieldInfos
            .Select(f => UnitTestAnalysisFactory.Default.CreateFieldData(f)).ToArray();

        static IEnumerable _specialTypeCases()
        {
            yield return new TestCaseData(nameof(SpecialTypeTestClass.AbstractField),
                "public FieldDataSignatureTests.SpecialTypeTestAbstractClass AbstractField;");
            yield return new TestCaseData(nameof(SpecialTypeTestClass.DynamicField),
                "public dynamic DynamicField;");
            yield return new TestCaseData(nameof(SpecialTypeTestClass.InterfaceField),
                "public FieldDataSignatureTests.SpecialTypeITestInterface InterfaceField;");
            yield return new TestCaseData(nameof(SpecialTypeTestClass.NullableField),
                "public int? NullableField;");
        }

        #endregion
    }
}
