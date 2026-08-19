namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-1 快捷档示例 —— 计数器模型接口。
    /// </summary>
    /// <remarks>
    /// <para><b>快捷档暴露面</b>：直接暴露可写 <see cref="ObservableValue{T}"/>，
    /// 表现层（MonoViewController）可直接改 <c>Count.Value</c>——这是快捷写法的合法路径，
    /// 适合原型与小功能；标准档起表现层写入必经 Command。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IModel"/>
    /// <seealso cref="SampleMvcQuickCounterModel"/>
    public interface ISampleMvcQuickCounterModel : IModel
    {
        /// <summary>
        /// 当前计数值，可观察属性，外部可监听也可直接修改。
        /// </summary>
        ObservableValue<int> Count { get; }
    }
}
