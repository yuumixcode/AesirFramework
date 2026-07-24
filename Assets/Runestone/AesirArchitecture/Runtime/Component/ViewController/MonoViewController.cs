using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// View + Controller 双角色基类。通过泛型上下文获取模块访问能力，无 Odin 依赖。
    /// </summary>
    public abstract class MonoViewController<T> : MonoBehaviour, IView, IController
        where T : AbstractContext<T>, new()
    {
        IContext IContextHolder.Context => AbstractContext<T>.Interface;
    }
}
