namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 减少计数的命令，调用 Model 的 Decrease 方法并记录日志
    /// </summary>
    public class SampleMvcDecreaseCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvcCounterModel>().Decrease();
            AesirArchitectureDebug.Log("Decrease Counter");
        }
    }
}
