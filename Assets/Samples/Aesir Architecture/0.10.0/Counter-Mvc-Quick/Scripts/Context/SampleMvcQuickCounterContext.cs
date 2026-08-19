namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-1 快捷档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 快捷档（第一课）：最少概念跑通闭环。Context 仅注册一个 Model，
    /// 表现层（MonoViewController）直写直读，不建 Command、不建独立 Controller。
    /// <para>对照：标准档见 Counter-MVC（Command 写入 + 独立 Controller）。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}"/>
    public sealed class SampleMvcQuickCounterContext : AbstractContext<SampleMvcQuickCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel<ISampleMvcQuickCounterModel>(new SampleMvcQuickCounterModel());
        }
    }
}
