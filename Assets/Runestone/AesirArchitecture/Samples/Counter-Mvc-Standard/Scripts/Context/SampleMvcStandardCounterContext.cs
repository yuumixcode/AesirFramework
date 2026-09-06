#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirArchitecture.Samples.MvcStandard
{
    /// <summary>
    /// MVC-2 标准档示例 —— 计数器 Demo 上下文。
    /// </summary>
    /// <remarks>
    /// 标准档（第二课）：仍按具体类注册 Model（不做接口抽象）；
    /// View 与 Controller 拆为两个实例、共享同一个 Model 实例，写入经 Model 写方法（不经 Command）。
    /// <para>对照：快捷档（Counter-Mvc-Quick）同样具体类注册；严格档（Counter-Mvc-Strict）接口注册。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractContext{T}" />
    [InternalContext]
    public sealed class SampleMvcStandardCounterContext : AbstractContext<SampleMvcStandardCounterContext>
    {
        /// <summary>
        /// 在上下文初始化时注册计数器 Model（按具体类注册）。
        /// </summary>
        protected override void Configure()
        {
            RegisterModel(new SampleMvcStandardCounterModel());
        }
    }
}
#endif
