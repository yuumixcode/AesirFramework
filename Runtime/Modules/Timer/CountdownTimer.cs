using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 倒计时：从给定时长倒数到零，到期回调一次后结束。对应工厂 <see cref="AesirTimer.Countdown" />。
    /// </summary>
    /// <remarks>
    /// 零或负时长在下一次结算即到期（<see cref="AbstractTimer.Progress" /> 到期前为 0、到期后为 1，不产生 NaN）。
    /// 到期后可 <see cref="AbstractTimer.Restart" /> 重新使用同一个引用。
    /// </remarks>
    public sealed class CountdownTimer : AesirTimer
    {
        /// <summary>创建一个倒计时并在同一次调用内开始计时。</summary>
        /// <param name="seconds">倒计时时长（秒）。零或负值在下一次结算到期。</param>
        /// <param name="onFinished">到期回调，可为 null。</param>
        /// <param name="unscaled">是否使用不受 <c>Time.timeScale</c> 影响的时间源。</param>
        internal CountdownTimer(float seconds, Action onFinished, bool unscaled) : base(seconds, 1,
            onFinished, unscaled) { }

        /// <inheritdoc />
        protected override bool Advance(float deltaTime, float unscaledDeltaTime)
        {
            RemainingValue -= SelectDelta(deltaTime, unscaledDeltaTime);

            if (RemainingValue > 0f)
            {
                return false;
            }

            RemainingValue = 0f;
            return true;
        }

        /// <inheritdoc />
        protected override bool Rearm() => RescheduleOrFinish();
    }
}
