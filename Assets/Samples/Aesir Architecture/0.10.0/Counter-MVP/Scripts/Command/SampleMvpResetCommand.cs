namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 重置计数命令。
    /// </summary>
    /// <seealso cref="SampleMvpIncreaseCommand"/>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractCommand"/>
    public class SampleMvpResetCommand : AbstractCommand
    {
        /// <summary>
        /// 执行重置计数逻辑：从 Context 获取 Model 并调用其 Reset 方法。
        /// </summary>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvpCounterModel>().Reset();
            AesirArchitectureDebug.Log("Reset Counter");
        }
    }
}
