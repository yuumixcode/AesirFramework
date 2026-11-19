using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 按固定间隔重复触发的计时器：每走满一个间隔回调一次，可限次或无限。对应工厂 <see cref="AesirTimer.Every" />。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     首次回调在间隔之后，不在创建当帧。每次结算最多回调一次：一帧卡顿跨越多个间隔时不补发，
    ///     装填用 <c>= interval</c> 而非 <c>+=</c>，避免卡顿后的回调雪崩。
    ///     </para>
    ///     <para>
    ///     无限形态（<see cref="AbstractTimer.RemainingExecutions" /> 为 -1）持有方必须在合适时机取消，
    ///     否则等于把计时器当成每帧逻辑用——那种需求请用 <see cref="MonoLifecycleProxy" />。
    ///     </para>
    /// </remarks>
    public sealed class RepeatingTimer : AesirTimer
    {
        /// <summary>创建一个重复计时器并在同一次调用内开始计时。</summary>
        /// <param name="interval">两次回调之间的间隔（秒）。零或负值表示每次结算都回调。</param>
        /// <param name="onTick">每次到期执行的回调。</param>
        /// <param name="repeatCount">最多执行次数，负数表示无限。</param>
        /// <param name="unscaled">是否使用不受 <c>Time.timeScale</c> 影响的时间源。</param>
        internal RepeatingTimer(float interval, Action onTick, int repeatCount, bool unscaled)
            : base(interval, repeatCount, onTick, unscaled)
        {
        }

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
        /// <remarks>重新装填间隔用 <c>= duration</c>：结算已把到期实例的剩余置 0，这里直接复位。</remarks>
        protected override bool Rearm()
        {
            if (!RescheduleOrFinish())
            {
                return false;
            }

            RemainingValue = DurationValue;
            return true;
        }
    }
}
