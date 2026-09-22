using System;
using System.Collections.Specialized;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 集合变更事件参数。每次变更携带单个变更项；批量操作（AddRange / RemoveRange 等）由集合逐项触发事件。
    /// </summary>
    /// <typeparam name="T">元素类型（字典集合为 <see cref="KeyValuePair{TKey,TValue}" />）。</typeparam>
    /// <remarks>
    /// 普通（非 ref）只读结构体，可自由存入集合与闭包；事件经 <see cref="MiniEvent{T}" /> 分发，
    /// 回调以值传递接收（结构体按字段拷贝，无堆分配）。
    /// <para>
    /// 各 <see cref="Action" /> 携带的字段：
    /// <see cref="NotifyCollectionChangedAction.Add" /> → <see cref="NewItem" /> / <see cref="NewStartingIndex" />；
    /// <see cref="NotifyCollectionChangedAction.Remove" /> → <see cref="OldItem" /> / <see cref="OldStartingIndex" />；
    /// <see cref="NotifyCollectionChangedAction.Replace" /> → <see cref="NewItem" /> / <see cref="OldItem" /> /
    /// <see cref="NewStartingIndex" />（等于 <see cref="OldStartingIndex" />）；
    /// <see cref="NotifyCollectionChangedAction.Move" /> → <see cref="NewItem" />（等于 <see cref="OldItem" />，
    /// 即被移动元素）/ 两个索引；
    /// <see cref="NotifyCollectionChangedAction.Reset" /> → 无附加字段（Clear / Sort / Reverse 共用，
    /// 监听方按"重建视图"处理）。
    /// </para>
    /// <para>无索引概念的集合（字典 / HashSet）所有索引固定为 -1。</para>
    /// </remarks>
    /// <seealso cref="IObservableCollection{T}" />
    public readonly struct CollectionChangedEventArgs<T>
    {
        /// <summary>
        /// 变更类型。
        /// </summary>
        public readonly NotifyCollectionChangedAction Action;

        /// <summary>
        /// 变更后的元素（Add / Replace / Move 有效，其余为 <c>default</c>）。
        /// </summary>
        public readonly T NewItem;

        /// <summary>
        /// 变更前的元素（Remove / Replace / Move 有效，其余为 <c>default</c>）。
        /// </summary>
        public readonly T OldItem;

        /// <summary>
        /// 新位置索引（Add / Replace / Move 有效）；无索引概念的集合为 -1。
        /// </summary>
        public readonly int NewStartingIndex;

        /// <summary>
        /// 原位置索引（Remove / Replace / Move 有效）；无索引概念的集合为 -1。
        /// </summary>
        public readonly int OldStartingIndex;

        CollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            T newItem,
            T oldItem,
            int newStartingIndex,
            int oldStartingIndex)
        {
            Action = action;
            NewItem = newItem;
            OldItem = oldItem;
            NewStartingIndex = newStartingIndex;
            OldStartingIndex = oldStartingIndex;
        }

        /// <summary>
        /// 构造添加变更。
        /// </summary>
        /// <param name="newItem">被添加的元素。</param>
        /// <param name="newStartingIndex">被添加到的索引；无索引概念的集合传 -1。</param>
        /// <returns>添加变更参数。</returns>
        public static CollectionChangedEventArgs<T> Add(T newItem, int newStartingIndex)
        {
            return new CollectionChangedEventArgs<T>(
                NotifyCollectionChangedAction.Add, newItem, default, newStartingIndex, -1);
        }

        /// <summary>
        /// 构造移除变更。
        /// </summary>
        /// <param name="oldItem">被移除的元素。</param>
        /// <param name="oldStartingIndex">移除前所在索引；无索引概念的集合传 -1。</param>
        /// <returns>移除变更参数。</returns>
        public static CollectionChangedEventArgs<T> Remove(T oldItem, int oldStartingIndex)
        {
            return new CollectionChangedEventArgs<T>(
                NotifyCollectionChangedAction.Remove, default, oldItem, -1, oldStartingIndex);
        }

        /// <summary>
        /// 构造替换变更。
        /// </summary>
        /// <param name="newItem">替换后的元素。</param>
        /// <param name="oldItem">替换前的元素。</param>
        /// <param name="index">替换位置索引；无索引概念的集合传 -1。</param>
        /// <returns>替换变更参数。</returns>
        public static CollectionChangedEventArgs<T> Replace(T newItem, T oldItem, int index)
        {
            return new CollectionChangedEventArgs<T>(
                NotifyCollectionChangedAction.Replace, newItem, oldItem, index, index);
        }

        /// <summary>
        /// 构造移动变更。
        /// </summary>
        /// <param name="movedItem">被移动的元素（同时写入 <see cref="NewItem" /> 与 <see cref="OldItem" />）。</param>
        /// <param name="newStartingIndex">移动后的索引。</param>
        /// <param name="oldStartingIndex">移动前的索引。</param>
        /// <returns>移动变更参数。</returns>
        public static CollectionChangedEventArgs<T> Move(T movedItem, int newStartingIndex, int oldStartingIndex)
        {
            return new CollectionChangedEventArgs<T>(
                NotifyCollectionChangedAction.Move, movedItem, movedItem, newStartingIndex, oldStartingIndex);
        }

        /// <summary>
        /// 构造重置变更（Clear / Sort / Reverse 共用，无附加字段）。
        /// </summary>
        /// <returns>重置变更参数。</returns>
        public static CollectionChangedEventArgs<T> Reset()
        {
            return new CollectionChangedEventArgs<T>(
                NotifyCollectionChangedAction.Reset, default, default, -1, -1);
        }
    }
}
