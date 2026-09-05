using System;

namespace Runestone.AesirArchitecture.Samples.MvpStandard
{
    /// <summary>
    /// MVP-2 标准档示例 —— 计数器被动视图接口。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>不继承 IView</b>：被动视图契约不携带任何 Context 能力——
    ///     从接口层面保证"View 不访问 Model"的 MVP 边界。
    ///     </para>
    ///     <para>
    ///     三档 MVP 的 View 契约完全相同（用户输入事件 + 刷新入口），
    ///     档次差异全部在 Model 暴露面与 Presenter 的读写路径。
    ///     </para>
    /// </remarks>
    /// <seealso cref="SampleMvpStandardCounterMainPanel" />
    public interface ISampleMvpStandardCounterView
    {
        /// <summary>
        /// 用户点击"增加"按钮时触发，由 Presenter 订阅处理。
        /// </summary>
        event Action IncreaseClicked;

        /// <summary>
        /// 用户点击"减少"按钮时触发，由 Presenter 订阅处理。
        /// </summary>
        event Action DecreaseClicked;

        /// <summary>
        /// 用户点击"重置"按钮时触发，由 Presenter 订阅处理。
        /// </summary>
        event Action ResetClicked;

        /// <summary>
        /// 由 Presenter 调用，将最新的计数值推送到 View 刷新显示。
        /// </summary>
        void UpdateCount(int count);
    }
}
