using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 下一帧执行一次的计时器：跳过时间累减，按结算周期号判定。对应工厂 <see cref="AesirTimer.NextFrame" />。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     语义是"此后第一次结算"，不是"当前帧剩下的某一次结算"：创建发生在一次结算之后（如 Update、
    ///     协程、另一个计时器回调）时落在下一帧；创建发生在当帧结算之前（如 AfterAssembliesLoaded）时落在当帧。
    ///     </para>
    ///     <para>
    ///     "下一帧"按结算周期计数，与 <c>Time.timeScale</c> 无关，因此不接受 unscaled 参数。
    ///     </para>
    /// </remarks>
    public sealed class NextFrameTimer : AesirTimer
    {
        /// <summary>创建一个"下一帧执行一次"的计时器。</summary>
        /// <param name="onFinished">下一帧结算时执行的回调。</param>
        internal NextFrameTimer(Action onFinished) : base(0f, 1, onFinished, false) { }

        /// <summary>调试窗口展示为"下一帧"形态。</summary>
        internal override bool IsNextFrameMode => true;

        /// <inheritdoc />
        /// <remarks>落在"创建（或恢复）之后开始的第一次结算"上。</remarks>
        protected override bool Advance(float deltaTime, float unscaledDeltaTime) =>
            CurrentPassId > CreatedPassId;

        /// <inheritdoc />
        protected override bool Rearm() => RescheduleOrFinish();
    }
}
