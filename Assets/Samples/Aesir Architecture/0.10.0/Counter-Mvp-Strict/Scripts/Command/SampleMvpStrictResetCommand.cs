namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 重置计数命令。
    /// </summary>
    /// <seealso cref="SampleMvpStrictIncreaseCommand"/>
    public class SampleMvpStrictResetCommand : AbstractCommand
    {
        /// <summary>
        /// 执行重置计数逻辑：从 Context 获取 Model 并调用其写方法。
        /// </summary>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvpStrictCounterModel>().Reset();
            AesirArchitectureDebug.Log("Strict Mvp Reset Counter");
        }
    }
}
