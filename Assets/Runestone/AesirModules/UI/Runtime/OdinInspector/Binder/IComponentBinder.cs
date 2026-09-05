namespace Runestone.AesirModules
{
    /// <summary>
    /// 绑定引用接口。由 <see cref="BinderAssistant" /> 生成的脚本实现，用于自动绑定场景中的组件引用。
    /// <para>
    /// 生成脚本中实现 <c>BindReferences()</c> 方法，内部通过 <c>transform.Find</c> 查找并赋值字段。
    /// 脚本编译后 <see cref="BinderAssistant" /> 会自动调用此方法完成首次绑定。
    /// </para>
    /// <para>
    /// 注意：当前生成的脚本方法名为 <c>BindReferences()</c>，
    /// 与本接口声明的 <see cref="BindComponents" /> 不一致，
    /// 由 <c>BindCommand()</c> 方法中转调用 <c>BindReferences()</c>。
    /// </para>
    /// </summary>
    public interface IComponentBinder
    {
        /// <summary>
        /// 绑定引用。查找并赋值场景中的组件引用。
        /// <para>
        /// 实现内部遍历所有标记的 <see cref="BinderInfo" /> 路径，
        /// 调用 <c>transform.Find(path).GetComponent&lt;T&gt;()</c> 获取组件并赋值到对应字段。
        /// </para>
        /// </summary>
        void BindComponents();
    }
}
