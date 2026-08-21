using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 快捷档示例 —— 计数器主面板（被动视图）。
    /// </summary>
    /// <remarks>
    /// 快捷档（第一课）：最少概念理解被动视图——View 不继承任何能力基类
    ///（对比 MVC 的 View 继承 <see cref="Runestone.AesirArchitecture.MonoView{T}"/> 自行订阅 Model），
    /// 用户输入以事件抛给 Presenter，显示由 Presenter 推送刷新；
    /// Model 的获取与修改全部在 Presenter 侧完成。
    /// <para><b>快捷档不建 View 接口</b>：面板不实现任何接口，Presenter 直接持有具体类
    ///（与 MVC-1 无任何接口抽象一致）；标准档起 View 契约才以接口形式存在。</para>
    /// <para>对照：标准档（Counter-Mvp-Standard）同样被动、Model 收窄为只读暴露 + 写方法；
    /// 严格档（Counter-Mvp-Strict）写入走 Command、读取走 Query。</para>
    /// <para>数据流：按钮点击 → View 事件 → Presenter 直改 Model → Presenter 推送 View 刷新。</para>
    /// </remarks>
    /// <seealso cref="SampleMvpQuickCounterPresenter"/>
    public sealed class SampleMvpQuickCounterMainPanel : MonoBehaviour
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
        SampleMvpQuickCounterPresenter _presenter;

        void Awake()
        {
            _presenter = new SampleMvpQuickCounterPresenter(this);
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

        /// <summary>
        /// 用户点击"增加"按钮时触发，由 Presenter 订阅处理。
        /// </summary>
        public event Action IncreaseClicked;

        /// <summary>
        /// 用户点击"减少"按钮时触发，由 Presenter 订阅处理。
        /// </summary>
        public event Action DecreaseClicked;

        /// <summary>
        /// 用户点击"重置"按钮时触发，由 Presenter 订阅处理。
        /// </summary>
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
