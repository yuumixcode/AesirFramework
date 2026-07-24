using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// UI 面板基类。子类覆写生命周期虚方法，通过 Context 访问 Model/Service。
    /// </summary>
    public abstract class AesirBasePanel : AesirMonoBehaviour, IUIPanel
    {
        [Tooltip("面板所在的 UI 层级")]
        [SerializeField]
        UILayer layer = UILayer.Normal;

        [Tooltip("关闭时是否销毁并回收实例（false 则隐藏复用）")]
        [SerializeField]
        bool destroyOnHide = true;

        /// <summary>
        /// 面板所在的 UI 层级。
        /// </summary>
        public UILayer Layer => layer;

        /// <summary>
        /// 关闭时是否销毁并回收实例。
        /// </summary>
        public bool DestroyOnHide => destroyOnHide;

        /// <summary>
        /// 当前是否处于显示状态。由 <see cref="UIModule" /> 驱动。
        /// </summary>
        public bool IsOpen { get; private set; }

        void IUIPanel.Initialize() => OnInit();

        void IUIPanel.Show(object payload)
        {
            IsOpen = true;
            OnShow(payload);
        }

        void IUIPanel.Hide()
        {
            IsOpen = false;
            OnHide();
        }

        void IUIPanel.DestroyPanel()
        {
            if (IsOpen)
            {
                OnHide();
            }

            OnClose();
            Destroy(gameObject);
        }

        /// <summary>
        /// 面板首次实例化后调用一次。子类可覆写进行一次性初始化。
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 每次显示时调用（含首次）。默认实现为 <c>gameObject.SetActive(true)</c>。
        /// </summary>
        /// <param name="payload">外部传入的数据。</param>
        protected virtual void OnShow(object payload) => gameObject.SetActive(true);

        /// <summary>
        /// 面板被隐藏时调用（不销毁实例）。子类可覆写清理显示状态。
        /// </summary>
        protected virtual void OnHide() => gameObject.SetActive(false);

        /// <summary>
        /// 面板即将销毁前调用。子类可覆写释放资源、解绑事件等。
        /// </summary>
        protected virtual void OnClose() { }

        /// <summary>
        /// 便捷关闭自身，等价于 <c>UIModule.Instance.HidePanel(GetType())</c>。
        /// </summary>
        protected void HideSelf() => UIModule.Instance.HidePanel(GetType());
    }
}
