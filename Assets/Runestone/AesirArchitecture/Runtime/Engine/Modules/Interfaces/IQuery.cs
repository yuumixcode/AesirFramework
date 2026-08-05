namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 查询接口。通过 Query 执行读操作并返回结果，无副作用。
    /// <para>
    /// 与 <see cref="ICommand" /> 的区别：Command 负责写操作（无返回值），Query 负责读操作（返回 <typeparamref name="TResult" />）。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetService, ExecuteQuery
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">查询结果类型</typeparam>
    public interface IQuery<TResult> : IContextHolder, ICanSetContext, ICanGetModel, ICanGetService,
        ICanExecuteQuery
    {
        /// <summary>
        /// 执行查询并返回结果
        /// </summary>
        /// <returns>查询结果，类型为 <typeparamref name="TResult" />。</returns>
        TResult Execute();
    }
}
