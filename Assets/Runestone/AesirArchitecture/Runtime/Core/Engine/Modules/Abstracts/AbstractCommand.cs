using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 命令基类。持有上下文引用，通过 <see cref="OnExecute" /> 执行命令逻辑。
    /// </summary>
    /// <remarks>
    /// Command 是只写操作，用于修改 Model 状态，无返回值。
    /// 通过显式接口实现 <see cref="ICanSetContext.SetContext" /> 接收上下文注入，
    /// 使命令在执行时具备 <c>GetModel&lt;T&gt;</c> / <c>GetService&lt;T&gt;</c> 能力。
    /// 子类应实现 <see cref="OnExecute" /> 编写命令逻辑，不应直接实现
    /// <see cref="ICommand.Execute" />——后者已由本基类委托至 <see cref="OnExecute" />。
    /// 标记 <see cref="SerializableAttribute" /> 以支持序列化场景。
    /// </remarks>
    [Serializable]
    public abstract class AbstractCommand : ICommand
    {
        IContext _context;
        IContext IContextHolder.Context => _context;
        void ICanSetContext.SetContext(IContext context) => _context = context;
        void ICommand.Execute() => OnExecute();

        /// <summary>
        /// 命令执行逻辑，子类必须实现
        /// </summary>
        /// <remarks>
        /// 子类在此实现命令逻辑，通过 <c>this.GetModel&lt;T&gt;()</c> / <c>this.GetService&lt;T&gt;()</c>
        /// 访问模块上下文中的 Model 和 Service，完成状态修改。
        /// </remarks>
        protected abstract void OnExecute();
    }
}
