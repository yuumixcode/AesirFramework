namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 严格档：Model 只读接口 + 写方法，Presenter 写入走 Command、读取走 Query。
    /// <para>与 MVC-3（Counter-Mvc-Strict）同构，差异仅在刷新路径（Presenter 推送 vs 订阅）。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}"/>
    public sealed class SampleMvpStrictCounterContext : AbstractContext<SampleMvpStrictCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel<ISampleMvpStrictCounterModel>(new SampleMvpStrictCounterModel());
        }
    }
}
