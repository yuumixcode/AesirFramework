namespace Runestone.AesirArchitecture.Samples.MvpStrict
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器模型接口。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>严格档暴露面</b>：只读 <see cref="IReadOnlyObservableValue{T}" /> + 显式写方法，
    ///     修改必经 Model 写方法（Command 内部也经写方法，不直接碰 ObservableValue 的值）。
    ///     </para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IModel" />
    /// <seealso cref="SampleMvpStrictCounterModel" />
    public interface ISampleMvpStrictCounterModel : IModel
    {
        /// <summary>
        /// 当前计数值（只读可观察属性），外部只可订阅与读取。
        /// </summary>
        IReadOnlyObservableValue<int> Count { get; }

        /// <summary>
        /// 计数 +1（写方法，Command 调用）。
        /// </summary>
        void Increase();

        /// <summary>
        /// 计数 -1（写方法，Command 调用）。
        /// </summary>
        void Decrease();

        /// <summary>
        /// 将计数重置为 0（写方法，Command 调用）。
        /// </summary>
        void Reset();
    }
}
