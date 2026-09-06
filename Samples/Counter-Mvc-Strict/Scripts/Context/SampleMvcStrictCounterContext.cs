#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirArchitecture.Samples.MvcStrict
{
    /// <summary>
    /// MVC-3 严格档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 严格档（第三课）：Model 按<b>接口</b>注册 + 只读暴露 + 写方法；
    /// View 与 Controller 拆为两个实例——写入经 Command、加工读取经 Query，
    /// View 按接口类型持有 Model 订阅刷新。
    /// <para>
    /// 对照：标准档（Counter-Mvc-Standard）具体类注册 + Controller 直调写方法；
    /// 快捷档（Counter-Mvc-Quick）具体类注册 + 直改 ObservableValue。
    /// </para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}" />
    [InternalContext]
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
#endif
