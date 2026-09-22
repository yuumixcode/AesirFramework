using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 只读可观察列表接口。
    /// <para>View 层通过此接口枚举元素并订阅变更，不能修改集合。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 事件语义与 <see cref="MiniEvent{T}" /> 一致：回调触发时集合已处于变更后的状态；
    /// 监听者抛异常按原生 C# 事件 fail-fast 向上传播，监听回调不应抛异常属框架约定。
    /// <para>
    /// 变更通知为单一事件（<see cref="IObservableCollection{T}.AddListener" />），载荷 <see cref="CollectionChangedEventArgs{T}" />：
    /// 批量操作逐项通知、Sort / Reverse / Clear 以 Reset 通知、无变更的写操作不通知。
    /// </para>
    /// </remarks>
    /// <seealso cref="IObservableList{T}" />
    /// <seealso cref="ObservableList{T}" />
    public interface IReadOnlyObservableList<T> : IReadOnlyList<T>, IObservableCollection<T>
    {
    }
}
