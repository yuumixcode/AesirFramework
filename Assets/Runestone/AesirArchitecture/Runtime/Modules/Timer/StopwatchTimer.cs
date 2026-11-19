using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 秒表：从零开始正计时，用于测量一段时长；不会自动结束，需持有引用读 <see cref="Elapsed" /> 或自行取消。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     与倒计时的根本区别是"没有终点"——因此 <see cref="AbstractTimer.IsFinished" /> 在取消前恒为 false，
    ///     <see cref="Progress" /> 无意义（恒 0），真正的读数是 <see cref="Elapsed" />。
    ///     </para>
    ///     <para>
    ///     设计取舍：原单一 <c>AesirTimer</c> 有意不做秒表（"永不结束 = 泄漏源"）。独立成类型后语义被约束——
    ///     调用方必须持有引用并在合适时机 <see cref="AbstractTimer.Cancel" /> / <see cref="AbstractTimer.Dispose" />；
    ///     只是想读当前时间戳的场合仍应直接用 <c>Time.time</c>，不必创建实例。
    ///     </para>
    /// </remarks>
    public sealed class StopwatchTimer : AesirTimer
    {
        float _elapsed;

        /// <summary>创建一个从零开始的正计时秒表。</summary>
        /// <param name="unscaled">是否使用不受 <c>Time.timeScale</c> 影响的时间源。</param>
        internal StopwatchTimer(bool unscaled)
            : base(0f, -1, null, unscaled)
        {
        }

        /// <summary>自创建（或最近一次 <see cref="AbstractTimer.Restart" />）起累计的秒数。</summary>
        public float Elapsed => _elapsed;

        /// <summary>秒表没有终点，剩余时间展示为已走过的秒数。</summary>
        public override float Remaining => _elapsed;

        /// <summary>秒表没有"已走完比例"，恒为 0。</summary>
        public override float Progress => 0f;

        /// <summary>秒表不触发到期回调。</summary>
        public override int RemainingExecutions => 0;

        /// <inheritdoc />
        /// <remarks>只累计时间、永不到期；取消由调用方显式发起。</remarks>
        protected override bool Advance(float deltaTime, float unscaledDeltaTime)
        {
            _elapsed += SelectDelta(deltaTime, unscaledDeltaTime);
            return false;
        }

        /// <inheritdoc />
        protected override bool Rearm() => false;

        /// <inheritdoc />
        /// <remarks>重开同时清零累计值。</remarks>
        public override void Restart()
        {
            base.Restart();
            _elapsed = 0f;
        }
    }
}
