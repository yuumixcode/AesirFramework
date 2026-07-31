using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 模块上下文接口。提供模块注册与获取。
    /// </summary>
    public interface IContext : IDisposable
    {
        /// <summary>
        /// 上下文是否已初始化
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// 注册 Model
        /// </summary>
        void RegisterModel<T>(T model) where T : class, IModel;

        /// <summary>
        /// 注册 Service
        /// </summary>
        void RegisterService<T>(T service) where T : class, IService;

        /// <summary>
        /// 获取已注册的 Model
        /// </summary>
        T GetModel<T>() where T : class, IModel;

        /// <summary>
        /// 获取已注册的 Service
        /// </summary>
        T GetService<T>() where T : class, IService;

        /// <summary>
        /// 获取所有已注册的 Model 列表
        /// </summary>
        IEnumerable<IModel> GetAllModels();

        /// <summary>
        /// 获取所有已注册的 Service 列表
        /// </summary>
        IEnumerable<IService> GetAllServices();
    }
}
