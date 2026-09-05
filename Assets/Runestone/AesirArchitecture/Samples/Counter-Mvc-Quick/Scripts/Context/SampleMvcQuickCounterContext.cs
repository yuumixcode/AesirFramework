namespace Runestone.AesirArchitecture.Samples.MvcQuick
{
    /// <summary>
    /// MVC-1 快捷档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 快捷档（第一课）：最少概念跑通闭环。Context 按具体类注册 Model（不做接口抽象），
    /// 表现层（MonoViewController）直写直读，不建 Command、不建独立 Controller。
    /// <para>
    /// 对照：标准档（Counter-Mvc-Standard）只读暴露 + 写方法；
    /// 严格档（Counter-Mvc-Strict）接口注册 + Command 写入。
    /// </para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}" />
    public sealed class SampleMvcQuickCounterContext : AbstractContext<SampleMvcQuickCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel(new SampleMvcQuickCounterModel());
        }
    }
}
