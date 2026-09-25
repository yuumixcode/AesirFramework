using Runestone.AesirArchitecture;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 窗口视图控制器基类（MVC 模式的 Controller 层兼 View 职责，Canvas 根窗口形态）。
    /// <para>
    /// 泛型参数 <typeparamref name="T" /> 指定窗口关联的 Context 类型，
    /// Context 作为 Model 和 Service 的聚合容器，在窗口与业务逻辑之间充当数据中转站。
    /// </para>
    /// <para>
    /// 继承链：<see cref="AesirBaseWindowViewController{T}" /> → <see cref="AesirBaseWindow" /> →
    /// <see cref="AesirMonoBehaviour" />。
    /// 子类通过 <c>Context</c> 属性（由 <see cref="IController" /> 能力组合提供）访问 Context 中持有的 Model / Service，
    /// 并可直接执行 Command / Query。
    /// </para>
    /// <example>
    /// 典型用法（通过 ICanExecuteCommand / ICanExecuteQuery 扩展方法分发命令与查询）：
    /// <code>
    /// public class MyWindowViewController : AesirBaseWindowViewController&lt;MyWindowContext&gt;
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
    /// <typeparam name="T">窗口关联的 Context 类型，须继承 <see cref="AbstractContext{T}" /> 并具有无参构造函数。</typeparam>
    public abstract class AesirBaseWindowViewController<T> : AesirBaseWindow, IController
        where T : AbstractContext<T>, new()
    {
        /// <summary>
        /// 获取窗口关联的全局 Context 单例。
        /// <para>
        /// <see cref="AbstractContext{T}" /> 以单例方式持有自身实例，
        /// <see cref="AbstractContext{T}.Instance" /> 返回其 <see cref="IContext" /> 接口形式，
        /// 供 Controller 层在不知道具体 Context 类型的情况下统一访问。
        /// </para>
        /// </summary>
        IContext IContextHolder.Context => AbstractContext<T>.Instance;
    }
}
