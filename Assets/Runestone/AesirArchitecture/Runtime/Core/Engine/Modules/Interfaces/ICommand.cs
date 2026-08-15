namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 同步命令接口。通过 Command 修改 Model 状态，只写无返回值。
    /// <para>
    /// 能力：GetModel, GetService, ExecuteCommand
    /// </para>
    /// </summary>
    public interface ICommand : IContextHolder, ICanSetContext, ICanGetModel, ICanGetService,
        ICanExecuteCommand
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <remarks>
        /// 执行前由 <c>CapabilityExtensions.ExecuteCommand</c> 自动注入上下文，
        /// 使命令在执行时具备 <c>GetModel&lt;T&gt;</c> / <c>GetService&lt;T&gt;</c> 能力。
        /// 实现者不应手动调用此方法，应统一经由能力扩展方法触发。
        /// </remarks>
        void Execute();
    }
}
