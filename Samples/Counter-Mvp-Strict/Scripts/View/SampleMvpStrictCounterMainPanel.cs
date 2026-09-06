#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples.MvpStrict
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器主面板（被动视图）。
    /// </summary>
    /// <remarks>
    /// 严格档（第三课）：View 仍为被动视图——不继承任何能力基类
    /// （对比 MVC 的 View 继承 <see cref="Runestone.AesirArchitecture.MonoView{T}" /> 自行订阅 Model），
    /// 仅实现 <see cref="ISampleMvpStrictCounterView" /> 契约；Start 中按接口类型
    /// <see cref="ISampleMvpStrictCounterPresenter" /> 存储 Presenter——
    /// 经接口仅可触达生命周期入口，类型层面拿不到 ExecuteCommand 等框架能力。
    /// <para>
    /// 对照：快捷档（Counter-Mvp-Quick）Presenter 直改可写 ObservableValue；
    /// 标准档（Counter-Mvp-Standard）写方法直调 + Model 直读——与 MVC 三档分级一致，差异仅在刷新路径。
    /// </para>
    /// <para>
    /// 数据流：按钮点击 → View 事件 → Presenter → Command → Model →
    /// Query 拉取 → Presenter 推送 View 刷新。
    /// </para>
    /// </remarks>
    /// <seealso cref="ISampleMvpStrictCounterView" />
    /// <seealso cref="ISampleMvpStrictCounterPresenter" />
    public sealed class SampleMvpStrictCounterMainPanel : MonoBehaviour, ISampleMvpStrictCounterView
    {
        /// <summary>
        /// 显示当前计数值的 UI 文本组件。
        /// </summary>
        [SerializeField]
        Text countText;

        /// <summary>
        /// 触发增加计数的按钮。
        /// </summary>
        [SerializeField]
        Button increaseButton;

        /// <summary>
        /// 触发减少计数的按钮。
        /// </summary>
        [SerializeField]
        Button decreaseButton;

        /// <summary>
        /// 触发重置计数的按钮。
        /// </summary>
        [SerializeField]
        Button resetButton;

        /// <summary>
        /// 中介本面板与 Model 的 Presenter（Awake 中 new，接口类型存储）。
        /// </summary>
        /// <remarks>
        /// 与 MVC-3 View 按业务接口存储 Controller 对称：经接口仅可触达
        /// 同步初始值 / 释放订阅两项生命周期入口，
        /// 类型层面拿不到 ExecuteCommand 等框架能力（见 <see cref="ISampleMvpStrictCounterPresenter" />）。
        /// </remarks>
        ISampleMvpStrictCounterPresenter _presenter;

        void Awake()
        {
            _presenter = new SampleMvpStrictCounterPresenter(this);
        }

        void Start()
        {
            _presenter.SyncInitialValue();
        }

        void OnEnable()
        {
            increaseButton.onClick.AddListener(RaiseIncreaseClicked);
            decreaseButton.onClick.AddListener(RaiseDecreaseClicked);
            resetButton.onClick.AddListener(RaiseResetClicked);
        }

        void OnDisable()
        {
            increaseButton.onClick.RemoveListener(RaiseIncreaseClicked);
            decreaseButton.onClick.RemoveListener(RaiseDecreaseClicked);
            resetButton.onClick.RemoveListener(RaiseResetClicked);
        }

        void OnDestroy()
        {
            _presenter.Dispose();
        }

        /// <inheritdoc />
        public event Action IncreaseClicked;

        /// <inheritdoc />
        public event Action DecreaseClicked;

        /// <inheritdoc />
        public event Action ResetClicked;

        /// <summary>
        /// 由 Presenter 调用，将最新计数值更新到 UI 文本。
        /// </summary>
        public void UpdateCount(int count)
        {
            countText.text = count.ToString();
        }

        void RaiseIncreaseClicked() => IncreaseClicked?.Invoke();
        void RaiseDecreaseClicked() => DecreaseClicked?.Invoke();
        void RaiseResetClicked() => ResetClicked?.Invoke();
    }
}
#endif
