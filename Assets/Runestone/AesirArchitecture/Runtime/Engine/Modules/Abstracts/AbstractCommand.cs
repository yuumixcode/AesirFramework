namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 命令基类。持有上下文引用，通过 <see cref="OnExecute" /> 执行命令逻辑。
    /// </summary>
    public abstract class AbstractCommand : ICommand
    {
        IContext _context;
        IContext IContextHolder.Context => _context;
        void ICanSetContext.SetContext(IContext context) => _context = context;
        void ICommand.Execute() => OnExecute();
        protected abstract void OnExecute();
    }
}
