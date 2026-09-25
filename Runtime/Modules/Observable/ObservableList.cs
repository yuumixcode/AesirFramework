using System;
using System.Collections;
using System.Collections.Generic;
using Runestone.AesirArchitecture.Internal;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察列表实现。
    /// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableList{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 内部组合 <see cref="List{T}" /> 存储元素，变更通知经 <see cref="MiniEvent{T}" /> 分发——Invoke 路径零分配
    /// （直接多播调用）。注意：订阅路径（<see cref="AddListener" /> / 句柄创建）有与监听者数量成正比的委托分配，
    /// 勿在每帧订阅场景使用。
    /// <para>
    /// <c>[SerializeField]</c> 标记 items 字段使其可在 Inspector 中编辑初始元素；
    /// 反序列化填充不触发任何事件（与 <see cref="ObservableValue{T}" /> 行为一致）。
    /// </para>
    /// <para>
    /// 变更通知为单一事件（<see cref="AddListener" />）：写操作完成后才触发，监听者回调中读取到的集合已是变更后的状态；
    /// 无变更的操作不通知（Remove 不存在的元素、Clear 空列表、索引器赋相同值）；
    /// 批量操作（AddRange / InsertRange / RemoveRange）逐项通知；
    /// <see cref="Sort()" /> / <see cref="Reverse()" /> / <see cref="Clear" /> 以
    /// <see cref="NotifyCollectionChangedAction.Reset" /> 通知（无附加字段，监听方按"重建视图"处理）。
    /// </para>
    /// <para>
    /// 遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 <see cref="IReadOnlyObservableList{T}" /> /
    /// <see cref="IEnumerable{T}" /> 接口遍历会装箱一次枚举器（与 BCL <see cref="List{T}" /> 行为一致）。
    /// </para>
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableList{T}" />
    /// <seealso cref="IObservableList{T}" />
    [Serializable]
    public sealed class ObservableList<T> : IObservableList<T>
    {
        [SerializeField]
        List<T> items = new List<T>();

        readonly MiniEvent<CollectionChangedEventArgs<T>> _changedEvent =
            new MiniEvent<CollectionChangedEventArgs<T>>();

        /// <summary>
        /// 默认构造，创建空列表。
        /// </summary>
        public ObservableList() { }

        /// <summary>
        /// 指定初始容量构造，避免批量添加时的多次数组扩容。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        public ObservableList(int capacity) => items = new List<T>(capacity);

        /// <summary>
        /// 指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。
        /// </summary>
        /// <param name="initialItems">初始元素序列。</param>
        public ObservableList(IEnumerable<T> initialItems)
        {
            if (initialItems != null)
            {
                items.AddRange(initialItems);
            }
        }

        /// <summary>
        /// 元素数量。
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// 固定返回 <c>false</c>，该集合可写。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 读写指定索引的元素。值变化时触发 Replace 通知，相同则不通知。
        /// </summary>
        /// <param name="index">元素索引。</param>
        /// <remarks>使用 <see cref="EqualityComparer{T}" />.Default 判断值是否变化，仅在变化时触发通知。</remarks>
        public T this[int index]
        {
            get => items[index];
            set
            {
                var oldItem = items[index];
                if (EqualityComparer<T>.Default.Equals(oldItem, value))
                {
                    return;
                }

                items[index] = value;
                _changedEvent.Invoke(CollectionChangedEventArgs<T>.Replace(value, oldItem, index));
            }
        }

        /// <summary>
        /// 在末尾添加元素，触发 Add 通知（索引为 <see cref="Count" /> - 1）。
        /// </summary>
        /// <param name="item">要添加的元素。</param>
        public void Add(T item)
        {
            var index = items.Count;
            items.Add(item);
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Add(item, index));
        }

        /// <summary>
        /// 批量添加元素。逐项添加并逐项触发 Add 通知。
        /// </summary>
        /// <param name="itemsToAdd">要添加的元素序列。</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemsToAdd" /> 为 null 时抛出（对齐 BCL
        /// <see cref="List{T}" /> 行为）。
        /// </exception>
        public void AddRange(IEnumerable<T> itemsToAdd)
        {
            if (itemsToAdd == null)
            {
                throw new ArgumentNullException(nameof(itemsToAdd));
            }

            var index = items.Count;
            using (var clone = new CloneCollection<T>(itemsToAdd))
            {
                // 先整体拷贝再插入，避免枚举过程中集合被修改
                items.AddRange(clone.AsEnumerable());
                NotifyAddedRange(clone.Span, index);
            }
        }

        /// <summary>
        /// 在指定索引插入元素，触发 Add 通知（索引为插入位置）。
        /// </summary>
        /// <param name="index">插入位置索引。</param>
        /// <param name="item">要插入的元素。</param>
        public void Insert(int index, T item)
        {
            items.Insert(index, item);
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Add(item, index));
        }

        /// <summary>
        /// 移除第一个匹配元素，成功时触发 Remove 通知。
        /// </summary>
        /// <param name="item">要移除的元素。</param>
        /// <returns>找到并移除返回 <c>true</c>；元素不存在时不触发通知，返回 <c>false</c>。</returns>
        public bool Remove(T item)
        {
            var index = items.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 移除指定索引的元素，触发 Remove 通知（参数含移除前索引与被移除元素）。
        /// </summary>
        /// <param name="index">要移除元素的索引。</param>
        public void RemoveAt(int index)
        {
            var item = items[index];
            items.RemoveAt(index);
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Remove(item, index));
        }

        /// <summary>
        /// 清空列表。列表非空时以 Reset 通知；已为空时不通知。
        /// </summary>
        public void Clear()
        {
            if (items.Count == 0)
            {
                return;
            }

            items.Clear();
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Reset());
        }

        /// <summary>
        /// 判断是否包含指定元素。
        /// </summary>
        /// <param name="item">要查找的元素。</param>
        /// <returns>包含返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Contains(T item) => items.Contains(item);

        /// <summary>
        /// 返回指定元素的索引；不存在时返回 -1。
        /// </summary>
        /// <param name="item">要查找的元素。</param>
        /// <returns>元素索引或 -1。</returns>
        public int IndexOf(T item) => items.IndexOf(item);

        /// <summary>
        /// 从指定数组索引开始复制元素到目标数组。
        /// </summary>
        /// <param name="array">目标数组。</param>
        /// <param name="arrayIndex">目标数组起始索引。</param>
        public void CopyTo(T[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();

        /// <inheritdoc cref="IObservableCollection{T}.AddListener" />
        public AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<T>> callback) =>
            _changedEvent.AddListener(callback);

        /// <inheritdoc cref="IObservableCollection{T}.RemoveListener" />
        public void RemoveListener(Action<CollectionChangedEventArgs<T>> callback) =>
            _changedEvent.RemoveListener(callback);

        /// <summary>
        /// 批量添加数组元素，逐项触发 Add 通知。
        /// </summary>
        /// <param name="itemsToAdd">要添加的元素数组。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToAdd" /> 为 null 时抛出。</exception>
        public void AddRange(T[] itemsToAdd)
        {
            if (itemsToAdd == null)
            {
                throw new ArgumentNullException(nameof(itemsToAdd));
            }

            var index = items.Count;
            items.AddRange(itemsToAdd);
            NotifyAddedRange(itemsToAdd, index);
        }

        /// <summary>
        /// 在指定索引插入数组元素，逐项触发 Add 通知。
        /// </summary>
        /// <param name="index">插入位置索引。</param>
        /// <param name="itemsToInsert">要插入的元素数组。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToInsert" /> 为 null 时抛出。</exception>
        public void InsertRange(int index, T[] itemsToInsert)
        {
            if (itemsToInsert == null)
            {
                throw new ArgumentNullException(nameof(itemsToInsert));
            }

            items.InsertRange(index, itemsToInsert);
            NotifyAddedRange(itemsToInsert, index);
        }

        /// <summary>
        /// 在指定索引插入元素序列，逐项触发 Add 通知。
        /// </summary>
        /// <param name="index">插入位置索引。</param>
        /// <param name="itemsToInsert">要插入的元素序列。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToInsert" /> 为 null 时抛出。</exception>
        public void InsertRange(int index, IEnumerable<T> itemsToInsert)
        {
            if (itemsToInsert == null)
            {
                throw new ArgumentNullException(nameof(itemsToInsert));
            }

            using (var clone = new CloneCollection<T>(itemsToInsert))
            {
                items.InsertRange(index, clone.AsEnumerable());
                NotifyAddedRange(clone.Span, index);
            }
        }

        /// <summary>
        /// 从指定索引移除指定数量的元素，逐项触发 Remove 通知（按原始顺序，索引为移除前位置）。
        /// </summary>
        /// <param name="index">起始索引。</param>
        /// <param name="count">移除数量。</param>
        public void RemoveRange(int index, int count)
        {
            if (count == 0)
            {
                return;
            }

            // 先拷贝被移除区间再移除：通知需要按原始顺序携带被移除元素
            using (var clone = new CloneCollection<T>(items, index, count))
            {
                items.RemoveRange(index, count);
                for (var i = 0; i < count; i++)
                {
                    _changedEvent.Invoke(CollectionChangedEventArgs<T>.Remove(clone.Span[i], index + i));
                }
            }
        }

        /// <summary>
        /// 把元素从 <paramref name="oldIndex" /> 移动到 <paramref name="newIndex" />，触发单次 Move 通知。
        /// </summary>
        /// <param name="oldIndex">元素当前索引。</param>
        /// <param name="newIndex">目标索引。</param>
        public void Move(int oldIndex, int newIndex)
        {
            // 同索引为零变化操作，不通知（与「无变更的操作不通知」口径一致）
            if (oldIndex == newIndex)
            {
                return;
            }

            var movedItem = items[oldIndex];
            items.RemoveAt(oldIndex);
            items.Insert(newIndex, movedItem);
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Move(movedItem, newIndex, oldIndex));
        }

        /// <summary>
        /// 对全表排序，以 Reset 通知（少于 2 个元素时排序无变化，不通知）。
        /// </summary>
        public void Sort()
        {
            items.Sort();
            NotifyReset();
        }

        /// <summary>
        /// 使用指定比较器对全表排序，以 Reset 通知（少于 2 个元素时排序无变化，不通知）。
        /// </summary>
        /// <param name="comparer">元素比较器。</param>
        public void Sort(IComparer<T> comparer)
        {
            items.Sort(comparer);
            NotifyReset();
        }

        /// <summary>
        /// 反转全表，以 Reset 通知（少于 2 个元素时反转无变化，不通知）。
        /// </summary>
        public void Reverse()
        {
            items.Reverse();
            NotifyReset();
        }

        /// <summary>
        /// 对每个元素执行指定操作。
        /// </summary>
        /// <param name="action">对每个元素执行的操作。</param>
        /// <exception cref="ArgumentNullException"><paramref name="action" /> 为 null 时抛出。</exception>
        public void ForEach(Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var item in items)
            {
                action(item);
            }
        }

        /// <summary>
        /// 返回遍历元素的结构体枚举器，foreach 具体类型时零分配。
        /// </summary>
        /// <returns>元素枚举器。</returns>
        public Enumerator GetEnumerator() => new Enumerator(items.GetEnumerator());

        /// <summary>
        /// 对新增区间的每个元素触发 Add 通知（批量操作逐项通知）。
        /// </summary>
        /// <param name="addedItems">按集合内顺序排列的新增元素。</param>
        /// <param name="startIndex">新增区间在集合中的起始索引。</param>
        void NotifyAddedRange(ReadOnlySpan<T> addedItems, int startIndex)
        {
            for (var i = 0; i < addedItems.Length; i++)
            {
                _changedEvent.Invoke(CollectionChangedEventArgs<T>.Add(addedItems[i], startIndex + i));
            }
        }

        /// <summary>
        /// 触发 Reset 通知（少于 2 个元素时重排无变化，跳过）。
        /// </summary>
        /// <remarks>供 Sort / Reverse 复用；Clear 已自行判空，不经此路径。</remarks>
        void NotifyReset()
        {
            if (items.Count < 2)
            {
                return;
            }

            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Reset());
        }

        /// <summary>
        /// 清空所有变更监听。
        /// </summary>
        /// <remarks>
        /// 清除全部监听引用，防止因监听者未释放导致的内存泄漏。
        /// 与 <see cref="Clear" /> 不同——后者清空的是列表元素。
        /// </remarks>
        public void ClearListeners()
        {
            _changedEvent.Dispose();
        }

        /// <summary>
        /// 元素枚举器。
        /// </summary>
        /// <remarks>
        /// 结构体枚举器，foreach 具体类型时零分配。
        /// 遍历期间修改列表会抛 <see cref="InvalidOperationException" />（继承自内部 <see cref="List{T}" /> 枚举器的版本检查，与 BCL 语义一致）。
        /// </remarks>
        public struct Enumerator : IEnumerator<T>
        {
            List<T>.Enumerator _inner;

            internal Enumerator(List<T>.Enumerator inner) => _inner = inner;

            /// <summary>
            /// 获取当前位置的元素。
            /// </summary>
            public T Current => _inner.Current;

            /// <summary>
            /// 前进到下一个元素。
            /// </summary>
            /// <returns>存在下一个元素返回 <c>true</c>，遍历结束返回 <c>false</c>。</returns>
            public bool MoveNext() => _inner.MoveNext();

            object IEnumerator.Current => _inner.Current;

            void IEnumerator.Reset() => throw new NotSupportedException();

            void IDisposable.Dispose() { }
        }
    }
}
