using Runestone.AesirArchitecture;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 窗口视图基类（MVP 模式的 View 层，Canvas 根窗口形态）。
    /// <para>
    /// 泛型参数 <typeparamref name="T" /> 指定窗口关联的 Context 类型，
    /// Context 作为 Model 和 Service 的聚合容器，在窗口与业务逻辑之间充当数据中转站。
    /// </para>
    /// <para>
    /// 继承链：<see cref="AesirBaseWindowView{T}" /> → <see cref="AesirBaseWindow" /> → <see cref="AesirMonoBehaviour" />。
    /// 子类通过 <c>Context</c> 属性（由 <see cref="IView" /> 接口定义）访问 Context 中持有的 Model / Service。
    /// </para>
    /// <example>
    /// 典型用法（通过 ICanGetModel / ICanGetService 扩展方法访问 Context）：
    /// <code>
    /// public class MyWindowView : AesirBaseWindowView&lt;MyWindowContext&gt;
    /// {
    ///     protected override void OnShow(object payload)
    ///     {
    ///         var model = this.GetModel&lt;MyModel&gt;();
    ///         UpdateUI(model);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">窗口关联的 Context 类型，须继承 <see cref="AbstractContext{T}" /> 并具有无参构造函数。</typeparam>
    public abstract class AesirBaseWindowView<T> : AesirBaseWindow, IView where T : AbstractContext<T>, new()
    {
        /// <summary>
        /// 获取窗口关联的全局 Context 单例。
        /// <para>
        /// <see cref="AbstractContext{T}" /> 以单例方式持有自身实例，
        /// <see cref="AbstractContext{T}.Instance" /> 返回其 <see cref="IContext" /> 接口形式，
        /// 供 View 层在不知道具体 Context 类型的情况下统一访问。
        /// </para>
        /// </summary>
        IContext IContextHolder.Context => AbstractContext<T>.Instance;
    }
}
