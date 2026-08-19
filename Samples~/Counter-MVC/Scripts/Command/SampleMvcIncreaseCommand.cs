namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 增加计数的命令，调用 Model 的 Increase 方法并记录日志
    /// </summary>
    public class SampleMvcIncreaseCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvcCounterModel>().Increase();
            AesirArchitectureDebug.Log("Increase Counter");
        }
    }
}
