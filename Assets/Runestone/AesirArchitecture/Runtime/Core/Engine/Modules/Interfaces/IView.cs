namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 表现层接口。View 层通过此接口与模块上下文交互。
    /// <para>
    /// 能力：GetModel, GetService
    /// </para>
    /// <para>
    /// View 可读取 Model 和 Service，但不能执行 Command 或修改 Model 状态。
    /// </para>
    /// </summary>
    /// <remarks>
    /// View 层的只读约束是架构设计的核心意图：防止 View 直接修改 Model 状态，
    /// 所有写操作必须通过 Command 执行。此约束在接口层面通过不继承
    /// <see cref="ICanExecuteCommand" /> / <see cref="ICanExecuteQuery" /> 来强制保证，
    /// 确保数据流单向可控——View 只能观察 Model 的变化（经 <c>IReadOnlyObservableValue&lt;T&gt;</c>），
    /// 任何状态变更都须经由 Controller / Presenter 发起 Command 完成。
    /// </remarks>
    public interface IView : IContextHolder, ICanGetModel, ICanGetService { }
}
