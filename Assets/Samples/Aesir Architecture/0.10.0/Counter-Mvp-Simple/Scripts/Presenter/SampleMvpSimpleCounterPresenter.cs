namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 简单档示例 —— 计数器 Presenter 实现（Presenter 直写 Model）。
    /// </summary>
    /// <remarks>
    /// <para><b>简单档写入</b>：Presenter 直接调用 Model 写方法（不建 Command）——
    /// 适合 UI 交互直接映射数据的场景，流程更简洁；
    /// 标准档（Counter-MVP）写入改走 Command，与本档形成对照。</para>
    /// <para>刷新路径：Presenter 推送刷新（与标准档一致的 MVP 模式特征）。</para>
    /// <para>数据流：View（用户输入）→ Presenter → Model → Presenter → View（刷新显示）。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvpSimpleCounterPresenter"/>
    public sealed class SampleMvpSimpleCounterPresenter : ISampleMvpSimpleCounterPresenter
    {
        readonly ISampleMvpSimpleCounterView _view;

        /// <summary>
        /// 获取当前 Context 中注册的计数器 Model。
        /// </summary>
        /// <remarks>
        /// ⚠️ 每次访问均执行一次字典查找 + 初始化检查。<b>不推荐用于 Update 等每帧路径</b>——
        /// 如确需每帧调用，请自行确认其必要性与开销；常规做法是缓存字段引用。
        /// </remarks>
        ISampleMvpSimpleCounterModel Model => this.GetModel<ISampleMvpSimpleCounterModel>();

        /// <summary>
        /// 创建 Presenter 并订阅 View 的用户输入事件。
        /// </summary>
        public SampleMvpSimpleCounterPresenter(ISampleMvpSimpleCounterView view)
        {
            _view = view;
            _view.IncreaseClicked += OnIncreaseClicked;
            _view.DecreaseClicked += OnDecreaseClicked;
            _view.ResetClicked += OnResetClicked;
        }

        /// <summary>
        /// 同步初始值到 View，避免场景残留文本与 Model 初始值不一致。
        /// </summary>
        public void SyncInitialValue()
        {
            _view.UpdateCount(Model.Count.Value);
        }

        void OnIncreaseClicked()
        {
            Model.Increase();
            _view.UpdateCount(Model.Count.Value);
            AesirArchitectureDebug.Log("Simple Increase Counter");
        }

        void OnDecreaseClicked()
        {
            Model.Decrease();
            _view.UpdateCount(Model.Count.Value);
            AesirArchitectureDebug.Log("Simple Decrease Counter");
        }

        void OnResetClicked()
        {
            Model.Reset();
            _view.UpdateCount(Model.Count.Value);
            AesirArchitectureDebug.Log("Simple Reset Counter");
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
