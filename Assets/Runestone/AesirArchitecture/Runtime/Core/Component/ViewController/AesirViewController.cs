namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// View + Controller 双角色基类。通过泛型上下文获取模块访问能力，自动支持 Odin Inspector 序列化。
    /// </summary>
    /// <typeparam name="T">上下文类型，必须继承自 <see cref="AbstractContext{T}"/> 且具有无参构造函数</typeparam>
    /// <remarks>
    /// 同时实现 <see cref="IView"/> 和 <see cref="IController"/>，具备只读数据访问 + 命令执行 + 查询能力。
    /// 通过显式接口实现 <see cref="IContextHolder.Context"/> 自动绑定到 <see cref="AbstractContext{T}.Interface"/> 单例。
    /// <para>
    /// 继承自 <see cref="AesirMonoBehaviour"/>，在编辑器环境或配置允许时自动获得 Odin 序列化能力。
    /// </para>
    /// </remarks>
    /// <seealso cref="MonoViewController{T}"/>
    /// <seealso cref="IView"/>
    /// <seealso cref="IController"/>
    public abstract class AesirViewController<T> : AesirMonoBehaviour, IView, IController
        where T : AbstractContext<T>, new()
    {
        IContext IContextHolder.Context => AbstractContext<T>.Interface;
    }
}
