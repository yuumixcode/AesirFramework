namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器 Presenter 实现（Command 写 + Query 读）。
    /// </summary>
    /// <remarks>
    /// <para><b>严格档</b>：写入走 <c>ExecuteCommand</c>（Command 经 Model 写方法修改），
    /// 读取走 <c>ExecuteQuery</c>（替代 Model.Count.Value 直读）——Presenter 对 Model 的写/读均不直接接触。</para>
    /// <para>对照 MVP-2 标准档（Counter-MVP）：Command 写 + Model 直读。</para>
    /// <para>数据流：View → Presenter → Command → Model → Query 拉取 → Presenter → View 刷新。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvpStrictCounterPresenter"/>
    public sealed class SampleMvpStrictCounterPresenter : ISampleMvpStrictCounterPresenter
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

        /// <summary>
        /// 注销所有事件订阅，释放 Presenter 持有的 View 引用。
        /// </summary>
        public void Dispose()
        {
            _view.IncreaseClicked -= OnIncreaseClicked;
            _view.DecreaseClicked -= OnDecreaseClicked;
            _view.ResetClicked -= OnResetClicked;
        }
    }
}
