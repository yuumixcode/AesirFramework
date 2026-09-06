#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples.MvpStandard
{
    /// <summary>
    /// MVP-2 标准档示例 —— 计数器主面板（被动视图）。
    /// </summary>
    /// <remarks>
    /// 标准档（第二课）：View 仍为被动视图——不继承任何能力基类
    /// （对比 MVC 的 View 继承 <see cref="Runestone.AesirArchitecture.MonoView{T}" /> 自行订阅 Model），
    /// 仅实现 <see cref="ISampleMvpStandardCounterView" /> 契约：用户输入以事件抛给 Presenter，
    /// 显示由 Presenter 推送刷新；Model 收窄为只读暴露 + 写方法，写入与读取全部在 Presenter 侧完成。
    /// <para>
    /// 对照：快捷档（Counter-Mvp-Quick）Presenter 直改可写 ObservableValue；
    /// 严格档（Counter-Mvp-Strict）写入走 Command、读取走 Query。
    /// </para>
    /// <para>数据流：按钮点击 → View 事件 → Presenter 调 Model 写方法 → Presenter 直读 → 推送 View 刷新。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvpStandardCounterView" />
    /// <seealso cref="SampleMvpStandardCounterPresenter" />
    public sealed class SampleMvpStandardCounterMainPanel : MonoBehaviour, ISampleMvpStandardCounterView
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
        /// 中介本面板与 Model 的 Presenter（Awake 中构造，具体类存储）。
        /// </summary>
        SampleMvpStandardCounterPresenter _presenter;

        void Awake()
        {
            _presenter = new SampleMvpStandardCounterPresenter(this);
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
