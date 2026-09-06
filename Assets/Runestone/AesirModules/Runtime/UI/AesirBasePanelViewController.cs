using Runestone.AesirArchitecture;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 面板视图控制器基类（MVC 模式的 Controller 层，兼 View 职责）。
    /// <para>
    /// 泛型参数 <typeparamref name="T" /> 指定面板关联的 Context 类型，
    /// Context 作为 Model 和 Service 的聚合容器，在面板与业务逻辑之间充当数据中转站。
    /// </para>
    /// <para>
    /// 继承链：<see cref="AesirBasePanelViewController{T}" /> → <see cref="AesirBasePanel" /> → <see cref="AesirMonoBehaviour" />。
    /// 子类通过 <c>Context</c> 属性（由 <see cref="IController" /> 能力组合提供）访问 Context 中持有的 Model / Service，
    /// 并可直接执行 Command / Query。
    /// </para>
    /// <example>
    /// 典型用法（通过 ICanExecuteCommand / ICanExecuteQuery 扩展方法分发命令与查询）：
    /// <code>
    /// public class MyPanelViewController : AesirBasePanelViewController&lt;MyPanelContext&gt;
    /// {
    ///     protected override void OnShow(object payload)
    ///     {
    ///         var model = this.GetModel&lt;MyModel&gt;();
    ///         UpdateUI(model);
    ///         this.ExecuteCommand(new RefreshCommand());
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">面板关联的 Context 类型，须继承 <see cref="AbstractContext{T}" /> 并具有无参构造函数。</typeparam>
    public abstract class AesirBasePanelViewController<T> : AesirBasePanel, IController where T : AbstractContext<T>, new()
    {
        /// <summary>
        /// 获取面板关联的全局 Context 单例。
        /// <para>
        /// <see cref="AbstractContext{T}" /> 以单例方式持有自身实例，
        /// <see cref="AbstractContext{T}.Instance" /> 返回其 <see cref="IContext" /> 接口形式，
        /// 供 Controller 层在不知道具体 Context 类型的情况下统一访问。
        /// </para>
        /// </summary>
        IContext IContextHolder.Context => AbstractContext<T>.Instance;
    }
}
