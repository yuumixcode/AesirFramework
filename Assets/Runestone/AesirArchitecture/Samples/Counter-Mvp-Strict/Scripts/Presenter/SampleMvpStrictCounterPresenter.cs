namespace Runestone.AesirArchitecture.Samples.MvpStrict
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器 Presenter 实现（Command 写 + Query 读）。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>严格档</b>：写入走 <c>ExecuteCommand</c>（Command 经 Model 写方法修改），
    ///     读取走 <c>ExecuteQuery</c>（替代 Model.Count.Value 直读）——Presenter 对 Model 的写/读均不直接接触。
    ///     </para>
    ///     <para>
    ///     双接口设计：业务接口 <see cref="ISampleMvpStrictCounterPresenter" /> 是 View 侧的持有类型
    ///     （只暴露生命周期入口）；框架角色接口 <see cref="Runestone.AesirArchitecture.IPresenter{T}" /> 提供 Command / Query 能力——
    ///     两者各司其职，与 MVC-3（Counter-Mvc-Strict）的双接口设计同构。
    ///     </para>
    ///     <para>
    ///     对照：快捷档（Counter-Mvp-Quick）Presenter 直改可写 ObservableValue；
    ///     标准档（Counter-Mvp-Standard）写方法直调 + Model 直读——与 MVC 三档分级一致，差异仅在刷新路径（Presenter 推送 vs View 订阅）。
    ///     </para>
    ///     <para>数据流：View → Presenter → Command → Model → Query 拉取 → Presenter → View 刷新。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvpStrictCounterPresenter" />
    /// <seealso cref="SampleMvpStrictCounterMainPanel" />
    public sealed class SampleMvpStrictCounterPresenter : ISampleMvpStrictCounterPresenter,
        IPresenter<SampleMvpStrictCounterContext>
    {
        readonly ISampleMvpStrictCounterView _view;

        /// <summary>
        /// 创建 Presenter 并订阅 View 的用户输入事件。
        /// </summary>
        public SampleMvpStrictCounterPresenter(ISampleMvpStrictCounterView view)
        {
            _view = view;
            _view.IncreaseClicked += OnIncreaseClicked;
            _view.DecreaseClicked += OnDecreaseClicked;
            _view.ResetClicked += OnResetClicked;
        }

        /// <summary>
        /// 同步初始值到 View。
        /// </summary>
        public void SyncInitialValue()
        {
            _view.UpdateCount(this.ExecuteQuery<GetCounterValueQuery, int>());
        }

        /// <summary>
        /// 注销所有事件订阅，释放 Presenter 持有的 View 引用。
        /// </summary>
        public void Dispose()
        {
            _view.IncreaseClicked -= OnIncreaseClicked;
            _view.DecreaseClicked -= OnDecreaseClicked;
            _view.ResetClicked -= OnResetClicked;
        }

        void OnIncreaseClicked()
        {
            this.ExecuteCommand<SampleMvpStrictIncreaseCommand>();
            _view.UpdateCount(this.ExecuteQuery<GetCounterValueQuery, int>());
        }

        void OnDecreaseClicked()
        {
            this.ExecuteCommand<SampleMvpStrictDecreaseCommand>();
            _view.UpdateCount(this.ExecuteQuery<GetCounterValueQuery, int>());
        }

        void OnResetClicked()
        {
            this.ExecuteCommand<SampleMvpStrictResetCommand>();
            _view.UpdateCount(this.ExecuteQuery<GetCounterValueQuery, int>());
        }
    }
}
