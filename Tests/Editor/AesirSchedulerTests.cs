using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// <see cref="AesirScheduler" /> 时间调度原语的 EditMode 测试。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     结算逻辑经 internal 的 <c>Tick(currentTime, currentFrame)</c> 以模拟时间/帧号驱动，
    ///     不依赖引擎真实走帧（EditMode 下 PlayerLoop 不运行）——帧粒度语义（同帧任务最早下一帧、
    ///     到期按帧结算）用显式帧号断言；钩子注册行为经 <see cref="AesirArchitecturePlayerLoop.GetHookCount" /> 验证。
    ///     </para>
    ///     <para>
    ///     <see cref="SetUp" /> / <see cref="TearDown" /> 经反射调用调度器私有 RIOLM 重置并重置 PlayerLoop 钩子表，
    ///     保证测试间与同域重复运行间的静态隔离。
    ///     </para>
    /// </remarks>
    /// <seealso cref="AesirScheduler" />
    /// <seealso cref="AesirArchitecturePlayerLoop" />
    public class AesirSchedulerTests
    {
        [SetUp]
        public void SetUp()
        {
            ResetSchedulerStatics();
        }

        [TearDown]
        public void TearDown()
        {
            ResetSchedulerStatics();
        }

        /// <summary>
        /// 验证：回调为 null 时按框架约定 fail-fast 抛 <see cref="ArgumentNullException" />。
        /// </summary>
        [Test]
        public void Delay_NullCallback_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => AesirScheduler.Delay(1f, null));
            Assert.Throws<ArgumentNullException>(() => AesirScheduler.NextFrame(null));
        }

        /// <summary>
        /// 验证：到期时间已过但仍在调度帧时不触发（同帧任务最早下一帧）。
        /// </summary>
        [Test]
        public void Delay_DoesNotFireOnSchedulingFrame_EvenIfTimeReached()
        {
            var fired = 0;
            AesirScheduler.Delay(0f, () => fired++);
            Assert.AreEqual(1, AesirScheduler.PendingCount, "调度后任务应处于待结算队列");

            AesirScheduler.Tick(Time.time, Time.frameCount);
            Assert.AreEqual(0, fired, "调度当帧即使时间已到也不应触发（BornFrame 守卫）");
        }

        /// <summary>
        /// 验证：NaN 延时不设防且<b>永不回落为下一帧触发</b>——DueTime 传播 NaN，
        /// 任何 currentTime 的 <c>&lt;=</c> 比较为 false，任务永不到期（与 XML 文档一致）。
        /// </summary>
        [Test]
        public void Delay_NaN_NeverFires_EvenAtMaxTime()
        {
            var fired = 0;
            AesirScheduler.Delay(float.NaN, () => fired++);
            Assert.AreEqual(1, AesirScheduler.PendingCount, "NaN 任务应正常入队");

            AesirScheduler.Tick(Time.time, Time.frameCount + 1);
            AesirScheduler.Tick(float.MaxValue, Time.frameCount + 1000);
            Assert.AreEqual(1, AesirScheduler.PendingCount, "NaN 的 DueTime 永不满足到期比较，任务应留在队列");
            Assert.AreEqual(0, fired, "NaN 任务在任何模拟时间下都不应触发");
        }

        /// <summary>
        /// 验证：帧号推进且时间到达后任务触发一次，后续帧不重复触发。
        /// </summary>
        [Test]
        public void Delay_FiresOnce_WhenFrameAdvancedAndTimeReached()
        {
            var fired = 0;
            AesirScheduler.Delay(2f, () => fired++);

            AesirScheduler.Tick(Time.time, Time.frameCount + 1);
            Assert.AreEqual(0, fired, "时间未到不应触发");

            AesirScheduler.Tick(Time.time + 2f, Time.frameCount + 1);
            Assert.AreEqual(1, fired, "时间到达且已进入下一帧，应触发一次");
            Assert.AreEqual(0, AesirScheduler.PendingCount, "触发后任务应出队");

            AesirScheduler.Tick(Time.time + 10f, Time.frameCount + 5);
            Assert.AreEqual(1, fired, "已触发的任务不应重复执行（一次性语义）");
        }

        /// <summary>
        /// 验证：负延时不抛异常，按 0 处理（下一帧触发）。
        /// </summary>
        [Test]
        public void Delay_NegativeSeconds_ClampsToNextFrame()
        {
            var fired = 0;
            Assert.DoesNotThrow(() => AesirScheduler.Delay(-3f, () => fired++));

            AesirScheduler.Tick(Time.time + 100f, Time.frameCount + 1);
            Assert.AreEqual(1, fired, "负延时应按下一帧触发");
        }

        /// <summary>
        /// 验证：<see cref="AesirScheduler.NextFrame" /> 语义等价 Delay(0)——恰好下一帧触发。
        /// </summary>
        [Test]
        public void NextFrame_FiresExactlyNextFrame()
        {
            var fired = 0;
            AesirScheduler.NextFrame(() => fired++);

            AesirScheduler.Tick(Time.time + 5f, Time.frameCount);
            Assert.AreEqual(0, fired, "调度当帧不应触发");

            AesirScheduler.Tick(Time.time + 5f, Time.frameCount + 1);
            Assert.AreEqual(1, fired, "下一帧应触发");
        }

        /// <summary>
        /// 验证：同一延时多次调度的任务按调度顺序触发（无稳定排序依赖，列表序即投递序）。
        /// </summary>
        [Test]
        public void Delay_MultipleTasks_SameDue_FireInSchedulingOrder()
        {
            var order = new List<int>();
            AesirScheduler.Delay(1f, () => order.Add(1));
            AesirScheduler.Delay(1f, () => order.Add(2));
            AesirScheduler.Delay(1f, () => order.Add(3));

            AesirScheduler.Tick(Time.time + 1f, Time.frameCount + 1);
            Assert.AreEqual(new[] { 1, 2, 3 }, order, "同到期时刻的任务应按调度顺序触发");
        }

        /// <summary>
        /// 验证：回调内再调度的新任务不参与当趟，下一帧开始参与结算（快照语义）。
        /// </summary>
        [Test]
        public void Delay_RescheduleInsideCallback_NewTaskSettlesNextTick()
        {
            var outerFired = 0;
            var innerFired = 0;
            AesirScheduler.Delay(0f, () =>
            {
                outerFired++;
                AesirScheduler.NextFrame(() => innerFired++);
            });

            AesirScheduler.Tick(Time.time, Time.frameCount + 1);
            Assert.AreEqual(1, outerFired, "外层任务应触发");
            Assert.AreEqual(0, innerFired, "回调内新调度的任务不应在当趟触发");

            AesirScheduler.Tick(Time.time, Time.frameCount + 2);
            Assert.AreEqual(1, innerFired, "再下一帧内层任务应触发");
        }

        /// <summary>
        /// 验证：未到期任务在时间到达后的结算帧触发，与到期时刻无关的帧号推进不影响判定。
        /// </summary>
        [Test]
        public void Delay_MixedTimeline_EachTaskFiresAtItsOwnDueTime()
        {
            var log = new List<string>();
            AesirScheduler.Delay(1f, () => log.Add("a"));
            AesirScheduler.Delay(3f, () => log.Add("b"));

            AesirScheduler.Tick(Time.time + 2f, Time.frameCount + 1);
            Assert.AreEqual(new[] { "a" }, log, "t+2 时仅 1 秒任务到期");

            AesirScheduler.Tick(Time.time + 3f, Time.frameCount + 2);
            Assert.AreEqual(new[] { "a", "b" }, log, "t+3 时 3 秒任务到期");
            Assert.AreEqual(0, AesirScheduler.PendingCount, "全部任务结算后队列应为空");
        }

        /// <summary>
        /// 验证：首次调度时懒注册 BeforeUpdate 钩子，RIOLM 重置后钩子注销且任务清空。
        /// </summary>
        [Test]
        public void Delay_LazyRegistersHook_ResetStaticsClearsQueueAndUnregisters()
        {
            var before = AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifecyclePhase.BeforeUpdate);
            AesirScheduler.Delay(1f, () => { });
            Assert.AreEqual(before + 1,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifecyclePhase.BeforeUpdate),
                "首次调度应注册恰好一个 BeforeUpdate 钩子");
            Assert.AreEqual(1, AesirScheduler.PendingCount);

            ResetSchedulerStatics();

            Assert.AreEqual(before,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifecyclePhase.BeforeUpdate),
                "重置应注销调度钩子");
            Assert.AreEqual(0, AesirScheduler.PendingCount, "重置应清空任务队列");
        }

        /// <summary>
        /// 经反射调用 <see cref="AesirScheduler" /> 私有 RIOLM 重置（与 AudioModule 测试同型范式），
        /// 并重置 PlayerLoop 钩子表确保测试间隔离。
        /// </summary>
        static void ResetSchedulerStatics()
        {
            var reset = typeof(AesirScheduler).GetMethod("ResetStatics",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(reset, "AesirScheduler 应按框架铁律在类内声明 RIOLM ResetStatics（非泛型类内自重置）");
            reset.Invoke(null, null);
            AesirArchitecturePlayerLoop.Reset();
        }
    }
}
