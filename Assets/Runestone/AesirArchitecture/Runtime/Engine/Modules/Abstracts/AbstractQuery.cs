namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 查询基类。持有上下文引用，通过 <see cref="OnExecute" /> 执行查询逻辑并返回结果。
    /// </summary>
    /// <typeparam name="TResult">查询结果类型，由子类的查询逻辑决定</typeparam>
    /// <remarks>
    /// Query 是只读操作，返回结果且无副作用——不修改任何 Model 状态。
    /// 与 <see cref="AbstractCommand" /> 的区别：Command 负责写操作且无返回值，
    /// Query 负责读操作并返回 <typeparamref name="TResult" />。
    /// 通过显式接口实现 <see cref="ICanSetContext.SetContext" /> 接收上下文注入，
    /// 使查询在执行时具备 <c>GetModel&lt;T&gt;</c> / <c>GetService&lt;T&gt;</c> 能力。
    /// 子类实现 <see cref="OnExecute" /> 返回查询结果，不应直接实现
    /// <see cref="IQuery{TResult}.Execute" />——后者已由本基类委托至 <see cref="OnExecute" />。
    /// </remarks>
    /// <seealso cref="AbstractCommand"/>
    /// <seealso cref="IQuery{TResult}"/>
    public abstract class AbstractQuery<TResult> : IQuery<TResult>
    {
        IContext _context;
        IContext IContextHolder.Context => _context;
        void ICanSetContext.SetContext(IContext context) => _context = context;
        TResult IQuery<TResult>.Execute() => OnExecute();

        /// <summary>
        /// 查询执行逻辑，子类必须实现
        /// </summary>
        /// <returns>查询结果，类型为 <typeparamref name="TResult" />。</returns>
        /// <remarks>
        /// 子类在此实现查询逻辑，通过 <c>this.GetModel&lt;T&gt;()</c> / <c>this.GetService&lt;T&gt;()</c>
        /// 读取模块状态并组装返回值。方法应为纯读操作，不产生任何副作用。
        /// </remarks>
        protected abstract TResult OnExecute();
    }
}
