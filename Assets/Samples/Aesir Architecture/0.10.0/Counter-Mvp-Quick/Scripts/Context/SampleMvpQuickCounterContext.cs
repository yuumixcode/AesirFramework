namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 快捷档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 快捷档（第一课）：最少概念理解被动视图。Context 按具体类注册 Model（不做接口抽象），
    /// Presenter 直写直读，不建 Command、不建 Query。
    /// <para>
    /// 对照：标准档（Counter-Mvp-Standard）收窄为只读暴露 + 写方法；
    /// 严格档（Counter-Mvp-Strict）接口注册 + Command 写入 + Query 读取。
    /// </para>
    /// <para>
    /// 与 MVC-1（Counter-Mvc-Quick）分级一致，差异仅在刷新路径：
    /// MVC 的 View 自己订阅 Model，MVP 的 View 被动、由 Presenter 推送刷新。
    /// </para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}" />
    public sealed class SampleMvpQuickCounterContext : AbstractContext<SampleMvpQuickCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel(new SampleMvpQuickCounterModel());
        }
    }
}
