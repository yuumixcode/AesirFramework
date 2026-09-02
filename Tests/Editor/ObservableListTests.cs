using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="ObservableList{T}" /> 的增删改清空事件与无变更跳过行为。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ObservableList 是 Model 层向 View 层暴露只读订阅的可观察列表载体，
    ///     写操作完成后才触发事件（回调中集合已是变更后状态）是其核心契约。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="ObservableList{T}" />
    public class ObservableListTests
    {
        /// <summary>
        /// 验证 Add 与 Insert 触发 Added 事件且索引、元素正确，回调中集合已含新元素。
        /// </summary>
        [Test]
        public void Add_And_Insert_FireAddedEventWithCorrectIndex()
        {
            var list = new ObservableList<string>();
            var received = new List<CollectionAddEventArgs<string>>();

            list.AddAddedListener(received.Add);
            list.Add("a");
            list.Insert(0, "b");

            Assert.AreEqual(2, received.Count, "两次写操作应各触发一次 Added");
            Assert.AreEqual(0, received[0].Index, "Add 的索引应为末尾");
            Assert.AreEqual("a", received[0].Item);
            Assert.AreEqual(0, received[1].Index, "Insert 的索引应为插入位置");
            Assert.AreEqual("b", received[1].Item);
            Assert.AreEqual(2, list.Count, "回调结束后集合应包含全部元素");
            AesirArchitectureDebug.LogTestInfo("Add/Insert: 索引与元素正确");
        }

        /// <summary>
        /// 验证 Remove 与 RemoveAt 触发 Removed 事件且参数为移除前的索引与元素。
        /// </summary>
        [Test]
        public void Remove_And_RemoveAt_FireRemovedEventWithPreRemovalState()
        {
            var list = new ObservableList<int> { 10, 20, 30 };
            var received = new List<CollectionRemoveEventArgs<int>>();

            list.AddRemovedListener(received.Add);
            Assert.IsTrue(list.Remove(20), "移除存在的元素应返回 true");
            list.RemoveAt(1);

            Assert.AreEqual(2, received.Count, "两次移除应各触发一次 Removed");
            Assert.AreEqual(1, received[0].Index, "Remove 的索引应为元素移除前位置");
            Assert.AreEqual(20, received[0].Item);
            Assert.AreEqual(1, received[1].Index, "RemoveAt 的索引应为元素移除前位置");
            Assert.AreEqual(30, received[1].Item);
            AesirArchitectureDebug.LogTestInfo("Remove/RemoveAt: 参数为移除前状态");
        }

        /// <summary>
        /// 验证索引器赋不同值触发 Replaced 事件（含旧项与新项），赋相同值不触发。
        /// </summary>
        [Test]
        public void Indexer_DifferentValue_Replaces_SameValue_Skips()
        {
            var list = new ObservableList<string> { "old" };
            var received = new List<CollectionReplaceEventArgs<string>>();

            list.AddReplacedListener(received.Add);
            list[0] = "old";
            Assert.AreEqual(0, received.Count, "赋相同值不应触发 Replaced");

            list[0] = "new";
            Assert.AreEqual(1, received.Count, "赋不同值应触发一次 Replaced");
            Assert.AreEqual(0, received[0].Index);
            Assert.AreEqual("old", received[0].OldItem);
            Assert.AreEqual("new", received[0].NewItem);
            AesirArchitectureDebug.LogTestInfo("索引器替换: 相同值跳过，不同值通知含新旧项");
        }

        /// <summary>
        /// 验证非空列表 Clear 触发 Cleared，空列表 Clear 不触发。
        /// </summary>
        [Test]
        public void Clear_FiresOnlyWhenNotEmpty()
        {
            var list = new ObservableList<int> { 1, 2 };
            var count = 0;

            list.AddClearedListener(() => count++);

            list.Clear();
            Assert.AreEqual(1, count, "非空列表清空应触发一次 Cleared");

            list.Clear();
            Assert.AreEqual(1, count, "空列表清空不应触发 Cleared");
            AesirArchitectureDebug.LogTestInfo("Clear: 仅非空清空触发");
        }

        /// <summary>
        /// 验证无变更操作不触发事件：Remove 不存在的元素返回 false。
        /// </summary>
        [Test]
        public void Remove_MissingItem_ReturnsFalseWithoutEvent()
        {
            var list = new ObservableList<int> { 1 };
            var received = new List<CollectionRemoveEventArgs<int>>();

            list.AddRemovedListener(received.Add);
            Assert.IsFalse(list.Remove(99), "移除不存在的元素应返回 false");
            Assert.AreEqual(0, received.Count, "移除不存在的元素不应触发 Removed");
            Assert.AreEqual(1, list.Count, "集合内容不应变化");
            AesirArchitectureDebug.LogTestInfo("Remove 缺失项: 返回 false 且不触发");
        }

        /// <summary>
        /// 验证 AddRange 逐项触发 Added 事件且索引递增。
        /// </summary>
        [Test]
        public void AddRange_FiresAddedPerItem()
        {
            var list = new ObservableList<int>();
            var received = new List<CollectionAddEventArgs<int>>();

            list.AddAddedListener(received.Add);
            list.AddRange(new[] { 7, 8, 9 });

            Assert.AreEqual(3, received.Count, "AddRange 应逐项触发 Added");
            Assert.AreEqual(0, received[0].Index);
            Assert.AreEqual(7, received[0].Item);
            Assert.AreEqual(2, received[2].Index);
            Assert.AreEqual(9, received[2].Item);
            AesirArchitectureDebug.LogTestInfo("AddRange: 逐项触发 Added");
        }

        /// <summary>
        /// 验证监听句柄 Dispose 后不再收到通知，ClearListeners 清空全部监听。
        /// </summary>
        [Test]
        public void HandleDispose_And_ClearListeners_StopNotifications()
        {
            var list = new ObservableList<int>();
            var addCount = 0;

            void OnAdded(CollectionAddEventArgs<int> _)
            {
                addCount++;
            }

            var handle = list.AddAddedListener(OnAdded);
            list.Add(1);
            Assert.AreEqual(1, addCount, "移除前应正常收到通知");

            handle.Dispose();
            list.Add(2);
            Assert.AreEqual(1, addCount, "句柄 Dispose 后不应再收到通知");

            list.AddAddedListener(OnAdded);
            list.ClearListeners();
            list.Add(3);
            Assert.AreEqual(1, addCount, "ClearListeners 清空全部监听后不应再收到通知");
            AesirArchitectureDebug.LogTestInfo("句柄/ClearListeners: 正确停止通知");
        }

        /// <summary>
        /// 验证带初始元素构造不触发任何事件，且枚举与只读访问可用。
        /// </summary>
        [Test]
        public void Constructor_WithInitialItems_NoEvents()
        {
            var addCount = 0;
            var list = new ObservableList<int>(new[] { 4, 5 });
            list.AddAddedListener(_ => addCount++);

            Assert.AreEqual(0, addCount, "初始元素构造不应触发 Added");
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(5, list[1]);

            var enumerated = new List<int>();
            foreach (int item in (IEnumerable<int>)list)
            {
                enumerated.Add(item);
            }

            CollectionAssert.AreEqual(new[] { 4, 5 }, enumerated, "枚举应按序返回全部元素");
            AesirArchitectureDebug.LogTestInfo("初始构造: 不触发事件且可枚举");
        }

        /// <summary>
        /// 验证通过组合接口 <see cref="IObservableList{T}" /> 访问 Count 与索引器不再有 IList/IReadOnlyList 双链多义性（CS0229）。
        /// </summary>
        /// <remarks>若接口未用 new 重声明统一双链成员，本测试将因编译多义性错误而失败。</remarks>
        [Test]
        public void CombinedInterface_Access_NoAmbiguity()
        {
            IObservableList<int> list = new ObservableList<int> { 1, 2 };

            Assert.AreEqual(2, list.Count, "组合接口访问 Count 应无多义性");
            Assert.AreEqual(1, list[0], "组合接口访问索引器应无多义性");
            list[1] = 3;
            Assert.AreEqual(3, list[1], "组合接口索引器应可写");
            AesirArchitectureDebug.LogTestInfo("组合接口访问: Count/索引器无多义性");
        }

        /// <summary>
        /// 验证 foreach 具体类型使用结构体枚举器按序遍历全部元素（零分配路径的行为正确性）。
        /// </summary>
        [Test]
        public void ForeachConcreteType_EnumeratesInOrder()
        {
            var list = new ObservableList<int> { 1, 2, 3 };

            var enumerated = new List<int>();
            foreach (int item in list)
            {
                enumerated.Add(item);
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, enumerated, "结构体枚举器应按序返回全部元素");
            AesirArchitectureDebug.LogTestInfo("结构体枚举器: 具体类型 foreach 按序遍历");
        }

        /// <summary>
        /// 验证容量构造创建空列表且可正常增删。
        /// </summary>
        [Test]
        public void CapacityConstructor_CreatesEmptyUsableList()
        {
            var list = new ObservableList<int>(16);

            Assert.AreEqual(0, list.Count, "容量构造应为空列表");
            list.Add(1);
            Assert.AreEqual(1, list[0], "容量构造后应可正常添加");
            AesirArchitectureDebug.LogTestInfo("容量构造: 空列表且可正常使用");
        }
    }
}
