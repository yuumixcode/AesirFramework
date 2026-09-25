using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 只读可观察字典接口。
    /// <para>View 层通过此接口读取键值并订阅变更，不能修改集合。</para>
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <remarks>
    /// 事件语义与 <see cref="MiniEvent{T}" /> 一致：回调触发时集合已处于变更后的状态；
    /// 监听者抛异常按原生 C# 事件 fail-fast 向上传播，监听回调不应抛异常属框架约定。
    /// <para>
    /// 变更通知为单一事件（<see cref="IObservableCollection{T}.AddListener" />），载荷为
    /// <see cref="CollectionChangedEventArgs{T}" />（<c>T</c> = <see cref="KeyValuePair{TKey,TValue}" />）：
    /// 新增键 → Add、移除键 → Remove、已有键赋新值 → Replace（旧值见 <see cref="CollectionChangedEventArgs{T}.OldItem" />）、
    /// Clear → Reset；字典无索引概念，事件索引固定 -1。
    /// </para>
    /// </remarks>
    /// <seealso cref="IObservableDictionary{TKey, TValue}" />
    /// <seealso cref="ObservableDictionary{TKey, TValue}" />
    public interface IReadOnlyObservableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>,
        IObservableCollection<KeyValuePair<TKey, TValue>> { }
}
