using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-2 标准档示例 —— 计数器主面板（View，与 Controller 分离实例）。
    /// </summary>
    /// <remarks>
    /// 标准档：面板继承 <see cref="MonoView{T}" />（仅只读能力，接口层面不可执行 Command），
    /// 与 Controller 拆为两个实例；Start 中 GetModel 按<b>具体类</b>缓存引用并订阅刷新，
    /// 随后把该实例注入 Controller——View 与 Controller <b>共享同一个 Model 实例</b>。
    /// <para>写入路径：按钮点击 → Controller 直调 Model 写方法（不经 Command）。</para>
    /// <para>
    /// 对照：快捷档（Counter-Mvc-Quick）View 兼 Controller 同一实例直改 ObservableValue；
    /// 严格档（Counter-Mvc-Strict）Controller 经 Context 发布 Command。
    /// </para>
    /// <para>数据流：按钮点击 → Controller 调 Model 写方法 → ObservableValue 通知 → 面板刷新。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.MonoView{T}" />
    /// <seealso cref="SampleMvcStandardCounterController" />
    public class SampleMvcStandardCounterMainPanel : MonoView<SampleMvcStandardCounterContext>
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
        /// 与 View 共享同一 Model 实例的控制器（Start 中构造注入）。
        /// </summary>
        SampleMvcStandardCounterController _controller;

        /// <summary>
        /// 当前 Context 中注册的计数器 Model（具体类存储）。
        /// </summary>
        /// <remarks>
        /// <c>GetModel</c> 每次调用执行字典查找 + 初始化检查，故按推荐做法在 Start 中
        /// 获取并缓存为字段，避免 Update 等每帧路径重复查找。
        /// </remarks>
        SampleMvcStandardCounterModel _model;

        void Start()
        {
            _model = this.GetModel<SampleMvcStandardCounterModel>();
            _controller = new SampleMvcStandardCounterController(_model);
            _model.Count.AddListenerAndInvoke(UpdateCountText)
                .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        }

        void OnEnable()
        {
            increaseButton.onClick.AddListener(Increase);
            decreaseButton.onClick.AddListener(Decrease);
            resetButton.onClick.AddListener(ResetCounter);
        }

        void OnDisable()
        {
            increaseButton.onClick.RemoveListener(Increase);
            decreaseButton.onClick.RemoveListener(Decrease);
            resetButton.onClick.RemoveListener(ResetCounter);
        }

        void Increase() => _controller.Increase();
        void Decrease() => _controller.Decrease();
        void ResetCounter() => _controller.ResetCounter();

        /// <summary>
        /// 根据当前计数值更新 UI 文本显示。
        /// </summary>
        public void UpdateCountText(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }
    }
}
