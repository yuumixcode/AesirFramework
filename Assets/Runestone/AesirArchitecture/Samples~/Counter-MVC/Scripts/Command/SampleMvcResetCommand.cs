namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 重置计数的命令，调用 Model 的 Reset 方法并记录日志
    /// </summary>
    public class SampleMvcResetCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvcCounterModel>().Reset();
            AesirArchitectureDebug.Log("Reset Counter");
        }
    }
}
