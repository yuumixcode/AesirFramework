namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-3 严格档示例 —— 减少计数命令。
    /// </summary>
    /// <seealso cref="SampleMvcStrictIncreaseCommand" />
    public class SampleMvcStrictDecreaseCommand : AbstractCommand
    {
        /// <summary>
        /// 执行减少计数逻辑：从 Context 获取 Model 并调用其写方法。
        /// </summary>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvcStrictCounterModel>().Decrease();
            AesirArchitectureDebug.Log("Strict Decrease Counter");
        }
    }
}
