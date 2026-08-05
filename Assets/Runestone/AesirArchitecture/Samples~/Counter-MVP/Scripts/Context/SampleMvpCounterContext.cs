namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器 Demo 上下文（MVP 版）。
    /// </summary>
    /// <remarks>
    /// Context 负责在初始化时将接口与具体实现绑定。
    /// 与 MVC 版本相比，MVP 的 Context 配置方式完全相同——差异体现在
    /// View 与 Model 之间的交互方式（通过 Presenter 中介而非 Command）。
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}"/>
    /// <seealso cref="Runestone.AesirArchitecture.IContext"/>
    /// <seealso cref="SampleMvcCounterContext"/>
    public sealed class SampleMvpCounterContext : AbstractContext<SampleMvpCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model。
        /// </summary>
        /// <remarks>
        /// 将 <see cref="ISampleMvpCounterModel"/> 接口绑定到 <see cref="SampleMvpCounterModel"/> 具体实现。
        /// Presenter 和 View 都通过 <c>GetModel&lt;ISampleMvpCounterModel&gt;()</c> 获取此实例。
        /// </remarks>
        protected override void Configure()
        {
            RegisterModel<ISampleMvpCounterModel>(new SampleMvpCounterModel());
        }
    }
}
