using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 上下文持有者接口。
    /// </summary>
    public interface IContextHolder
    {
        /// <summary>
        /// 获取持有的模块上下文
        /// </summary>
        IContext Context { get; }
    }

    /// <summary>
    /// 可设置上下文引用接口
    /// </summary>
    public interface ICanSetContext
    {
        /// <summary>
        /// 设置上下文引用
        /// </summary>
        void SetContext(IContext context);
    }

    /// <summary>
    /// 可初始化接口。提供初始化与初始化状态标记。
    /// <para>
    /// 被 <see cref="IModel" /> 和 <see cref="IService" /> 继承。
    /// </para>
    /// </summary>
    public interface ICanInitialize : IDisposable
    {
        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// 获取 Model 的能力接口
    /// </summary>
    public interface ICanGetModel : IContextHolder { }

    /// <summary>
    /// 获取 Service 的能力接口
    /// </summary>
    public interface ICanGetService : IContextHolder { }

    /// <summary>
    /// 执行命令的能力接口
    /// </summary>
    public interface ICanExecuteCommand : IContextHolder { }

    /// <summary>
    /// 执行查询的能力接口
    /// </summary>
    public interface ICanExecuteQuery : IContextHolder { }
}
