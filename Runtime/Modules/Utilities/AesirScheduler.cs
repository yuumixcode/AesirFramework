using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 帧粒度时间调度器 —— 纯 C# 静态 API，为无协程能力的 Model / Service / Command 提供合法的延时执行手段。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     任务经 <see cref="AesirArchitecturePlayerLoop" /> 的 BeforeUpdate 钩子结算（早于当帧全部
    ///     <c>MonoBehaviour.Update</c>），无需任何场景物体存在。首次使用时自动注册钩子（注册即自愈）。
    ///     </para>
    ///     <para>
    ///     <b>有意收窄的能力边界</b>：
    ///     <list type="bullet">
    ///         <item><b>帧粒度</b>——计时按帧结算，<c>Delay(0.05f)</c> 在 60fps 下约 3-4 帧后触发；
    ///         所有任务最早下一帧执行（含 <c>Delay(0)</c>，不做同帧投递）。</item>
    ///         <item><b>游戏时间</b>——计时基于 <see cref="Time.time" />，受 <c>timeScale</c> 影响
    ///         （<c>timeScale = 0</c> 期间暂停计时，随游戏时间推进）。</item>
    ///         <item><b>一次性任务</b>——无句柄、无取消、无暂停、不池化；高频反复调度请评估直接持有句柄型事件。</item>
    ///         <item><b>仅主线程</b>——框架铁律；从异步回调访问请先调度回主线程。</item>
    ///     </list>
    ///     </para>
    ///     <para>
    ///     <b>语义要点</b>：回调内再调度的新任务从下一帧开始参与结算（不参与当趟）；
    ///     回调不应抛异常（框架约定 fail-fast）——抛出时异常向上传播由 PlayerLoop 捕获记日志，
    ///     本趟后续任务跳过且不补投递（已出队任务不会重试）；
    ///     <c>seconds</c> 为 NaN 时不设防（任务永不触发，极简原则，误用自行排查）。
    ///     </para>
    ///     <para>
    ///     <b>稳态零分配</b>：任务列表与结算缓冲复用（仅列表扩容时分配）；空队列时钩子零成本直接返回。
    ///     </para>
    /// </remarks>
    /// <seealso cref="AesirArchitecturePlayerLoop" />
    public static class AesirScheduler
    {
        /// <summary>
        /// 待结算任务
        /// </summary>
        /// <remarks>
        /// 结构体存储：列表本身零分配承载任务（无每任务堆分配），<see cref="DueTime" /> 为 <see cref="Time.time" /> 域上的绝对到期时刻。
        /// </remarks>
        struct ScheduledTask
        {
            public float DueTime;
            public int BornFrame;
            public Action Callback;
        }

        static readonly List<ScheduledTask> Tasks = new List<ScheduledTask>();

        /// <summary>结算缓冲区（复用，稳态零分配）：先出队后投递，回调抛异常不破坏队列完整性。</summary>
        static readonly List<ScheduledTask> FireBuffer = new List<ScheduledTask>();

        static AutoRemoveListenerHandle _tickHandle;
        static bool _tickRegistered;

        /// <summary>
        /// 当前待结算任务数量（含未到期），供调试与测试观察队列状态。
        /// </summary>
        public static int PendingCount => Tasks.Count;

        /// <summary>
        /// 下一帧执行一次指定回调。
        /// </summary>
        /// <param name="callback">下一帧 BeforeUpdate 阶段执行的回调</param>
        /// <remarks>语义等价 <see cref="Delay(float, Action)" /> 传 0——帧粒度下"最早下一帧"即最短延时。</remarks>
        public static void NextFrame(Action callback) => Delay(0f, callback);

        /// <summary>
        /// 延时执行一次指定回调（帧粒度，最早下一帧）。
        /// </summary>
        /// <param name="seconds">延时秒数（<see cref="Time.time" /> 域）；负值按 0 处理（下一帧触发）</param>
        /// <param name="callback">到期时执行的回调</param>
        /// <remarks>
        /// 任务一次性且不可取消；重复调用各自独立入队。首次调用自动注册 PlayerLoop 钩子。
        /// </remarks>
        public static void Delay(float seconds, Action callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            EnsureRegistered();
            Tasks.Add(new ScheduledTask
            {
                DueTime = Time.time + (seconds > 0f ? seconds : 0f),
                BornFrame = Time.frameCount,
                Callback = callback
            });
        }

        /// <summary>
        /// 重置静态状态：清空任务队列并注销 PlayerLoop 钩子。
        /// </summary>
        /// <remarks>
        /// 由 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c> 自动触发，
        /// 兼容 Disable Domain Reload——关闭域重载时静态任务不会跨 Play 会话残留
        /// （<see cref="AutoRemoveListenerHandle.Dispose" /> 重复调用安全，与 PlayerLoop 自身的域加载重置次序无依赖）。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _tickHandle.Dispose();
            _tickHandle = default;
            _tickRegistered = false;
            Tasks.Clear();
            FireBuffer.Clear();
        }

        /// <summary>
        /// 首次使用时注册 BeforeUpdate 钩子（注册即自愈——PlayerLoop 被第三方覆盖时随下次调度自动补插）。
        /// </summary>
        static void EnsureRegistered()
        {
            if (_tickRegistered)
            {
                return;
            }

            _tickHandle = AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate,
                OnBeforeUpdateTick);
            _tickRegistered = true;
        }

        /// <summary>
        /// PlayerLoop 钩子入口：空队列零成本返回，避免无任务期每帧空跑结算。
        /// </summary>
        static void OnBeforeUpdateTick()
        {
            if (Tasks.Count == 0)
            {
                return;
            }

            Tick(Time.time, Time.frameCount);
        }

        /// <summary>
        /// 结算一帧：出队并投递全部到期任务。internal 供 EditMode 测试以模拟时间/帧号驱动（不依赖引擎真实走帧）。
        /// </summary>
        /// <param name="currentTime">当前 <see cref="Time.time" />（测试可传模拟值）</param>
        /// <param name="currentFrame">当前 <see cref="Time.frameCount" />（测试可传模拟值）</param>
        /// <remarks>
        /// 到期条件：<c>BornFrame &lt; currentFrame</c>（同帧任务最早下一帧）且 <c>DueTime &lt;= currentTime</c>。
        /// 出队采用写指针压缩（零分配）；投递阶段基于缓冲区遍历——回调内新增任务不参与本趟（快照语义）。
        /// </remarks>
        internal static void Tick(float currentTime, int currentFrame)
        {
            var count = Tasks.Count;
            FireBuffer.Clear();
            var write = 0;
            for (var i = 0; i < count; i++)
            {
                var task = Tasks[i];
                if (task.BornFrame < currentFrame && task.DueTime <= currentTime)
                {
                    FireBuffer.Add(task);
                }
                else
                {
                    Tasks[write++] = task;
                }
            }

            if (write < count)
            {
                Tasks.RemoveRange(write, count - write);
            }

            // 先出队后投递：回调抛异常时队列已完成压缩，不会出现"已触发任务下帧重跑"的重复投递
            for (var i = 0; i < FireBuffer.Count; i++)
            {
                FireBuffer[i].Callback();
            }
        }
    }
}
