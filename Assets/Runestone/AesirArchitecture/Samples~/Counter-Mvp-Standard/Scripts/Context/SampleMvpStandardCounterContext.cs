namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-2 标准档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 标准档（第二课）：仍按具体类注册 Model（不做接口抽象）；
    /// Presenter 写入经 Model 写方法（不经 Command）、读取直取只读属性——
    /// 与 MVC-2（Counter-Mvc-Standard）分级一致，差异仅在刷新路径（Presenter 推送 vs View 订阅）。
    /// <para>对照：快捷档（Counter-Mvp-Quick）可写 ObservableValue 直接暴露、Presenter 直改；
    /// 严格档（Counter-Mvp-Strict）接口注册 + Command 写入 + Query 读取。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}"/>
    public sealed class SampleMvpStandardCounterContext : AbstractContext<SampleMvpStandardCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model（按具体类注册）。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel(new SampleMvpStandardCounterModel());
        }
    }
}
