namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 服务层接口。万能协调层，封装跨模块业务逻辑，协调模块间交互与通信。
    /// <para>
    /// Service 能读写 Model、调用其他 Service、监听和发布事件，完成跨模块协调。
    /// 不包含 <see cref="ICanExecuteCommand" /> 和 <see cref="ICanExecuteQuery" />——Command/Query 的执行入口应由 Controller/Presenter 触发。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetService, InvokeEvent, AddListener, Initialize, Dispose
    /// </para>
    /// </summary>
    public interface IService : IContextHolder, ICanSetContext, ICanGetModel, ICanGetService, ICanInvokeEvent,
        ICanAddListener, ICanInitialize { }
}
