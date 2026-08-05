namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 服务层接口。万能协调层，封装跨模块业务逻辑，协调模块间交互与通信。
    /// <para>
    /// Service 能读写 Model、调用其他 Service，完成跨模块协调。
    /// 不包含 <see cref="ICanExecuteCommand" /> 和 <see cref="ICanExecuteQuery" />——Command/Query 的执行入口应由
    /// Controller/Presenter 触发。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetService, Initialize, Dispose
    /// </para>
    /// </summary>
    /// <seealso cref="AbstractService"/>
    /// <seealso cref="ICanInitialize"/>
    public interface IService : IContextHolder, ICanSetContext, ICanGetModel, ICanGetService,
        ICanInitialize { }
}
