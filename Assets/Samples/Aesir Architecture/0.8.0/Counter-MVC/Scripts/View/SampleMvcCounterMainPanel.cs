using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 示例计数器主面板视图。
    /// <para>通过 MonoView 获取上下文，并在 Awake 中初始化 Model 监听和 Controller 实例。</para>
    /// </summary>
    public class SampleMvcCounterMainPanel : MonoView<SampleMvcCounterContext>
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

        ISampleMvcCounterController _ctrl;

        /// <summary>
        /// 每次访问都从 Context 获取当前 Model，而非缓存字段引用。
        /// <para>这样在运行时通过 RegisterModel 动态替换 Model 后，始终能拿到最新实例；</para>
        /// <para>旧实例在无人持有后可被 GC 正常回收，支持运行时热替换（如切换为继承 MonoBehaviour 的可视化 Model）。</para>
        /// </summary>
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
        /// 根据当前计数值更新 UI 文本显示
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
