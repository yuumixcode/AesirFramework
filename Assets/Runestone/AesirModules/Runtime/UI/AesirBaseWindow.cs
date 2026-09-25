using Runestone.AesirArchitecture;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Canvas 根 UI 窗口基类。窗口预制体根节点自带 Canvas（独立渲染根），
    /// 由 <see cref="UIModule" /> 实例化后直接挂载到 UIRoot 下，按 <see cref="SortingOrder" /> 排序，
    /// 默认恒在全部面板层之上。子类覆写生命周期虚方法，通过 Context 访问 Model/Service。
    /// </summary>
    /// <remarks>
    /// 预制体内部结构约定：
    /// <code>
    /// XxxWindow（根：Canvas + CanvasScaler + GraphicRaycaster + 窗口脚本）
    /// ├── Mask        蒙版：Image 全屏拉伸（raycastTarget 拦截其下一切 UI 的射线）+ 可选 Button（承接点击）
    /// └── Content     实际 UI 元素容器（框架不触碰，仅作为结构约定）
    /// </code>
    /// 蒙版显隐由 <see cref="UIModule" /> 按 <see cref="UIMaskMode" /> 统一调度；
    /// 蒙版点击经 Button 接线回调 <see cref="OnMaskClicked" />。
    /// </remarks>
    public abstract class AesirBaseWindow : AesirMonoBehaviour, IUIWindow
    {
        internal const string SortingOrderFieldName = nameof(sortingOrder);
        internal const string DestroyOnHideFieldName = nameof(destroyOnHide);
        internal const string CloseOnMaskClickFieldName = nameof(closeOnMaskClick);

        /// <summary>蒙版子物体的约定名（须为根节点的直接子物体）。</summary>
        internal const string MaskChildName = "Mask";

        [Tooltip("窗口根 Canvas 的 sortingOrder。默认 500 恒在面板四层（≤400）之上，多窗口按此值自治排序")]
        [SerializeField]
        int sortingOrder = 500;

        [Tooltip("关闭时是否销毁并回收实例（false 则隐藏复用）")]
        [SerializeField]
        bool destroyOnHide = true;

        [Tooltip("点击蒙版是否关闭本窗口（蒙版子物体须挂 Button）")]
        [SerializeField]
        bool closeOnMaskClick;

        Transform _maskTransform;
        Canvas _canvas;

        void OnDestroy()
        {
            // 实例被销毁（外部 Destroy / 场景卸载 / DestroyWindow）时反向通知 UIModule 清理注册表并重算蒙版
            UIModule.RemoveWindowRecord(this);
        }

        /// <summary>
        /// 窗口根 Canvas 的 sortingOrder。
        /// </summary>
        public int SortingOrder => sortingOrder;

        /// <summary>
        /// 关闭时是否销毁并回收实例。
        /// </summary>
        public bool DestroyOnHide => destroyOnHide;

        /// <summary>
        /// 当前是否处于打开状态。由 <see cref="UIModule" /> 驱动。
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// 窗口根 Canvas（懒缓存）。
        /// </summary>
        public Canvas Canvas => _canvas != null ? _canvas : _canvas = GetComponent<Canvas>();

        void IUIWindow.Initialize()
        {
            // 蒙版接线先于 OnInit：子类初始化时即可安全假设蒙版已就绪
            _maskTransform = transform.Find(MaskChildName);
            if (_maskTransform != null)
            {
                var maskButton = _maskTransform.GetComponent<Button>();
                if (maskButton != null)
                {
                    maskButton.onClick.AddListener(OnMaskButtonClicked);
                }
            }

            OnInit();
        }

        void IUIWindow.Show(object payload)
        {
            IsOpen = true;
            OnShow(payload);
        }

        void IUIWindow.Hide()
        {
            IsOpen = false;
            OnHide();
        }

        void IUIWindow.DestroyWindow()
        {
            if (IsOpen)
            {
                OnHide();
            }

            OnClose();
            Destroy(gameObject);
        }

        void IUIWindow.SetMaskVisible(bool visible)
        {
            if (_maskTransform != null)
            {
                _maskTransform.gameObject.SetActive(visible);
            }
        }

        /// <summary>蒙版 Button 的统一入口，转发到 <see cref="OnMaskClicked" /> 虚方法。</summary>
        void OnMaskButtonClicked() => OnMaskClicked();

        /// <summary>
        /// 蒙版被点击时回调。默认按 <see cref="closeOnMaskClick" /> 决定是否关闭本窗口；
        /// 子类可覆写实现自定义行为（如提示"先完成当前操作"）。
        /// </summary>
        protected virtual void OnMaskClicked()
        {
            if (closeOnMaskClick)
            {
                CloseSelf();
            }
        }

        /// <summary>
        /// 窗口首次实例化后调用一次。子类可覆写进行一次性初始化。
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 每次打开时调用（含首次）。默认实现为 <c>gameObject.SetActive(true)</c>。
        /// </summary>
        /// <param name="payload">外部传入的数据。</param>
        protected virtual void OnShow(object payload) => gameObject.SetActive(true);

        /// <summary>
        /// 窗口被隐藏时调用（默认不销毁实例）。子类可覆写清理显示状态。
        /// </summary>
        protected virtual void OnHide() => gameObject.SetActive(false);

        /// <summary>
        /// 窗口经 <see cref="UIModule.CloseWindow" /> 受控销毁前调用（<see cref="IUIWindow.DestroyOnHide" /> 为 true 的关闭路径）。
        /// 子类可覆写释放资源、解绑事件等。
        /// </summary>
        /// <remarks>
        /// 仅受控销毁路径调用本方法；场景卸载、外部 <c>Destroy</c> 等非受控销毁只触发 <c>OnDestroy</c>。
        /// 因此事件解绑、订阅释放等必须放在 <c>OnDestroy</c> 中（或两处都写），
        /// 仅写在本方法会在场景卸载时泄漏。
        /// </remarks>
        protected virtual void OnClose() { }

        /// <summary>
        /// 便捷关闭自身，等价于 <c>UIModule.Instance.CloseWindow(GetType())</c>。
        /// </summary>
        protected void CloseSelf() => UIModule.Instance.CloseWindow(GetType());
    }
}
