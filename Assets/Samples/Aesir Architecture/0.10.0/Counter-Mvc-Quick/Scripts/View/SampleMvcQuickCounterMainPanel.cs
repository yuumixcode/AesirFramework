using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-1 快捷档示例 —— 计数器主面板（View 兼 Controller）。
    /// </summary>
    /// <remarks>
    /// 快捷档：面板继承 <see cref="MonoViewController{T}" />（View 与 Controller 为同一实例），
    /// 按钮回调中<b>直接修改 Model 的可写 ObservableValue</b>——不建 Command、不建独立 Controller，
    /// Model 以具体类注册与存储，用最少概念（Context + Model + 面板）跑通"数据驱动 UI"的闭环。
    /// <para>
    /// <b>这是快捷写法</b>：绕过 Command 直写 ObservableValue，适合原型/小功能；
    /// 标准档（Counter-Mvc-Standard）收窄为只读暴露 + 写方法；
    /// 严格档（Counter-Mvc-Strict）再加接口注册 + Command 写入。
    /// </para>
    /// <para>数据流：按钮点击 → 面板直改 count.Value → ObservableValue 通知 → 面板刷新。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.MonoViewController{T}" />
    public class SampleMvcQuickCounterMainPanel : MonoViewController<SampleMvcQuickCounterContext>
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
        /// 当前 Context 中注册的计数器 Model。
        /// </summary>
        /// <remarks>
        /// <c>GetModel</c> 每次调用执行字典查找 + 初始化检查，故按推荐做法在 Start 中
        /// 获取并缓存为字段，避免 Update 等每帧路径重复查找。
        /// </remarks>
        SampleMvcQuickCounterModel _model;

        void Start()
        {
            _model = this.GetModel<SampleMvcQuickCounterModel>();
            _model.count.AddListenerAndInvoke(UpdateCountText)
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

        void Increase() => _model.count.Value++;
        void Decrease() => _model.count.Value--;
        void ResetCounter() => _model.count.Value = 0;

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
