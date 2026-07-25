namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Demo 游戏上下文
    /// </summary>
    public sealed class SampleMvcCounterContext : AbstractContext<SampleMvcCounterContext>
    {
        protected override void Configure()
        {
            RegisterModel<ISampleMvcCounterModel>(new SampleMvcCounterModel());
        }
    }
}
