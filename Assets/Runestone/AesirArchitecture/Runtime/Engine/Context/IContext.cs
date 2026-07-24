using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 模块上下文接口。提供模块注册、获取与事件操作。
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

        /// <summary>
        /// 注册事件监听，返回可自动移除的监听句柄
        /// </summary>
        AutoRemoveListenerHandle AddListener<T>(Action<T> listener) where T : IEventArgs;

        /// <summary>
        /// 注册无参事件监听，返回可自动移除的监听句柄
        /// </summary>
        AutoRemoveListenerHandle AddListener<T>(Action listener) where T : IEventArgs;

        /// <summary>
        /// 移除事件监听
        /// </summary>
        void RemoveListener<T>(Action<T> listener) where T : IEventArgs;

        /// <summary>
        /// 移除无参事件监听
        /// </summary>
        void RemoveListener<T>(Action listener) where T : IEventArgs;

        /// <summary>
        /// 发布事件
        /// </summary>
        void InvokeEvent<T>(T args) where T : IEventArgs;

        /// <summary>
        /// 发布无参事件
        /// </summary>
        void InvokeEvent<T>() where T : IEventArgs, new();
    }
}
