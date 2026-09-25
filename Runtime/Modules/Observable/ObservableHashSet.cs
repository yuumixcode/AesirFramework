using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察集合实现。
    /// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableHashSet{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 内部组合 <see cref="HashSet{T}" /> 存储元素，变更通知经 <see cref="MiniEvent{T}" /> 分发——Invoke 路径零分配
    /// （直接多播调用）。注意：订阅路径（<see cref="AddListener" /> / 句柄创建）有与监听者数量成正比的委托分配，
    /// 勿在每帧订阅场景使用。
    /// <para>
    /// <c>[SerializeField]</c> 标记 set 字段——Unity 原生不序列化 <see cref="HashSet{T}" />，
    /// 安装 Odin Inspector 后该字段可被 Odin 序列化，便于在 Inspector 中编辑初始元素（与 <see cref="ObservableDictionary{TKey, TValue}" />
    /// 行为一致）。
    /// </para>
    /// <para>
    /// 变更通知为单一事件（<see cref="AddListener" />）：写操作完成后才触发，监听者回调中读取到的集合已是变更后的状态；
    /// 无变更的操作不通知（Add 重复元素、Remove 不存在的元素、Clear 空集合）；
    /// 批量操作（AddRange / RemoveRange）逐项通知实际变更的元素；
    /// <see cref="Clear" /> 以 <see cref="NotifyCollectionChangedAction.Reset" /> 通知。
    /// 集合无索引概念，载荷索引固定 -1。
    /// </para>
    /// <para>
    /// 遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 <see cref="IReadOnlyObservableHashSet{T}" /> /
    /// <see cref="IEnumerable{T}" /> 接口遍历会装箱一次枚举器（与 BCL <see cref="HashSet{T}" /> 行为一致）。
    /// </para>
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableHashSet{T}" />
    /// <seealso cref="IObservableHashSet{T}" />
    [Serializable]
    public sealed class ObservableHashSet<T> : IObservableHashSet<T>
    {
        readonly MiniEvent<CollectionChangedEventArgs<T>> _changedEvent =
            new MiniEvent<CollectionChangedEventArgs<T>>();

        HashSet<T> set = new HashSet<T>();

        /// <summary>
        /// 默认构造，创建空集合。
        /// </summary>
        public ObservableHashSet() { }

        /// <summary>
        /// 指定初始容量构造，避免批量添加时的多次扩容（rehash）。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        public ObservableHashSet(int capacity) => set = new HashSet<T>(capacity);

        /// <summary>
        /// 指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。
        /// </summary>
        /// <param name="initialItems">初始元素序列。</param>
        public ObservableHashSet(IEnumerable<T> initialItems)
        {
            if (initialItems != null)
            {
                set = new HashSet<T>(initialItems);
            }
        }

        /// <summary>
        /// 指定元素比较器构造。
        /// </summary>
        /// <param name="comparer">元素比较器；为 null 时使用 <see cref="EqualityComparer{T}" />.Default。</param>
        public ObservableHashSet(IEqualityComparer<T> comparer) => set = new HashSet<T>(comparer);

        /// <summary>
        /// 内部 <see cref="HashSet{T}" /> 使用的元素比较器。
        /// </summary>
        public IEqualityComparer<T> Comparer => set.Comparer;

        /// <summary>
        /// 元素数量。
        /// </summary>
        public int Count => set.Count;

        /// <summary>
        /// 固定返回 <c>false</c>，该集合可写。
        /// </summary>
        public bool IsReadOnly => false;

        void ICollection<T>.Add(T item) => Add(item);

        /// <summary>
        /// 移除指定元素，成功时触发 Remove 通知（参数为该元素）。
        /// </summary>
        /// <param name="item">要移除的元素。</param>
        /// <returns>找到并移除返回 <c>true</c>；元素不存在时不触发通知，返回 <c>false</c>。</returns>
        public bool Remove(T item)
        {
            if (!set.Remove(item))
            {
                return false;
            }

            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Remove(item, -1));
            return true;
        }

        /// <summary>
        /// 清空集合。集合非空时以 Reset 通知；已为空时不通知。
        /// </summary>
        public void Clear()
        {
            if (set.Count == 0)
            {
                return;
            }

            set.Clear();
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Reset());
        }

        /// <summary>
        /// 判断是否包含指定元素。
        /// </summary>
        /// <param name="item">要查找的元素。</param>
        /// <returns>包含返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Contains(T item) => set.Contains(item);

        /// <summary>
        /// 从指定数组索引开始复制元素到目标数组。
        /// </summary>
        /// <param name="array">目标数组。</param>
        /// <param name="arrayIndex">目标数组起始索引。</param>
        public void CopyTo(T[] array, int arrayIndex) => set.CopyTo(array, arrayIndex);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)set).GetEnumerator();

        /// <inheritdoc cref="IObservableCollection{T}.AddListener" />
        public AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<T>> callback) =>
            _changedEvent.AddListener(callback);

        /// <inheritdoc cref="IObservableCollection{T}.RemoveListener" />
        public void RemoveListener(Action<CollectionChangedEventArgs<T>> callback) =>
            _changedEvent.RemoveListener(callback);

        /// <summary>
        /// 添加元素，实际添加时触发 Add 通知（参数为该元素）。
        /// </summary>
        /// <param name="item">要添加的元素。</param>
        /// <returns>新添加返回 <c>true</c>；元素已存在时不触发通知，返回 <c>false</c>。</returns>
        public bool Add(T item)
        {
            if (!set.Add(item))
            {
                return false;
            }

            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Add(item, -1));
            return true;
        }

        /// <summary>
        /// 批量添加元素序列，逐项触发 Add 通知（仅实际新增的元素）。
        /// </summary>
        /// <param name="itemsToAdd">要添加的元素序列。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToAdd" /> 为 null 时抛出。</exception>
        public void AddRange(IEnumerable<T> itemsToAdd)
        {
            if (itemsToAdd == null)
            {
                throw new ArgumentNullException(nameof(itemsToAdd));
            }

            foreach (var item in itemsToAdd)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 批量添加元素数组，逐项触发 Add 通知（仅实际新增的元素）。
        /// </summary>
        /// <param name="itemsToAdd">要添加的元素数组。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToAdd" /> 为 null 时抛出（与序列重载一致）。</exception>
        public void AddRange(T[] itemsToAdd)
        {
            if (itemsToAdd == null)
            {
                throw new ArgumentNullException(nameof(itemsToAdd));
            }

            foreach (var item in itemsToAdd)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 批量移除元素序列，逐项触发 Remove 通知（仅实际被移除的元素）。
        /// </summary>
        /// <param name="itemsToRemove">要移除的元素序列。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToRemove" /> 为 null 时抛出。</exception>
        public void RemoveRange(IEnumerable<T> itemsToRemove)
        {
            if (itemsToRemove == null)
            {
                throw new ArgumentNullException(nameof(itemsToRemove));
            }

            foreach (var item in itemsToRemove)
            {
                Remove(item);
            }
        }

        /// <summary>
        /// 批量移除元素数组，逐项触发 Remove 通知（仅实际被移除的元素）。
        /// </summary>
        /// <param name="itemsToRemove">要移除的元素数组。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToRemove" /> 为 null 时抛出（与序列重载一致）。</exception>
        public void RemoveRange(T[] itemsToRemove)
        {
            if (itemsToRemove == null)
            {
                throw new ArgumentNullException(nameof(itemsToRemove));
            }

            foreach (var item in itemsToRemove)
            {
                Remove(item);
            }
        }

        /// <summary>
        /// 按键取回集合中实际存储的等值元素（用于取回引用类型元素本身）。
        /// </summary>
        /// <param name="equalValue">用于比较的元素。</param>
        /// <param name="actualValue">集合中实际存储的等值元素。</param>
        /// <returns>集合中存在等值元素返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue) =>
            set.TryGetValue(equalValue, out actualValue);

        /// <summary>
        /// 返回遍历元素的结构体枚举器，foreach 具体类型时零分配。
        /// </summary>
        /// <returns>元素枚举器。</returns>
        public Enumerator GetEnumerator() => new Enumerator(set.GetEnumerator());

        /// <summary>
        /// 清空所有变更监听。
        /// </summary>
        /// <remarks>
        /// 清除全部监听引用，防止因监听者未释放导致的内存泄漏。
        /// 与 <see cref="Clear" /> 不同——后者清空的是集合元素。
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
        /// 遍历期间修改集合会抛 <see cref="InvalidOperationException" />（继承自内部 <see cref="HashSet{T}" /> 枚举器的版本检查，与 BCL 语义一致）。
        /// </remarks>
        public struct Enumerator : IEnumerator<T>
        {
            HashSet<T>.Enumerator _inner;

            internal Enumerator(HashSet<T>.Enumerator inner) => _inner = inner;

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
