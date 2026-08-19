namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 表现层控制器接口。Controller 层可通过此接口执行命令、查询、获取 Model 和 Service
    /// <para>
    /// 能力：GetModel, GetService, ExecuteCommand, ExecuteQuery
    /// </para>
    /// </summary>
    public interface IController : IContextHolder, ICanGetModel, ICanGetService, ICanExecuteCommand,
        ICanExecuteQuery { }

    /// <summary>
    /// 泛型控制器接口。绑定指定上下文类型，实现者自动获得 <see cref="IContextHolder.Context" /> 绑定。
    /// </summary>
    /// <typeparam name="T">
    /// 上下文类型，必须继承 <see cref="AbstractContext{T}" /> 并提供无参构造。
    /// </typeparam>
    /// <remarks>
    /// 通过显式接口实现 <see cref="IContextHolder.Context" /> 自动绑定到
    /// <see cref="AbstractContext{T}.Instance" /> 单例，无需手动注入上下文。
    /// 此设计使 Controller 与具体上下文类型解耦——只需声明泛型参数即可获得对应模块的全局上下文访问权。
    /// </remarks>
    public interface IController<T> : IController where T : AbstractContext<T>, new()
    {
        IContext IContextHolder.Context => AbstractContext<T>.Instance;
    }
}
