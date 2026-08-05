using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC 示例 —— 计数器主面板视图。
    /// </summary>
    /// <remarks>
    /// View 仅负责 UI 展示与用户输入转发，不包含任何业务逻辑。
    /// 通过 <see cref="MonoView{T}"/> 基类自动获取当前 Context，
    /// 在 Awake 中订阅 Model 的 <see cref="ObservableValue{T}"/> 变更回调，
    /// 并将按钮点击转发给 Controller 执行 Command。
    /// <para>数据流（展示）：Model → ObservableValue 通知 → UpdateCountText → UI 刷新。</para>
    /// <para>数据流（输入）：按钮点击 → Controller → Command → Model。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.MonoView{T}"/>
    /// <seealso cref="ISampleMvcCounterController"/>
    /// <seealso cref="ISampleMvcCounterModel"/>
    public class SampleMvcCounterMainPanel : MonoView<SampleMvcCounterContext>
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

        ISampleMvcCounterController _ctrl;

        /// <summary>
        /// 获取当前 Context 中注册的计数器 Model。
        /// </summary>
        /// <remarks>
        /// 每次访问都从 Context 获取当前 Model，而非缓存字段引用。
        /// 这样在运行时通过 RegisterModel 动态替换 Model 后，始终能拿到最新实例；
        /// 旧实例在无人持有后可被 GC 正常回收，支持运行时热替换
        /// （如切换为继承 MonoBehaviour 的可视化 Model）。
        /// </remarks>
        ISampleMvcCounterModel Model => this.GetModel<ISampleMvcCounterModel>();

        void Awake()
        {
            Model.Count.AddListener(UpdateCountText).RemoveListenerWhenGameObjectOnDestroyed(gameObject);
            _ctrl = new SampleMvcCounterController();
        }

        void OnEnable()
        {
            increaseButton.onClick.AddListener(_ctrl.Increase);
            decreaseButton.onClick.AddListener(_ctrl.Decrease);
            resetButton.onClick.AddListener(_ctrl.ResetCounter);
        }

        void OnDisable()
        {
            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
            resetButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 根据当前计数值更新 UI 文本显示。
        /// </summary>
        /// <param name="count">最新的计数值，由 ObservableValue 变更回调传入。</param>
        /// <remarks>
        /// 此方法注册为 <see cref="ISampleMvcCounterModel.Count"/> 的监听回调，
        /// 在 Model 数据变更时自动调用，实现 View 对 Model 的响应式刷新。
        /// </remarks>
        public void UpdateCountText(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }
    }
}
