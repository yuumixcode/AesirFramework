using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirArchitecture.Tests
{
    /// <summary>
    /// 验证 <see cref="AesirTimer" /> 在真实 PlayerLoop 驱动下的跨帧行为。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     这些行为只能在 PlayMode 下验证：计时推进依赖注入到 <c>PlayerLoop.Update</c> 的结算钩子，
    ///     EditMode 没有帧循环（确定性结算语义见 EditMode 的 <c>AesirTimerTests</c>）。
    ///     </para>
    ///     <para>
    ///     <b>等待按真实时间截止，不按帧数</b>：batchmode 无 vsync 时帧率可达数千 fps，
    ///     "300 帧"换算成真实时间可能不足 0.1 秒，帧预算会把正确的实现判成失败。
    ///     用 <c>Time.realtimeSinceStartup</c> 的 5 秒截止既与帧率解耦，也仍然是有界等待。
    ///     </para>
    /// </remarks>
    /// <seealso cref="AesirTimer" />
    public class AesirTimerTests
    {
        /// <summary>
        /// 单用例最多等待的真实时间（秒）。远大于用例所需（计时器时长 0.1 秒）。
        /// </summary>
        const float WaitTimeoutSeconds = 5f;

        float _originalTimeScale;

        [SetUp]
        public void SetUp()
        {
            _originalTimeScale = Time.timeScale;

            // 清理上一用例可能残留的实例，并让钩子在本用例首次使用时重新注册
            AesirTimer.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _originalTimeScale;
            AesirTimer.Reset();
        }

        /// <summary>
        /// 倒计时在跨帧推进后回调一次，并且不会在后续帧重复回调。
        /// </summary>
        [UnityTest]
        public IEnumerator Countdown_RealPlayerLoop_InvokedOnceAcrossFrames()
        {
            var fired = 0;
            AesirTimer.Countdown(0.1f, () => fired++);

            var deadline = Time.realtimeSinceStartup + WaitTimeoutSeconds;
            while (fired == 0 && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.AreEqual(1, fired, "倒计时应在帧预算内到期一次");
            Assert.AreEqual(0, AesirTimer.RunningCount, "到期后应自动出队");

            for (var i = 0; i < 5; i++)
            {
                yield return null;
            }

            Assert.AreEqual(1, fired, "出队后不应再次回调");
        }

        /// <summary>
        /// 在某一帧内创建的下一帧计时器，正好在下一帧执行。
        /// </summary>
        [UnityTest]
        public IEnumerator NextFrame_CreatedDuringFrame_InvokedOnFollowingFrame()
        {
            var createdFrame = Time.frameCount;
            var firedFrame = -1;

            AesirTimer.NextFrame(() => firedFrame = Time.frameCount);

            var deadline = Time.realtimeSinceStartup + WaitTimeoutSeconds;
            while (firedFrame < 0 && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.GreaterOrEqual(firedFrame, 0, "下一帧计时器应在帧预算内执行");
            Assert.AreEqual(createdFrame + 1, firedFrame, "应恰好落在创建帧的下一帧");
        }

        /// <summary>
        /// <c>Time.timeScale = 0</c> 时，受缩放的计时器冻结，不受缩放的计时器照常走完。
        /// </summary>
        [UnityTest]
        public IEnumerator Countdown_TimeScaleZero_OnlyUnscaledTimerProceeds()
        {
            Time.timeScale = 0f;

            var scaledFired = 0;
            var unscaledFired = 0;

            AesirTimer.Countdown(0.1f, () => scaledFired++);
            AesirTimer.Countdown(0.1f, () => unscaledFired++, true);

            var deadline = Time.realtimeSinceStartup + WaitTimeoutSeconds;
            while (unscaledFired == 0 && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.AreEqual(1, unscaledFired, "不受缩放的计时器在暂停时仍应走完");
            Assert.AreEqual(0, scaledFired, "受缩放的计时器在 timeScale 为 0 时应冻结");
        }
    }
}
