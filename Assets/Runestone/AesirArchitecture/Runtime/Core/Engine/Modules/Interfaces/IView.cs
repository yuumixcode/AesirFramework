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
    /// View 层的只读约束是架构设计的核心意图：防止 View 直接修改 Model 状态。
    /// 接口层面强制保证的是「命令执行入口」——通过不继承
    /// <see cref="ICanExecuteCommand" /> / <see cref="ICanExecuteQuery" />，
    /// View 在类型系统上拿不到命令执行能力，只能观察 Model 的变化（经 <c>IReadOnlyObservableValue&lt;T&gt;</c>）。
    /// <para>
    /// 注意：「任何状态变更都须经由 Controller / Presenter 发起 Command 完成」是<b>严格档</b>的编写约定，
    /// 并非接口层强制——若 Model 暴露了公开写方法或 Service 暴露了可变状态，View 在类型层面仍可直调；
    /// 快捷档 / 标准档按各自档位约定放开此约束（快捷档 View 兼 Controller 直写 ObservableValue）。
    /// </para>
    /// </remarks>
    public interface IView : IContextHolder, ICanGetModel, ICanGetService { }
}
