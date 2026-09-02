namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 快捷档示例 —— 计数器 Presenter 实现（Presenter 直改 Model）。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>快捷档写入</b>：Presenter 直接修改 Model 的可写 ObservableValue
    ///     （<c>count.Value++</c>，不建 Command、不经写方法），读取直取 <c>count.Value</c>——
    ///     与 MVC-1（Counter-Mvc-Quick）的 View 兼 Controller 直改写法一致。
    ///     </para>
    ///     <para>
    ///     <b>这是快捷写法</b>：绕过写方法直改 ObservableValue，适合原型 / 小功能；
    ///     标准档（Counter-Mvp-Standard）收窄为只读暴露 + 写方法；
    ///     严格档（Counter-Mvp-Strict）再加接口注册 + Command 写入 + Query 读取。
    ///     </para>
    ///     <para>
    ///     <b>快捷档不建 View 接口</b>：Presenter 直接持有具体面板类（与 MVC-1 无任何接口抽象一致）；
    ///     标准档起 View 契约才以接口（<c>IXxxView</c>）形式存在。
    ///     </para>
    ///     <para>刷新路径：Presenter 推送刷新（MVP 模式特征——View 被动，不订阅 Model）。</para>
    ///     <para>数据流：View（用户输入）→ Presenter 直改 Model → Presenter 直读 → 推送 View（刷新显示）。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IPresenter{T}" />
    /// <seealso cref="SampleMvpQuickCounterMainPanel" />
    public sealed class SampleMvpQuickCounterPresenter : IPresenter<SampleMvpQuickCounterContext>
    {
        /// <summary>
        /// 当前 Context 中注册的计数器 Model（构造时缓存，具体类存储）。
        /// </summary>
        /// <remarks>
        /// <c>GetModel</c> 每次调用执行字典查找 + 初始化检查，故按推荐做法在构造函数中
        /// 获取并缓存为字段，避免每帧路径重复查找。
        /// </remarks>
        readonly SampleMvpQuickCounterModel _model;

        readonly SampleMvpQuickCounterMainPanel _view;

        /// <summary>
        /// 创建 Presenter：缓存 Model 并订阅 View 的用户输入事件。
        /// </summary>
        public SampleMvpQuickCounterPresenter(SampleMvpQuickCounterMainPanel view)
        {
            _view = view;
            _model = this.GetModel<SampleMvpQuickCounterModel>();
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
            _view.UpdateCount(_model.count.Value);
        }

        void OnIncreaseClicked()
        {
            _model.count.Value++;
            _view.UpdateCount(_model.count.Value);
        }

        void OnDecreaseClicked()
        {
            _model.count.Value--;
            _view.UpdateCount(_model.count.Value);
        }

        void OnResetClicked()
        {
            _model.count.Value = 0;
            _view.UpdateCount(_model.count.Value);
        }
    }
}
