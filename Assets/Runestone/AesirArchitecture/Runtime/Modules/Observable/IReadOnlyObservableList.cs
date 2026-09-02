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
    /// 变更通知仅覆盖游戏 UI 绑定最常用的四种：Added / Removed / Replaced / Cleared。
    /// 需要 Move、Sort、SynchronizedView、R3 集成等高级能力时，建议使用完整方案
    /// <a href="https://github.com/Cysharp/ObservableCollections">Cysharp.ObservableCollections</a>。
    /// </para>
    /// </remarks>
    /// <seealso cref="IObservableList{T}" />
    /// <seealso cref="ObservableList{T}" />
    public interface IReadOnlyObservableList<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// 添加元素监听者。回调参数包含新元素及其索引。
        /// </summary>
        /// <param name="callback">元素添加时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听，避免手动管理生命周期。</returns>
        AutoRemoveListenerHandle AddAddedListener(Action<CollectionAddEventArgs<T>> callback);

        /// <summary>
        /// 移除元素添加监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddAddedListener" /> 注册的回调函数。</param>
        void RemoveAddedListener(Action<CollectionAddEventArgs<T>> callback);

        /// <summary>
        /// 添加元素移除监听者。回调参数包含被移除元素及其移除前所在索引。
        /// </summary>
        /// <param name="callback">元素移除时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        AutoRemoveListenerHandle AddRemovedListener(Action<CollectionRemoveEventArgs<T>> callback);

        /// <summary>
        /// 移除元素移除监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddRemovedListener" /> 注册的回调函数。</param>
        void RemoveRemovedListener(Action<CollectionRemoveEventArgs<T>> callback);

        /// <summary>
        /// 添加元素替换监听者。索引器赋值且新旧值不同时触发，回调参数包含索引、旧项与新项。
        /// </summary>
        /// <param name="callback">元素替换时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        AutoRemoveListenerHandle AddReplacedListener(Action<CollectionReplaceEventArgs<T>> callback);

        /// <summary>
        /// 移除元素替换监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddReplacedListener" /> 注册的回调函数。</param>
        void RemoveReplacedListener(Action<CollectionReplaceEventArgs<T>> callback);

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
