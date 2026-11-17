namespace Runestone.AesirModules
{
    /// <summary>
    /// 绑定引用接口。由 <see cref="BinderAssistant" /> 生成的脚本实现，用于自动绑定场景中的组件引用。
    /// <para>
    /// 生成脚本在「绑定字段（自动生成）」region 之后实现 <see cref="BindComponents" /> 方法，
    /// 内部按配置的层级路径 <c>transform.Find</c> 并 <c>GetComponent</c> 赋值字段；
    /// 同一实现挂有 <c>[ContextMenu("绑定引用")]</c>，可在 Inspector 右键手动触发。
    /// 脚本编译后 <see cref="BinderAssistant" /> 会自动挂载组件并调用一次此方法完成首次绑定。
    /// </para>
    /// </summary>
    public interface IComponentBinder
    {
        /// <summary>
        /// 绑定引用。查找并赋值场景中的组件引用。
        /// </summary>
        void BindComponents();
    }
}
