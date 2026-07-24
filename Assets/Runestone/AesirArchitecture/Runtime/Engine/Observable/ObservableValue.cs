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
        public void SetValue(T v)
        {
            Value = v;
        }

        /// <summary>
        /// 添加监听者。回调参数为新值。
        /// </summary>
        public AutoRemoveListenerHandle AddListener(Action<T> callback) =>
            _valueChangedEvent.AddListener(callback);

        /// <summary>
        /// 移除监听者。
        /// </summary>
        public void RemoveListener(Action<T> callback) => _valueChangedEvent.RemoveListener(callback);

        /// <summary>
        /// 添加监听并立即触发一次当前值，用于初始化时同步监听方状态。
        /// </summary>
        public AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback)
        {
            var handle = AddListener(callback);
            callback?.Invoke(value);
            return handle;
        }

        /// <summary>
        /// 触发值变更通知，用于强制刷新订阅方状态。
        /// </summary>
        public void InvokeEvent()
        {
            _valueChangedEvent.Invoke(value);
        }

        /// <summary>
        /// 清除所有监听。
        /// </summary>
        public void Clear()
        {
            _valueChangedEvent.Dispose();
        }
    }
}
