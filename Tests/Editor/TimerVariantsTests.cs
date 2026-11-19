using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证新增三种计时形态（秒表 / 区间 / 频率）各自的核心契约。
    /// </summary>
    /// <remarks>
    /// 与前三个既有形态共用静态注册表，每个测试进出各复位一次，避免用例间共享残留实例。
    /// 直调 <c>AesirTimer.TickOnce</c> 确定性推进，不依赖 PlayerLoop。
    /// </remarks>
    public class TimerVariantsTests
    {
        [SetUp]
        public void SetUp() => AesirTimer.Reset();

        [TearDown]
        public void TearDown() => AesirTimer.Reset();

        #region 秒表 StopwatchTimer

        /// <summary>秒表累计每次结算的时间增量。</summary>
        [Test]
        public void Stopwatch_AccumulatedDelta_ElapsedGrows()
        {
            var stopwatch = AesirTimer.Stopwatch();

            AesirTimer.TickOnce(0.25f, 0.25f);
            AesirTimer.TickOnce(0.5f, 0.5f);

            Assert.AreEqual(0.75f, stopwatch.Elapsed, 0.0001f);
            Assert.AreEqual(0f, stopwatch.Progress, 0.0001f, "秒表没有进度概念");
            Assert.AreEqual(0, stopwatch.RemainingExecutions);
        }

        /// <summary>unscaled 秒表只累计不受缩放的增量。</summary>
        [Test]
        public void Stopwatch_Unscaled_ReadsOnlyUnscaledDelta()
        {
            var stopwatch = AesirTimer.Stopwatch(unscaled: true);

            AesirTimer.TickOnce(0f, 0.4f);

            Assert.AreEqual(0.4f, stopwatch.Elapsed, 0.0001f);
        }

        /// <summary>秒表不会自动结束，取消后才出队。</summary>
        [Test]
        public void Stopwatch_NeverAutoFinishes_StaysUntilCancelled()
        {
            var stopwatch = AesirTimer.Stopwatch();

            AesirTimer.TickOnce(10f, 10f);

            Assert.IsFalse(stopwatch.IsFinished, "秒表没有终点");
            Assert.AreEqual(1, AesirTimer.RunningCount);

            stopwatch.Cancel();
            Assert.IsTrue(stopwatch.IsCancelled);
            Assert.AreEqual(0, AesirTimer.RunningCount);
        }

        #endregion

        #region 区间 IntervalTimer

        /// <summary>区间计时器在总时长内每隔固定间隔回调一次，到点结束。</summary>
        [Test]
        public void Interval_EachInterval_FiresThenFinishesAtTotal()
        {
            var fired = 0;
            var timer = AesirTimer.Interval(1f, 0.5f, () => fired++);

            Assert.AreEqual(2, timer.RemainingExecutions, "1 秒内每 0.5 秒应触发 2 次");

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(1, fired);
            Assert.IsFalse(timer.IsFinished);

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(2, fired);
            Assert.IsTrue(timer.IsFinished);
            Assert.AreEqual(0, AesirTimer.RunningCount);
        }

        /// <summary>末尾不足一个间隔的余量不再触发额外回调。</summary>
        [Test]
        public void Interval_TrailingPartialGap_NoExtraCallback()
        {
            var fired = 0;
            AesirTimer.Interval(1f, 0.4f, () => fired++);

            AesirTimer.TickOnce(0.4f, 0.4f);
            AesirTimer.TickOnce(0.4f, 0.4f);
            Assert.AreEqual(2, fired, "1 秒内每 0.4 秒只应触发 2 次");

            AesirTimer.TickOnce(0.2f, 0.2f);
            Assert.AreEqual(2, fired, "末尾 0.2 秒不足一个间隔，不应再触发");
            Assert.AreEqual(0, AesirTimer.RunningCount, "到点应结束");
        }

        /// <summary>重开区间计时器复位间隔触发点，可再次触发满同一批事件。</summary>
        [Test]
        public void Interval_Restart_ResetsIntervals()
        {
            var fired = 0;
            var timer = AesirTimer.Interval(1f, 0.5f, () => fired++);

            AesirTimer.TickOnce(0.5f, 0.5f);
            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(2, fired);

            timer.Restart();
            Assert.AreEqual(2, timer.RemainingExecutions, "重开后应恢复剩余次数");

            AesirTimer.TickOnce(0.5f, 0.5f);
            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(4, fired);
        }

        #endregion

        #region 频率 FrequencyTimer

        /// <summary>频率计时器按每秒 N 次触发，剩余次数为无限。</summary>
        [Test]
        public void Frequency_TicksPerSecond_FiresAtRate()
        {
            var fired = 0;
            var timer = AesirTimer.Frequency(4, () => fired++);

            Assert.AreEqual(4, timer.TicksPerSecond);
            Assert.AreEqual(-1, timer.RemainingExecutions, "无限重复剩余次数为 -1");

            AesirTimer.TickOnce(0.25f, 0.25f);
            Assert.AreEqual(1, fired);
            Assert.AreEqual(1, AesirTimer.RunningCount);
        }

        /// <summary>一次结算跨越多个周期时只触发一次，不补发。</summary>
        [Test]
        public void Frequency_HitchSpanningTicks_FiresOnceWithoutCatchUp()
        {
            var fired = 0;
            AesirTimer.Frequency(4, () => fired++);

            AesirTimer.TickOnce(0.75f, 0.75f);

            Assert.AreEqual(1, fired, "一帧卡顿跨 3 个周期也只触发一次");
        }

        /// <summary>频率计时器保持无限驻留，取消后出队。</summary>
        [Test]
        public void Frequency_Infinite_StaysUntilCancelled()
        {
            var timer = AesirTimer.Frequency(2, () => { });

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.IsFalse(timer.IsFinished);

            timer.Cancel();
            Assert.IsTrue(timer.IsCancelled);
            Assert.AreEqual(0, AesirTimer.RunningCount);
        }

        #endregion
    }
}
