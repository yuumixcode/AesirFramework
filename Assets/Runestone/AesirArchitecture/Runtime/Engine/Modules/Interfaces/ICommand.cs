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
        void Execute();
    }
}
