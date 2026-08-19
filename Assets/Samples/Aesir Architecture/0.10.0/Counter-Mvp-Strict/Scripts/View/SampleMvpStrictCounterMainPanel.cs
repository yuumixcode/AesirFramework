using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器主面板视图。
    /// </summary>
    /// <seealso cref="SampleMvpStrictCounterPresenter"/>
    public sealed class SampleMvpStrictCounterMainPanel : MonoView<SampleMvpStrictCounterContext>,
        ISampleMvpStrictCounterView
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

        SampleMvpStrictCounterPresenter _presenter;

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

        void RaiseIncreaseClicked() => IncreaseClicked?.Invoke();
        void RaiseDecreaseClicked() => DecreaseClicked?.Invoke();
        void RaiseResetClicked() => ResetClicked?.Invoke();

        /// <summary>
        /// 由 Presenter 调用，将最新计数值更新到 UI 文本。
        /// </summary>
        public void UpdateCount(int count)
        {
            countText.text = count.ToString();
        }
    }
}
