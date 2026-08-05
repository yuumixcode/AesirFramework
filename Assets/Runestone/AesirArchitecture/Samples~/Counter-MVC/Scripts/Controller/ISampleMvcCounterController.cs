namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC 示例 —— 计数器控制器接口。
    /// </summary>
    /// <remarks>
    /// Controller 是 View 与 Model 之间的"翻译层"。
    /// View 不直接操作 Model，而是调用 Controller 暴露的方法；
    /// Controller 再将操作意图转发给具体的 Command 执行。
    /// 通过接口定义 Controller，便于在测试中替换 Mock 实现。
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IController{T}"/>
    /// <seealso cref="SampleMvcCounterController"/>
    public interface ISampleMvcCounterController : IController<SampleMvcCounterContext>
    {
        /// <summary>
        /// 请求增加计数值。实际执行逻辑由 <see cref="SampleMvcIncreaseCommand"/> 完成。
        /// </summary>
        void Increase();

        /// <summary>
        /// 请求减少计数值。实际执行逻辑由 <see cref="SampleMvcDecreaseCommand"/> 完成。
        /// </summary>
        void Decrease();

        /// <summary>
        /// 请求重置计数值。实际执行逻辑由 <see cref="SampleMvcResetCommand"/> 完成。
        /// </summary>
        void ResetCounter();
    }
}
