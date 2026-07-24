using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 子模块基类。持有上下文引用，通过 <see cref="OnInitialize" /> 和 <see cref="OnDispose" /> 管理生命周期。
    /// <para>Model 和 Service 的公共逻辑统一在此实现。</para>
    /// </summary>
    [Serializable]
    public abstract class AbstractSubmodule : IContextHolder, ICanSetContext, ICanInitialize
    {
        IContext _context;

        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        public bool Initialized { get; private set; }

        void ICanInitialize.Initialize()
        {
            OnInitialize();
            Initialized = true;
        }

        /// <summary>
        /// 释放资源，触发 <see cref="OnDispose" />
        /// </summary>
        public void Dispose()
        {
            OnDispose();
            _context = null;
        }

        void ICanSetContext.SetContext(IContext context) => _context = context;

        IContext IContextHolder.Context => _context;

        /// <summary>
        /// 初始化逻辑，子类必须实现
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 释放时的清理逻辑，子类可覆写
        /// </summary>
        protected virtual void OnDispose() { }
    }
}
