namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Demo 游戏上下文（MVP 版）
    /// </summary>
    public sealed class SampleMvpCounterContext : AbstractContext<SampleMvpCounterContext>
    {
        protected override void Configure()
        {
            RegisterModel<ISampleMvpCounterModel>(new SampleMvpCounterModel());
        }
    }
}
