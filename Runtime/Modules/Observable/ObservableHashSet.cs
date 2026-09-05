using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察集合实现。
    /// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableHashSet{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 内部组合 <see cref="HashSet{T}" /> 存储元素，使用 <see cref="MiniEvent" /> 管理监听者，零分配事件系统。
    /// <para>
    /// <c>[SerializeField]</c> 标记 set 字段——Unity 原生不序列化 <see cref="HashSet{T}" />，
    /// 安装 Odin Inspector 后该字段可被 Odin 序列化，便于在 Inspector 中编辑初始元素（与 <see cref="ObservableDictionary{TKey, TValue}" /> 行为一致）。
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
    /// 需要同步视图、R3 集成等高级能力时，建议使用完整方案
    /// <a href="https://github.com/Cysharp/ObservableCollections">Cysharp.ObservableCollections</a>。
    /// </para>
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableHashSet{T}" />
    /// <seealso cref="IObservableHashSet{T}" />
    [Serializable]
    public sealed class ObservableHashSet<T> : IObservableHashSet<T>
    {
        [SerializeField]
        HashSet<T> set = new HashSet<T>();

        readonly MiniEvent<T> _addedEvent = new MiniEvent<T>();
        readonly MiniEvent<T> _removedEvent = new MiniEvent<T>();
        readonly MiniEvent _clearedEvent = new MiniEvent();

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
        /// 指定初始元素构造。初始元素不触发 Added 事件（语义同反序列化填充）。
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
        /// 元素数量。
        /// </summary>
        public int Count => set.Count;

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
            if (!set.Add(item))
            {
                return false;
            }

            _addedEvent.Invoke(item);
            return true;
        }

        void ICollection<T>.Add(T item) => Add(item);

        /// <summary>
        /// 移除指定元素，成功时触发 Removed 事件（参数为该元素）。
        /// </summary>
        /// <param name="item">要移除的元素。</param>
        /// <returns>找到并移除返回 <c>true</c>；元素不存在时不触发事件，返回 <c>false</c>。</returns>
        public bool Remove(T item)
        {
            if (!set.Remove(item))
            {
                return false;
            }

            _removedEvent.Invoke(item);
            return true;
        }

        /// <summary>
        /// 清空集合。集合非空时触发 Cleared 事件；已为空时不触发。
        /// </summary>
        public void Clear()
        {
            if (set.Count == 0)
            {
                return;
            }

            set.Clear();
            _clearedEvent.Invoke();
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
            foreach (T item in other)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 差集运算：逐项复用 <see cref="Remove" />，仅对实际存在的元素触发 Removed 事件。
        /// </summary>
        /// <param name="other">要移除的元素集合。</param>
        /// <remarks>传入集合自身时短路为 <see cref="Clear" />（语义与 BCL <see cref="HashSet{T}" /> 一致）——
        /// 若无此短路，枚举期间的自移除会抛 <see cref="InvalidOperationException" />。</remarks>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (ReferenceEquals(this, other))
            {
                Clear();
                return;
            }

            foreach (T item in other)
            {
                Remove(item);
            }
        }

        /// <summary>
        /// 交集运算：移除不在 <paramref name="other" /> 中的元素，逐项触发 Removed 事件。
        /// </summary>
        /// <param name="other">保留元素的比较集合。</param>
        /// <remarks>先物化 <paramref name="other" /> 与自身快照再逐项移除，避免枚举期间修改自身。
        /// 传入集合自身时为无变化操作，不触发事件。</remarks>
        public void IntersectWith(IEnumerable<T> other)
        {
            var keep = new HashSet<T>(other);
            var snapshot = new List<T>(set);
            foreach (T item in snapshot)
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
            foreach (T item in snapshot)
            {
                if (otherSet.Remove(item))
                {
                    Remove(item);
                }
            }

            foreach (T item in otherSet)
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

        /// <summary>
        /// 返回遍历元素的结构体枚举器，foreach 具体类型时零分配。
        /// </summary>
        /// <returns>元素枚举器。</returns>
        public Enumerator GetEnumerator() => new Enumerator(set.GetEnumerator());

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
            private HashSet<T>.Enumerator _inner;

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
