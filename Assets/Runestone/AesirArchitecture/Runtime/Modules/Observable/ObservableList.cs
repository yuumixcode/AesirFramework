using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察列表实现。
    /// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableList{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 内部组合 <see cref="List{T}" /> 存储元素，使用 <see cref="MiniEvent" /> 管理监听者，零分配事件系统。
    /// <para>
    /// <c>[SerializeField]</c> 标记 items 字段使其可在 Inspector 中编辑初始元素；
    /// 反序列化填充不触发任何事件（与 <see cref="ObservableValue{T}" /> 行为一致）。
    /// </para>
    /// <para>
    /// 写操作完成后才触发事件，监听者回调中读取到的集合已是变更后的状态。
    /// 无变更的操作不触发事件：Remove 不存在的元素、Clear 空列表、索引器赋相同值。
    /// </para>
    /// <para>
    /// 遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 <see cref="IReadOnlyObservableList{T}" /> /
    /// <see cref="IEnumerable{T}" /> 接口遍历会装箱一次枚举器（与 BCL <see cref="List{T}" /> 行为一致）。
    /// </para>
    /// <para>
    /// 需要 Move、Sort、SynchronizedView、R3 集成等高级能力时，建议使用完整方案
    /// <a href="https://github.com/Cysharp/ObservableCollections">Cysharp.ObservableCollections</a>。
    /// </para>
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableList{T}" />
    /// <seealso cref="IObservableList{T}" />
    [Serializable]
    public sealed class ObservableList<T> : IObservableList<T>
    {
        [SerializeField]
        List<T> items = new List<T>();

        readonly MiniEvent<CollectionAddEventArgs<T>> _addedEvent = new MiniEvent<CollectionAddEventArgs<T>>();
        readonly MiniEvent<CollectionRemoveEventArgs<T>> _removedEvent = new MiniEvent<CollectionRemoveEventArgs<T>>();
        readonly MiniEvent<CollectionReplaceEventArgs<T>> _replacedEvent = new MiniEvent<CollectionReplaceEventArgs<T>>();
        readonly MiniEvent _clearedEvent = new MiniEvent();

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
        /// 指定初始元素构造。初始元素不触发 Added 事件（语义同反序列化填充）。
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
        /// 读写指定索引的元素。赋值与旧值不同时触发 Replaced 事件，相同则跳过。
        /// </summary>
        /// <param name="index">元素索引。</param>
        /// <remarks>使用 <see cref="EqualityComparer{T}" />.Default 判断值是否变化，仅在变化时触发事件。</remarks>
        public T this[int index]
        {
            get => items[index];
            set
            {
                T oldItem = items[index];
                if (EqualityComparer<T>.Default.Equals(oldItem, value))
                {
                    return;
                }

                items[index] = value;
                _replacedEvent.Invoke(new CollectionReplaceEventArgs<T>(index, oldItem, value));
            }
        }

        /// <summary>
        /// 在末尾添加元素，触发 Added 事件（索引为 <see cref="Count" /> - 1）。
        /// </summary>
        /// <param name="item">要添加的元素。</param>
        public void Add(T item)
        {
            items.Add(item);
            _addedEvent.Invoke(new CollectionAddEventArgs<T>(items.Count - 1, item));
        }

        /// <summary>
        /// 批量添加元素。逐项添加并逐项触发 Added 事件。
        /// </summary>
        /// <param name="items">要添加的元素序列。</param>
        public void AddRange(IEnumerable<T> itemsToAdd)
        {
            foreach (T item in itemsToAdd)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 在指定索引插入元素，触发 Added 事件（索引为插入位置）。
        /// </summary>
        /// <param name="index">插入位置索引。</param>
        /// <param name="item">要插入的元素。</param>
        public void Insert(int index, T item)
        {
            items.Insert(index, item);
            _addedEvent.Invoke(new CollectionAddEventArgs<T>(index, item));
        }

        /// <summary>
        /// 移除第一个匹配元素，成功时触发 Removed 事件。
        /// </summary>
        /// <param name="item">要移除的元素。</param>
        /// <returns>找到并移除返回 <c>true</c>；元素不存在时不触发事件，返回 <c>false</c>。</returns>
        public bool Remove(T item)
        {
            int index = items.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 移除指定索引的元素，触发 Removed 事件（参数含移除前索引与被移除元素）。
        /// </summary>
        /// <param name="index">要移除元素的索引。</param>
        public void RemoveAt(int index)
        {
            T item = items[index];
            items.RemoveAt(index);
            _removedEvent.Invoke(new CollectionRemoveEventArgs<T>(index, item));
        }

        /// <summary>
        /// 清空列表。列表非空时触发 Cleared 事件；已为空时不触发。
        /// </summary>
        public void Clear()
        {
            if (items.Count == 0)
            {
                return;
            }

            items.Clear();
            _clearedEvent.Invoke();
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

        /// <summary>
        /// 返回遍历元素的结构体枚举器，foreach 具体类型时零分配。
        /// </summary>
        /// <returns>元素枚举器。</returns>
        public Enumerator GetEnumerator() => new Enumerator(items.GetEnumerator());

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();

        /// <inheritdoc cref="IReadOnlyObservableList{T}.AddAddedListener" />
        public AutoRemoveListenerHandle AddAddedListener(Action<CollectionAddEventArgs<T>> callback) =>
            _addedEvent.AddListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableList{T}.RemoveAddedListener" />
        public void RemoveAddedListener(Action<CollectionAddEventArgs<T>> callback) =>
            _addedEvent.RemoveListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableList{T}.AddRemovedListener" />
        public AutoRemoveListenerHandle AddRemovedListener(Action<CollectionRemoveEventArgs<T>> callback) =>
            _removedEvent.AddListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableList{T}.RemoveRemovedListener" />
        public void RemoveRemovedListener(Action<CollectionRemoveEventArgs<T>> callback) =>
            _removedEvent.RemoveListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableList{T}.AddReplacedListener" />
        public AutoRemoveListenerHandle AddReplacedListener(Action<CollectionReplaceEventArgs<T>> callback) =>
            _replacedEvent.AddListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableList{T}.RemoveReplacedListener" />
        public void RemoveReplacedListener(Action<CollectionReplaceEventArgs<T>> callback) =>
            _replacedEvent.RemoveListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableList{T}.AddClearedListener" />
        public AutoRemoveListenerHandle AddClearedListener(Action callback) =>
            _clearedEvent.AddListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableList{T}.RemoveClearedListener" />
        public void RemoveClearedListener(Action callback) =>
            _clearedEvent.RemoveListener(callback);

        /// <summary>
        /// 清空所有事件监听。
        /// </summary>
        /// <remarks>
        /// 清除全部监听引用，防止因监听者未释放导致的内存泄漏。
        /// 与 <see cref="Clear" /> 不同——后者清空的是列表元素。
        /// </remarks>
        public void ClearListeners()
        {
            _addedEvent.Dispose();
            _removedEvent.Dispose();
            _replacedEvent.Dispose();
            _clearedEvent.Dispose();
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
            private List<T>.Enumerator _inner;

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
