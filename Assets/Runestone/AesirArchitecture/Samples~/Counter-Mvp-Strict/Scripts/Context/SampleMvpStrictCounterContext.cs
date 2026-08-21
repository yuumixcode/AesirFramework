namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 严格档（第三课）：Model 按<b>接口</b>注册 + 只读暴露 + 写方法，
    /// Presenter 写入走 Command、读取走 Query——读写全解耦。
    /// <para>对照：快捷档（Counter-Mvp-Quick）可写 ObservableValue 直接暴露、Presenter 直改；
    /// 标准档（Counter-Mvp-Standard）具体类注册 + 写方法直调。</para>
    /// <para>与 MVC-3（Counter-Mvc-Strict）分级一致，差异仅在刷新路径（Presenter 推送 vs View 订阅）。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}"/>
    public sealed class SampleMvpStrictCounterContext : AbstractContext<SampleMvpStrictCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model（按接口注册）。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel<ISampleMvpStrictCounterModel>(new SampleMvpStrictCounterModel());
        }
    }
}
