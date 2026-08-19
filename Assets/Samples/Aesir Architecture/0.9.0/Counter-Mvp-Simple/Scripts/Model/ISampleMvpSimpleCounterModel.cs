namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 简单档示例 —— 计数器模型接口。
    /// </summary>
    /// <remarks>
    /// <para><b>简单档暴露面</b>：直接暴露可写 <see cref="ObservableValue{T}"/> + 便捷写方法，
    /// Presenter 可直接调用——这是 MVP 简单写法的合法路径；
    /// 标准档起写入必经 Command。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IModel"/>
    /// <seealso cref="SampleMvpSimpleCounterModel"/>
    public interface ISampleMvpSimpleCounterModel : IModel
    {
        /// <summary>
        /// 当前计数值，可观察属性。
        /// </summary>
        ObservableValue<int> Count { get; }

        /// <summary>
        /// 计数 +1。
        /// </summary>
        void Increase();

        /// <summary>
        /// 计数 -1。
        /// </summary>
        void Decrease();

        /// <summary>
        /// 将计数重置为 0。
        /// </summary>
        void Reset();
    }
}
