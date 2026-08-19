namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 简单档示例 —— 计数器 Presenter 接口。
    /// </summary>
    /// <remarks>
    /// 通过默认接口实现绑定 Context（仿照 IController&lt;T&gt; 的写法）。
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IPresenter{T}"/>
    public interface ISampleMvpSimpleCounterPresenter : IPresenter<SampleMvpSimpleCounterContext> { }
}
