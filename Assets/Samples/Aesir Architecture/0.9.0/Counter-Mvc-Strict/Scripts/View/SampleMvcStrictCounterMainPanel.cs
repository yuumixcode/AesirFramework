using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-3 严格档示例 —— 计数器主面板（View 兼 Controller，不持有 Model）。
    /// </summary>
    /// <remarks>
    /// 严格档：面板继承 <see cref="MonoViewController{T}"/>，
    /// 写入经 <c>ExecuteCommand</c>，读取经 <c>ExecuteQuery</c>——
    /// <b>不订阅 Model、不读 Model 字段</b>，View 对 Model 零持有。
    /// 每次 Command 执行后重新 Query 拉取最新值刷新显示。
    /// <para><b>取舍</b>：牺牲响应式推送（订阅刷新），换来最严格的解耦；
    /// 对照标准档（Counter-MVC）Controller 持有 Model + 订阅刷新。</para>
    /// <para>数据流：按钮点击 → ExecuteCommand → Model → ExecuteQuery 拉取 → 面板刷新。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.MonoViewController{T}"/>
    public class SampleMvcStrictCounterMainPanel : MonoViewController<SampleMvcStrictCounterContext>
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

        void Start()
        {
            RefreshDisplay();
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

        void Increase()
        {
            this.ExecuteCommand<SampleMvcStrictIncreaseCommand>();
            RefreshDisplay();
        }

        void Decrease()
        {
            this.ExecuteCommand<SampleMvcStrictDecreaseCommand>();
            RefreshDisplay();
        }

        void ResetCounter()
        {
            this.ExecuteCommand<SampleMvcStrictResetCommand>();
            RefreshDisplay();
        }

        void RefreshDisplay()
        {
            var value = this.ExecuteQuery<GetCounterValueQuery, int>();
            if (countText != null)
            {
                countText.text = value.ToString();
            }
        }
    }
}
