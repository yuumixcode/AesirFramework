using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-1 快捷档示例 —— 计数器主面板（View 兼 Controller）。
    /// </summary>
    /// <remarks>
    /// 快捷档：面板继承 <see cref="MonoViewController{T}"/>（同一类型同时是 IView + IController），
    /// 按钮回调中<b>直接修改 Model 的 ObservableValue</b>——不建 Command、不建独立 Controller，
    /// 用最少概念（Context + Model + 面板）跑通"数据驱动 UI"的闭环。
    /// <para><b>这是快捷写法</b>：绕过 Command 直写 Model，适合原型/小功能；
    /// 标准写法（Command 唯一写入入口）见 Counter-MVC 示例。</para>
    /// <para>数据流：按钮点击 → 面板直改 Count.Value → ObservableValue 通知 → 面板刷新。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.MonoViewController{T}"/>
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
        /// 获取当前 Context 中注册的计数器 Model。
        /// </summary>
        /// <remarks>
        /// ⚠️ 每次访问均执行一次字典查找 + 初始化检查。<b>不推荐用于 Update 等每帧路径</b>——
        /// 如确需每帧调用，请自行确认其必要性与开销；常规做法是在 Awake/Initialize 缓存字段引用。
        /// </remarks>
        ISampleMvcQuickCounterModel Model => this.GetModel<ISampleMvcQuickCounterModel>();

        void Awake()
        {
            Model.Count.AddListenerAndInvoke(UpdateCountText).RemoveListenerWhenGameObjectOnDestroyed(gameObject);
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

        void Increase() => Model.Count.Value++;
        void Decrease() => Model.Count.Value--;
        void ResetCounter() => Model.Count.Value = 0;

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
