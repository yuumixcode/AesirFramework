using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 上下文基类。纯 C# 实现，不依赖 MonoBehaviour。
    /// <para>子类在 <see cref="Configure" /> 中注册 Model 和 Service，通过 <see cref="Interface" /> 获取全局单例。</para>
    /// </summary>
    [Serializable]
    public abstract class AbstractContext<T> : IContext where T : AbstractContext<T>, new()
    {
        static T _instance;
        GenericLocator<IModel> _modelLocator = new GenericLocator<IModel>();
        GenericLocator<IService> _serviceLocator = new GenericLocator<IService>();

        static AbstractContext()
        {
            ResetStaticsAssistant.Register(() => _instance = null);
        }

        /// <summary>
        /// 获取当前上下文类型的单例接口实例。首次访问时自动创建并初始化。
        /// </summary>
        public static IContext Interface
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = new T();
                _instance.Initialize();
                return _instance;
            }
        }

        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// 注册 Model 并绑定上下文。
        /// <para>若该类型已注册，旧实例会被 <see cref="Dispose" /> 后再覆盖，避免事件订阅等资源泄漏。</para>
        /// </summary>
        public void RegisterModel<TModel>(TModel model) where TModel : class, IModel
        {
            if (_modelLocator.TryGet<TModel>(out var existing))
            {
                existing.Dispose();
            }

            model.SetContext(this);
            _modelLocator.Register(model);

            if (!Initialized)
            {
                return;
            }

            model.Initialize();
        }

        /// <summary>
        /// 注册 Service 并绑定上下文。
        /// <para>若上下文已完成统一初始化，则立即初始化该 Service。若该类型已注册，旧实例会被 <see cref="Dispose" /> 后再覆盖，避免资源泄漏。</para>
        /// </summary>
        public void RegisterService<TService>(TService service) where TService : class, IService
        {
            if (_serviceLocator.TryGet<TService>(out var existing))
            {
                existing.Dispose();
            }

            service.SetContext(this);
            _serviceLocator.Register(service);

            if (!Initialized)
            {
                return;
            }

            service.Initialize();
        }

        /// <summary>
        /// 获取已注册的 Model。
        /// </summary>
        public TModel GetModel<TModel>() where TModel : class, IModel => _modelLocator.Get<TModel>();

        /// <summary>
        /// 获取已注册的 Service。
        /// </summary>
        public TService GetService<TService>() where TService : class, IService =>
            _serviceLocator.Get<TService>();

        /// <summary>
        /// 注册事件监听，返回可自动移除的监听句柄
        /// </summary>
        public AutoRemoveListenerHandle AddListener<TEvent>(Action<TEvent> listener)
            where TEvent : IEventArgs => MiniEventBus<TEvent>.AddListener(listener);

        /// <summary>
        /// 注册无参事件监听，返回可自动移除的监听句柄
        /// </summary>
        public AutoRemoveListenerHandle AddListener<TEvent>(Action listener) where TEvent : IEventArgs =>
            MiniEventBus<TEvent>.AddListener(listener);

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public void RemoveListener<TEvent>(Action<TEvent> listener) where TEvent : IEventArgs =>
            MiniEventBus<TEvent>.RemoveListener(listener);

        /// <summary>
        /// 移除无参事件监听
        /// </summary>
        public void RemoveListener<TEvent>(Action listener) where TEvent : IEventArgs =>
            MiniEventBus<TEvent>.RemoveListener(listener);

        /// <summary>
        /// 发布事件
        /// </summary>
        public void InvokeEvent<TEvent>(TEvent args) where TEvent : IEventArgs =>
            MiniEventBus<TEvent>.Invoke(args);

        /// <summary>
        /// 发布无参事件
        /// </summary>
        public void InvokeEvent<TEvent>() where TEvent : IEventArgs, new() =>
            MiniEventBus<TEvent>.Invoke(new TEvent());

        /// <summary>
        /// 释放资源。逆序销毁 Service 和 Model，清空容器。
        /// </summary>
        public virtual void Dispose()
        {
            if (!Initialized)
            {
                return;
            }

            OnDispose();

            foreach (var service in _serviceLocator.GetAll())
            {
                service.Dispose();
            }

            foreach (var model in _modelLocator.GetAll())
            {
                model.Dispose();
            }

            _serviceLocator.Clear();
            _modelLocator.Clear();

            Initialized = false;
        }

        /// <summary>
        /// 获取所有已注册的 Model 列表
        /// </summary>
        public IEnumerable<IModel> GetAllModels() => _modelLocator.GetAll();

        /// <summary>
        /// 获取所有已注册的 Service 列表
        /// </summary>
        public IEnumerable<IService> GetAllServices() => _serviceLocator.GetAll();

        /// <summary>
        /// 统一初始化。调用 <see cref="Configure" /> 注册模块后，按注册顺序依次初始化 Model 和 Service。
        /// <para>开发者需保证注册顺序满足依赖关系——被依赖的模块先注册。运行时通过 <c>GetModel</c> / <c>GetService</c> 获取未注册模块会抛出异常。</para>
        /// </summary>
        public void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            Configure();

            foreach (var model in _modelLocator.GetAll())
            {
                model.Initialize();
            }

            foreach (var service in _serviceLocator.GetAll())
            {
                service.Initialize();
            }

            Initialized = true;
        }

        /// <summary>
        /// 配置上下文模块，子类在此注册 Model 和 Service。
        /// </summary>
        protected abstract void Configure();

        /// <summary>
        /// 子类可选覆写，在释放前执行自定义清理
        /// </summary>
        protected virtual void OnDispose() { }
    }
}
