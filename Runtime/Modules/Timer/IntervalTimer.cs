using System;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 区间计时器：在一段总时长内每隔固定间隔触发一次事件，总时长走完即结束。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     与 <see cref="RepeatingTimer" /> 的区别：<see cref="RepeatingTimer" /> 以"次数"为终点（重复 N 次或无限），
    ///     本类以"总时长"为终点（如 10 秒内每 1 秒触发一次）。两者都遵循"每结算最多一次回调、卡顿不补发"。
    ///     </para>
    ///     <para>
    ///     首个事件在第 <c>interval</c> 秒触发，而非创建当帧；若 <c>interval</c> 不能整除总时长，
    ///     最后一个完整间隔之后的余量不再触发事件，到点直接结束。
    ///     </para>
    /// </remarks>
    public sealed class IntervalTimer : AesirTimer
    {
        /// <summary>
        /// 判定"总时长耗尽"的容差。逐帧累减会留下极小正残差（如 0.2 - 0.2 的浮点误差），
        /// 不按耗尽处理会让区间计时器在到点后永远驻留在注册表。
        /// </summary>
        const float EndEpsilon = 0.0001f;

        readonly float _interval;
        float _nextIntervalAt;

        /// <summary>创建一个区间计时器并在同一次调用内开始计时。</summary>
        /// <param name="totalSeconds">总时长（秒），走完即结束。</param>
        /// <param name="intervalSeconds">触发间隔（秒），必须大于 0。</param>
        /// <param name="onInterval">每次间隔到点时执行的回调。</param>
        /// <param name="unscaled">是否使用不受 <c>Time.timeScale</c> 影响的时间源。</param>
        internal IntervalTimer(float totalSeconds, float intervalSeconds, Action onInterval, bool unscaled) :
            base(totalSeconds, -1, onInterval, unscaled)
        {
            _interval = Mathf.Max(0f, intervalSeconds);
            _nextIntervalAt = _interval > 0f ? totalSeconds - _interval : -1f;
        }

        /// <summary>还会触发几次间隔事件（0 表示间隔非法或已结束，未结束时为正数）。</summary>
        public override int RemainingExecutions
        {
            get
            {
                if (IsFinished || IsCancelled || _nextIntervalAt < 0f || _interval <= 0f)
                {
                    return 0;
                }

                return Mathf.FloorToInt(_nextIntervalAt / _interval) + 1;
            }
        }

        /// <inheritdoc />
        protected override bool Advance(float deltaTime, float unscaledDeltaTime)
        {
            RemainingValue -= SelectDelta(deltaTime, unscaledDeltaTime);

            var exhausted = RemainingValue <= EndEpsilon;

            if (exhausted)
            {
                RemainingValue = 0f;
            }

            var intervalDue = _nextIntervalAt >= 0f && RemainingValue <= _nextIntervalAt;

            // 总时长耗尽且这一帧没有间隔事件：直接结束，无回调（引擎见 _finished 即出队）
            if (exhausted && !intervalDue)
            {
                Finish();
            }

            if (intervalDue)
            {
                _nextIntervalAt -= _interval;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        /// <remarks>总时长耗尽则结束（最后一次间隔回调恰与到点同帧时在此收尾），否则继续驻留。</remarks>
        protected override bool Rearm()
        {
            if (RemainingValue <= 0f)
            {
                Finish();
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        /// <remarks>重开同时复位下一个间隔触发点。</remarks>
        public override void Restart()
        {
            base.Restart();
            _nextIntervalAt = _interval > 0f ? DurationValue - _interval : -1f;
        }
    }
}
