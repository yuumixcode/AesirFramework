namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 完整可观察属性接口。
    /// <para>Presenter 层通过此接口读写数据。</para>
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    /// <remarks>
    /// Presenter 层通过此接口读写数据，View 层通过 <see cref="IReadOnlyObservableValue{T}" /> 只读订阅。
    /// <para>
    /// <see cref="Value" /> 的 setter 使用 <see cref="EqualityComparer{T}" />.Default 比较新旧值，仅在值变化时触发通知。
    /// </para>
    /// <para>
    /// <see cref="SetValueSilently" /> 用于反序列化或批量更新场景——先静默设值再统一调用 <see cref="InvokeEvent" /> 触发通知，避免中间状态触发多次回调。
    /// </para>
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableValue{T}" />
    /// <seealso cref="ObservableValue{T}" />
    public interface IObservableValue<T> : IReadOnlyObservableValue<T>
    {
        /// <summary>
        /// 读写属性值。设置新值时若与旧值不同，则触发变更通知。
        /// </summary>
        /// <remarks>设置新值时，内部使用 <see cref="EqualityComparer{T}" />.Default 与旧值比较，仅在新旧值不同时触发变更通知。</remarks>
        new T Value { get; set; }

        /// <summary>
        /// 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。
        /// </summary>
        /// <param name="value">要设置的新值。</param>
        /// <remarks>不触发任何变更通知。适用于反序列化或批量更新场景——先静默设值，再统一调用 <see cref="InvokeEvent" /> 触发通知，避免中间状态触发多次回调。</remarks>
        void SetValueSilently(T value);

        /// <summary>
        /// 设置值。语义等价于 <see cref="Value" /> 的 setter，便于以方法形式调用。
        /// </summary>
        /// <param name="value">要设置的新值。</param>
        void SetValue(T value);
    }
}
