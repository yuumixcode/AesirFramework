using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 模块上下文接口。提供模块注册与获取。
    /// </summary>
    /// <remarks>
    /// 此接口定义了上下文的模块注册与获取契约。
    /// <see cref="AbstractContext{T}" /> 是其默认实现，提供了懒加载单例、统一初始化和有序释放等完整功能。
    /// <para>实现类应在初始化阶段注册所有需要的 Model 和 Service，运行时通过 <see cref="GetModel{T}" /> / <see cref="GetService{T}" /> 获取模块实例。</para>
    /// </remarks>
    /// <seealso cref="AbstractContext{T}"/>
    public interface IContext : IDisposable
    {
        /// <summary>
        /// 上下文是否已初始化
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// 注册 Model
        /// </summary>
        /// <typeparam name="T">要注册的 Model 类型，必须为引用类型并实现 <see cref="IModel" /></typeparam>
        /// <param name="model">要注册的 Model 实例</param>
        void RegisterModel<T>(T model) where T : class, IModel;

        /// <summary>
        /// 注册 Service
        /// </summary>
        /// <typeparam name="T">要注册的 Service 类型，必须为引用类型并实现 <see cref="IService" /></typeparam>
        /// <param name="service">要注册的 Service 实例</param>
        void RegisterService<T>(T service) where T : class, IService;

        /// <summary>
        /// 获取已注册的 Model
        /// </summary>
        /// <typeparam name="T">要获取的 Model 类型，必须为引用类型并实现 <see cref="IModel" /></typeparam>
        /// <returns>已注册的 Model 实例；若未注册则返回 <c>null</c></returns>
        T GetModel<T>() where T : class, IModel;

        /// <summary>
        /// 获取已注册的 Service
        /// </summary>
        /// <typeparam name="T">要获取的 Service 类型，必须为引用类型并实现 <see cref="IService" /></typeparam>
        /// <returns>已注册的 Service 实例；若未注册则返回 <c>null</c></returns>
        T GetService<T>() where T : class, IService;

        /// <summary>
        /// 获取所有已注册的 Model 列表
        /// </summary>
        /// <returns>所有已注册 Model 实例的集合；若无注册则返回空集合</returns>
        IEnumerable<IModel> GetAllModels();

        /// <summary>
        /// 获取所有已注册的 Service 列表
        /// </summary>
        /// <returns>所有已注册 Service 实例的集合；若无注册则返回空集合</returns>
        IEnumerable<IService> GetAllServices();
    }
}
