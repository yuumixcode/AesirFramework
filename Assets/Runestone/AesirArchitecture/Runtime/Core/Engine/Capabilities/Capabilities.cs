using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 上下文持有者接口。
    /// </summary>
    /// <remarks>
    /// 该接口是整个能力接口体系的根基，所有需要访问上下文的角色类型均继承此接口。
    /// <para>
    /// 继承此接口的角色包括：<see cref="IModel" />、<see cref="IService" />、
    /// View、Controller、Presenter、Command 以及 Query。
    /// </para>
    /// <para>
    /// 通过持有 <see cref="IContext" /> 引用，角色类型可以在运行时获取已注册的 Model、Service，
    /// 或执行命令与查询，从而实现模块间的松耦合协作。
    /// </para>
    /// </remarks>
    public interface IContextHolder
    {
        /// <summary>
        /// 获取持有的模块上下文
        /// </summary>
        /// <remarks>
        /// 该属性返回的上下文引用由 <see cref="ICanSetContext.SetContext" /> 在注册阶段注入，
        /// 角色类型无需手动管理其生命周期。
        /// </remarks>
        IContext Context { get; }
    }

    /// <summary>
    /// 可设置上下文引用接口
    /// </summary>
    /// <remarks>
    /// 该接口定义了上下文注入的入口。框架在注册模块时由 <c>AbstractContext&lt;T&gt;</c>
    /// 自动调用 <see cref="SetContext(IContext)" /> 将自身引用注入到被注册的对象中，
    /// 因此子类无需也不应手动调用此方法。
    /// <para>
    /// 通过将注入逻辑收口在注册流程中，保证了所有模块在进入业务逻辑之前
    /// 一定持有有效的上下文引用，避免了空引用和时序问题。
    /// </para>
    /// </remarks>
    public interface ICanSetContext
    {
        /// <summary>
        /// 设置上下文引用
        /// </summary>
        /// <param name="context">要注入的模块上下文</param>
        void SetContext(IContext context);
    }

    /// <summary>
    /// 可初始化接口。提供初始化与初始化状态标记。
    /// <para>
    /// 被 <see cref="IModel" /> 和 <see cref="IService" /> 继承。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 框架的初始化遵循严格的两阶段流程：
    /// <para>
    /// 1. Context 先调用 <c>Configure()</c>，此时所有 Model 和 Service 通过
    /// <c>RegisterModel</c> / <c>RegisterService</c> 注册到容器中，但尚未初始化。
    /// </para>
    /// <para>
    /// 2. 注册完成后，按注册顺序依次调用各模块的 <see cref="Initialize()" />，
    /// 先初始化全部 Model，再初始化全部 Service。
    /// </para>
    /// <para>
    /// 这种两阶段设计确保了模块在被初始化时，其所依赖的其他模块已经全部注册完毕，
    /// 从而可以在 <see cref="Initialize()" /> 中安全地获取对端模块的引用。
    /// </para>
    /// </remarks>
    public interface ICanInitialize : IDisposable
    {
        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <remarks>
        /// 在此方法中可以安全地访问 Context 中已注册的其他模块。
        /// 注意：初始化顺序由 Context 控制，模块之间不应在初始化阶段产生循环依赖。
        /// </remarks>
        void Initialize();
    }

    /// <summary>
    /// 获取 Model 的能力接口
    /// </summary>
    /// <remarks>
    /// 这是一个标记接口（marker interface），本身不包含任何方法。
    /// 实际的 <c>GetModel</c> 功能通过 <see cref="CapabilityExtensions.GetModel{T}(ICanGetModel)" /> 扩展方法实现，
    /// 由 <c>this</c> 上的 <see cref="IContextHolder.Context" /> 属性提供底层支持。
    /// <para>
    /// 通过标记接口 + 扩展方法的组合，编译器可以确保只有声明了此能力的类型才能调用 <c>GetModel</c>，
    /// 从而在编译期实现细粒度的访问控制。
    /// </para>
    /// </remarks>
    /// <seealso cref="CapabilityExtensions.GetModel{T}(ICanGetModel)" />
    public interface ICanGetModel : IContextHolder { }

    /// <summary>
    /// 获取 Service 的能力接口
    /// </summary>
    /// <remarks>
    /// 这是一个标记接口（marker interface），本身不包含任何方法。
    /// 实际的 <c>GetService</c> 功能通过 <see cref="CapabilityExtensions.GetService{T}(ICanGetService)" /> 扩展方法实现，
    /// 由 <c>this</c> 上的 <see cref="IContextHolder.Context" /> 属性提供底层支持。
    /// <para>
    /// 通过标记接口 + 扩展方法的组合，编译器可以确保只有声明了此能力的类型才能调用 <c>GetService</c>，
    /// 从而在编译期实现细粒度的访问控制。
    /// </para>
    /// </remarks>
    /// <seealso cref="CapabilityExtensions.GetService{T}(ICanGetService)" />
    public interface ICanGetService : IContextHolder { }

    /// <summary>
    /// 执行命令的能力接口
    /// </summary>
    /// <remarks>
    /// 这是一个标记接口（marker interface），本身不包含任何方法。
    /// 实际的 <c>ExecuteCommand</c> 功能通过 <see cref="CapabilityExtensions.ExecuteCommand{T}(ICanExecuteCommand, T)" /> 扩展方法实现。
    /// <para>
    /// 通过标记接口 + 扩展方法的组合，编译器可以确保只有声明了此能力的类型才能调用 <c>ExecuteCommand</c>，
    /// 从而在编译期实现细粒度的访问控制。
    /// </para>
    /// </remarks>
    /// <seealso cref="CapabilityExtensions.ExecuteCommand{T}(ICanExecuteCommand, T)" />
    public interface ICanExecuteCommand : IContextHolder { }

    /// <summary>
    /// 执行查询的能力接口
    /// </summary>
    /// <remarks>
    /// 这是一个标记接口（marker interface），本身不包含任何方法。
    /// 实际的 <c>ExecuteQuery</c> 功能通过 <see cref="CapabilityExtensions.ExecuteQuery{TResult}(ICanExecuteQuery, IQuery{TResult})" /> 扩展方法实现。
    /// <para>
    /// 通过标记接口 + 扩展方法的组合，编译器可以确保只有声明了此能力的类型才能调用 <c>ExecuteQuery</c>，
    /// 从而在编译期实现细粒度的访问控制。
    /// </para>
    /// </remarks>
    /// <seealso cref="CapabilityExtensions.ExecuteQuery{TResult}(ICanExecuteQuery, IQuery{TResult})" />
    public interface ICanExecuteQuery : IContextHolder { }
}
