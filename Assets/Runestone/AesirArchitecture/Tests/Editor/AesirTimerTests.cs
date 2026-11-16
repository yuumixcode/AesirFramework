using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirTimer" /> 的结算语义：到期判定、执行顺序、重入、取消与重新入队。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     测试直调 <c>AesirTimer.TickOnce</c> 模拟一次结算，不依赖 PlayerLoop，
    ///     因此可以在 EditMode 下确定性地推进时间（真实帧驱动见 PlayMode 的 <c>AesirTimerTests</c>）。
    ///     </para>
    ///     <para>
    ///     每个测试在进出时各复位一次静态注册表，避免用例间共享残留实例；
    ///     <c>Reset</c> 同时退订结算钩子，防止钩子跨用例叠加导致重复结算。
    ///     </para>
    /// </remarks>
    /// <seealso cref="AesirTimer" />
    public class AesirTimerTests
    {
        [SetUp]
        public void SetUp() => AesirTimer.Reset();

        [TearDown]
        public void TearDown() => AesirTimer.Reset();

        /// <summary>
        /// 累加到时长时回调一次并自动出队，之后不再重复回调。
        /// </summary>
        [Test]
        public void Countdown_AccumulatedDelta_InvokesOnceThenLeavesRegistry()
        {
            var fired = 0;
            AesirTimer.Countdown(0.5f, () => fired++);

            AesirTimer.TickOnce(0.25f, 0.25f);
            Assert.AreEqual(0, fired, "未走满时长不应回调");
            Assert.AreEqual(1, AesirTimer.RunningCount);

            AesirTimer.TickOnce(0.25f, 0.25f);
            Assert.AreEqual(1, fired);
            Assert.AreEqual(0, AesirTimer.RunningCount, "到期后应自动出队");

            AesirTimer.TickOnce(0.25f, 0.25f);
            Assert.AreEqual(1, fired, "出队后不应再次回调");
        }

        /// <summary>
        /// 同一次结算中多个计时器到期时，严格按创建顺序回调。
        /// </summary>
        /// <remarks>
        /// 就地压实若打乱顺序，冷却与提示的先后关系会随实例地址变化而抖动。
        /// </remarks>
        [Test]
        public void TickOnce_MultipleDueInSamePass_InvokedInCreationOrder()
        {
            var order = new List<int>();

            for (var i = 0; i < 4; i++)
            {
                var index = i;
                AesirTimer.Countdown(0f, () => order.Add(index));
            }

            AesirTimer.TickOnce(0.016f, 0.016f);

            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, order);
        }

        /// <summary>
        /// 到期回调中新建的计时器不会在本次结算中被推进。
        /// </summary>
        /// <remarks>
        /// 结算与执行两阶段分离的直接后果：新实例落到注册表尾部，本次遍历已越过它，
        /// 因此回调内新建计时器无需挂起队列，也不会出现"同一帧内套娃执行"。
        /// </remarks>
        [Test]
        public void TickOnce_CallbackStartsTimer_NewInstanceSurvivesToNextPass()
        {
            var outerFired = 0;
            var innerFired = 0;

            AesirTimer.Countdown(0.5f, () =>
            {
                outerFired++;
                AesirTimer.Countdown(0.5f, () => innerFired++);
            });

            AesirTimer.TickOnce(0.5f, 0.5f);

            Assert.AreEqual(1, outerFired);
            Assert.AreEqual(0, innerFired, "回调内新建的计时器不应在同一帧被推进");
            Assert.AreEqual(1, AesirTimer.RunningCount);

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(1, innerFired);
        }

        /// <summary>
        /// 未到期时取消：不回调、出队，且 <c>IsFinished</c> 与 <c>IsCancelled</c> 互斥。
        /// </summary>
        [Test]
        public void Cancel_BeforeDue_NoCallbackAndFlagsExclusive()
        {
            var fired = 0;
            var timer = AesirTimer.Countdown(1f, () => fired++);

            timer.Cancel();
            timer.Cancel();

            Assert.IsTrue(timer.IsCancelled);
            Assert.IsFalse(timer.IsFinished);
            Assert.AreEqual(0, AesirTimer.RunningCount);

            AesirTimer.TickOnce(2f, 2f);
            Assert.AreEqual(0, fired, "取消后不应触发到期回调");
        }

        /// <summary>
        /// 零时长计时器在下一次结算到期，到期前进度为 0、到期后为 1（不产生 NaN）。
        /// </summary>
        [Test]
        public void Countdown_ZeroDuration_DueOnNextPassWithProgressBounds()
        {
            var timer = AesirTimer.Countdown(0f);

            Assert.AreEqual(0f, timer.Progress, 0.0001f);

            AesirTimer.TickOnce(0.016f, 0.016f);

            Assert.IsTrue(timer.IsFinished);
            Assert.AreEqual(1f, timer.Progress, 0.0001f);
        }

        /// <summary>
        /// 到期后 <c>Restart</c> 重新入队并可再次回调，重复调用不会重复入队。
        /// </summary>
        /// <remarks>
        /// 对应"重复注册导致双倍速度"这一类缺陷：入队状态以单一标志为准，重复调用必须幂等。
        /// </remarks>
        [Test]
        public void Restart_AfterFinished_InvokesAgainWithoutDuplicateRegistration()
        {
            var fired = 0;
            var timer = AesirTimer.Countdown(0.5f, () => fired++);

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(1, fired);

            timer.Restart();
            timer.Restart();
            Assert.AreEqual(1, AesirTimer.RunningCount, "重复 Restart 不应重复入队");
            Assert.IsFalse(timer.IsFinished);

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(2, fired);
        }

        /// <summary>
        /// 一次结算之后创建的下一帧计时器，在随后的第一次结算执行。
        /// </summary>
        [Test]
        public void NextFrame_CreatedAfterPass_InvokedOnFollowingPass()
        {
            AesirTimer.TickOnce(0.016f, 0.016f);

            var fired = 0;
            AesirTimer.NextFrame(() => fired++);

            Assert.AreEqual(0, fired, "创建时不应立即执行");
            Assert.AreEqual(1, AesirTimer.RunningCount);

            AesirTimer.TickOnce(0.016f, 0.016f);
            Assert.AreEqual(1, fired);
            Assert.AreEqual(0, AesirTimer.RunningCount);
        }

        /// <summary>
        /// 在到期回调内创建的下一帧计时器，不会落在本次结算中。
        /// </summary>
        /// <remarks>
        /// 这是"下一帧执行"与"当帧执行"的边界用例：回调执行阶段已经不属于本次结算的推进范围。
        /// </remarks>
        [Test]
        public void NextFrame_CreatedInsideCallback_InvokedOnFollowingPass()
        {
            var nextFrameFired = 0;

            AesirTimer.Countdown(0.5f, () => AesirTimer.NextFrame(() => nextFrameFired++));

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(0, nextFrameFired, "回调内创建的下一帧计时器不应在本次结算执行");

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(1, nextFrameFired);
        }

        /// <summary>
        /// <c>unscaled</c> 计时器只读取不受缩放的增量时间。
        /// </summary>
        [Test]
        public void Countdown_Unscaled_ReadsOnlyUnscaledDelta()
        {
            var scaledFired = 0;
            var unscaledFired = 0;

            AesirTimer.Countdown(0.5f, () => scaledFired++);
            AesirTimer.Countdown(0.5f, () => unscaledFired++, unscaled: true);

            AesirTimer.TickOnce(0f, 0.5f);

            Assert.AreEqual(0, scaledFired, "受缩放的计时器在缩放增量为 0 时应冻结");
            Assert.AreEqual(1, unscaledFired, "不受缩放的计时器应按 unscaled 增量走完");
        }

        /// <summary>
        /// 无限重复：每走满一个间隔回调一次，且始终留在注册表中，剩余次数为 -1。
        /// </summary>
        [Test]
        public void Every_InfiniteRepeat_InvokesEachIntervalAndStaysRegistered()
        {
            var fired = 0;
            var timer = AesirTimer.Every(0.5f, () => fired++);

            Assert.AreEqual(-1, timer.RemainingExecutions, "无限重复的剩余次数恒为 -1");

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(1, fired);
            Assert.AreEqual(1, AesirTimer.RunningCount, "无限重复不应出队");

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(2, fired);
            Assert.IsFalse(timer.IsFinished);
        }

        /// <summary>
        /// 有限重复：次数用尽后自动出队并转为到期，之后不再回调。
        /// </summary>
        [Test]
        public void Every_FiniteRepeat_DrainsThenLeavesRegistry()
        {
            var fired = 0;
            var timer = AesirTimer.Every(0.5f, () => fired++, repeatCount: 2);

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(1, fired);
            Assert.AreEqual(1, timer.RemainingExecutions);

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(2, fired);
            Assert.IsTrue(timer.IsFinished);
            Assert.AreEqual(0, AesirTimer.RunningCount, "次数用尽后应自动出队");

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(2, fired, "出队后不应再次回调");
        }

        /// <summary>
        /// 一次结算跨越多个间隔时只回调一次：卡顿帧不补发，避免"越慢越忙"的回调雪崩。
        /// </summary>
        [Test]
        public void Every_HitchSpanningMultipleIntervals_InvokesOnceWithoutCatchUp()
        {
            var fired = 0;
            AesirTimer.Every(0.1f, () => fired++);

            AesirTimer.TickOnce(0.35f, 0.35f);

            Assert.AreEqual(1, fired, "一次结算最多执行一次，跨过的间隔不补发");
        }

        /// <summary>
        /// 暂停：剩余时间冻结、回调停止，但实例仍在注册表中；恢复后按冻结的剩余时间继续。
        /// </summary>
        [Test]
        public void Pause_FreezesRemainingAndKeepsRegistration()
        {
            var fired = 0;
            var timer = AesirTimer.Every(1f, () => fired++);

            AesirTimer.TickOnce(0.4f, 0.4f);
            var remaining = timer.Remaining;

            timer.Pause();
            AesirTimer.TickOnce(2f, 2f);

            Assert.IsTrue(timer.IsPaused);
            Assert.AreEqual(remaining, timer.Remaining, 0.0001f, "暂停期间剩余时间不应变化");
            Assert.AreEqual(0, fired, "暂停期间不应回调");
            Assert.AreEqual(1, AesirTimer.RunningCount, "暂停的实例仍留在注册表（可被调试面板看到）");

            timer.Resume();
            AesirTimer.TickOnce(0.6f, 0.6f);

            Assert.AreEqual(1, fired, "恢复后应从暂停时刻的剩余时间继续走完");
        }

        /// <summary>
        /// 暂停的下一帧计时器在恢复后的第一次结算执行，而不是在暂停期间就被判到期。
        /// </summary>
        /// <remarks>
        /// "下一帧"以结算周期号判定；若恢复时不对齐周期号，暂停期间前进的周期号会让它
        /// 在恢复瞬间立刻到期，表现为"恢复即执行"。
        /// </remarks>
        [Test]
        public void Pause_NextFrame_InvokesOnFirstPassAfterResume()
        {
            AesirTimer.TickOnce(0.016f, 0.016f);

            var fired = 0;
            var timer = AesirTimer.NextFrame(() => fired++);

            timer.Pause();
            AesirTimer.TickOnce(0.016f, 0.016f);
            AesirTimer.TickOnce(0.016f, 0.016f);

            Assert.AreEqual(0, fired, "暂停期间不应执行");
            Assert.AreEqual(1, AesirTimer.RunningCount);

            timer.Resume();
            AesirTimer.TickOnce(0.016f, 0.016f);

            Assert.AreEqual(1, fired, "恢复后的第一次结算执行");
        }

        /// <summary>
        /// 暂停中的有限重复计时器经 <c>Restart</c> 复原：执行计数与暂停态一并复位。
        /// </summary>
        [Test]
        public void Restart_PausedRepeatingTimer_ResetsCountersAndResumes()
        {
            var fired = 0;
            var timer = AesirTimer.Every(0.5f, () => fired++, repeatCount: 2);

            AesirTimer.TickOnce(0.5f, 0.5f);
            Assert.AreEqual(1, fired);

            timer.Pause();
            timer.Restart();

            Assert.IsFalse(timer.IsPaused, "Restart 不应保留暂停态");
            Assert.AreEqual(2, timer.RemainingExecutions, "Restart 应重置执行计数");

            AesirTimer.TickOnce(0.5f, 0.5f);
            AesirTimer.TickOnce(0.5f, 0.5f);

            Assert.AreEqual(3, fired, "重新开始后应再执行满两次");
            Assert.IsTrue(timer.IsFinished);
        }
    }
}
