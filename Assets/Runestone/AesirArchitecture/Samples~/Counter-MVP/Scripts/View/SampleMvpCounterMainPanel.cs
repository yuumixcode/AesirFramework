using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 计数器示例入口。View 通过 [ContextMenu] 模拟按钮点击，
    /// Presenter 在幕后完成 Model 调度与事件发布，View 全程不直接访问 Model。
    /// </summary>
    public sealed class SampleMvpCounterMainPanel : MonoView<SampleMvpCounterContext>, ISampleMvpCounterView
    {
        /// <summary>
        /// 显示计数值的文本组件
        /// </summary>
        [SerializeField]
        Text countText;

        /// <summary>
        /// 增加计数的按钮
        /// </summary>
        [SerializeField]
        Button increaseButton;

        /// <summary>
        /// 减少计数的按钮
        /// </summary>
        [SerializeField]
        Button decreaseButton;

        /// <summary>
        /// 重置计数的按钮
        /// </summary>
        [SerializeField]
        Button resetButton;

        SampleMvpCounterPresenter _presenter;

        void Awake()
        {
            _presenter = new SampleMvpCounterPresenter(this);
        }

        void OnEnable()
        {
            increaseButton.onClick.AddListener(IncreaseClicked.Invoke);
            decreaseButton.onClick.AddListener(DecreaseClicked.Invoke);
            resetButton.onClick.AddListener(ResetClicked.Invoke);
        }

        void OnDisable()
        {
            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
            resetButton.onClick.RemoveAllListeners();
        }

        void OnDestroy()
        {
            _presenter.Dispose();
        }

        public Action IncreaseClicked { get; set; }
        public Action DecreaseClicked { get; set; }
        public Action ResetClicked { get; set; }

        public void UpdateCount(int count)
        {
            countText.text = count.ToString();
            AesirArchitectureDebug.Log($"[Counter-MVP] Count = {count}");
        }
    }
}
