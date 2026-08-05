namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Presenter 接口。框架未提供泛型 IPresenter&lt;T&gt;，
    /// 因此仿照 IController&lt;T&gt; 的写法，通过默认接口实现绑定 Context。
    /// </summary>
    public interface ISampleMvpCounterPresenter : IPresenter<SampleMvpCounterContext> { }
}
