namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC 示例 —— 增加计数命令。
    /// </summary>
    /// <remarks>
    /// 在 MVC 模式中，Controller 本身不应直接修改 Model 的状态，
    /// 而是将意图封装为 Command，由 Command 执行实际的数据变更操作。
    /// 这样做的好处是：每一次状态变更都有明确的入口，便于审计、撤销与回放。
    /// <para>数据流：View → Controller → Command → Model → ObservableValue 通知 → View 刷新。</para>
    /// </remarks>
    /// <seealso cref="SampleMvcDecreaseCommand"/>
    /// <seealso cref="SampleMvcResetCommand"/>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractCommand"/>
    /// <seealso cref="Runestone.AesirArchitecture.ICommand"/>
    public class SampleMvcIncreaseCommand : AbstractCommand
    {
        /// <summary>
        /// 执行增加计数逻辑：从 Context 获取 Model 并调用其 Increase 方法。
        /// </summary>
        /// <remarks>
        /// 通过 <c>this.GetModel&lt;T&gt;()</c> 获取注册在 Context 中的 Model 实例，
        /// 确保始终操作的是当前生效的 Model，而非某个缓存引用。
        /// </remarks>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvcCounterModel>().Increase();
            AesirArchitectureDebug.Log("Increase Counter");
        }
    }
}
