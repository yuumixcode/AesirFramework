using System;
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
    /// 变更通知仅覆盖最常用的四种：Added / Removed / Updated / Cleared。
    /// 需要同步视图、R3 集成等高级能力时，建议使用完整方案
    /// <a href="https://github.com/Cysharp/ObservableCollections">Cysharp.ObservableCollections</a>。
    /// </para>
    /// </remarks>
    /// <seealso cref="IObservableDictionary{TKey, TValue}" />
    /// <seealso cref="ObservableDictionary{TKey, TValue}" />
    public interface IReadOnlyObservableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// 添加键值监听者。回调参数为新增的键值对。
        /// </summary>
        /// <param name="callback">键值对添加时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听，避免手动管理生命周期。</returns>
        AutoRemoveListenerHandle AddAddedListener(Action<KeyValuePair<TKey, TValue>> callback);

        /// <summary>
        /// 移除键值添加监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddAddedListener" /> 注册的回调函数。</param>
        void RemoveAddedListener(Action<KeyValuePair<TKey, TValue>> callback);

        /// <summary>
        /// 添加键值移除监听者。回调参数为被移除的键值对（含移除前的值）。
        /// </summary>
        /// <param name="callback">键值对移除时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        AutoRemoveListenerHandle AddRemovedListener(Action<KeyValuePair<TKey, TValue>> callback);

        /// <summary>
        /// 移除键值移除监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddRemovedListener" /> 注册的回调函数。</param>
        void RemoveRemovedListener(Action<KeyValuePair<TKey, TValue>> callback);

        /// <summary>
        /// 添加值更新监听者。索引器为已存在的键赋新值且新旧值不同时触发，回调参数包含键、旧值与新值。
        /// </summary>
        /// <param name="callback">值更新时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        AutoRemoveListenerHandle AddUpdatedListener(Action<DictionaryUpdateEventArgs<TKey, TValue>> callback);

        /// <summary>
        /// 移除值更新监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddUpdatedListener" /> 注册的回调函数。</param>
        void RemoveUpdatedListener(Action<DictionaryUpdateEventArgs<TKey, TValue>> callback);

        /// <summary>
        /// 添加清空监听者。字典被清空且清空前非空时触发。
        /// </summary>
        /// <param name="callback">字典清空时调用的回调函数。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        AutoRemoveListenerHandle AddClearedListener(Action callback);

        /// <summary>
        /// 移除清空监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddClearedListener" /> 注册的回调函数。</param>
        void RemoveClearedListener(Action callback);
    }
}
