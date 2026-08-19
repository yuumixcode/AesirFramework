using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器被动视图接口。
    /// </summary>
    /// <remarks>
    /// <para><b>不继承 IView</b>：被动视图契约不携带任何 Context 能力。</para>
    /// </remarks>
    /// <seealso cref="SampleMvpStrictCounterMainPanel"/>
    public interface ISampleMvpStrictCounterView
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
