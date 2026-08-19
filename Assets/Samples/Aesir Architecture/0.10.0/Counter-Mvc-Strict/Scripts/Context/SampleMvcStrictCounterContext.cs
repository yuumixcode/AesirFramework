namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-3 严格档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 严格档（进阶课）：Model 对外只读 + 写方法，表现层不持有 Model——
    /// 写入用 Command，读取用 Query，View 对 Model 零持有。
    /// <para>对照：标准档（Counter-MVC）Controller 持有 Model + 订阅刷新。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}"/>
    public sealed class SampleMvcStrictCounterContext : AbstractContext<SampleMvcStrictCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel<ISampleMvcStrictCounterModel>(new SampleMvcStrictCounterModel());
        }
    }
}
