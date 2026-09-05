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
    /// 变更通知仅覆盖最常用的三种：Added / Removed / Cleared。集合没有索引与键，
    /// 也就没有 Replaced / Updated 语义；需要同步视图、R3 集成等高级能力时，建议使用完整方案
    /// <a href="https://github.com/Cysharp/ObservableCollections">Cysharp.ObservableCollections</a>。
    /// </para>
    /// <para>
    /// .NET Standard 2.1 无 <c>IReadOnlySet&lt;T&gt;</c>（.NET 5 才引入），只读侧无法继承只读集合契约，
    /// 因此本接口自行声明 <see cref="Contains" />，其余读取能力继承自 <see cref="IReadOnlyCollection{T}" />。
    /// </para>
    /// </remarks>
    /// <seealso cref="IObservableHashSet{T}" />
    /// <seealso cref="ObservableHashSet{T}" />
    public interface IReadOnlyObservableHashSet<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// 判断是否包含指定元素。
        /// </summary>
        /// <param name="item">要查找的元素。</param>
        /// <returns>包含返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        bool Contains(T item);

        /// <summary>
        /// 添加元素监听者。回调参数为新增的元素。
        /// </summary>
        /// <param name="callback">元素添加时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听，避免手动管理生命周期。</returns>
        AutoRemoveListenerHandle AddAddedListener(Action<T> callback);

        /// <summary>
        /// 移除元素添加监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddAddedListener" /> 注册的回调函数。</param>
        void RemoveAddedListener(Action<T> callback);

        /// <summary>
        /// 添加元素移除监听者。回调参数为被移除的元素。
        /// </summary>
        /// <param name="callback">元素移除时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        AutoRemoveListenerHandle AddRemovedListener(Action<T> callback);

        /// <summary>
        /// 移除元素移除监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddRemovedListener" /> 注册的回调函数。</param>
        void RemoveRemovedListener(Action<T> callback);

        /// <summary>
        /// 添加清空监听者。集合被清空且清空前非空时触发。
        /// </summary>
        /// <param name="callback">集合清空时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        AutoRemoveListenerHandle AddClearedListener(Action callback);

        /// <summary>
        /// 移除清空监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddClearedListener" /> 注册的回调函数。</param>
        void RemoveClearedListener(Action callback);
    }
}
