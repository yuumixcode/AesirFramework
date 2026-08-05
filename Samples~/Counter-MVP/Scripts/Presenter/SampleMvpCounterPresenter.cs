namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器 Presenter 实现。
    /// </summary>
    /// <remarks>
    /// Presenter 是 MVP 模式中的"协调者"：它同时持有 View 接口引用和 Model 访问入口，
    /// 将 View 的用户输入事件转化为对 Model 的操作，再将 Model 的最新值推送回 View。
    /// <para>与 MVC 不同，MVP 不经过 Command 层，Presenter 直接调用 Model 方法。
    /// 这样做的好处是流程更简洁、适合简单的 UI 交互；
    /// 如果需要更复杂的状态管理（如撤销/重做、操作日志），可以引入 Command 层。</para>
    /// <para>数据流：View（用户输入）→ Presenter → Model → Presenter → View（刷新显示）。</para>
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

        void OnIncreaseClicked()
        {
            Model.Increase();
            _view.UpdateCount(Model.Count.Value);
            AesirArchitectureDebug.Log("Increase Counter");
        }

        void OnDecreaseClicked()
        {
            Model.Decrease();
            _view.UpdateCount(Model.Count.Value);
            AesirArchitectureDebug.Log("Decrease Counter");
        }

        void OnResetClicked()
        {
            Model.Reset();
            _view.UpdateCount(Model.Count.Value);
            AesirArchitectureDebug.Log("Reset Counter");
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
