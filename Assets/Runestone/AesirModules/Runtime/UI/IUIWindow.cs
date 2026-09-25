namespace Runestone.AesirModules
{
    /// <summary>
    /// Canvas 根 UI 窗口契约。生命周期：Initialize → Show → Hide → DestroyWindow，与 <see cref="IUIPanel" /> 平行。
    /// <para>
    /// 窗口预制体根节点自带 <see cref="UnityEngine.Canvas" />（独立渲染根），
    /// 由 <see cref="UIModule" /> 实例化后直接挂载到 UIRoot 下（不经四层 Canvas），
    /// 排序按 <see cref="SortingOrder" /> 自治（默认基准 500，恒在全部面板层之上）。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 窗口与面板是两种并列的 UI 形态（选型对比见包内 Documentation/ui-module.md）；
    /// 一个类型不应同时实现 <see cref="IUIPanel" /> 与本接口。
    /// </remarks>
    public interface IUIWindow
    {
        /// <summary>
        /// 窗口根 Canvas 的 sortingOrder。默认基准 500，恒在面板四层（100–400）之上；
        /// 多窗口按此值自治排序，同值时以打开顺序（sibling 位置居后）为高。
        /// </summary>
        int SortingOrder { get; }

        /// <summary>
        /// 关闭时是否销毁并回收实例。<c>false</c> 则仅隐藏，再次打开时复用。
        /// </summary>
        bool DestroyOnHide { get; }

        /// <summary>
        /// 当前是否处于打开状态。
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 首次创建后由 <see cref="UIModule" /> 调用一次。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 打开窗口，可接收外部数据。
        /// </summary>
        /// <param name="payload">外部传入的数据。</param>
        void Show(object payload = null);

        /// <summary>
        /// 隐藏窗口（不销毁实例）。
        /// </summary>
        void Hide();

        /// <summary>
        /// 销毁窗口实例，释放资源。
        /// </summary>
        void DestroyWindow();

        /// <summary>
        /// 设置蒙版子物体（约定名 <c>Mask</c>）的显隐，由 <see cref="UIModule" /> 按单遮/叠遮模式统一调度。
        /// 预制体无 <c>Mask</c> 子物体时为无操作。
        /// </summary>
        /// <param name="visible">蒙版是否可见（可见时拦截其下一切 UI 的射线）。</param>
        void SetMaskVisible(bool visible);
    }
}
