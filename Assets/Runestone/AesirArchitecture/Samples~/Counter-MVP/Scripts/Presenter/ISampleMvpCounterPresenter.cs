namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器 Presenter 接口。
    /// </summary>
    /// <remarks>
    /// Presenter 是 MVP 模式的核心角色，作为 View 和 Model 之间的中介：
    /// 监听 View 的用户输入事件，调用 Model 方法修改数据，再将结果推回 View 更新显示。
    /// View 全程不直接访问 Model，实现了真正的"被动视图"（Passive View）。
    /// <para>框架未提供泛型 IPresenter&lt;T&gt;，因此仿照 IController&lt;T&gt; 的写法，
    /// 通过默认接口实现绑定 Context。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IPresenter{T}"/>
    /// <seealso cref="SampleMvpCounterPresenter"/>
    public interface ISampleMvpCounterPresenter : IPresenter<SampleMvpCounterContext> { }
}
