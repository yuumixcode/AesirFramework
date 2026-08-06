namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Presenter 实现。
    /// <para>直接调用 Model 方法（而非 MVC 的 Command，也可以增加一层 Command）
    /// 用于对比展示 MVP 与 MVC 的差异。</para>
    /// </summary>
    public sealed class SampleMvpCounterPresenter : ISampleMvpCounterPresenter
    {
        readonly ISampleMvpCounterView _view;

        /// <summary>
        /// 每次访问都从 Context 获取当前 Model，而非缓存字段引用。
        /// <para>这样在运行时通过 RegisterModel 动态替换 Model 后，始终能拿到最新实例；</para>
        /// <para>旧实例在无人持有后可被 GC 正常回收，支持运行时热替换（如切换为继承 MonoBehaviour 的可视化 Model）。</para>
        /// </summary>
        ISampleMvpCounterModel Model => this.GetModel<ISampleMvpCounterModel>();

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

        public void Dispose()
        {
            _view.IncreaseClicked -= OnIncreaseClicked;
            _view.DecreaseClicked -= OnDecreaseClicked;
            _view.ResetClicked -= OnResetClicked;
        }
    }
}
