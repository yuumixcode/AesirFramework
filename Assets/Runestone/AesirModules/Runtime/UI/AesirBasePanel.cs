using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// UI 面板基类。子类覆写生命周期虚方法，通过 Context 访问 Model/Service。
    /// </summary>
    public abstract class AesirBasePanel : AesirMonoBehaviour, IUIPanel
    {
        internal const string LayerFieldName = nameof(layer);
        internal const string DestroyOnHideFieldName = nameof(destroyOnHide);

        [Tooltip("面板所在的 UI 层级")]
        [SerializeField]
        UILayer layer = UILayer.Normal;

        [Tooltip("隐藏时是否销毁并回收实例（false 则隐藏复用）")]
        [SerializeField]
        bool destroyOnHide = true;

        /// <summary>
        /// 实例被销毁时的反清理回调（向 <see cref="UIModule" /> 注销注册表条目）。
        /// </summary>
        /// <remarks>
        /// 子类覆写本方法时必须调用 <c>base.OnDestroy()</c>：Unity 的消息派发只调用最派生类型的声明，
        /// 不调用 <c>base</c> 会使注册表残留已销毁实例。注册表另有自愈兜底
        /// （命中已销毁条目时按未注册处理并重建），但仍应保留 base 调用以维持时序与开销。
        /// </remarks>
        protected virtual void OnDestroy()
        {
            // 实例被销毁（外部 Destroy / 场景卸载 / DestroyPanel）时反向通知 UIModule 清理注册表，
            // 避免字典残留已销毁实例，导致后续 ShowPanel 命中无效条目
            UIModule.RemovePanelRecord(this);
        }

        /// <summary>
        /// 面板所在的 UI 层级。
        /// </summary>
        public UILayer Layer => layer;

        /// <summary>
        /// 隐藏时是否销毁并回收实例。
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
        /// 面板被隐藏时调用（默认不销毁实例）。子类可覆写清理显示状态。
        /// </summary>
        protected virtual void OnHide() => gameObject.SetActive(false);

        /// <summary>
        /// 面板经 <see cref="UIModule.HidePanel" /> 受控销毁前调用（<see cref="IUIPanel.DestroyOnHide" /> 为 true 的关闭路径）。
        /// 子类可覆写释放资源、解绑事件等。
        /// </summary>
        /// <remarks>
        /// 仅受控销毁路径调用本方法；场景卸载、外部 <c>Destroy</c> 等非受控销毁只触发 <c>OnDestroy</c>。
        /// 因此事件解绑、订阅释放等必须放在 <c>OnDestroy</c> 中（或两处都写），
        /// 仅写在本方法会在场景卸载时泄漏。
        /// </remarks>
        protected virtual void OnClose() { }

        /// <summary>
        /// 便捷关闭自身，等价于 <c>UIModule.Instance.HidePanel(GetType())</c>。
        /// <para>
        /// 走非创建式获取：面板销毁/场景卸载阶段 UIModule 可能已随之消失，此时"关闭自己"无对象可关，
        /// 静默返回即可——若走 <see cref="UIModule.Instance" /> 会重建 DDOL 宿主（详见
        /// <see cref="UIModule.TryGetExisting" /> 的说明）。
        /// </para>
        /// </summary>
        protected void HideSelf()
        {
            if (!UIModule.TryGetExisting(out var module))
            {
                return;
            }

            module.HidePanel(GetType());
        }
    }
}
