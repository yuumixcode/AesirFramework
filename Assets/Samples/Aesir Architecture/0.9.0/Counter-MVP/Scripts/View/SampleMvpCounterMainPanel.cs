using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器主面板视图。
    /// </summary>
    /// <remarks>
    /// View 同时实现 <see cref="MonoView{T}"/> 和 <see cref="ISampleMvpCounterView"/>，
    /// 既是 MonoBehaviour 的 UI 容器，也是 Presenter 可驱动的被动视图。
    /// <para>View 全程不直接访问 Model，只通过事件将用户输入通知 Presenter，
    /// 并由 Presenter 回调 <see cref="UpdateCount"/> 推送显示数据。</para>
    /// <para>数据流：按钮点击 → View 事件 → Presenter → Command → Model → Presenter → View 刷新。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.MonoView{T}"/>
    /// <seealso cref="ISampleMvpCounterView"/>
    /// <seealso cref="SampleMvpCounterPresenter"/>
    public sealed class SampleMvpCounterMainPanel : MonoView<SampleMvpCounterContext>, ISampleMvpCounterView
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

        SampleMvpCounterPresenter _presenter;

        void Awake()
        {
            _presenter = new SampleMvpCounterPresenter(this);
        }

        void Start()
        {
            // 同步初始值：避免场景残留文本与 Model 初始值不一致
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
        /// <param name="count">最新的计数值。</param>
        public void UpdateCount(int count)
        {
            countText.text = count.ToString();
            AesirArchitectureDebug.Log($"[Counter-MVP] Count = {count}");
        }
    }
}
