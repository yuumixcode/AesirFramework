using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="ObservableDictionary{TKey, TValue}" /> 的单轨变更通知：新键 Add、已有键 Replace（含旧值）、
    /// 移除 Remove、清空 Reset，以及无变更跳过行为。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ObservableDictionary 是 Model 层向 View 层暴露只读订阅的可观察字典载体，
    ///     索引器按"新键 Add / 已有键 Replace（含旧值）"分流是其核心契约。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="ObservableDictionary{TKey, TValue}" />
    public class ObservableDictionaryTests
    {
        /// <summary>
        /// 验证 Add 触发 Add 通知且参数为新增键值对。
        /// </summary>
        [Test]
        public void Add_FiresAddWithPair()
        {
            var dict = new ObservableDictionary<string, int>();
            var received = new List<CollectionChangedEventArgs<KeyValuePair<string, int>>>();

            dict.AddListener(received.Add);
            dict.Add("hp", 100);

            Assert.AreEqual(1, received.Count, "一次添加应触发一次 Add 通知");
            Assert.AreEqual(NotifyCollectionChangedAction.Add, received[0].Action);
            Assert.AreEqual("hp", received[0].NewItem.Key);
            Assert.AreEqual(100, received[0].NewItem.Value);
            AesirArchitectureDebug.LogTestInfo("Add: 触发 Add 且键值正确");
        }

        /// <summary>
        /// 验证索引器分流：新键触发 Add，已有键赋不同值触发 Replace（含旧值），赋相同值不触发。
        /// </summary>
        [Test]
        public void Indexer_NewKey_Add_ExistingKey_ReplaceOrSkipped()
        {
            var dict = new ObservableDictionary<string, int>();
            var received = new List<CollectionChangedEventArgs<KeyValuePair<string, int>>>();

            dict.AddListener(received.Add);

            dict["hp"] = 100;
            Assert.AreEqual(1, received.Count, "新键应触发 Add");
            Assert.AreEqual(NotifyCollectionChangedAction.Add, received[0].Action);
            Assert.AreEqual(100, received[0].NewItem.Value);

            dict["hp"] = 80;
            Assert.AreEqual(2, received.Count, "已有键赋不同值应触发 Replace");
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, received[1].Action);
            Assert.AreEqual("hp", received[1].NewItem.Key);
            Assert.AreEqual(80, received[1].NewItem.Value, "Replace 载荷 NewItem 为新键值对");
            Assert.AreEqual(100, received[1].OldItem.Value, "Replace 载荷 OldItem 含旧值");

            dict["hp"] = 80;
            Assert.AreEqual(2, received.Count, "已有键赋相同值不应触发通知");
            AesirArchitectureDebug.LogTestInfo("索引器分流: 新键 Add / 已有键 Replace / 相同值跳过");
        }

        /// <summary>
        /// 验证 Remove 触发 Remove 通知且参数含被移除的键值对；移除不存在的键返回 false 且不触发。
        /// </summary>
        [Test]
        public void Remove_FiresWithRemovedPair_MissingKey_ReturnsFalse()
        {
            var dict = new ObservableDictionary<string, int> { ["hp"] = 100 };
            var received = new List<CollectionChangedEventArgs<KeyValuePair<string, int>>>();

            dict.AddListener(received.Add);
            Assert.IsFalse(dict.Remove("mp"), "移除不存在的键应返回 false");
            Assert.AreEqual(0, received.Count, "移除不存在的键不应触发通知");

            Assert.IsTrue(dict.Remove("hp"), "移除存在的键应返回 true");
            Assert.AreEqual(1, received.Count, "移除存在的键应触发一次 Remove 通知");
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, received[0].Action);
            Assert.AreEqual("hp", received[0].OldItem.Key);
            Assert.AreEqual(100, received[0].OldItem.Value);
            AesirArchitectureDebug.LogTestInfo("Remove: 参数含被移除键值对，缺失键不触发");
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

            Assert.IsTrue(dict.TryGetValue("hp", out var value), "存在的键应命中 TryGetValue");
            Assert.AreEqual(100, value);
            Assert.IsFalse(dict.TryGetValue("mp", out var missing), "不存在的键 TryGetValue 应返回 false");
            Assert.AreEqual(0, missing, "未命中时 out 值应为类型默认值");
            AesirArchitectureDebug.LogTestInfo("TryGetValue/ContainsKey: 读写路径正确");
        }

        /// <summary>
        /// 验证非空字典 Clear 触发 Reset，空字典 Clear 不触发。
        /// </summary>
        [Test]
        public void Clear_FiresResetOnlyWhenNotEmpty()
        {
            var dict = new ObservableDictionary<string, int> { ["hp"] = 100 };
            var received = new List<CollectionChangedEventArgs<KeyValuePair<string, int>>>();

            dict.AddListener(received.Add);

            dict.Clear();
            Assert.AreEqual(1, received.Count, "非空字典清空应触发一次 Reset");
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, received[0].Action);

            dict.Clear();
            Assert.AreEqual(1, received.Count, "空字典清空不应触发通知");
            AesirArchitectureDebug.LogTestInfo("Clear: 仅非空清空触发 Reset");
        }

        /// <summary>
        /// 验证监听句柄 Dispose 后不再收到通知，ClearListeners 清空全部监听。
        /// </summary>
        [Test]
        public void HandleDispose_And_ClearListeners_StopNotifications()
        {
            var dict = new ObservableDictionary<string, int>();
            var callCount = 0;

            void OnChanged(CollectionChangedEventArgs<KeyValuePair<string, int>> _)
            {
                callCount++;
            }

            var handle = dict.AddListener(OnChanged);
            dict.Add("a", 1);
            Assert.AreEqual(1, callCount, "移除前应正常收到通知");

            handle.Dispose();
            dict.Add("b", 2);
            Assert.AreEqual(1, callCount, "句柄 Dispose 后不应再收到通知");

            dict.AddListener(OnChanged);
            dict.ClearListeners();
            dict.Add("c", 3);
            Assert.AreEqual(1, callCount, "ClearListeners 清空全部监听后不应再收到通知");
            AesirArchitectureDebug.LogTestInfo("句柄/ClearListeners: 正确停止通知");
        }

        /// <summary>
        /// 验证带初始键值构造不触发任何通知，且 Keys、Values 与枚举可用。
        /// </summary>
        [Test]
        public void Constructor_WithInitialItems_NoEvents_Enumerable()
        {
            var callCount = 0;
            var dict = new ObservableDictionary<string, int>(new[]
                { new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2) });
            dict.AddListener(_ => callCount++);

            Assert.AreEqual(0, callCount, "初始键值构造不应触发通知");
            Assert.AreEqual(2, dict.Count);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, dict.Keys, "Keys 应包含全部键");
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, dict.Values, "Values 应包含全部值");

            var enumerated = new List<KeyValuePair<string, int>>();
            foreach (var pair in (IEnumerable<KeyValuePair<string, int>>)dict)
            {
                enumerated.Add(pair);
            }

            Assert.AreEqual(2, enumerated.Count, "枚举应返回全部键值对");
            AesirArchitectureDebug.LogTestInfo("初始构造: 不触发通知且可枚举");
        }

        /// <summary>
        /// 验证重复添加同一键抛异常（fail-fast），且集合状态不变。
        /// </summary>
        [Test]
        public void Add_DuplicateKey_ThrowsAndKeepsState()
        {
            var dict = new ObservableDictionary<string, int> { ["hp"] = 100 };
            var received = new List<CollectionChangedEventArgs<KeyValuePair<string, int>>>();
            dict.AddListener(received.Add);

            Assert.Throws<ArgumentException>(() => dict.Add("hp", 200), "重复添加应抛 ArgumentException");
            Assert.AreEqual(0, received.Count, "添加失败不应触发通知");
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
            Assert.IsTrue(dict.TryGetValue("hp", out var value), "组合接口访问 TryGetValue 应无多义性");
            Assert.AreEqual(100, value);
            CollectionAssert.AreEquivalent(new[] { "hp" }, dict.Keys, "组合接口访问 Keys 应无多义性");
            CollectionAssert.AreEquivalent(new[] { 100 }, dict.Values, "组合接口访问 Values 应无多义性");
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
            foreach (var pair in dict)
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
