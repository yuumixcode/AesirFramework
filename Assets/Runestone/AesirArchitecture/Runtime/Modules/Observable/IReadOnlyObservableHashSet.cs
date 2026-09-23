using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 只读可观察集合接口。
    /// <para>View 层通过此接口读取元素并订阅变更，不能修改集合。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 事件语义与 <see cref="MiniEvent{T}" /> 一致：回调触发时集合已处于变更后的状态；
    /// 监听者抛异常按原生 C# 事件 fail-fast 向上传播，监听回调不应抛异常属框架约定。
    /// <para>
    /// 变更通知为单一事件（<see cref="IObservableCollection{T}.AddListener" />），载荷 <see cref="CollectionChangedEventArgs{T}" />：
    /// 批量操作（AddRange / RemoveRange）逐项通知实际变更的元素、Clear 以 Reset 通知、无变更的写操作不通知。
    /// </para>
    /// <para>
    /// .NET Standard 2.1 无 <c>IReadOnlySet&lt;T&gt;</c>（.NET 5 才引入），只读侧无法继承只读集合契约，
    /// 因此本接口自行声明 <see cref="Contains" />，其余读取能力继承自 <see cref="IReadOnlyCollection{T}" />。
    /// </para>
    /// </remarks>
    /// <seealso cref="IObservableHashSet{T}" />
    /// <seealso cref="ObservableHashSet{T}" />
    public interface IReadOnlyObservableHashSet<T> : IReadOnlyCollection<T>, IObservableCollection<T>
    {
        /// <summary>
        /// 判断是否包含指定元素。
        /// </summary>
        /// <param name="item">要查找的元素。</param>
        /// <returns>包含返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        bool Contains(T item);
    }
}
