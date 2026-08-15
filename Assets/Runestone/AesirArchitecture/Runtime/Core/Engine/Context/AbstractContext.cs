using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 上下文基类。纯 C# 实现，不依赖 MonoBehaviour。
    /// <para>子类在 <see cref="Configure" /> 中注册 Model 和 Service，通过 <see cref="Interface" /> 获取全局单例。</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// 本类采用泛型自引用模式（CRTP）：泛型约束 <c>where T : AbstractContext&lt;T&gt;, new()</c>
    /// 要求子类将自身作为类型参数传入，例如 <c>class MyContext : AbstractContext&lt;MyContext&gt;</c>。
    /// 这样 <see cref="Interface" /> 静态属性就能在编译期确定具体类型并返回其单例，
    /// 避免了反射或运行时类型查找的开销。
    /// </para>
    /// <para>
    /// <b>初始化流程</b>（由 <see cref="Interface" /> 首次访问触发）：
    /// <list type="number">
    /// <item>创建子类实例 <c>new T()</c></item>
    /// <item>调用 <see cref="Initialize" /></item>
    /// <item><see cref="Initialize" /> 先调用 <see cref="Configure" />，由子类注册全部 Model 和 Service</item>
    /// <item>按注册顺序依次调用各 Model 的 <c>Initialize</c></item>
    /// <item>按注册顺序依次调用各 Service 的 <c>Initialize</c></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>释放流程</b>（由 <see cref="Dispose" /> 触发）：
    /// <list type="number">
    /// <item>逆序于初始化地先销毁所有 Service</item>
    /// <item>再逆序于初始化地销毁所有 Model</item>
    /// <item>清空 Model 与 Service 容器</item>
    /// </list>
    /// 先 Service 后 Model 的销毁顺序确保 Service 在销毁时仍可访问所依赖的 Model。
    /// </para>
    /// <para>
    /// <b>域加载安全</b>：静态构造函数通过 <see cref="ResetStaticsAssistant.Register(Action)" />
    /// 注册 <c>_instance = null</c> 重置回调。当 Unity 关闭 Domain Reload（Enter Play Mode Settings）
    /// 时，静态字段不会被运行时自动清零，该回调确保下次进入 Play 模式时单例被正确重建。
    /// 之所以经助手注册而非类内声明 <c>[RuntimeInitializeOnLoadMethod]</c>：泛型类中的该方法特性
    /// 会被 Unity 静默跳过（不执行也不报错），只能由非泛型的中心位置代为触发。
    /// </para>
    /// </remarks>
    /// <typeparam name="T">具体上下文子类类型，必须具有无参公共构造函数</typeparam>
    /// <seealso cref="IContext"/>
    [Serializable]
    public abstract class AbstractContext<T> : IContext where T : AbstractContext<T>, new()
    {
        static T _instance;
        GenericLocator<IModel> _modelLocator = new GenericLocator<IModel>();
        GenericLocator<IService> _serviceLocator = new GenericLocator<IService>();

        static AbstractContext()
        {
            ResetStaticsAssistant.Register(() =>
            {
                _instance?.Dispose();
                _instance = null;
            });
        }

        /// <summary>
        /// 获取当前上下文类型的单例接口实例。首次访问时自动创建并初始化。
        /// </summary>
        /// <remarks>
        /// 采用懒加载单例模式：首次访问时通过 <c>new T()</c> 创建实例并调用 <see cref="Initialize" />，
        /// 初始化成功后才写入静态字段 <c>_instance</c>。后续访问直接返回缓存实例，不再重复初始化。
        /// <para>
        /// <b>初始化失败</b>：单例不会被缓存，后续每次访问都会重新创建并重新初始化，
        /// 根因异常每次抛出（而非只抛一次后拿到 <see cref="Initialized" /> 为 <c>false</c> 的坏上下文）。
        /// 已初始化到一半的模块不做回滚 Dispose，随被丢弃的实例交由 GC 回收——初始化失败属启动期编程错误，
        /// 应修复根因而非优雅降级。
        /// </para>
        /// <para>
        /// <b>重入约定</b>：<see cref="Configure" /> 及各模块的初始化方法中禁止访问 <c>Interface</c>，
        /// 否则会因单例尚未发布而递归创建第二个上下文实例。
        /// </para>
        /// <para>该属性返回 <see cref="IContext" /> 接口类型，调用方可通过 <c>((T)Interface)</c> 访问子类特有成员。</para>
        /// </remarks>
        public static IContext Interface
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var context = new T();
                context.Initialize();
                _instance = context;
                return context;
            }
        }

        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// 注册 Model 并绑定上下文。
        /// <para>若该类型已注册，旧实例会被 <see cref="Dispose" /> 后再覆盖。</para>
        /// </summary>
        /// <typeparam name="TModel">要注册的 Model 接口类型，必须为引用类型并实现 <see cref="IModel" /></typeparam>
        /// <param name="model">要注册的 Model 实例，注册后会绑定到当前上下文</param>
        /// <remarks>
        /// 运行时替换 Model 属测试/调试用途，替换后旧实例上的订阅不会迁移——已订阅的 View 需自行重新订阅。
        /// </remarks>
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
        /// <para>若上下文已完成统一初始化，则立即初始化该 Service。若该类型已注册，旧实例会被 <see cref="Dispose" /> 后再覆盖。</para>
        /// </summary>
        /// <typeparam name="TService">要注册的 Service 接口类型，必须为引用类型并实现 <see cref="IService" /></typeparam>
        /// <param name="service">要注册的 Service 实例，注册后会绑定到当前上下文</param>
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
        /// <typeparam name="TModel">要获取的 Model 类型，必须为引用类型并实现 <see cref="IModel" /></typeparam>
        /// <returns>已注册的 Model 实例</returns>
        /// <exception cref="InvalidOperationException">目标 Model 未注册时抛出，与 <c>CapabilityExtensions.GetModel</c> 的防护语义一致</exception>
        public TModel GetModel<TModel>() where TModel : class, IModel
        {
            if (_modelLocator.TryGet<TModel>(out var model))
            {
                return model;
            }

            throw new InvalidOperationException(
                $"{AesirArchitectureDebug.ErrorTag} [Context] 尝试获取 Model [{typeof(TModel).Name}]，" +
                $"但该 Model 未在 Context 中注册，需要提前调用 RegisterModel<{typeof(TModel).Name}>() 注册到 Context 中。");
        }

        /// <summary>
        /// 获取已注册的 Service。
        /// </summary>
        /// <typeparam name="TService">要获取的 Service 类型，必须为引用类型并实现 <see cref="IService" /></typeparam>
        /// <returns>已注册的 Service 实例</returns>
        /// <exception cref="InvalidOperationException">目标 Service 未注册时抛出，与 <c>CapabilityExtensions.GetService</c> 的防护语义一致</exception>
        public TService GetService<TService>() where TService : class, IService
        {
            if (_serviceLocator.TryGet<TService>(out var service))
            {
                return service;
            }

            throw new InvalidOperationException(
                $"{AesirArchitectureDebug.ErrorTag} [Context] 尝试获取 Service [{typeof(TService).Name}]，" +
                $"但该 Service 未在 Context 中注册，需要提前调用 RegisterService<{typeof(TService).Name}>() 注册到 Context 中。");
        }

        /// <summary>
        /// 释放资源。逆序销毁 Service 和 Model，清空容器。
        /// </summary>
        /// <remarks>
        /// 释放顺序与初始化顺序相反：先销毁所有 Service，再销毁所有 Model，最后清空两个容器。
        /// <para>
        /// 先 Service 后 Model 的原因：Service 通常依赖 Model 完成自身逻辑，
        /// 在 Service 释放时仍可能需要读取 Model 状态，因此 Model 必须晚于 Service 销毁。
        /// </para>
        /// <para>若上下文尚未初始化，此方法直接返回不做任何操作。</para>
        /// </remarks>
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
        /// <returns>所有已注册 Model 实例的集合；若无注册则返回空集合</returns>
        public IEnumerable<IModel> GetAllModels() => _modelLocator.GetAll();

        /// <summary>
        /// 获取所有已注册的 Service 列表
        /// </summary>
        /// <returns>所有已注册 Service 实例的集合；若无注册则返回空集合</returns>
        public IEnumerable<IService> GetAllServices() => _serviceLocator.GetAll();

        /// <summary>
        /// 统一初始化。调用 <see cref="Configure" /> 注册模块后，按注册顺序依次初始化 Model 和 Service。
        /// <para>开发者需保证注册顺序满足依赖关系——被依赖的模块先注册。运行时通过 <c>GetModel</c> / <c>GetService</c> 获取未注册模块会抛出异常。</para>
        /// </summary>
        /// <remarks>
        /// 此方法由 <see cref="Interface" /> 在首次访问时自动调用，通常不需要手动调用。
        /// <para>
        /// 执行步骤：
        /// <list type="number">
        /// <item>调用 <see cref="Configure" />，让子类在其中通过 <see cref="RegisterModel{TModel}" /> 和 <see cref="RegisterService{TService}" /> 注册所有模块</item>
        /// <item>按注册顺序遍历并调用各 Model 的 <c>Initialize</c></item>
        /// <item>按注册顺序遍历并调用各 Service 的 <c>Initialize</c></item>
        /// </list>
        /// </para>
        /// <para>若已初始化则直接返回，保证幂等性。初始化过程中抛出的异常直接向上传播，不做回滚——
        /// 初始化失败属启动期编程错误，应修复根因（见 <see cref="Interface" /> 备注）。</para>
        /// </remarks>
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
