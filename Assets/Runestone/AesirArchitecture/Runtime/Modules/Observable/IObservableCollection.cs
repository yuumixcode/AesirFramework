#nullable enable
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 集合变更事件委托。以 <c>in</c> 参数传递载荷（<c>readonly ref struct</c>），分发路径零分配。
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="e">变更参数。</param>
    public delegate void NotifyCollectionChangedEventHandler<T>(in NotifyCollectionChangedEventArgs<T> e);

    /// <summary>
    /// 可观察集合契约：变更通知 + 同步根。
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 由 <see cref="ObservableList{T}" /> / <see cref="ObservableDictionary{TKey, TValue}" /> /
    /// <see cref="ObservableHashSet{T}" /> / <see cref="ObservableQueue{T}" /> 统一实现。
    /// <para>
    /// 变更通知为<b>上游语义</b>（对齐 Cysharp.ObservableCollections）：每次写操作都通知、
    /// 批量操作通知单次事件、Sort / Reverse 以 <see cref="NotifyCollectionChangedAction.Reset" />
    /// 携带 <see cref="SortOperation{T}" /> 通知。
    /// </para>
    /// </remarks>
    public interface IObservableCollection<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// 集合变更事件。
        /// </summary>
        event NotifyCollectionChangedEventHandler<T> CollectionChanged;

        /// <summary>
        /// 同步根对象。所有写操作与变更分发均在此对象上加锁。
        /// </summary>
        object SyncRoot { get; }
    }
}
