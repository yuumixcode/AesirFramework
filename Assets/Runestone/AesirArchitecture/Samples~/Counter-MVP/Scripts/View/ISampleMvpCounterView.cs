using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器被动视图接口。
    /// </summary>
    /// <remarks>
    /// 在 MVP 模式中，View 被设计为"被动视图"（Passive View）：
    /// 它不包含任何业务逻辑，不直接访问 Model，
    /// 仅通过事件向 Presenter 报告用户输入，并暴露方法供 Presenter 驱动刷新。
    /// 这种设计使 View 可被完全替换（如从 UGUI 切换到 UI Toolkit）而不影响 Presenter 和 Model。
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.IView"/>
    /// <seealso cref="SampleMvpCounterMainPanel"/>
    /// <seealso cref="SampleMvpCounterPresenter"/>
    public interface ISampleMvpCounterView : IView
    {
        /// <summary>
        /// 用户点击"增加"按钮时触发的事件回调。
        /// </summary>
        /// <remarks>
        /// View 将用户输入转发为事件，由 Presenter 订阅并处理，
        /// View 本身不关心点击后会发生什么。
        /// </remarks>
        Action IncreaseClicked { get; set; }

        /// <summary>
        /// 用户点击"减少"按钮时触发的事件回调。
        /// </summary>
        /// <remarks>
        /// View 将用户输入转发为事件，由 Presenter 订阅并处理，
        /// View 本身不关心点击后会发生什么。
        /// </remarks>
        Action DecreaseClicked { get; set; }

        /// <summary>
        /// 用户点击"重置"按钮时触发的事件回调。
        /// </summary>
        /// <remarks>
        /// View 将用户输入转发为事件，由 Presenter 订阅并处理，
        /// View 本身不关心点击后会发生什么。
        /// </remarks>
        Action ResetClicked { get; set; }

        /// <summary>
        /// 由 Presenter 调用，将最新的计数值推送到 View 刷新显示。
        /// </summary>
        /// <param name="count">最新的计数值。</param>
        /// <remarks>
        /// 这是 Presenter 驱动 View 更新的唯一入口，
        /// 体现了"被动视图"的核心原则——View 不主动拉取数据，只接受 Presenter 的推送。
        /// </remarks>
        void UpdateCount(int count);
    }
}
