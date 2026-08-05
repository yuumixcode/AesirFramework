namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC 示例 —— 计数器模型接口。
    /// </summary>
    /// <remarks>
    /// Model 负责持有和变更业务数据。通过接口定义 Model，使 Controller / View
    /// 依赖抽象而非具体实现，便于替换实现类型（如运行时热替换为继承 MonoBehaviour 的 Model）。
    /// <para>计数值使用 <see cref="ObservableValue{T}"/> 包装，变更时自动通知所有监听者，无需手动广播事件。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IModel"/>
    /// <seealso cref="SampleMvcCounterModel"/>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    public interface ISampleMvcCounterModel : IModel
    {
        /// <summary>
        /// 当前计数值，作为可观察属性暴露给外部监听。
        /// </summary>
        /// <remarks>
        /// 外部通过 <c>Count.AddListener(...)</c> 注册变更回调，
        /// 当 Model 内部修改 <c>Count.Value</c> 时自动触发通知。
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
