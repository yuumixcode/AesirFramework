namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 表现层接口。View 层通过此接口与模块上下文交互。
    /// <para>
    /// 能力：GetModel, GetService, AddListener, InvokeEvent
    /// </para>
    /// <para>
    /// View 可通过事件向上通信，可读取 Model 和 Service，但不能执行 Command 或修改 Model 状态。
    /// </para>
    /// </summary>
    public interface IView : IContextHolder, ICanGetModel, ICanGetService, ICanAddListener,
        ICanInvokeEvent { }
}
