using System;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 频率计时器：每秒触发固定次数（如每秒 5 次），用于以"频率"而非"间隔秒数"表达的节奏。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     与 <see cref="RepeatingTimer" /> 的 1/frequency 等价，但调用方给出的是 int 频率、无需自行做除法；
    ///     <see cref="TicksPerSecond" /> 可直接读回。同样遵循"每结算最多一次回调、卡顿不补发"。
    ///     </para>
    ///     <para>
    ///     无限触发（<see cref="AbstractTimer.RemainingExecutions" /> 为 -1），持有方须在合适时机取消。
    ///     </para>
    /// </remarks>
    public sealed class FrequencyTimer : AesirTimer
    {
        /// <summary>每秒触发的次数（构造时给定）。</summary>
        public int TicksPerSecond { get; }

        /// <summary>创建一个频率计时器并在同一次调用内开始计时。</summary>
        /// <param name="ticksPerSecond">每秒触发次数，必须大于 0。</param>
        /// <param name="onTick">每次触发执行的回调。</param>
        /// <param name="unscaled">是否使用不受 <c>Time.timeScale</c> 影响的时间源。</param>
        internal FrequencyTimer(int ticksPerSecond, Action onTick, bool unscaled)
            : base(1f / Mathf.Max(1, ticksPerSecond), -1, onTick, unscaled)
        {
            TicksPerSecond = Mathf.Max(1, ticksPerSecond);
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
        /// <remarks>无限重复：装填间隔后继续驻留。</remarks>
        protected override bool Rearm()
        {
            RemainingValue = DurationValue;
            return true;
        }
    }
}
