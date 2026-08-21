namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-2 标准档示例 —— 计数器 Presenter 实现（写方法直调 + Model 直读）。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>标准档读写</b>：写入直接调用 Model 写方法（不经 Command），
    ///     读取直取只读属性 <c>Count.Value</c>——Model 封装修改入口，读写路径保持简单。
    ///     </para>
    ///     <para>
    ///     对照：快捷档（Counter-Mvp-Quick）Presenter 直改可写 ObservableValue；
    ///     严格档（Counter-Mvp-Strict）写入改走 Command、读取改走 Query——
    ///     与 MVC-2（Counter-Mvc-Standard）Controller 直调写方法分级一致，差异仅在刷新路径（Presenter 推送 vs View 订阅）。
    ///     </para>
    ///     <para>刷新路径：Presenter 推送刷新（MVP 模式特征——View 被动，不订阅 Model）。</para>
    ///     <para>数据流：View（用户输入）→ Presenter → Model 写方法 → Presenter 直读 → 推送 View（刷新显示）。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IPresenter{T}" />
    /// <seealso cref="SampleMvpStandardCounterMainPanel" />
    public sealed class SampleMvpStandardCounterPresenter : IPresenter<SampleMvpStandardCounterContext>
    {
        /// <summary>
        /// 当前 Context 中注册的计数器 Model（构造时缓存，具体类存储）。
        /// </summary>
        /// <remarks>
        /// <c>GetModel</c> 每次调用执行字典查找 + 初始化检查，故按推荐做法在构造函数中
        /// 获取并缓存为字段，避免每帧路径重复查找。
        /// </remarks>
        readonly SampleMvpStandardCounterModel _model;

        readonly ISampleMvpStandardCounterView _view;

        /// <summary>
        /// 创建 Presenter：缓存 Model 并订阅 View 的用户输入事件。
        /// </summary>
        public SampleMvpStandardCounterPresenter(ISampleMvpStandardCounterView view)
        {
            _view = view;
            _model = this.GetModel<SampleMvpStandardCounterModel>();
            _view.IncreaseClicked += OnIncreaseClicked;
            _view.DecreaseClicked += OnDecreaseClicked;
            _view.ResetClicked += OnResetClicked;
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

        /// <summary>
        /// 同步初始值到 View，避免场景残留文本与 Model 初始值不一致。
        /// </summary>
        public void SyncInitialValue()
        {
            _view.UpdateCount(_model.Count.Value);
        }

        void OnIncreaseClicked()
        {
            _model.Increase();
            _view.UpdateCount(_model.Count.Value);
            AesirArchitectureDebug.Log("Standard Mvp Increase Counter");
        }

        void OnDecreaseClicked()
        {
            _model.Decrease();
            _view.UpdateCount(_model.Count.Value);
            AesirArchitectureDebug.Log("Standard Mvp Decrease Counter");
        }

        void OnResetClicked()
        {
            _model.Reset();
            _view.UpdateCount(_model.Count.Value);
            AesirArchitectureDebug.Log("Standard Mvp Reset Counter");
        }
    }
}
