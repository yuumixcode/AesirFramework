namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 查询基类。持有上下文引用，通过 <see cref="OnExecute" /> 执行查询逻辑并返回结果。
    /// </summary>
    /// <typeparam name="TResult">查询结果类型</typeparam>
    public abstract class AbstractQuery<TResult> : IQuery<TResult>
    {
        IContext _context;
        IContext IContextHolder.Context => _context;
        void ICanSetContext.SetContext(IContext context) => _context = context;
        TResult IQuery<TResult>.Execute() => OnExecute();
        protected abstract TResult OnExecute();
    }
}
