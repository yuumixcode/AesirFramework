#nullable enable
using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察集合契约：单一变更事件的订阅与退订。
    /// </summary>
    /// <typeparam name="T">元素类型（字典集合为 <see cref="KeyValuePair{TKey,TValue}" />）。</typeparam>
    /// <remarks>
    /// 由 <see cref="ObservableList{T}" /> / <see cref="ObservableDictionary{TKey, TValue}" /> /
    /// <see cref="ObservableHashSet{T}" /> / <see cref="ObservableQueue{T}" /> 统一实现。
    /// <para>
    /// 变更通知为单轨事件（内部由 <see cref="MiniEvent{T}" /> 承载）：无变更的写操作不通知
    /// （索引器赋相同值、Remove 不存在的元素、Clear 空集合等）；批量操作逐项通知；
    /// Sort / Reverse / Clear 以 <see cref="NotifyCollectionChangedAction.Reset" /> 通知（无附加字段）。
    /// </para>
    /// <para>
    /// <see cref="AddListener" /> 返回 <see cref="AutoRemoveListenerHandle" />，
    /// 可用 using 语句在作用域结束时自动移除，或经 <see cref="RemoveListenerExtensions" />
    /// 绑定到 Unity 生命周期（OnDestroy / OnDisable / 场景卸载）自动清理。
    /// </para>
    /// <para>
    /// 集合内部不加锁，不做任何线程同步——与框架整体边界一致，仅约定主线程使用。
    /// </para>
    /// </remarks>
    public interface IObservableCollection<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// 添加集合变更监听者。回调参数为本次变更（见 <see cref="CollectionChangedEventArgs{T}" /> 的字段约定）。
        /// </summary>
        /// <param name="callback">集合变更时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听，避免手动管理生命周期。</returns>
        AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<T>> callback);

        /// <summary>
        /// 移除集合变更监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddListener" /> 注册的回调函数。</param>
        void RemoveListener(Action<CollectionChangedEventArgs<T>> callback);
    }
}
