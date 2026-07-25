using Runestone.AesirArchitecture;

namespace Runestone.AesirModules
{
    public abstract class AesirBasePanelView<T> : AesirBasePanel, IView where T : AbstractContext<T>, new()
    {
        IContext IContextHolder.Context => AbstractContext<T>.Interface;
    }
}
