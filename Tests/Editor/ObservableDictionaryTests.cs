using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="ObservableDictionary{TKey, TValue}" /> 的增删改清空事件与无变更跳过行为。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ObservableDictionary 是 Model 层向 View 层暴露只读订阅的可观察字典载体，
    ///     索引器按"新键 Added / 已有键 Updated（含旧值）"分流是其核心契约。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="ObservableDictionary{TKey, TValue}" />
    public class ObservableDictionaryTests
    {
        /// <summary>
        /// 验证 Add 触发 Added 事件且参数为新增键值对。
        /// </summary>
        [Test]
        public void Add_FiresAddedEventWithPair()
        {
            var dict = new ObservableDictionary<string, int>();
            var received = new List<KeyValuePair<string, int>>();

            dict.AddAddedListener(received.Add);
            dict.Add("hp", 100);

            Assert.AreEqual(1, received.Count, "一次添加应触发一次 Added");
            Assert.AreEqual("hp", received[0].Key);
            Assert.AreEqual(100, received[0].Value);
            AesirArchitectureDebug.LogTestInfo("Add: 触发 Added 且键值正确");
        }

        /// <summary>
        /// 验证索引器分流：新键触发 Added，已有键赋不同值触发 Updated（含旧值），赋相同值跳过。
        /// </summary>
        [Test]
        public void Indexer_NewKey_Added_ExistingKey_UpdatedOrSkipped()
        {
            var dict = new ObservableDictionary<string, int>();
            var added = new List<KeyValuePair<string, int>>();
            var updated = new List<DictionaryUpdateEventArgs<string, int>>();

            dict.AddAddedListener(added.Add);
            dict.AddUpdatedListener(updated.Add);

            dict["hp"] = 100;
            Assert.AreEqual(1, added.Count, "新键应触发 Added");
            Assert.AreEqual(0, updated.Count, "新键不应触发 Updated");

            dict["hp"] = 80;
            Assert.AreEqual(1, added.Count, "已有键赋值不应触发 Added");
            Assert.AreEqual(1, updated.Count, "已有键赋不同值应触发 Updated");
            Assert.AreEqual("hp", updated[0].Key);
            Assert.AreEqual(100, updated[0].OldValue);
            Assert.AreEqual(80, updated[0].NewValue);

            dict["hp"] = 80;
            Assert.AreEqual(1, updated.Count, "已有键赋相同值不应触发 Updated");
            AesirArchitectureDebug.LogTestInfo("索引器分流: 新键 Added / 已有键 Updated / 相同值跳过");
        }

        /// <summary>
        /// 验证 Remove 触发 Removed 事件且参数含被移除的值；移除不存在的键返回 false 且不触发。
        /// </summary>
        [Test]
        public void Remove_FiresWithRemovedValue_MissingKey_ReturnsFalse()
        {
            var dict = new ObservableDictionary<string, int> { ["hp"] = 100 };
            var received = new List<KeyValuePair<string, int>>();

            dict.AddRemovedListener(received.Add);
            Assert.IsFalse(dict.Remove("mp"), "移除不存在的键应返回 false");
            Assert.AreEqual(0, received.Count, "移除不存在的键不应触发 Removed");

            Assert.IsTrue(dict.Remove("hp"), "移除存在的键应返回 true");
            Assert.AreEqual(1, received.Count, "移除存在的键应触发一次 Removed");
            Assert.AreEqual("hp", received[0].Key);
            Assert.AreEqual(100, received[0].Value);
            AesirArchitectureDebug.LogTestInfo("Remove: 参数含被移除值，缺失键不触发");
        }

        /// <summary>
        /// 验证 TryGetValue 与 ContainsKey 的读写路径。
        /// </summary>
        [Test]
        public void TryGetValue_And_ContainsKey_WorkCorrectly()
        {
            var dict = new ObservableDictionary<string, int> { ["hp"] = 100 };

            Assert.IsTrue(dict.ContainsKey("hp"), "存在的键应命中 ContainsKey");
            Assert.IsFalse(dict.ContainsKey("mp"));

            Assert.IsTrue(dict.TryGetValue("hp", out int value), "存在的键应命中 TryGetValue");
            Assert.AreEqual(100, value);
            Assert.IsFalse(dict.TryGetValue("mp", out int missing), "不存在的键 TryGetValue 应返回 false");
            Assert.AreEqual(0, missing, "未命中时 out 值应为类型默认值");
            AesirArchitectureDebug.LogTestInfo("TryGetValue/ContainsKey: 读写路径正确");
        }

        /// <summary>
        /// 验证非空字典 Clear 触发 Cleared，空字典 Clear 不触发。
        /// </summary>
        [Test]
        public void Clear_FiresOnlyWhenNotEmpty()
        {
            var dict = new ObservableDictionary<string, int> { ["hp"] = 100 };
            var count = 0;

            dict.AddClearedListener(() => count++);

            dict.Clear();
            Assert.AreEqual(1, count, "非空字典清空应触发一次 Cleared");

            dict.Clear();
            Assert.AreEqual(1, count, "空字典清空不应触发 Cleared");
            AesirArchitectureDebug.LogTestInfo("Clear: 仅非空清空触发");
        }

        /// <summary>
        /// 验证监听句柄 Dispose 后不再收到通知，ClearListeners 清空全部监听。
        /// </summary>
        [Test]
        public void HandleDispose_And_ClearListeners_StopNotifications()
        {
            var dict = new ObservableDictionary<string, int>();
            var addCount = 0;

            void OnAdded(KeyValuePair<string, int> _)
            {
                addCount++;
            }

            var handle = dict.AddAddedListener(OnAdded);
            dict.Add("a", 1);
            Assert.AreEqual(1, addCount, "移除前应正常收到通知");

            handle.Dispose();
            dict.Add("b", 2);
            Assert.AreEqual(1, addCount, "句柄 Dispose 后不应再收到通知");

            dict.AddAddedListener(OnAdded);
            dict.ClearListeners();
            dict.Add("c", 3);
            Assert.AreEqual(1, addCount, "ClearListeners 清空全部监听后不应再收到通知");
            AesirArchitectureDebug.LogTestInfo("句柄/ClearListeners: 正确停止通知");
        }

        /// <summary>
        /// 验证带初始键值构造不触发任何事件，且 Keys、Values 与枚举可用。
        /// </summary>
        [Test]
        public void Constructor_WithInitialItems_NoEvents_Enumerable()
        {
            var addCount = 0;
            var dict = new ObservableDictionary<string, int>(
                new[] { new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2) });
            dict.AddAddedListener(_ => addCount++);

            Assert.AreEqual(0, addCount, "初始键值构造不应触发 Added");
            Assert.AreEqual(2, dict.Count);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, (IEnumerable<string>)dict.Keys, "Keys 应包含全部键");
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, (IEnumerable<int>)dict.Values, "Values 应包含全部值");

            var enumerated = new List<KeyValuePair<string, int>>();
            foreach (KeyValuePair<string, int> pair in (IEnumerable<KeyValuePair<string, int>>)dict)
            {
                enumerated.Add(pair);
            }

            Assert.AreEqual(2, enumerated.Count, "枚举应返回全部键值对");
            AesirArchitectureDebug.LogTestInfo("初始构造: 不触发事件且可枚举");
        }

        /// <summary>
        /// 验证重复添加同一键抛异常（fail-fast），且集合状态不变。
        /// </summary>
        [Test]
        public void Add_DuplicateKey_ThrowsAndKeepsState()
        {
            var dict = new ObservableDictionary<string, int> { ["hp"] = 100 };
            var received = new List<KeyValuePair<string, int>>();
            dict.AddAddedListener(received.Add);

            Assert.Throws<System.ArgumentException>(() => dict.Add("hp", 200), "重复添加应抛 ArgumentException");
            Assert.AreEqual(0, received.Count, "添加失败不应触发 Added");
            Assert.AreEqual(100, dict["hp"], "添加失败不应改变已有键值");
            AesirArchitectureDebug.LogTestInfo("重复添加: fail-fast 且状态不变");
        }

        /// <summary>
        /// 验证通过组合接口 <see cref="IObservableDictionary{TKey, TValue}" /> 访问双链同名成员不再有多义性（CS0229）。
        /// </summary>
        /// <remarks>若接口未用 new 重声明统一 IDictionary/IReadOnlyDictionary 双链成员，本测试将因编译多义性错误而失败。</remarks>
        [Test]
        public void CombinedInterface_Access_NoAmbiguity()
        {
            IObservableDictionary<string, int> dict = new ObservableDictionary<string, int> { ["hp"] = 100 };

            Assert.AreEqual(1, dict.Count, "组合接口访问 Count 应无多义性");
            Assert.AreEqual(100, dict["hp"], "组合接口访问索引器应无多义性");
            Assert.IsTrue(dict.ContainsKey("hp"), "组合接口访问 ContainsKey 应无多义性");
            Assert.IsTrue(dict.TryGetValue("hp", out int value), "组合接口访问 TryGetValue 应无多义性");
            Assert.AreEqual(100, value);
            CollectionAssert.AreEquivalent(new[] { "hp" }, (IEnumerable<string>)dict.Keys, "组合接口访问 Keys 应无多义性");
            CollectionAssert.AreEquivalent(new[] { 100 }, (IEnumerable<int>)dict.Values, "组合接口访问 Values 应无多义性");
            dict["hp"] = 80;
            Assert.AreEqual(80, dict["hp"], "组合接口索引器应可写");
            AesirArchitectureDebug.LogTestInfo("组合接口访问: 双链成员无多义性");
        }

        /// <summary>
        /// 验证 foreach 具体类型使用结构体枚举器遍历全部键值对（零分配路径的行为正确性）。
        /// </summary>
        [Test]
        public void ForeachConcreteType_EnumeratesAllPairs()
        {
            var dict = new ObservableDictionary<string, int> { ["a"] = 1, ["b"] = 2 };

            var keys = new List<string>();
            var values = new List<int>();
            foreach (KeyValuePair<string, int> pair in dict)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }

            Assert.AreEqual(2, keys.Count, "结构体枚举器应返回全部键值对");
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, keys, "键应完整");
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, values, "值应完整");
            AesirArchitectureDebug.LogTestInfo("结构体枚举器: 具体类型 foreach 遍历全部键值对");
        }

        /// <summary>
        /// 验证容量构造创建空字典且可正常增删。
        /// </summary>
        [Test]
        public void CapacityConstructor_CreatesEmptyUsableDictionary()
        {
            var dict = new ObservableDictionary<string, int>(16);

            Assert.AreEqual(0, dict.Count, "容量构造应为空字典");
            dict.Add("hp", 100);
            Assert.AreEqual(100, dict["hp"], "容量构造后应可正常添加");
            AesirArchitectureDebug.LogTestInfo("容量构造: 空字典且可正常使用");
        }
    }
}
