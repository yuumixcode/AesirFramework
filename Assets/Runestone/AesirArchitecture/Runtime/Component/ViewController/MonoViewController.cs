using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// View + Controller 双角色基类。通过泛型上下文获取模块访问能力，无 Odin 依赖。
    /// </summary>
    /// <typeparam name="T">上下文类型，必须继承自 <see cref="AbstractContext{T}"/> 且具有无参构造函数</typeparam>
    /// <remarks>
    /// 与 <see cref="AesirViewController{T}"/> 功能相同，但直接继承 <c>MonoBehaviour</c> 而非 <see cref="AesirMonoBehaviour"/>，
    /// 不依赖 Odin 序列化。适用于不需要 Odin Inspector 特性的项目或需要最小依赖的场景。
    /// </remarks>
    /// <seealso cref="AesirViewController{T}"/>
    /// <seealso cref="IView"/>
    /// <seealso cref="IController"/>
    public abstract class MonoViewController<T> : MonoBehaviour, IView, IController
        where T : AbstractContext<T>, new()
    {
        IContext IContextHolder.Context => AbstractContext<T>.Interface;
    }
}
