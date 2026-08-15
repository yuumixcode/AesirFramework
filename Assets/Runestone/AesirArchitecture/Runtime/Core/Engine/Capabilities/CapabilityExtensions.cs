using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 能力扩展方法集合
    /// </summary>
    /// <remarks>
    /// 能力接口组合模式是本框架角色系统的核心设计。
    /// <para>
    /// 角色类型（如 Model、Service、Command、Query 等）通过组合不同的细粒度能力接口
    /// （<see cref="ICanGetModel" />、<see cref="ICanGetService" />、<see cref="ICanExecuteCommand" />、
    /// <see cref="ICanExecuteQuery" />）来声明自己可以执行的操作，而非通过继承庞大的基类获得全部权限。
    /// </para>
    /// <para>
    /// 本类中的扩展方法通过接口约束（<c>where T : ICanXxx</c>）确保类型安全——
    /// 只有显式声明了对应能力接口的类型才能调用相应的扩展方法，将访问控制前置到编译期。
    /// </para>
    /// </remarks>
    public static class CapabilityExtensions
    {
        /// <summary>
        /// 获取已注册的 Model。未注册时由 <see cref="IContext.GetModel{T}"/> 抛出异常；
        /// 已注册但尚未初始化时，抛出注册顺序错误或循环依赖异常。
        /// </summary>
        /// <typeparam name="T">要获取的 Model 类型，必须实现 <see cref="IModel" /></typeparam>
        /// <param name="self">调用方实例，必须已持有有效的上下文引用</param>
        /// <returns>已注册且已初始化完成的 Model 实例</returns>
        /// <exception cref="InvalidOperationException">
        /// 目标 Model 已注册但尚未初始化时抛出——通常表示注册顺序错误或存在循环依赖，被依赖的 Model 应先注册。
        /// </exception>
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel
        {
            var model = self.Context.GetModel<T>();

            if (!model.Initialized)
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureDebug.ErrorTag} [{self.GetType().Name}] 尝试获取 Model [{typeof(T).Name}]，" +
                    "但该 Model 尚未初始化。这通常表示注册顺序错误或存在循环依赖——" +
                    $"被依赖的 Model 应先注册。请检查 Configure() 中 RegisterModel<{typeof(T).Name}>() 的调用顺序。");
            }

            return model;
        }

        /// <summary>
        /// 获取已注册的 Service。未注册时由 <see cref="IContext.GetService{T}"/> 抛出异常；
        /// 已注册但尚未初始化时，抛出注册顺序错误或循环依赖异常。
        /// </summary>
        /// <typeparam name="T">要获取的 Service 类型，必须实现 <see cref="IService" /></typeparam>
        /// <param name="self">调用方实例，必须已持有有效的上下文引用</param>
        /// <returns>已注册且已初始化完成的 Service 实例</returns>
        /// <exception cref="InvalidOperationException">
        /// 目标 Service 已注册但尚未初始化时抛出——通常表示注册顺序错误或存在循环依赖，被依赖的 Service 应先注册。
        /// </exception>
        public static T GetService<T>(this ICanGetService self) where T : class, IService
        {
            var service = self.Context.GetService<T>();

            if (!service.Initialized)
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureDebug.ErrorTag} [{self.GetType().Name}] 尝试获取 Service [{typeof(T).Name}]，" +
                    "但该 Service 尚未初始化。这通常表示注册顺序错误或存在循环依赖——" +
                    $"被依赖的 Service 应先注册。请检查 Configure() 中 RegisterService<{typeof(T).Name}>() 的调用顺序。");
            }

            return service;
        }

        /// <summary>
        /// 执行带参命令
        /// </summary>
        /// <typeparam name="T">命令类型，必须实现 <see cref="ICommand" /></typeparam>
        /// <param name="self">调用方实例，必须已持有有效的上下文引用</param>
        /// <param name="command">要执行的命令实例</param>
        /// <remarks>
        /// 命令在执行前会通过 <see cref="ICanSetContext.SetContext" /> 注入当前上下文引用，
        /// 使其具备 <c>GetModel</c> / <c>GetService</c> 等能力，
        /// 从而可以在 <c>Execute</c> 方法内部访问已注册的模块。
        /// </remarks>
        public static void ExecuteCommand<T>(this ICanExecuteCommand self, T command) where T : ICommand
        {
            command.SetContext(self.Context);
            command.Execute();
        }

        /// <summary>
        /// 执行无参命令
        /// </summary>
        /// <typeparam name="T">命令类型，必须实现 <see cref="ICommand" /> 并具有无参公共构造函数（<c>new()</c>），
        /// 因为框架需要通过无参构造创建命令实例</typeparam>
        /// <param name="self">调用方实例，必须已持有有效的上下文引用</param>
        /// <remarks>
        /// 命令在执行前会通过 <see cref="ICanSetContext.SetContext" /> 注入当前上下文引用，
        /// 使其具备 <c>GetModel</c> / <c>GetService</c> 等能力，
        /// 从而可以在 <c>Execute</c> 方法内部访问已注册的模块。
        /// </remarks>
        public static void ExecuteCommand<T>(this ICanExecuteCommand self) where T : ICommand, new()
        {
            var command = new T();
            command.SetContext(self.Context);
            command.Execute();
        }

        /// <summary>
        /// 执行带参查询
        /// </summary>
        /// <typeparam name="TResult">查询返回值类型</typeparam>
        /// <param name="self">调用方实例，必须已持有有效的上下文引用</param>
        /// <param name="query">要执行的查询实例</param>
        /// <returns>查询执行结果</returns>
        /// <remarks>
        /// 查询在执行前会通过 <see cref="ICanSetContext.SetContext" /> 注入当前上下文引用，
        /// 使其具备 <c>GetModel</c> / <c>GetService</c> 等能力，
        /// 从而可以在 <c>Execute</c> 方法内部访问已注册的模块。
        /// </remarks>
        public static TResult ExecuteQuery<TResult>(this ICanExecuteQuery self, IQuery<TResult> query)
        {
            query.SetContext(self.Context);
            return query.Execute();
        }

        /// <summary>
        /// 执行无参查询
        /// </summary>
        /// <typeparam name="TQuery">查询类型，必须实现 <see cref="IQuery{TResult}" /> 并具有无参公共构造函数（<c>new()</c>），
        /// 因为框架需要通过无参构造创建查询实例</typeparam>
        /// <typeparam name="TResult">查询返回值类型</typeparam>
        /// <param name="self">调用方实例，必须已持有有效的上下文引用</param>
        /// <returns>查询执行结果</returns>
        /// <remarks>
        /// 查询在执行前会通过 <see cref="ICanSetContext.SetContext" /> 注入当前上下文引用，
        /// 使其具备 <c>GetModel</c> / <c>GetService</c> 等能力，
        /// 从而可以在 <c>Execute</c> 方法内部访问已注册的模块。
        /// </remarks>
        public static TResult ExecuteQuery<TQuery, TResult>(this ICanExecuteQuery self)
            where TQuery : IQuery<TResult>, new()
        {
            var query = new TQuery();
            query.SetContext(self.Context);
            return query.Execute();
        }
    }
}
