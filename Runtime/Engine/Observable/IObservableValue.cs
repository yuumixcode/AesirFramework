namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 完整可观察属性接口。
    /// <para>Presenter 层通过此接口读写数据。</para>
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    public interface IObservableValue<T> : IReadOnlyObservableValue<T>
    {
        /// <summary>
        /// 读写属性值。设置新值时若与旧值不同，则触发变更通知。
        /// </summary>
        new T Value { get; set; }

        /// <summary>
        /// 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。
        /// </summary>
        void SetValueSilently(T value);

        /// <summary>
        /// 设置值。语义等价于 <see cref="Value" /> 的 setter，便于以方法形式调用。
        /// </summary>
        void SetValue(T value);
    }
}
