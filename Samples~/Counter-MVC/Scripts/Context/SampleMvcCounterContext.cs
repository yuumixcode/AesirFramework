namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC 示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// Context 是整个 MVC 架构的"组装工厂"与"依赖容器"。
    /// 它在 Configure 中将接口与具体实现绑定，使 View、Controller、Command
    /// 都能通过 <c>GetModel&lt;T&gt;()</c> 获取到正确的 Model 实例，
    /// 而无需知道具体的实现类型。这样可以在运行时替换 Model 实现而不影响调用方。
    /// <para>本示例仅注册了一个 Model，生产项目中可注册多个 Model、Controller 等。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}"/>
    /// <seealso cref="Runestone.AesirArchitecture.IContext"/>
    public sealed class SampleMvcCounterContext : AbstractContext<SampleMvcCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册所有需要用到的 Model。
        /// </summary>
        /// <remarks>
        /// 将 <see cref="ISampleMvcCounterModel"/> 接口绑定到 <see cref="SampleMvcCounterModel"/> 具体实现。
        /// 后续所有通过 <c>GetModel&lt;ISampleMvcCounterModel&gt;()</c> 的调用都会返回此实例。
        /// </remarks>
        protected override void Configure()
        {
            RegisterModel<ISampleMvcCounterModel>(new SampleMvcCounterModel());
        }
    }
}
