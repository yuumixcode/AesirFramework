namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// View 基类。通过泛型上下文获取模块访问能力，仅具备只读权限，AesirView 自动支持 Odin Inspector 序列化。
    /// </summary>
    /// <typeparam name="T">上下文类型，必须继承自 <see cref="AbstractContext{T}" /> 且具有无参构造函数</typeparam>
    /// <remarks>
    /// 上下文绑定经 <see cref="IView{T}" /> 的默认接口实现（DIM）自动指向 <see cref="AbstractContext{T}.Instance" /> 单例，
    /// 使 View 无需手动引用即可访问上下文中的只读数据。
    /// <para>
    /// 继承自 <see cref="AesirMonoBehaviour" />，在编辑器环境或配置允许时自动获得 Odin 序列化能力。
    /// </para>
    /// </remarks>
    /// <seealso cref="MonoView{T}" />
    /// <seealso cref="IView{T}" />
    public abstract class AesirView<T> : AesirMonoBehaviour, IView<T> where T : AbstractContext<T>, new()
    {
    }
}
