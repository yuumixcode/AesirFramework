namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 减少计数命令。
    /// </summary>
    /// <seealso cref="SampleMvpIncreaseCommand"/>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractCommand"/>
    public class SampleMvpDecreaseCommand : AbstractCommand
    {
        /// <summary>
        /// 执行减少计数逻辑：从 Context 获取 Model 并调用其 Decrease 方法。
        /// </summary>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvpCounterModel>().Decrease();
            AesirArchitectureDebug.Log("Decrease Counter");
        }
    }
}
