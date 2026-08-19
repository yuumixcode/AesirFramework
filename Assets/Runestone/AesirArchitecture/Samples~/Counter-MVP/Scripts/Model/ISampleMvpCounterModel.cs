namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器模型接口。
    /// </summary>
    /// <remarks>
    /// 与 MVC 版本（<see cref="ISampleMvcCounterModel"/>）的接口定义相同，
    /// 差异在于调用方不同：MVC-2 中由 Command 调用，MVP-2 标准档中同样由 Command 调用；
    /// MVP-1 简单档（Counter-Mvp-Simple）中由 Presenter 直接调用（作为对照）。
    /// Model 本身不关心是谁在调用，保持了数据层的纯粹性。
    /// <para><b>通常档暴露面</b>：直接暴露可写 <see cref="ObservableValue{T}"/>；
    /// 严格档收窄为只读接口 + 写方法，见 Counter-Mvp-Strict 示例。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IModel"/>
    /// <seealso cref="SampleMvpCounterModel"/>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    /// <seealso cref="ISampleMvcCounterModel"/>
    public interface ISampleMvpCounterModel : IModel
    {
        /// <summary>
        /// 当前计数值，作为可观察属性暴露给外部监听。
        /// </summary>
        /// <remarks>
        /// Presenter 在每次操作后读取 <c>Count.Value</c> 并推送给 View 刷新。
        /// </remarks>
        ObservableValue<int> Count { get; }

        /// <summary>
        /// 计数 +1 并通过 <see cref="Count"/> 发布变更事件。
        /// </summary>
        void Increase();

        /// <summary>
        /// 计数 -1 并通过 <see cref="Count"/> 发布变更事件。
        /// </summary>
        void Decrease();

        /// <summary>
        /// 将计数重置为 0 并通过 <see cref="Count"/> 发布变更事件。
        /// </summary>
        void Reset();
    }
}
