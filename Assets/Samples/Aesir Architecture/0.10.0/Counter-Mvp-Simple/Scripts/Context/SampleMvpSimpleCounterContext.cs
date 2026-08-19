namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 简单档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 简单档：Presenter 直写 Model（不建 Command），适合 UI 交互直接映射数据的场景。
    /// <para>对照：标准档见 Counter-MVP（写入走 Command）。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}"/>
    public sealed class SampleMvpSimpleCounterContext : AbstractContext<SampleMvpSimpleCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel<ISampleMvpSimpleCounterModel>(new SampleMvpSimpleCounterModel());
        }
    }
}
