using System;
using System.Collections;
using System.Collections.Generic;
using Runestone.AesirArchitecture.Internal;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察字典实现。
    /// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableDictionary{TKey, TValue}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <remarks>
    /// 内部组合 <see cref="Dictionary{TKey,TValue}" /> 存储键值，变更通知经 <see cref="MiniEvent{T}" /> 分发——Invoke
    /// 路径零分配（直接多播调用）。注意：订阅路径（<see cref="AddListener" /> / 句柄创建）有与监听者数量成正比的委托分配，
    /// 勿在每帧订阅场景使用。
    /// <para>
    /// <c>[SerializeField]</c> 标记 dictionary 字段——Unity 原生不序列化 <see cref="Dictionary{TKey, TValue}" />，
    /// 安装 Odin Inspector 后该字段可被 Odin 序列化，便于在 Inspector 中编辑初始键值。
    /// </para>
    /// <para>
    /// 变更通知为单一事件（<see cref="AddListener" />），载荷为 <see cref="CollectionChangedEventArgs{T}" />
    /// （<c>T</c> = <see cref="KeyValuePair{TKey,TValue}" />）：写操作完成后才触发，监听者回调中读取到的集合已是变更后的状态；
    /// 索引器为不存在的键赋值触发 Add、为已有键赋新值触发 Replace（旧值见载荷 <c>OldItem</c>）、赋相同值不通知；
    /// Remove 不存在的键、Clear 空字典不通知。字典无索引概念，载荷索引固定 -1。
    /// </para>
    /// <para>
    /// 遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 <see cref="IReadOnlyObservableDictionary{TKey, TValue}" /> /
    /// <see cref="IEnumerable{T}" /> 接口遍历会装箱一次枚举器（与 BCL <see cref="Dictionary{TKey, TValue}" /> 行为一致）。
    /// </para>
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableDictionary{TKey, TValue}" />
    /// <seealso cref="IObservableDictionary{TKey, TValue}" />
    [Serializable]
    public sealed class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>
    {
        readonly MiniEvent<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>> _changedEvent =
            new MiniEvent<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>>();

        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        /// <summary>
        /// 默认构造，创建空字典。
        /// </summary>
        public ObservableDictionary()
        {
        }

        /// <summary>
        /// 指定初始键值构造。初始键值不触发变更通知（语义同反序列化填充）。
        /// </summary>
        /// <param name="initialItems">初始键值序列。</param>
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> initialItems)
        {
            if (initialItems == null)
            {
                return;
            }

            foreach (var pair in initialItems)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// 指定键比较器构造。
        /// </summary>
        /// <param name="comparer">键比较器；为 null 时使用 <see cref="EqualityComparer{TKey}" />.Default。</param>
        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        /// <summary>
        /// 键值对数量。
        /// </summary>
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        /// <summary>
        /// 内部 <see cref="Dictionary{TKey, TValue}" /> 使用的键比较器。
        /// </summary>
        public IEqualityComparer<TKey> Comparer => dictionary.Comparer;

        /// <summary>
        /// 固定返回 <c>false</c>，该集合可写。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 所有键的集合。
        /// </summary>
        public IEnumerable<TKey> Keys => dictionary.Keys;

        /// <summary>
        /// 所有值的集合。
        /// </summary>
        public IEnumerable<TValue> Values => dictionary.Values;

        /// <summary>
        /// 读写指定键的值。
        /// <para>读取：键不存在时抛 <see cref="KeyNotFoundException" />（fail-fast）。</para>
        /// <para>写入：键不存在时添加并触发 Add；键已存在且新值不同时更新并触发 Replace（旧值见载荷 OldItem）；值相同不通知。</para>
        /// </summary>
        /// <param name="key">键。</param>
        public TValue this[TKey key]
        {
            get => dictionary[key];
            set
            {
                if (dictionary.TryGetValue(key, out var oldValue))
                {
                    if (EqualityComparer<TValue>.Default.Equals(oldValue, value))
                    {
                        return;
                    }

                    dictionary[key] = value;
                    _changedEvent.Invoke(CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Replace(
                        new KeyValuePair<TKey, TValue>(key, value),
                        new KeyValuePair<TKey, TValue>(key, oldValue),
                        -1));
                }
                else
                {
                    dictionary[key] = value;
                    _changedEvent.Invoke(CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Add(
                        new KeyValuePair<TKey, TValue>(key, value),
                        -1));
                }
            }
        }

        /// <summary>
        /// 添加键值对，触发 Add 通知。键已存在时抛 <see cref="ArgumentException" />（fail-fast）。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
            _changedEvent.Invoke(CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Add(
                new KeyValuePair<TKey, TValue>(key, value),
                -1));
        }

        /// <summary>
        /// 添加键值对，触发 Add 通知。键已存在时抛 <see cref="ArgumentException" />（fail-fast）。
        /// </summary>
        /// <param name="item">要添加的键值对。</param>
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <summary>
        /// 判断是否包含指定键。
        /// </summary>
        /// <param name="key">要查找的键。</param>
        /// <returns>包含返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

        /// <summary>
        /// 判断是否包含指定键值对（键存在且值相等）。
        /// </summary>
        /// <param name="item">要查找的键值对。</param>
        /// <returns>包含返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item) =>
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);

        /// <summary>
        /// 获取与指定键关联的值。
        /// </summary>
        /// <param name="key">要查找的键。</param>
        /// <param name="value">键存在时为关联的值，否则为 <typeparamref name="TValue" /> 的默认值。</param>
        /// <returns>键存在返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

        /// <summary>
        /// 移除指定键的键值对，成功时触发 Remove 通知（参数含被移除的键值对）。
        /// </summary>
        /// <param name="key">要移除的键。</param>
        /// <returns>找到并移除返回 <c>true</c>；键不存在时不触发通知，返回 <c>false</c>。</returns>
        /// <remarks>使用 <see cref="Dictionary{TKey, TValue}.Remove(TKey, out TValue)" /> 在移除的同时取回旧值，单次哈希查找。</remarks>
        public bool Remove(TKey key)
        {
            if (!dictionary.Remove(key, out var value))
            {
                return false;
            }

            _changedEvent.Invoke(CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Remove(
                new KeyValuePair<TKey, TValue>(key, value),
                -1));
            return true;
        }

        /// <summary>
        /// 移除与指定键值对匹配的项（键存在且值相等），成功时触发 Remove 通知。
        /// </summary>
        /// <param name="item">要移除的键值对。</param>
        /// <returns>找到并移除返回 <c>true</c>；未匹配时不触发通知，返回 <c>false</c>。</returns>
        /// <remarks>不复用 <see cref="Remove(TKey)" />——其按键删除不校验值；此处先验证键值对完全匹配再移除，避免误删同键不同值。</remarks>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!dictionary.TryGetValue(item.Key, out var value) ||
                !EqualityComparer<TValue>.Default.Equals(value, item.Value))
            {
                return false;
            }

            dictionary.Remove(item.Key);
            _changedEvent.Invoke(CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Remove(
                new KeyValuePair<TKey, TValue>(item.Key, value),
                -1));
            return true;
        }

        /// <summary>
        /// 清空字典。字典非空时以 Reset 通知；已为空时不通知。
        /// </summary>
        public void Clear()
        {
            if (dictionary.Count == 0)
            {
                return;
            }

            dictionary.Clear();
            _changedEvent.Invoke(CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Reset());
        }

        /// <summary>
        /// 从指定数组索引开始复制键值对到目标数组。
        /// </summary>
        /// <param name="array">目标数组。</param>
        /// <param name="arrayIndex">目标数组起始索引。</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
            dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => dictionary.Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => dictionary.Values;

        /// <inheritdoc cref="IObservableCollection{T}.AddListener" />
        public AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>> callback) =>
            _changedEvent.AddListener(callback);

        /// <inheritdoc cref="IObservableCollection{T}.RemoveListener" />
        public void RemoveListener(Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>> callback) =>
            _changedEvent.RemoveListener(callback);

        /// <summary>
        /// 返回遍历键值对的结构体枚举器，foreach 具体类型时零分配。
        /// </summary>
        /// <returns>键值对枚举器。</returns>
        public Enumerator GetEnumerator() => new Enumerator(dictionary.GetEnumerator());

        /// <summary>
        /// 清空所有变更监听。
        /// </summary>
        /// <remarks>
        /// 清除全部监听引用，防止因监听者未释放导致的内存泄漏。
        /// 与 <see cref="Clear" /> 不同——后者清空的是字典键值。
        /// </remarks>
        public void ClearListeners()
        {
            _changedEvent.Dispose();
        }

        /// <summary>
        /// 键值对枚举器。
        /// </summary>
        /// <remarks>
        /// 结构体枚举器，foreach 具体类型时零分配。
        /// 遍历期间修改字典会抛 <see cref="InvalidOperationException" />（继承自内部 <see cref="Dictionary{TKey, TValue}" /> 枚举器的版本检查，与
        /// BCL 语义一致）。
        /// </remarks>
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            Dictionary<TKey, TValue>.Enumerator _inner;

            internal Enumerator(Dictionary<TKey, TValue>.Enumerator inner) => _inner = inner;

            /// <summary>
            /// 获取当前位置的键值对。
            /// </summary>
            public KeyValuePair<TKey, TValue> Current => _inner.Current;

            /// <summary>
            /// 前进到下一个键值对。
            /// </summary>
            /// <returns>存在下一个键值对返回 <c>true</c>，遍历结束返回 <c>false</c>。</returns>
            public bool MoveNext() => _inner.MoveNext();

            object IEnumerator.Current => _inner.Current;

            void IEnumerator.Reset() => throw new NotSupportedException();

            void IDisposable.Dispose() { }
        }
    }
}
