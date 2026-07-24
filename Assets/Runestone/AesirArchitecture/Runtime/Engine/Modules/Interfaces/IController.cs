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
    /// 上下文类型，必须继承 <see cref="AbstractContext{T}" />
    /// </typeparam>
    public interface IController<T> : IController where T : AbstractContext<T>, new()
    {
        IContext IContextHolder.Context => AbstractContext<T>.Interface;
    }
}
