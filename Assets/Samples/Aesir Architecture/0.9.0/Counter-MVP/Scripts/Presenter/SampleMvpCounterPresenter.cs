namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器 Presenter 实现（标准档）。
    /// </summary>
    /// <remarks>
    /// Presenter 是 MVP 模式中的"协调者"：它同时持有 View 接口引用和 Model 访问入口，
    /// 将 View 的用户输入事件转化为 Command 执行，再将 Model 的最新值推送回 View。
    /// <para><b>标准档写入</b>：表现层写入必经 Command（与 MVC-2 共享同一条写入铁律）；
    /// MVP-1 简单档（Counter-Mvp-Simple）保留 Presenter 直写 Model 作为对照。</para>
    /// <para><b>刷新路径</b>：Presenter 推送刷新（<c>_view.UpdateCount(...)</c>）是 MVP 模式特征，
    /// 与 MVC 的 ObservableValue 订阅刷新形成教学对比。</para>
    /// <para>数据流：View（用户输入）→ Presenter → Command → Model → Presenter → View（刷新显示）。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvpCounterPresenter"/>
    /// <seealso cref="Runestone.AesirArchitecture.IPresenter{T}"/>
    /// <seealso cref="ISampleMvpCounterView"/>
    /// <seealso cref="ISampleMvpCounterModel"/>
    public sealed class SampleMvpCounterPresenter : ISampleMvpCounterPresenter
    {
        readonly ISampleMvpCounterView _view;

        /// <summary>
        /// 获取当前 Context 中注册的计数器 Model。
        /// </summary>
        /// <remarks>
        /// 每次访问都从 Context 获取当前 Model，而非缓存字段引用。
        /// 这样在运行时通过 RegisterModel 动态替换 Model 后，始终能拿到最新实例；
        /// 旧实例在无人持有后可被 GC 正常回收，支持运行时热替换
        /// （如切换为继承 MonoBehaviour 的可视化 Model）。
        /// <para>⚠️ 每次访问均执行一次字典查找 + 初始化检查。<b>不推荐用于 Update 等每帧路径</b>——
        /// 如确需每帧调用，请自行确认其必要性与开销；常规做法是缓存字段引用。</para>
        /// </remarks>
        ISampleMvpCounterModel Model => this.GetModel<ISampleMvpCounterModel>();

        /// <summary>
        /// 创建 Presenter 并订阅 View 的用户输入事件。
        /// </summary>
        /// <param name="view">被中介的被动视图，Presenter 通过此接口驱动 View 刷新。</param>
        /// <remarks>
        /// 构造时即完成事件订阅，确保不会遗漏任何用户输入。
        /// 调用方（通常是 View 自身）需在销毁时调用 <see cref="Dispose"/> 注销事件，避免内存泄漏。
        /// </remarks>
        public SampleMvpCounterPresenter(ISampleMvpCounterView view)
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
            this.ExecuteCommand<SampleMvpIncreaseCommand>();
            _view.UpdateCount(Model.Count.Value);
        }

        void OnDecreaseClicked()
        {
            this.ExecuteCommand<SampleMvpDecreaseCommand>();
            _view.UpdateCount(Model.Count.Value);
        }

        void OnResetClicked()
        {
            this.ExecuteCommand<SampleMvpResetCommand>();
            _view.UpdateCount(Model.Count.Value);
        }

        /// <summary>
        /// 注销所有事件订阅，释放 Presenter 持有的 View 引用。
        /// </summary>
        /// <remarks>
        /// 必须在 View 销毁时调用（如在 MonoBehaviour 的 OnDestroy 中），
        /// 否则 View 的事件委托仍持有 Presenter 的回调引用，导致无法被 GC 回收。
        /// </remarks>
        public void Dispose()
        {
            _view.IncreaseClicked -= OnIncreaseClicked;
            _view.DecreaseClicked -= OnDecreaseClicked;
            _view.ResetClicked -= OnResetClicked;
        }
    }
}
