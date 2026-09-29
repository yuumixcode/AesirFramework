using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// View 基类。通过泛型上下文获取模块访问能力，仅具备只读权限。
    /// </summary>
    /// <typeparam name="T">上下文类型，必须继承自 <see cref="AbstractContext{T}" /> 且具有无参构造函数</typeparam>
    /// <remarks>
    /// 上下文绑定经 <see cref="IView{T}" /> 的默认接口实现（DIM）自动指向 <see cref="AbstractContext{T}.Instance" /> 单例，
    /// 与 <see cref="AesirView{T}" /> 功能相同，但直接继承 <c>MonoBehaviour</c> 而非 <see cref="AesirMonoBehaviour" />，
    /// 不依赖 Odin 序列化。适用于不需要 Odin Inspector 特性的项目或需要最小依赖的场景。
    /// </remarks>
    /// <seealso cref="AesirView{T}" />
    /// <seealso cref="IView{T}" />
    public abstract class MonoView<T> : MonoBehaviour, IView<T> where T : AbstractContext<T>, new()
    {
    }
}
