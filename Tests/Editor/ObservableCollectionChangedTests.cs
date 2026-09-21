using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// <see cref="IObservableCollection{T}.CollectionChanged" /> 的上游语义回归：每次写操作都通知
    /// （含赋相同值的索引器写入与空集合的 <c>Clear</c>）、批量操作通知单次事件、
    /// <c>Sort</c> / <c>Reverse</c> / <c>Clear</c> 以 <see cref="NotifyCollectionChangedAction.Reset" />
    /// 携带 <see cref="SortOperation{T}" /> 通知。
    /// </summary>
    /// <remarks>
    /// 载荷是 <c>readonly ref struct</c>，无法存进集合，因此用例把关心字段就地抄进 <see cref="Capture{T}" />。
    /// </remarks>
    public class ObservableCollectionChangedTests
    {
        /// <summary>就地抄录事件载荷的订阅者。</summary>
        sealed class Capture<T>
        {
            public int Calls;
            public NotifyCollectionChangedAction Action;
            public bool IsSingle;
            public int NewStartingIndex = -1;
            public int OldStartingIndex = -1;
            public int NewItemsLength;
            public int OldItemsLength;
            public int SortIndex;
            public int SortCount;
            public bool SortIsClear;
            public bool SortIsSort;
            public bool SortIsReverse;
            public T LastNewItem;
            public T LastOldItem;

            public void OnChanged(in NotifyCollectionChangedEventArgs<T> e)
            {
                Calls++;
                Action = e.Action;
                IsSingle = e.IsSingleItem;
                NewStartingIndex = e.NewStartingIndex;
                OldStartingIndex = e.OldStartingIndex;
                NewItemsLength = e.NewItems.Length;
                OldItemsLength = e.OldItems.Length;
                SortIndex = e.SortOperation.Index;
                SortCount = e.SortOperation.Count;
                SortIsClear = e.SortOperation.IsClear;
                SortIsSort = e.SortOperation.IsSort;
                SortIsReverse = e.SortOperation.IsReverse;
                LastNewItem = e.NewItem;
                LastOldItem = e.OldItem;
            }
        }

        // ---------- ObservableList<T> ----------

        [Test]
        public void List_Add_NotifiesSingleItemWithIndex()
        {
            var list = new ObservableList<int>();
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.Add(42);

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Action);
            Assert.IsTrue(capture.IsSingle);
            Assert.AreEqual(0, capture.NewStartingIndex);
            Assert.AreEqual(42, capture.LastNewItem);
        }

        [Test]
        public void List_AddRange_NotifiesOnceWithSpanPayload()
        {
            var list = new ObservableList<int>();
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.Add(1);
            list.AddRange(new[] { 2, 3, 4 });

            Assert.AreEqual(2, capture.Calls, "AddRange 只应通知一次");
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Action);
            Assert.IsFalse(capture.IsSingle);
            Assert.AreEqual(3, capture.NewItemsLength);
            Assert.AreEqual(1, capture.NewStartingIndex);
        }

        [Test]
        public void List_RemoveAt_NotifiesRemoveWithPreRemovalIndex()
        {
            var list = new ObservableList<int> { 10, 20, 30 };
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.RemoveAt(1);

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, capture.Action);
            Assert.IsTrue(capture.IsSingle);
            Assert.AreEqual(20, capture.LastOldItem);
            Assert.AreEqual(1, capture.OldStartingIndex);
        }

        [Test]
        public void List_RemoveRange_NotifiesOnceWithSpanPayload()
        {
            var list = new ObservableList<int> { 10, 20, 30, 40 };
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.RemoveRange(1, 2);

            Assert.AreEqual(1, capture.Calls, "RemoveRange 只应通知一次");
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, capture.Action);
            Assert.IsFalse(capture.IsSingle);
            Assert.AreEqual(2, capture.OldItemsLength);
            Assert.AreEqual(1, capture.OldStartingIndex);
        }

        [Test]
        public void List_Remove_MissingItem_DoesNotNotify()
        {
            var list = new ObservableList<int> { 10 };
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            var removed = list.Remove(99);

            Assert.IsFalse(removed);
            Assert.AreEqual(0, capture.Calls);
        }

        [Test]
        public void List_Indexer_SameValue_StillNotifies()
        {
            var list = new ObservableList<int> { 10 };
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list[0] = 10;

            Assert.AreEqual(1, capture.Calls, "上游语义：每次写操作都通知，即使值未变");
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, capture.Action);
            Assert.AreEqual(10, capture.LastOldItem);
            Assert.AreEqual(0, capture.NewStartingIndex);
        }

        [Test]
        public void List_Move_NotifiesMoveWithBothIndices()
        {
            var list = new ObservableList<int> { 10, 20, 30 };
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.Move(0, 2);

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Move, capture.Action);
            Assert.AreEqual(2, capture.NewStartingIndex);
            Assert.AreEqual(0, capture.OldStartingIndex);
        }

        [Test]
        public void List_Sort_NotifiesResetWithSortOperation()
        {
            var list = new ObservableList<int> { 30, 10, 20 };
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.Sort();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Action);
            Assert.IsTrue(capture.SortIsSort);
            Assert.AreEqual(0, capture.SortIndex);
            Assert.AreEqual(3, capture.SortCount);
        }

        [Test]
        public void List_Reverse_NotifiesResetWithReverseOperation()
        {
            var list = new ObservableList<int> { 10, 20, 30 };
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.Reverse();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Action);
            Assert.IsTrue(capture.SortIsReverse);
            Assert.AreEqual(0, capture.SortIndex);
            Assert.AreEqual(3, capture.SortCount);
        }

        [Test]
        public void List_Clear_NotifiesResetWithClearOperation()
        {
            var list = new ObservableList<int> { 10, 20 };
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.Clear();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Action);
            Assert.IsTrue(capture.SortIsClear);
        }

        [Test]
        public void List_Clear_OnEmptyList_StillNotifies()
        {
            var list = new ObservableList<int>();
            var capture = new Capture<int>();
            list.CollectionChanged += capture.OnChanged;

            list.Clear();

            Assert.AreEqual(1, capture.Calls, "上游语义：空集合的 Clear 也通知（轻量事件不通知）");
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Action);
            Assert.IsTrue(capture.SortIsClear);
        }

        // ---------- ObservableDictionary<TKey, TValue> ----------

        [Test]
        public void Dictionary_Add_NotifiesWithIndexMinusOne()
        {
            var dictionary = new ObservableDictionary<string, int>();
            var capture = new Capture<KeyValuePair<string, int>>();
            dictionary.CollectionChanged += capture.OnChanged;

            dictionary["攻击力"] = 10;

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Action);
            Assert.AreEqual(-1, capture.NewStartingIndex, "字典无索引概念");
            Assert.AreEqual(10, capture.LastNewItem.Value);
        }

        [Test]
        public void Dictionary_Indexer_SameValue_StillNotifies()
        {
            var dictionary = new ObservableDictionary<string, int>();
            dictionary.Add("攻击力", 10);
            var capture = new Capture<KeyValuePair<string, int>>();
            dictionary.CollectionChanged += capture.OnChanged;

            dictionary["攻击力"] = 10;

            Assert.AreEqual(1, capture.Calls, "上游语义：赋相同值也通知（轻量事件不通知）");
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, capture.Action);
            Assert.AreEqual(-1, capture.NewStartingIndex);
            Assert.AreEqual(10, capture.LastOldItem.Value);
        }

        // ---------- ObservableHashSet<T> ----------

        [Test]
        public void HashSet_Add_NotifiesAtIndexMinusOne()
        {
            var set = new ObservableHashSet<string>();
            var capture = new Capture<string>();
            set.CollectionChanged += capture.OnChanged;

            var added = set.Add("玩家A");

            Assert.IsTrue(added);
            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Action);
            Assert.AreEqual(-1, capture.NewStartingIndex, "集合无索引概念");
            Assert.AreEqual("玩家A", capture.LastNewItem);
        }

        [Test]
        public void HashSet_Add_Duplicate_DoesNotNotify()
        {
            var set = new ObservableHashSet<string> { "玩家A" };
            var capture = new Capture<string>();
            set.CollectionChanged += capture.OnChanged;

            var added = set.Add("玩家A");

            Assert.IsFalse(added);
            Assert.AreEqual(0, capture.Calls, "实际未发生增删时不通知");
        }

        // ---------- ObservableQueue<T> ----------

        [Test]
        public void Queue_Enqueue_NotifiesAddWithTailIndex()
        {
            var queue = new ObservableQueue<string>(new[] { "A" });
            var capture = new Capture<string>();
            queue.CollectionChanged += capture.OnChanged;

            queue.Enqueue("B");

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Action);
            Assert.AreEqual(1, capture.NewStartingIndex, "入队追加到队尾");
            Assert.AreEqual("B", capture.LastNewItem);
        }

        [Test]
        public void Queue_Dequeue_NotifiesRemoveAtIndexZero()
        {
            var queue = new ObservableQueue<string>(new[] { "A", "B" });
            var capture = new Capture<string>();
            queue.CollectionChanged += capture.OnChanged;

            var item = queue.Dequeue();

            Assert.AreEqual("A", item);
            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, capture.Action);
            Assert.AreEqual(0, capture.OldStartingIndex, "出队总是移除队首");
        }

        [Test]
        public void Queue_DequeueRange_NotifiesOnceWithSpanPayload()
        {
            var queue = new ObservableQueue<string>(new[] { "A", "B", "C" });
            var capture = new Capture<string>();
            queue.CollectionChanged += capture.OnChanged;

            queue.DequeueRange(2);

            Assert.AreEqual(1, capture.Calls, "DequeueRange 只应通知一次");
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, capture.Action);
            Assert.IsFalse(capture.IsSingle);
            Assert.AreEqual(2, capture.OldItemsLength);
            Assert.AreEqual(0, capture.OldStartingIndex);
        }
    }
}
