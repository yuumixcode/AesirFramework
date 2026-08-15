using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器被动视图接口。Presenter 通过该接口驱动 View，View 不直接访问 Model。
    /// </summary>
    public interface ISampleMvpCounterView : IView
    {
        /// <summary>
        /// 点击“增加”时触发
        /// </summary>
        Action IncreaseClicked { get; set; }

        /// <summary>
        /// 点击“减少”时触发
        /// </summary>
        Action DecreaseClicked { get; set; }

        /// <summary>
        /// 点击“重置”时触发
        /// </summary>
        Action ResetClicked { get; set; }

        /// <summary>
        /// 由 Presenter 调用，刷新计数显示
        /// </summary>
        void UpdateCount(int count);
    }
}
