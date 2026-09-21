using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Runestone.AesirArchitecture.Internal;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察集合实现。
    /// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableHashSet{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 内部组合 <see cref="HashSet{T}" /> 存储元素，使用 <see cref="MiniEvent" /> 管理监听者——Invoke 路径零分配（直接多播调用）。
    /// <para>
    /// <c>[SerializeField]</c> 标记 set 字段——Unity 原生不序列化 <see cref="HashSet{T}" />，
    /// 安装 Odin Inspector 后该字段可被 Odin 序列化，便于在 Inspector 中编辑初始元素（与 <see cref="ObservableDictionary{TKey, TValue}" />
    /// 行为一致）。
    /// </para>
    /// <para>
    /// 写操作完成后才触发事件，监听者回调中读取到的集合已是变更后的状态。
    /// 无变更的操作不触发事件：Add 重复元素、Remove 不存在的元素、Clear 空集合。
    /// </para>
    /// <para>
    /// 集合代数操作逐项触发事件：UnionWith / ExceptWith 逐项复用 <see cref="Add" /> / <see cref="Remove" />，天然去重；
    /// IntersectWith / SymmetricExceptWith 需物化参数集合与自身快照（各两次临时分配，低频批量操作可接受），
    /// SymmetricExceptWith 先触发全部 Removed、再触发全部 Added。
    /// </para>
    /// <para>
    /// 遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 <see cref="IReadOnlyObservableHashSet{T}" /> /
    /// <see cref="IEnumerable{T}" /> 接口遍历会装箱一次枚举器（与 BCL <see cref="HashSet{T}" /> 行为一致）。
    /// </para>
    /// <para>
    /// 除轻量事件外还提供 <see cref="IObservableCollection{T}.CollectionChanged" />
    /// （对齐 Cysharp.ObservableCollections 语义），同步视图与 R3 集成基于后者构建。
    /// </para>
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableHashSet{T}" />
    /// <seealso cref="IObservableHashSet{T}" />
    [Serializable]
    public sealed partial class ObservableHashSet<T> : IObservableHashSet<T>
    {
        readonly MiniEvent<T> _addedEvent = new MiniEvent<T>();
        readonly MiniEvent _clearedEvent = new MiniEvent();
        readonly MiniEvent<T> _removedEvent = new MiniEvent<T>();

        HashSet<T> set = new HashSet<T>();

        /// <summary>
        /// 同步根对象。所有写操作与 <see cref="CollectionChanged" /> 分发均在此对象上加锁，
        /// 同步视图（<see cref="ISynchronizedView{T, TView}" />）依赖它保证视图与集合一致。
        /// </summary>
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// 集合变更事件，语义与 Cysharp.ObservableCollections 的 <c>IObservableCollection&lt;T&gt;.CollectionChanged</c> 一致：
        /// 实际发生增删时通知，批量操作（<see cref="AddRange(IEnumerable{T})" /> 等）通知单次批量事件。
        /// 集合无索引概念，事件的索引参数固定为 -1。
        /// </summary>
        public event NotifyCollectionChangedEventHandler<T> CollectionChanged;

        /// <summary>
        /// 默认构造，创建空集合。
        /// </summary>
        public ObservableHashSet()
        {
            ObservableCollectionRegistry.Register(this);
        }

        /// <summary>
        /// 指定初始容量构造，避免批量添加时的多次扩容（rehash）。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        public ObservableHashSet(int capacity)
        {
            ObservableCollectionRegistry.Register(this);
            set = new HashSet<T>(capacity);
        }

        /// <summary>
        /// 指定初始元素构造。初始元素不触发 Added 事件（语义同反序列化填充）。
        /// </summary>
        /// <param name="initialItems">初始元素序列。</param>
        public ObservableHashSet(IEnumerable<T> initialItems)
        {
            ObservableCollectionRegistry.Register(this);
            if (initialItems != null)
            {
                set = new HashSet<T>(initialItems);
            }
        }

        /// <summary>
        /// 指定元素比较器构造。
        /// </summary>
        /// <param name="comparer">元素比较器；为 null 时使用 <see cref="EqualityComparer{T}" />.Default。</param>
        public ObservableHashSet(IEqualityComparer<T> comparer)
        {
            ObservableCollectionRegistry.Register(this);
            set = new HashSet<T>(comparer);
        }

        /// <summary>
        /// 指定初始容量与元素比较器构造。
        /// </summary>
        /// <param name="capacity">初始容量。</param>
        /// <param name="comparer">元素比较器；为 null 时使用 <see cref="EqualityComparer{T}" />.Default。</param>
        public ObservableHashSet(int capacity, IEqualityComparer<T> comparer)
        {
            ObservableCollectionRegistry.Register(this);
            set = new HashSet<T>(capacity, comparer);
        }

        /// <summary>
        /// 指定初始元素与元素比较器构造。初始元素不触发事件。
        /// </summary>
        /// <param name="initialItems">初始元素序列。</param>
        /// <param name="comparer">元素比较器；为 null 时使用 <see cref="EqualityComparer{T}" />.Default。</param>
        public ObservableHashSet(IEnumerable<T> initialItems, IEqualityComparer<T> comparer)
        {
            ObservableCollectionRegistry.Register(this);
            set = new HashSet<T>(comparer);

            if (initialItems != null)
            {
                set.UnionWith(initialItems);
            }
        }

        /// <summary>
        /// 元素数量。
        /// </summary>
        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return set.Count;
                }
            }
        }

        /// <summary>
        /// 内部 <see cref="HashSet{T}" /> 使用的元素比较器。
        /// </summary>
        public IEqualityComparer<T> Comparer => set.Comparer;

        /// <summary>
        /// 固定返回 <c>false</c>，该集合可写。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 添加元素，实际添加时触发 Added 事件（参数为该元素）。
        /// </summary>
        /// <param name="item">要添加的元素。</param>
        /// <returns>新添加返回 <c>true</c>；元素已存在时不触发事件，返回 <c>false</c>。</returns>
        public bool Add(T item)
        {
            lock (SyncRoot)
            {
                if (!set.Add(item))
                {
                    return false;
                }

                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(item, -1));
                _addedEvent.Invoke(item);
                return true;
            }
        }

        /// <summary>
        /// 批量添加元素序列，触发单次批量 Added 通知（仅包含实际新增的元素）。
        /// </summary>
        /// <param name="itemsToAdd">要添加的元素序列。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToAdd" /> 为 null 时抛出。</exception>
        public void AddRange(IEnumerable<T> itemsToAdd)
        {
            if (itemsToAdd == null)
            {
                throw new ArgumentNullException(nameof(itemsToAdd));
            }

            lock (SyncRoot)
            {
                if (!itemsToAdd.TryGetNonEnumeratedCount(out var capacity))
                {
                    capacity = 4;
                }

                using (var added = new ResizableArray<T>(capacity))
                {
                    foreach (var item in itemsToAdd)
                    {
                        if (set.Add(item))
                        {
                            added.Add(item);
                            _addedEvent.Invoke(item);
                        }
                    }

                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(added.Span, -1));
                }
            }
        }

        /// <summary>
        /// 批量添加元素数组，触发单次批量 Added 通知（仅包含实际新增的元素）。
        /// </summary>
        /// <param name="itemsToAdd">要添加的元素数组。</param>
        public void AddRange(T[] itemsToAdd) => AddRange(itemsToAdd.AsSpan());

        /// <summary>
        /// 批量添加元素（只读跨度重载），触发单次批量 Added 通知（仅包含实际新增的元素）。
        /// </summary>
        /// <param name="itemsToAdd">要添加的元素只读跨度。</param>
        public void AddRange(ReadOnlySpan<T> itemsToAdd)
        {
            lock (SyncRoot)
            {
                using (var added = new ResizableArray<T>(itemsToAdd.Length))
                {
                    foreach (var item in itemsToAdd)
                    {
                        if (set.Add(item))
                        {
                            added.Add(item);
                            _addedEvent.Invoke(item);
                        }
                    }

                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Add(added.Span, -1));
                }
            }
        }

        /// <summary>
        /// 批量移除元素序列，触发单次批量 Removed 通知（仅包含实际被移除的元素）。
        /// </summary>
        /// <param name="itemsToRemove">要移除的元素序列。</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToRemove" /> 为 null 时抛出。</exception>
        public void RemoveRange(IEnumerable<T> itemsToRemove)
        {
            if (itemsToRemove == null)
            {
                throw new ArgumentNullException(nameof(itemsToRemove));
            }

            lock (SyncRoot)
            {
                if (!itemsToRemove.TryGetNonEnumeratedCount(out var capacity))
                {
                    capacity = 4;
                }

                using (var removed = new ResizableArray<T>(capacity))
                {
                    foreach (var item in itemsToRemove)
                    {
                        if (set.Remove(item))
                        {
                            removed.Add(item);
                            _removedEvent.Invoke(item);
                        }
                    }

                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Remove(removed.Span, -1));
                }
            }
        }

        /// <summary>
        /// 批量移除元素数组，触发单次批量 Removed 通知（仅包含实际被移除的元素）。
        /// </summary>
        /// <param name="itemsToRemove">要移除的元素数组。</param>
        public void RemoveRange(T[] itemsToRemove) => RemoveRange(itemsToRemove.AsSpan());

        /// <summary>
        /// 批量移除元素（只读跨度重载），触发单次批量 Removed 通知（仅包含实际被移除的元素）。
        /// </summary>
        /// <param name="itemsToRemove">要移除的元素只读跨度。</param>
        public void RemoveRange(ReadOnlySpan<T> itemsToRemove)
        {
            lock (SyncRoot)
            {
                using (var removed = new ResizableArray<T>(itemsToRemove.Length))
                {
                    foreach (var item in itemsToRemove)
                    {
                        if (set.Remove(item))
                        {
                            removed.Add(item);
                            _removedEvent.Invoke(item);
                        }
                    }

                    CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Remove(removed.Span, -1));
                }
            }
        }

        /// <summary>
        /// 按键取回集合中实际存储的等值元素（用于取回引用类型元素本身）。
        /// </summary>
        /// <param name="equalValue">用于比较的元素。</param>
        /// <param name="actualValue">集合中实际存储的等值元素。</param>
        /// <returns>集合中存在等值元素返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
        {
            lock (SyncRoot)
            {
                return set.TryGetValue(equalValue, out actualValue);
            }
        }

        void ICollection<T>.Add(T item) => Add(item);

        /// <summary>
        /// 移除指定元素，成功时触发 Removed 事件（参数为该元素）。
        /// </summary>
        /// <param name="item">要移除的元素。</param>
        /// <returns>找到并移除返回 <c>true</c>；元素不存在时不触发事件，返回 <c>false</c>。</returns>
        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                if (!set.Remove(item))
                {
                    return false;
                }

                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Remove(item, -1));
                _removedEvent.Invoke(item);
                return true;
            }
        }

        /// <summary>
        /// 清空集合。集合非空时触发 Cleared 事件；已为空时不触发。
        /// </summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                var hadItems = set.Count > 0;
                set.Clear();
                CollectionChanged?.Invoke(NotifyCollectionChangedEventArgs<T>.Reset());

                // 轻量事件保留"空集合不通知"语义，与 CollectionChanged 并存
                if (hadItems)
                {
                    _clearedEvent.Invoke();
                }
            }
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

        /// <summary>
        /// 并集运算：逐项复用 <see cref="Add" />，仅对实际新增的元素触发 Added 事件。
        /// </summary>
        /// <param name="other">另一集合。</param>
        /// <remarks>逐项 Add 对已存在元素天然跳过，参数含重复项或传入集合自身时均为无变化操作。</remarks>
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 差集运算：逐项复用 <see cref="Remove" />，仅对实际存在的元素触发 Removed 事件。
        /// </summary>
        /// <param name="other">要移除的元素集合。</param>
        /// <remarks>
        /// 传入集合自身时短路为 <see cref="Clear" />（语义与 BCL <see cref="HashSet{T}" /> 一致）——
        /// 若无此短路，枚举期间的自移除会抛 <see cref="InvalidOperationException" />。
        /// </remarks>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (ReferenceEquals(this, other))
            {
                Clear();
                return;
            }

            foreach (var item in other)
            {
                Remove(item);
            }
        }

        /// <summary>
        /// 交集运算：移除不在 <paramref name="other" /> 中的元素，逐项触发 Removed 事件。
        /// </summary>
        /// <param name="other">保留元素的比较集合。</param>
        /// <remarks>
        /// 先物化 <paramref name="other" /> 与自身快照再逐项移除，避免枚举期间修改自身。
        /// 传入集合自身时为无变化操作，不触发事件。
        /// </remarks>
        public void IntersectWith(IEnumerable<T> other)
        {
            var keep = new HashSet<T>(other);
            var snapshot = new List<T>(set);
            foreach (var item in snapshot)
            {
                if (!keep.Contains(item))
                {
                    Remove(item);
                }
            }
        }

        /// <summary>
        /// 对称差集运算：移除双方共有的元素，添加仅 <paramref name="other" /> 拥有的元素。
        /// </summary>
        /// <param name="other">另一集合。</param>
        /// <remarks>
        /// 先触发全部 Removed、再触发全部 Added。物化 <paramref name="other" /> 后边扫描边消费，
        /// 一次遍历同时识别交集（待移除）与差集（待添加）。
        /// 传入集合自身时短路为 <see cref="Clear" />（语义与 BCL <see cref="HashSet{T}" /> 一致）。
        /// </remarks>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (ReferenceEquals(this, other))
            {
                Clear();
                return;
            }

            var otherSet = new HashSet<T>(other);
            var snapshot = new List<T>(set);
            foreach (var item in snapshot)
            {
                if (otherSet.Remove(item))
                {
                    Remove(item);
                }
            }

            foreach (var item in otherSet)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 判断当前集合是否为 <paramref name="other" /> 的子集。
        /// </summary>
        /// <param name="other">比较集合。</param>
        /// <returns>是子集返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool IsSubsetOf(IEnumerable<T> other) => set.IsSubsetOf(other);

        /// <summary>
        /// 判断当前集合是否为 <paramref name="other" /> 的真子集。
        /// </summary>
        /// <param name="other">比较集合。</param>
        /// <returns>是真子集返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other) => set.IsProperSubsetOf(other);

        /// <summary>
        /// 判断当前集合是否为 <paramref name="other" /> 的超集。
        /// </summary>
        /// <param name="other">比较集合。</param>
        /// <returns>是超集返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool IsSupersetOf(IEnumerable<T> other) => set.IsSupersetOf(other);

        /// <summary>
        /// 判断当前集合是否为 <paramref name="other" /> 的真超集。
        /// </summary>
        /// <param name="other">比较集合。</param>
        /// <returns>是真超集返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other) => set.IsProperSupersetOf(other);

        /// <summary>
        /// 判断当前集合与 <paramref name="other" /> 是否存在共同元素。
        /// </summary>
        /// <param name="other">比较集合。</param>
        /// <returns>存在共同元素返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Overlaps(IEnumerable<T> other) => set.Overlaps(other);

        /// <summary>
        /// 判断当前集合与 <paramref name="other" /> 是否包含完全相同的元素。
        /// </summary>
        /// <param name="other">比较集合。</param>
        /// <returns>元素相同返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool SetEquals(IEnumerable<T> other) => set.SetEquals(other);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)set).GetEnumerator();

        /// <inheritdoc cref="IReadOnlyObservableHashSet{T}.AddAddedListener" />
        public AutoRemoveListenerHandle AddAddedListener(Action<T> callback) =>
            _addedEvent.AddListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableHashSet{T}.RemoveAddedListener" />
        public void RemoveAddedListener(Action<T> callback) =>
            _addedEvent.RemoveListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableHashSet{T}.AddRemovedListener" />
        public AutoRemoveListenerHandle AddRemovedListener(Action<T> callback) =>
            _removedEvent.AddListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableHashSet{T}.RemoveRemovedListener" />
        public void RemoveRemovedListener(Action<T> callback) =>
            _removedEvent.RemoveListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableHashSet{T}.AddClearedListener" />
        public AutoRemoveListenerHandle AddClearedListener(Action callback) =>
            _clearedEvent.AddListener(callback);

        /// <inheritdoc cref="IReadOnlyObservableHashSet{T}.RemoveClearedListener" />
        public void RemoveClearedListener(Action callback) =>
            _clearedEvent.RemoveListener(callback);

        /// <summary>
        /// 返回遍历元素的结构体枚举器，foreach 具体类型时零分配。
        /// </summary>
        /// <returns>元素枚举器。</returns>
        public Enumerator GetEnumerator() => new Enumerator(set.GetEnumerator());

        /// <summary>
        /// 清空所有事件监听。
        /// </summary>
        /// <remarks>
        /// 清除全部监听引用，防止因监听者未释放导致的内存泄漏。
        /// 与 <see cref="Clear" /> 不同——后者清空的是集合元素。
        /// </remarks>
        public void ClearListeners()
        {
            _addedEvent.Dispose();
            _removedEvent.Dispose();
            _clearedEvent.Dispose();
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
