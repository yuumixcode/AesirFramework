using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察属性实现。
    /// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableValue{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    /// <remarks>
    /// 内部使用 <see cref="MiniEvent{T}" /> 管理监听者，零分配事件系统。
    /// <para>
    /// <c>[SerializeField]</c> 标记 value 字段使其可在 Inspector 中编辑。
    /// </para>
    /// <para>
    /// <see cref="PrivateValueFieldName" /> 和 <see cref="InvokeMethodName" /> 常量供 ObservableValueAttributeProcessor
    /// 反射引用，避免硬编码字符串导致的重构断裂。
    /// </para>
    /// <para>
    /// Model 层持有可写实例（<see cref="IObservableValue{T}" />），View 层通过 <see cref="IReadOnlyObservableValue{T}" /> 只读订阅。
    /// </para>
    /// <para>
    /// 值比较使用 <see cref="EqualityComparer{T}" />.Default.Equals，支持值类型和引用类型的正确比较。
    /// </para>
    /// </remarks>
    /// <seealso cref="IObservableValue{T}" />
    /// <seealso cref="IReadOnlyObservableValue{T}" />
    /// <seealso cref="MiniEvent{T}" />
    [Serializable]
    public sealed class ObservableValue<T> : IObservableValue<T>
    {
        /// <summary>
        /// 序列化字段名称常量，供 Odin AttributeProcessor 反射引用，避免硬编码字符串。
        /// </summary>
        public const string PrivateValueFieldName = nameof(value);

        /// <summary>
        /// 触发通知方法名称常量，供 Odin AttributeProcessor 反射引用，避免硬编码字符串。
        /// </summary>
        public const string InvokeMethodName = nameof(InvokeEvent);

        [SerializeField]
        T value;

        readonly MiniEvent<T> _valueChangedEvent = new MiniEvent<T>();

        /// <summary>
        /// 默认构造，使用类型 T 的默认值
        /// </summary>
        public ObservableValue() { }

        /// <summary>
        /// 指定初始值构造
        /// </summary>
        /// <param name="initialValue">初始值</param>
        public ObservableValue(T initialValue) => value = initialValue;

        /// <summary>
        /// 读写属性值。设置新值时若与旧值不同，则触发变更通知。
        /// </summary>
        /// <remarks>使用 <see cref="EqualityComparer{T}" />.Default 判断值是否变化，仅在变化时触发 <see cref="MiniEvent{T}" />.Invoke。</remarks>
        public T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    return;
                }

                this.value = value;
                _valueChangedEvent.Invoke(value);
            }
        }

        /// <summary>
        /// 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。
        /// </summary>
        /// <param name="v">要设置的新值。</param>
        /// <remarks>不触发变更通知，但仍做值比较——值未变时跳过赋值，避免无意义的引用替换。</remarks>
        public void SetValueSilently(T v)
        {
            if (EqualityComparer<T>.Default.Equals(value, v))
            {
                return;
            }

            value = v;
        }

        /// <summary>
        /// 设置值。语义等价于 <see cref="Value" /> 的 setter。
        /// </summary>
        /// <param name="v">要设置的新值。</param>
        public void SetValue(T v)
        {
            Value = v;
        }

        /// <summary>
        /// 添加监听者。回调参数为新值。
        /// </summary>
        /// <param name="callback">值变更时调用的回调函数，参数为变更后的新值。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听，避免手动管理生命周期。</returns>
        public AutoRemoveListenerHandle AddListener(Action<T> callback) =>
            _valueChangedEvent.AddListener(callback);

        /// <summary>
        /// 移除监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddListener" /> 注册的回调函数。</param>
        public void RemoveListener(Action<T> callback) => _valueChangedEvent.RemoveListener(callback);

        /// <summary>
        /// 添加监听并立即触发一次当前值，用于初始化时同步监听方状态。
        /// </summary>
        /// <param name="callback">值变更时调用的回调函数，参数为变更后的新值。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        /// <remarks>先通过 <see cref="AddListener" /> 添加监听，再立即用当前值触发一次回调，确保订阅方在注册瞬间即可同步初始状态。</remarks>
        public AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback)
        {
            var handle = AddListener(callback);
            callback?.Invoke(value);
            return handle;
        }

        /// <summary>
        /// 触发值变更通知，用于强制刷新订阅方状态。
        /// </summary>
        /// <remarks>强制触发通知，用于值未变但需要刷新订阅方状态的场景。</remarks>
        public void InvokeEvent()
        {
            _valueChangedEvent.Invoke(value);
        }

        /// <summary>
        /// 清除所有监听。
        /// </summary>
        /// <remarks>清除所有监听引用，防止因监听者未释放导致的内存泄漏。</remarks>
        public void Clear()
        {
            _valueChangedEvent.Dispose();
        }
    }
}
