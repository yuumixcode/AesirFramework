namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// View + Controller 双角色基类。通过泛型上下文获取模块访问能力，自动支持 Odin Inspector 序列化。
    /// </summary>
    /// <typeparam name="T">上下文类型，必须继承自 <see cref="AbstractContext{T}" /> 且具有无参构造函数</typeparam>
    /// <remarks>
    /// 同时实现 <see cref="IView" /> 和 <see cref="IController" />，具备只读数据访问 + 命令执行 + 查询能力。
    /// 上下文绑定经 <see cref="IController{T}" /> 的默认接口实现（DIM）自动指向 <see cref="AbstractContext{T}.Instance" /> 单例——
    /// View 角色不存在泛型 DIM 接口，由 IController&lt;T&gt; 单独供给，避免双 DIM 冲突。
    /// <para>
    /// 继承自 <see cref="AesirMonoBehaviour" />，在编辑器环境或配置允许时自动获得 Odin 序列化能力。
    /// </para>
    /// </remarks>
    /// <seealso cref="MonoViewController{T}" />
    /// <seealso cref="IView" />
    /// <seealso cref="IController{T}" />
    public abstract class AesirViewController<T> : AesirMonoBehaviour, IView, IController<T>
        where T : AbstractContext<T>, new()
    {
    }
}
