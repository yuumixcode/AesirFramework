using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="ObservableHashSet{T}" /> 的增删清空事件、无变更跳过与集合代数操作的事件语义。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ObservableHashSet 是 Model 层向 View 层暴露只读订阅的可观察集合载体，
    ///     写操作完成后才触发事件（回调中集合已是变更后状态）是其核心契约。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="ObservableHashSet{T}" />
    public class ObservableHashSetTests
    {
        /// <summary>
        /// 验证 Add 新元素触发 Added 事件，重复添加不触发且返回 false。
        /// </summary>
        [Test]
        public void Add_FiresAddedOnlyForNewItem()
        {
            var set = new ObservableHashSet<string>();
            var received = new List<string>();

            set.AddAddedListener(received.Add);
            Assert.IsTrue(set.Add("a"), "新元素应添加成功");
            Assert.IsFalse(set.Add("a"), "重复元素应添加失败");

            Assert.AreEqual(1, received.Count, "仅首次添加应触发 Added");
            Assert.AreEqual("a", received[0]);
            Assert.AreEqual(1, set.Count, "重复添加不应增加元素数量");
            AesirArchitectureDebug.LogTestInfo("Add: 新元素触发 Added，重复添加跳过");
        }

        /// <summary>
        /// 验证 Remove 存在元素触发 Removed 事件，移除不存在的元素返回 false 且不触发。
        /// </summary>
        [Test]
        public void Remove_FiresRemovedForExistingItem_MissingNoEvent()
        {
            var set = new ObservableHashSet<int> { 10 };
            var received = new List<int>();

            set.AddRemovedListener(received.Add);
            Assert.IsFalse(set.Remove(99), "移除不存在的元素应返回 false");
            Assert.AreEqual(0, received.Count, "移除不存在的元素不应触发 Removed");

            Assert.IsTrue(set.Remove(10), "移除存在的元素应返回 true");
            Assert.AreEqual(1, received.Count, "移除存在的元素应触发一次 Removed");
            Assert.AreEqual(10, received[0]);
            AesirArchitectureDebug.LogTestInfo("Remove: 存在触发 Removed，缺失不触发");
        }

        /// <summary>
        /// 验证非空集合 Clear 触发 Cleared，空集合 Clear 不触发。
        /// </summary>
        [Test]
        public void Clear_FiresOnlyWhenNotEmpty()
        {
            var set = new ObservableHashSet<int> { 1, 2 };
            var count = 0;

            set.AddClearedListener(() => count++);

            set.Clear();
            Assert.AreEqual(1, count, "非空集合清空应触发一次 Cleared");

            set.Clear();
            Assert.AreEqual(1, count, "空集合清空不应触发 Cleared");
            AesirArchitectureDebug.LogTestInfo("Clear: 仅非空清空触发");
        }

        /// <summary>
        /// 验证 UnionWith 仅对实际新增的元素逐项触发 Added。
        /// </summary>
        [Test]
        public void UnionWith_FiresAddedPerNewItem()
        {
            var set = new ObservableHashSet<int> { 1 };
            var received = new List<int>();

            set.AddAddedListener(received.Add);
            set.UnionWith(new[] { 1, 2, 3 });

            Assert.AreEqual(2, received.Count, "已存在的 1 不应触发 Added");
            CollectionAssert.AreEquivalent(new[] { 2, 3 }, received, "Added 应逐项覆盖新增元素");
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, set, "并集结果应包含全部元素");
            AesirArchitectureDebug.LogTestInfo("UnionWith: 仅新增元素触发 Added");
        }

        /// <summary>
        /// 验证 ExceptWith 仅对实际存在的元素逐项触发 Removed。
        /// </summary>
        [Test]
        public void ExceptWith_FiresRemovedPerPresentItem()
        {
            var set = new ObservableHashSet<int> { 1, 2, 3 };
            var received = new List<int>();

            set.AddRemovedListener(received.Add);
            set.ExceptWith(new[] { 2, 4 });

            Assert.AreEqual(1, received.Count, "不存在的 4 不应触发 Removed");
            Assert.AreEqual(2, received[0]);
            CollectionAssert.AreEquivalent(new[] { 1, 3 }, set, "差集结果应移除存在的 2");
            AesirArchitectureDebug.LogTestInfo("ExceptWith: 仅存在元素触发 Removed");
        }

        /// <summary>
        /// 验证 ExceptWith 传入集合自身短路为 Clear（BCL 语义），触发一次 Cleared 而非逐项 Removed。
        /// </summary>
        [Test]
        public void ExceptWith_Self_BehavesAsClear()
        {
            var set = new ObservableHashSet<int> { 1, 2 };
            var removedCount = 0;
            var clearedCount = 0;

            set.AddRemovedListener(_ => removedCount++);
            set.AddClearedListener(() => clearedCount++);
            set.ExceptWith(set);

            Assert.AreEqual(0, set.Count, "自差集结果应为空集");
            Assert.AreEqual(0, removedCount, "自身短路不应逐项触发 Removed");
            Assert.AreEqual(1, clearedCount, "自身短路应触发一次 Cleared");
            AesirArchitectureDebug.LogTestInfo("ExceptWith 自身: 短路为 Clear 且触发 Cleared");
        }

        /// <summary>
        /// 验证 IntersectWith 移除不在另一集合中的元素并逐项触发 Removed；传入自身为无变化操作。
        /// </summary>
        [Test]
        public void IntersectWith_RemovesItemsNotInOther_SelfNoEvent()
        {
            var set = new ObservableHashSet<int> { 1, 2, 3 };
            var received = new List<int>();

            set.AddRemovedListener(received.Add);
            set.IntersectWith(new[] { 2, 3, 4 });

            Assert.AreEqual(1, received.Count, "仅不在交集内的 1 应触发 Removed");
            Assert.AreEqual(1, received[0]);
            CollectionAssert.AreEquivalent(new[] { 2, 3 }, set, "交集结果应仅保留共有元素");

            set.IntersectWith(set);
            Assert.AreEqual(1, received.Count, "自身交集为无变化操作，不应触发 Removed");
            AesirArchitectureDebug.LogTestInfo("IntersectWith: 移除差集元素，自身无变化");
        }

        /// <summary>
        /// 验证 SymmetricExceptWith 先触发全部 Removed（交集）、再触发全部 Added（仅另一集合拥有的元素）；
        /// 传入自身短路为 Clear。
        /// </summary>
        [Test]
        public void SymmetricExceptWith_RemovesIntersectionThenAddsRemainder()
        {
            var set = new ObservableHashSet<int> { 1, 2, 3 };
            var log = new List<(string Kind, int Item)>();

            set.AddRemovedListener(item => log.Add(("removed", item)));
            set.AddAddedListener(item => log.Add(("added", item)));
            set.SymmetricExceptWith(new[] { 2, 3, 4 });

            Assert.AreEqual(3, log.Count, "移除 2、3 并添加 4 应各触发一次");
            CollectionAssert.AreEqual(
                new[] { ("removed", 2), ("removed", 3), ("added", 4) }, log,
                "应先触发全部 Removed、再触发全部 Added");
            CollectionAssert.AreEquivalent(new[] { 1, 4 }, set, "对称差集结果应为 {1, 4}");

            set.AddClearedListener(() => log.Add(("cleared", 0)));
            set.SymmetricExceptWith(set);
            Assert.AreEqual(4, log.Count, "自身对称差集应短路为 Clear");
            Assert.AreEqual(("cleared", 0), log[3]);
            Assert.AreEqual(0, set.Count, "自身对称差集结果应为空集");
            AesirArchitectureDebug.LogTestInfo("SymmetricExceptWith: 先 Removed 后 Added，自身短路 Clear");
        }

        /// <summary>
        /// 验证子集/超集/重叠/相等判定委托内部 HashSet 正确执行。
        /// </summary>
        [Test]
        public void QueryOperations_DelegateToInnerSet()
        {
            var set = new ObservableHashSet<int> { 1, 2 };

            Assert.IsTrue(set.IsSubsetOf(new[] { 1, 2, 3 }), "应判定为子集");
            Assert.IsTrue(set.IsProperSubsetOf(new[] { 1, 2, 3 }), "应判定为真子集");
            Assert.IsTrue(set.IsSupersetOf(new[] { 1 }), "应判定为超集");
            Assert.IsTrue(set.IsProperSupersetOf(new[] { 1 }), "应判定为真超集");
            Assert.IsTrue(set.Overlaps(new[] { 2, 5 }), "存在共同元素应判定重叠");
            Assert.IsTrue(set.SetEquals(new[] { 2, 1 }), "元素相同应判定相等");
            AesirArchitectureDebug.LogTestInfo("查询操作: 委托内部 HashSet 正确");
        }

        /// <summary>
        /// 验证监听句柄 Dispose 后不再收到通知，ClearListeners 清空全部监听。
        /// </summary>
        [Test]
        public void HandleDispose_And_ClearListeners_StopNotifications()
        {
            var set = new ObservableHashSet<int>();
            var addCount = 0;

            void OnAdded(int _)
            {
                addCount++;
            }

            var handle = set.AddAddedListener(OnAdded);
            set.Add(1);
            Assert.AreEqual(1, addCount, "移除前应正常收到通知");

            handle.Dispose();
            set.Add(2);
            Assert.AreEqual(1, addCount, "句柄 Dispose 后不应再收到通知");

            set.AddAddedListener(OnAdded);
            set.ClearListeners();
            set.Add(3);
            Assert.AreEqual(1, addCount, "ClearListeners 清空全部监听后不应再收到通知");
            AesirArchitectureDebug.LogTestInfo("句柄/ClearListeners: 正确停止通知");
        }

        /// <summary>
        /// 验证带初始元素构造不触发任何事件，且枚举与 Contains 可用。
        /// </summary>
        [Test]
        public void Constructor_WithInitialItems_NoEvents()
        {
            var addCount = 0;
            var set = new ObservableHashSet<int>(new[] { 4, 5 });
            set.AddAddedListener(_ => addCount++);

            Assert.AreEqual(0, addCount, "初始元素构造不应触发 Added");
            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains(4));

            var enumerated = new List<int>();
            foreach (int item in (IEnumerable<int>)set)
            {
                enumerated.Add(item);
            }

            CollectionAssert.AreEquivalent(new[] { 4, 5 }, enumerated, "枚举应返回全部元素");
            AesirArchitectureDebug.LogTestInfo("初始构造: 不触发事件且可枚举");
        }

        /// <summary>
        /// 验证通过组合接口 <see cref="IObservableHashSet{T}" /> 访问 Count 与 Contains 不再有 ISet/IReadOnlyCollection 双链多义性（CS0229）。
        /// </summary>
        /// <remarks>若接口未用 new 重声明统一双链成员，本测试将因编译多义性错误而失败。</remarks>
        [Test]
        public void CombinedInterface_Access_NoAmbiguity()
        {
            IObservableHashSet<int> set = new ObservableHashSet<int> { 1, 2 };

            Assert.AreEqual(2, set.Count, "组合接口访问 Count 应无多义性");
            Assert.IsTrue(set.Contains(1), "组合接口访问 Contains 应无多义性");
            set.Add(3);
            Assert.IsTrue(set.Contains(3), "组合接口 Add 应可用");
            AesirArchitectureDebug.LogTestInfo("组合接口访问: Count/Contains 无多义性");
        }

        /// <summary>
        /// 验证 foreach 具体类型使用结构体枚举器遍历全部元素（零分配路径的行为正确性）。
        /// </summary>
        [Test]
        public void ForeachConcreteType_EnumeratesAllItems()
        {
            var set = new ObservableHashSet<int> { 1, 2, 3 };

            var enumerated = new List<int>();
            foreach (int item in set)
            {
                enumerated.Add(item);
            }

            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, enumerated, "结构体枚举器应返回全部元素");
            AesirArchitectureDebug.LogTestInfo("结构体枚举器: 具体类型 foreach 遍历全部元素");
        }

        /// <summary>
        /// 验证容量构造创建空集合且可正常增删。
        /// </summary>
        [Test]
        public void CapacityConstructor_CreatesEmptyUsableSet()
        {
            var set = new ObservableHashSet<int>(16);

            Assert.AreEqual(0, set.Count, "容量构造应为空集合");
            set.Add(1);
            Assert.IsTrue(set.Contains(1), "容量构造后应可正常添加");
            AesirArchitectureDebug.LogTestInfo("容量构造: 空集合且可正常使用");
        }
    }
}
