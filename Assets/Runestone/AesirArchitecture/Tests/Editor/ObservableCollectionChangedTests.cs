using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 单轨变更通知的语义回归：无变更的写操作不通知、批量操作逐项通知、
    /// Sort / Reverse / Clear 以 <see cref="NotifyCollectionChangedAction.Reset" /> 通知、
    /// 句柄 Dispose / ClearListeners 停止通知、句柄可绑定 Unity 生命周期自动移除。
    /// </summary>
    /// <remarks>
    /// 载荷是普通只读结构体，可直接存进集合，用例用 <see cref="Capture{T}" /> 全量抄录事件序列。
    /// </remarks>
    public class ObservableCollectionChangedTests
    {
        // ---------- ObservableList<T> ----------

        [Test]
        public void List_Add_NotifiesAddWithIndex()
        {
            var list = new ObservableList<int>();
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.Add(42);

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Events[0].Action);
            Assert.AreEqual(42, capture.Events[0].NewItem);
            Assert.AreEqual(0, capture.Events[0].NewStartingIndex);
        }

        [Test]
        public void List_AddRange_NotifiesPerItem()
        {
            var list = new ObservableList<int>();
            list.Add(1);
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.AddRange(new[] { 2, 3, 4 });

            Assert.AreEqual(3, capture.Calls, "AddRange 应逐项通知");
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Events[i].Action);
                Assert.AreEqual(i + 2, capture.Events[i].NewItem, "载荷应为被添加元素本身");
                Assert.AreEqual(i + 1, capture.Events[i].NewStartingIndex, "索引应接在既有元素之后连续递增");
            }
        }

        [Test]
        public void List_InsertRange_NotifiesPerItemWithInsertionIndices()
        {
            var list = new ObservableList<int> { 10, 40 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.InsertRange(1, new[] { 20, 30 });

            Assert.AreEqual(2, capture.Calls, "InsertRange 应逐项通知");
            Assert.AreEqual(20, capture.Events[0].NewItem);
            Assert.AreEqual(1, capture.Events[0].NewStartingIndex);
            Assert.AreEqual(30, capture.Events[1].NewItem);
            Assert.AreEqual(2, capture.Events[1].NewStartingIndex);
        }

        [Test]
        public void List_RemoveAt_NotifiesRemoveWithPreRemovalIndex()
        {
            var list = new ObservableList<int> { 10, 20, 30 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.RemoveAt(1);

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, capture.Events[0].Action);
            Assert.AreEqual(20, capture.Events[0].OldItem);
            Assert.AreEqual(1, capture.Events[0].OldStartingIndex, "索引应为移除前位置");
        }

        [Test]
        public void List_RemoveRange_NotifiesPerItemWithPreRemovalIndices()
        {
            var list = new ObservableList<int> { 10, 20, 30, 40 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.RemoveRange(1, 2);

            Assert.AreEqual(2, capture.Calls, "RemoveRange 应逐项通知");
            Assert.AreEqual(20, capture.Events[0].OldItem);
            Assert.AreEqual(1, capture.Events[0].OldStartingIndex, "索引应为移除前位置且按原始顺序递增");
            Assert.AreEqual(30, capture.Events[1].OldItem);
            Assert.AreEqual(2, capture.Events[1].OldStartingIndex);
        }

        [Test]
        public void List_Remove_MissingItem_DoesNotNotify()
        {
            var list = new ObservableList<int> { 10 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            var removed = list.Remove(99);

            Assert.IsFalse(removed);
            Assert.AreEqual(0, capture.Calls, "无变更的操作不应通知");
        }

        [Test]
        public void List_Indexer_ChangedValue_NotifiesReplaceWithOldItem()
        {
            var list = new ObservableList<int> { 10 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list[0] = 88;

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, capture.Events[0].Action);
            Assert.AreEqual(88, capture.Events[0].NewItem);
            Assert.AreEqual(10, capture.Events[0].OldItem);
            Assert.AreEqual(0, capture.Events[0].NewStartingIndex);
        }

        [Test]
        public void List_Indexer_SameValue_DoesNotNotify()
        {
            var list = new ObservableList<int> { 10 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list[0] = 10;

            Assert.AreEqual(0, capture.Calls, "无变更的写操作不应通知");
        }

        [Test]
        public void List_Move_NotifiesSingleMoveWithBothIndices()
        {
            var list = new ObservableList<int> { 10, 20, 30 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.Move(0, 2);

            Assert.AreEqual(1, capture.Calls, "Move 应通知单次 Move 事件，而非 Remove + Add 两次");
            Assert.AreEqual(NotifyCollectionChangedAction.Move, capture.Events[0].Action);
            Assert.AreEqual(10, capture.Events[0].NewItem, "Move 载荷的 NewItem 与 OldItem 均为被移动元素");
            Assert.AreEqual(10, capture.Events[0].OldItem);
            Assert.AreEqual(2, capture.Events[0].NewStartingIndex);
            Assert.AreEqual(0, capture.Events[0].OldStartingIndex);
        }

        [Test]
        public void List_Sort_NotifiesReset()
        {
            var list = new ObservableList<int> { 30, 10, 20 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.Sort();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Events[0].Action);
        }

        [Test]
        public void List_Sort_SingleElement_DoesNotNotify()
        {
            var list = new ObservableList<int> { 10 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.Sort();
            list.Reverse();

            Assert.AreEqual(0, capture.Calls, "单元素排序 / 反转无变化，不应通知");
        }

        [Test]
        public void List_Reverse_NotifiesReset()
        {
            var list = new ObservableList<int> { 10, 20, 30 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.Reverse();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Events[0].Action);
        }

        [Test]
        public void List_Clear_NotifiesReset()
        {
            var list = new ObservableList<int> { 10, 20 };
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.Clear();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Events[0].Action);
        }

        [Test]
        public void List_Clear_OnEmptyList_DoesNotNotify()
        {
            var list = new ObservableList<int>();
            var capture = new Capture<int>();
            list.AddListener(capture.OnChanged);

            list.Clear();

            Assert.AreEqual(0, capture.Calls, "空集合的 Clear 无变更，不应通知");
        }

        // ---------- ObservableDictionary<TKey, TValue> ----------

        [Test]
        public void Dictionary_Indexer_NewKey_NotifiesAddWithIndexMinusOne()
        {
            var dictionary = new ObservableDictionary<string, int>();
            var capture = new Capture<KeyValuePair<string, int>>();
            dictionary.AddListener(capture.OnChanged);

            dictionary["攻击力"] = 10;

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Events[0].Action);
            Assert.AreEqual(-1, capture.Events[0].NewStartingIndex, "字典无索引概念");
            Assert.AreEqual(10, capture.Events[0].NewItem.Value);
        }

        [Test]
        public void Dictionary_Indexer_ExistingKey_NotifiesReplaceWithOldValue()
        {
            var dictionary = new ObservableDictionary<string, int>();
            dictionary.Add("攻击力", 10);
            var capture = new Capture<KeyValuePair<string, int>>();
            dictionary.AddListener(capture.OnChanged);

            dictionary["攻击力"] = 80;

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, capture.Events[0].Action);
            Assert.AreEqual(80, capture.Events[0].NewItem.Value, "Replace 载荷 NewItem 为新键值对");
            Assert.AreEqual(10, capture.Events[0].OldItem.Value, "Replace 载荷 OldItem 为旧键值对（含旧值）");
            Assert.AreEqual(-1, capture.Events[0].NewStartingIndex);
        }

        [Test]
        public void Dictionary_Indexer_SameValue_DoesNotNotify()
        {
            var dictionary = new ObservableDictionary<string, int>();
            dictionary.Add("攻击力", 10);
            var capture = new Capture<KeyValuePair<string, int>>();
            dictionary.AddListener(capture.OnChanged);

            dictionary["攻击力"] = 10;

            Assert.AreEqual(0, capture.Calls, "无变更的写操作不应通知");
        }

        [Test]
        public void Dictionary_Remove_MissingKey_DoesNotNotify()
        {
            var dictionary = new ObservableDictionary<string, int>();
            dictionary.Add("hp", 100);
            var capture = new Capture<KeyValuePair<string, int>>();
            dictionary.AddListener(capture.OnChanged);

            Assert.IsFalse(dictionary.Remove("mp"));
            Assert.AreEqual(0, capture.Calls);
        }

        [Test]
        public void Dictionary_Clear_NotifiesReset()
        {
            var dictionary = new ObservableDictionary<string, int>();
            dictionary.Add("hp", 100);
            var capture = new Capture<KeyValuePair<string, int>>();
            dictionary.AddListener(capture.OnChanged);

            dictionary.Clear();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Events[0].Action);
        }

        // ---------- ObservableHashSet<T> ----------

        [Test]
        public void HashSet_Add_NotifiesAddAtIndexMinusOne()
        {
            var set = new ObservableHashSet<string>();
            var capture = new Capture<string>();
            set.AddListener(capture.OnChanged);

            var added = set.Add("玩家A");

            Assert.IsTrue(added);
            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Events[0].Action);
            Assert.AreEqual(-1, capture.Events[0].NewStartingIndex, "集合无索引概念");
            Assert.AreEqual("玩家A", capture.Events[0].NewItem);
        }

        [Test]
        public void HashSet_Add_Duplicate_DoesNotNotify()
        {
            var set = new ObservableHashSet<string>();
            set.Add("玩家A");
            var capture = new Capture<string>();
            set.AddListener(capture.OnChanged);

            var added = set.Add("玩家A");

            Assert.IsFalse(added);
            Assert.AreEqual(0, capture.Calls, "实际未发生增删时不通知");
        }

        [Test]
        public void HashSet_AddRange_NotifiesPerNewItemOnly()
        {
            var set = new ObservableHashSet<string>();
            set.Add("已有");
            var capture = new Capture<string>();
            set.AddListener(capture.OnChanged);

            set.AddRange(new[] { "已有", "新1", "新2" });

            Assert.AreEqual(2, capture.Calls, "已存在的元素不应通知，仅新增元素逐项通知");
            CollectionAssert.AreEqual(new[] { "新1", "新2" },
                new[] { capture.Events[0].NewItem, capture.Events[1].NewItem });
        }

        [Test]
        public void HashSet_Clear_NotifiesReset()
        {
            var set = new ObservableHashSet<int> { 1, 2 };
            var capture = new Capture<int>();
            set.AddListener(capture.OnChanged);

            set.Clear();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Events[0].Action);
        }

        // ---------- ObservableQueue<T> ----------

        [Test]
        public void Queue_Enqueue_NotifiesAddWithTailIndex()
        {
            var queue = new ObservableQueue<string>(new[] { "A" });
            var capture = new Capture<string>();
            queue.AddListener(capture.OnChanged);

            queue.Enqueue("B");

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, capture.Events[0].Action);
            Assert.AreEqual(1, capture.Events[0].NewStartingIndex, "入队追加到队尾");
            Assert.AreEqual("B", capture.Events[0].NewItem);
        }

        [Test]
        public void Queue_EnqueueRange_NotifiesPerItem()
        {
            var queue = new ObservableQueue<string>();
            var capture = new Capture<string>();
            queue.AddListener(capture.OnChanged);

            queue.EnqueueRange(new[] { "A", "B", "C" });

            Assert.AreEqual(3, capture.Calls, "EnqueueRange 应逐项通知");
            Assert.AreEqual(0, capture.Events[0].NewStartingIndex);
            Assert.AreEqual(2, capture.Events[2].NewStartingIndex);
        }

        [Test]
        public void Queue_Dequeue_NotifiesRemoveAtIndexZero()
        {
            var queue = new ObservableQueue<string>(new[] { "A", "B" });
            var capture = new Capture<string>();
            queue.AddListener(capture.OnChanged);

            var item = queue.Dequeue();

            Assert.AreEqual("A", item);
            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, capture.Events[0].Action);
            Assert.AreEqual(0, capture.Events[0].OldStartingIndex, "出队总是移除队首");
            Assert.AreEqual("A", capture.Events[0].OldItem);
        }

        [Test]
        public void Queue_DequeueRange_NotifiesPerItem()
        {
            var queue = new ObservableQueue<string>(new[] { "A", "B", "C" });
            var capture = new Capture<string>();
            queue.AddListener(capture.OnChanged);

            queue.DequeueRange(2);

            Assert.AreEqual(2, capture.Calls, "DequeueRange 应逐项通知");
            CollectionAssert.AreEqual(new[] { "A", "B" },
                new[] { capture.Events[0].OldItem, capture.Events[1].OldItem });
        }

        [Test]
        public void Queue_TryDequeue_Empty_DoesNotNotify()
        {
            var queue = new ObservableQueue<string>();
            var capture = new Capture<string>();
            queue.AddListener(capture.OnChanged);

            Assert.IsFalse(queue.TryDequeue(out _));
            Assert.AreEqual(0, capture.Calls, "空队列 TryDequeue 无变更，不应通知");
        }

        [Test]
        public void Queue_Clear_NotifiesReset()
        {
            var queue = new ObservableQueue<string>(new[] { "A" });
            var capture = new Capture<string>();
            queue.AddListener(capture.OnChanged);

            queue.Clear();

            Assert.AreEqual(1, capture.Calls);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, capture.Events[0].Action);
        }

        // ---------- 订阅管理 ----------

        [Test]
        public void Handle_Dispose_StopsNotifications()
        {
            var list = new ObservableList<int>();
            var calls = 0;

            var handle = list.AddListener(_ => calls++);
            list.Add(1);
            Assert.AreEqual(1, calls, "移除前应正常收到通知");

            handle.Dispose();
            list.Add(2);
            Assert.AreEqual(1, calls, "句柄 Dispose 后不应再收到通知");
        }

        [Test]
        public void RemoveListener_RemovesSpecificCallback()
        {
            var list = new ObservableList<int>();
            var calls = 0;

            void OnChanged(CollectionChangedEventArgs<int> _)
            {
                calls++;
            }

            list.AddListener(OnChanged);
            list.Add(1);
            Assert.AreEqual(1, calls);

            list.RemoveListener(OnChanged);
            list.Add(2);
            Assert.AreEqual(1, calls, "RemoveListener 后该回调不应再收到通知");
        }

        [Test]
        public void ClearListeners_StopsNotifications()
        {
            var list = new ObservableList<int>();
            var calls = 0;

            list.AddListener(_ => calls++);
            list.Add(1);
            Assert.AreEqual(1, calls);

            list.ClearListeners();
            list.Add(2);
            Assert.AreEqual(1, calls, "ClearListeners 清空全部监听后不应再收到通知");
        }

        [Test]
        public void Handle_BoundToGameObjectOnDisable_AutoRemoves()
        {
            var gameObject = new GameObject("ObservableDisableHost");
            try
            {
                var list = new ObservableList<int>();
                var calls = 0;

                list.AddListener(_ => calls++).RemoveListenerWhenGameObjectOnDisable(gameObject);
                list.Add(1);
                Assert.AreEqual(1, calls, "绑定后应正常收到通知");

                var trigger = gameObject.GetComponent<RemoveListenerOnDisableTrigger>();
                Assert.IsNotNull(trigger, "扩展方法应自动挂载触发器组件");

                trigger.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(trigger, null);

                list.Add(2);
                Assert.AreEqual(1, calls, "GameObject OnDisable 后监听应已自动移除");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        /// <summary>全量抄录事件载荷的订阅者。</summary>
        sealed class Capture<T>
        {
            public int Calls;

            public List<CollectionChangedEventArgs<T>> Events { get; } =
                new List<CollectionChangedEventArgs<T>>();

            public void OnChanged(CollectionChangedEventArgs<T> e)
            {
                Calls++;
                Events.Add(e);
            }
        }
    }
}
