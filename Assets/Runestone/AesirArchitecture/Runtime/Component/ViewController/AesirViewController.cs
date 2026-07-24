namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// View + Controller 双角色基类。通过泛型上下文获取模块访问能力，自动支持 Odin Inspector 序列化。
    /// </summary>
    public abstract class AesirViewController<T> : AesirMonoBehaviour, IView, IController
        where T : AbstractContext<T>, new()
    {
        IContext IContextHolder.Context => AbstractContext<T>.Interface;
    }
}
