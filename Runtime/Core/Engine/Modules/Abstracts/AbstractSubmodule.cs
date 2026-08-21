using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 子模块基类。持有上下文引用，通过 <see cref="OnInitialize" /> 和 <see cref="OnDispose" /> 管理生命周期。
    /// <para>Model 和 Service 的公共逻辑统一在此实现。</para>
    /// </summary>
    /// <remarks>
    /// 作为 <see cref="AbstractModel" /> 和 <see cref="AbstractService" /> 的公共基类，
    /// 统一管理上下文引用和生命周期。上下文通过显式接口实现 <see cref="ICanSetContext.SetContext" /> 注入，
    /// 供子类经由能力扩展方法（如 <c>GetModel&lt;T&gt;</c>、<c>GetService&lt;T&gt;</c>）访问其他模块。
    /// 生命周期由 <see cref="OnInitialize" /> 和 <see cref="OnDispose" /> 两个虚方法控制，
    /// 子类按需覆写即可在初始化和释放阶段执行自定义逻辑。
    /// </remarks>
    [Serializable]
    public abstract class AbstractSubmodule : IContextHolder, ICanSetContext, ICanInitialize
    {
        IContext _context;

        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        /// <remarks>
        /// 由 <see cref="ICanInitialize.Initialize" /> 在调用 <see cref="OnInitialize" /> 之后设为 <c>true</c>。
        /// 一旦初始化完成便不可重置，用于在运行时判断子模块是否已就绪。
        /// </remarks>
        public bool Initialized { get; private set; }

        void ICanInitialize.Initialize()
        {
            OnInitialize();
            Initialized = true;
        }

        /// <summary>
        /// 释放资源，触发 <see cref="OnDispose" />
        /// </summary>
        /// <remarks>
        /// 先调用 <see cref="OnDispose" /> 执行子类清理逻辑，随后将上下文引用置为 <c>null</c>，
        /// 断开与模块体系的连接以避免后续误用已释放的上下文。
        /// </remarks>
        public void Dispose()
        {
            OnDispose();
            _context = null;
        }

        void ICanSetContext.SetContext(IContext context) => _context = context;

        IContext IContextHolder.Context => _context;

        /// <summary>
        /// 初始化逻辑，子类可选覆写（默认空实现）。
        /// </summary>
        /// <remarks>
        /// 子类按需覆写此方法以执行初始化逻辑，例如创建 <see cref="ObservableValue{T}" />、
        /// 订阅事件或加载持久化数据；无初始化需求的子类可不覆写（默认空实现）。
        /// 此方法在 <see cref="ICanInitialize.Initialize" /> 中被调用，
        /// 调用完成后 <see cref="Initialized" /> 自动置为 <c>true</c>。
        /// <para>
        /// 区别于 <c>AbstractCommand.OnExecute</c> / <c>AbstractQuery.OnExecute</c>（abstract，子类必须实现）——
        /// 本方法为 virtual 空实现，覆写是可选的。
        /// </para>
        /// </remarks>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 释放时的清理逻辑，子类可覆写
        /// </summary>
        /// <remarks>
        /// 默认空实现。子类覆写此方法以执行清理逻辑，例如取消订阅、释放资源或保存状态。
        /// 此方法在 <see cref="Dispose" /> 中被调用，执行完毕后上下文引用将被清空。
        /// </remarks>
        protected virtual void OnDispose() { }
    }
}
