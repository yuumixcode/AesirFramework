namespace Runestone.AesirModules
{
    /// <summary>
    /// UI 面板契约。生命周期：Initialize → Show → Hide → DestroyPanel。
    /// </summary>
    public interface IUIPanel
    {
        /// <summary>
        /// 面板所在的 UI 层级。
        /// </summary>
        UILayer Layer { get; }

        /// <summary>
        /// 关闭时是否销毁并回收实例。<c>false</c> 则仅隐藏，再次打开时复用。
        /// </summary>
        bool DestroyOnHide { get; }

        /// <summary>
        /// 当前是否处于显示状态。
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 首次创建后由 <see cref="UIModule" /> 调用一次。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 显示面板，可接收外部数据。
        /// </summary>
        /// <param name="payload">外部传入的数据。</param>
        void Show(object payload = null);

        /// <summary>
        /// 隐藏面板（不销毁实例）。
        /// </summary>
        void Hide();

        /// <summary>
        /// 销毁面板实例，释放资源。
        /// </summary>
        void DestroyPanel();
    }
}
