#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples.MvcStrict
{
    /// <summary>
    /// MVC-3 严格档示例 —— 计数器主面板（View，与 Controller 分离实例）。
    /// </summary>
    /// <remarks>
    /// 严格档：面板继承 <see cref="MonoView{T}" />（仅只读能力，接口层面不可执行 Command），
    /// 与 Controller 拆为两个实例；Start 中 GetModel 按<b>接口类型</b>缓存引用（而非具体类）
    /// 并订阅刷新，随后 new 出 Controller 并按接口类型 <see cref="ISampleMvcStrictCounterController" />
    /// 存储——写入全部经 Controller 发布 Command。
    /// <para>
    /// 显示取值：原始值直接来自只读订阅（通常情况直接用只读值即可）；
    /// 加工值（十位近似）经 Controller 查询 <see cref="GetRoundedCountQuery" />。
    /// </para>
    /// <para>
    /// 对照：标准档（Counter-Mvc-Standard）View 持具体类 Model、Controller 直调写方法；
    /// 快捷档（Counter-Mvc-Quick）View 兼 Controller 直改 ObservableValue。
    /// </para>
    /// <para>
    /// 数据流：按钮点击 → Controller → ExecuteCommand → Model 写方法 →
    /// ObservableValue 通知 → 面板刷新（原始值 + Query 加工值）。
    /// </para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.MonoView{T}" />
    /// <seealso cref="ISampleMvcStrictCounterController" />
    /// <seealso cref="SampleMvcStrictCounterController" />
    public class SampleMvcStrictCounterMainPanel : MonoView<SampleMvcStrictCounterContext>
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
        /// 经 Context 发布 Command 的控制器（Start 中 new，与 View 分离实例，接口类型存储）。
        /// </summary>
        /// <remarks>
        /// 与 <see cref="_model" /> 的接口存储对称：View 按业务接口持有引用而非具体类，
        /// 经接口仅可触达增 / 减 / 重置 / 查询近似值四项业务操作，
        /// 类型层面拿不到 ExecuteCommand 等框架能力（见 <see cref="ISampleMvcStrictCounterController" />）。
        /// </remarks>
        ISampleMvcStrictCounterController _controller;

        /// <summary>
        /// 当前 Context 中注册的计数器 Model（接口类型存储，非具体类）。
        /// </summary>
        /// <remarks>
        /// <c>GetModel</c> 每次调用执行字典查找 + 初始化检查，故按推荐做法在 Start 中
        /// 获取并缓存为字段，避免 Update 等每帧路径重复查找。
        /// </remarks>
        ISampleMvcStrictCounterModel _model;

        void Start()
        {
            _controller = new SampleMvcStrictCounterController();
            _model = this.GetModel<ISampleMvcStrictCounterModel>();
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
        /// 根据当前计数值更新 UI 文本显示（原始值 + Query 加工值）。
        /// </summary>
        public void UpdateCountText(int count)
        {
            if (countText != null)
            {
                // 原始值：只读订阅直取；加工值：经 Controller 查询 Query（十位四舍五入），原始值不变
                countText.text = $"{count}（≈{_controller.GetRoundedCount()}）";
            }
        }
    }
}
#endif
