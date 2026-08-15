namespace Runestone.AesirModules
{
    /// <summary>
    /// 绑定引用接口。由 Object Binder 生成的脚本实现，用于自动绑定场景中的组件引用。
    /// </summary>
    public interface IComponentBinder
    {
        /// <summary>
        /// 绑定引用。查找并赋值场景中的组件引用。
        /// </summary>
        void BindComponents();
    }
}
