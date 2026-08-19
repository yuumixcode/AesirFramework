namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 增加计数命令。
    /// </summary>
    /// <remarks>
    /// 在 MVP-2 标准档中，Presenter 不直接修改 Model 的状态，
    /// 而是将意图封装为 Command，由 Command 执行实际的数据变更操作——
    /// 与 MVC-2 共享同一条写入铁律：表现层写入 Model 的唯一入口是 Command。
    /// <para>数据流：View → Presenter → Command → Model → Presenter 推送 → View 刷新。</para>
    /// </remarks>
    /// <seealso cref="SampleMvpDecreaseCommand"/>
    /// <seealso cref="SampleMvpResetCommand"/>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractCommand"/>
    /// <seealso cref="Runestone.AesirArchitecture.ICommand"/>
    public class SampleMvpIncreaseCommand : AbstractCommand
    {
        /// <summary>
        /// 执行增加计数逻辑：从 Context 获取 Model 并调用其 Increase 方法。
        /// </summary>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvpCounterModel>().Increase();
            AesirArchitectureDebug.Log("Increase Counter");
        }
    }
}
