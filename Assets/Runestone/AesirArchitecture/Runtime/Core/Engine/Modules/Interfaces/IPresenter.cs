using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 表现层 MVP 中介接口。Presenter 层彻底区分 Model 与 View，
    /// 作为两者间的唯一通信桥梁：从 Model 读取数据并格式化后推送给 View，处理 View 事件并转发给 Model。
    /// <para>
    /// 与 <see cref="IController" /> 的区别：Presenter 实现 <see cref="IDisposable" /> 支持显式释放。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetService, ExecuteCommand, ExecuteQuery
    /// </para>
    /// </summary>
    public interface IPresenter : IContextHolder, ICanExecuteCommand, ICanExecuteQuery, ICanGetModel,
        ICanGetService, IDisposable { }

    /// <summary>
    /// 泛型 MVP 中介接口。绑定指定上下文类型，实现者自动获得 <see cref="IContextHolder.Context" /> 绑定。
    /// </summary>
    /// <typeparam name="T">上下文类型，必须继承 <see cref="AbstractContext{T}" /> 并提供无参构造。</typeparam>
    /// <remarks>
    /// 通过显式接口实现 <see cref="IContextHolder.Context" /> 自动绑定到
    /// <see cref="AbstractContext{T}.Instance" /> 单例，无需手动注入上下文。
    /// 此设计使 Presenter 与具体上下文类型解耦——只需声明泛型参数即可获得对应模块的全局上下文访问权。
    /// </remarks>
    public interface IPresenter<T> : IPresenter where T : AbstractContext<T>, new()
    {
        IContext IContextHolder.Context => AbstractContext<T>.Instance;
    }
}
