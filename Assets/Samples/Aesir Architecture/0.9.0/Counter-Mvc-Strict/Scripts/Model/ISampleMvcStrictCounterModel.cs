namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-3 严格档示例 —— 计数器模型接口。
    /// </summary>
    /// <remarks>
    /// <para><b>严格档暴露面</b>：对外只暴露只读 <see cref="IReadOnlyObservableValue{T}"/>
    /// + 显式写方法（<see cref="Increase"/> / <see cref="Decrease"/> / <see cref="Reset"/>）——
    /// 外部（View / Query）只能订阅与读取，修改必经 Model 的写方法；
    /// Command 内部也经写方法修改，不直接碰 ObservableValue 的值。</para>
    /// <para>对照：通常档（Counter-MVC / Counter-Mvc-Quick）直接暴露可写 ObservableValue。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IModel"/>
    /// <seealso cref="SampleMvcStrictCounterModel"/>
    public interface ISampleMvcStrictCounterModel : IModel
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
