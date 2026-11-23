using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件链路快照。诊断门面 <see cref="AesirEventDiagnostics" /> 推送与查询的最小不可变数据单元。
    /// <para>
    /// 用 struct 存于固定容量环形缓冲，记录路径无托管分配；字段刻意保持精简，
    /// 只覆盖"谁在什么时刻触发/绑定了什么事件、命中多少订阅者"这一层可观测信息。
    /// <see cref="Timestamp" /> 为 UTC 时间，展示时请自行 <c>ToLocalTime()</c>。
    /// </para>
    /// </summary>
    public readonly struct AesirEventTraceSnapshot
    {
        public readonly AesirEventTraceKind Kind;
        public readonly DateTime Timestamp;
        public readonly string EventName;
        public readonly Type EventType;
        public readonly string ActorName;
        public readonly object Actor;
        public readonly int InvokedCount;
        public readonly int TotalSubscriberCount;
        public readonly float ExecutionMs;
        public readonly bool Cancelled;
        public readonly int FilterCount;

        public AesirEventTraceSnapshot(AesirEventTraceKind kind,
            string eventName,
            Type eventType,
            string actorName,
            object actor,
            int invokedCount,
            int totalSubscriberCount,
            float executionMs,
            bool cancelled,
            int filterCount)
        {
            Kind = kind;
            Timestamp = DateTime.UtcNow;
            EventName = eventName;
            EventType = eventType;
            ActorName = actorName;
            Actor = actor;
            InvokedCount = invokedCount;
            TotalSubscriberCount = totalSubscriberCount;
            ExecutionMs = executionMs;
            Cancelled = cancelled;
            FilterCount = filterCount;
        }
    }

    /// <summary>
    /// 单个事件类型的订阅汇总快照（供监视面板展示"某事件有多少订阅者"）。
    /// </summary>
    public readonly struct AesirEventSubscriptionSnapshot
    {
        public readonly Type EventType;
        public readonly string EventName;
        public readonly int SubscriberCount;

        public AesirEventSubscriptionSnapshot(Type eventType, string eventName, int subscriberCount)
        {
            EventType = eventType;
            EventName = eventName;
            SubscriberCount = subscriberCount;
        }
    }

    /// <summary>
    /// 单条订阅绑定快照（供监视面板展开订阅者列表）。
    /// </summary>
    public readonly struct AesirSubscriberSnapshot
    {
        public readonly object Subscriber;
        public readonly string SubscriberName;
        public readonly string MemberName;
        public readonly SubscriberPriority Priority;
        public readonly bool IsAlive;

        public AesirSubscriberSnapshot(object subscriber,
            string subscriberName,
            string memberName,
            SubscriberPriority priority,
            bool isAlive)
        {
            Subscriber = subscriber;
            SubscriberName = subscriberName;
            MemberName = memberName;
            Priority = priority;
            IsAlive = isAlive;
        }
    }

    /// <summary>
    /// 事件链路快照的种类。
    /// </summary>
    public enum AesirEventTraceKind
    {
        /// <summary>事件被触发并完成分发。</summary>
        Raised,

        /// <summary>订阅者被绑定。</summary>
        SubscriberBound,

        /// <summary>订阅者被解绑。</summary>
        SubscriberUnbound
    }

    /// <summary>
    /// 事件模块诊断门面：对外暴露运行时订阅状态查询与事件链路推送，全部为公开 API。
    /// <para>
    /// <b>设计定位</b>：数据层与视图层解耦的唯一契约。本类只依赖运行时类型，不引用任何编辑器或
    /// Odin 代码，因此未安装 Odin 的消费者可直接调用这些 API 自建监视面板；有 Odin 时编辑器窗口
    /// 也只是同一套 API 的另一个消费者。
    /// </para>
    /// <para>
    /// <b>性能</b>：采集默认在编辑器与 Development Build 开启、正式包关闭，由 <see cref="Enabled" /> 控制。
    /// 关闭时 <see cref="EventModule" /> 的分发热路径只多一次静态 bool 读取，不分配、不构造快照。
    /// 开启时快照写入固定容量环形缓冲（struct 覆盖槽位，无逐事件分配）。
    /// </para>
    /// </summary>
    public static class AesirEventDiagnostics
    {
        /// <summary>环形缓冲默认容量。</summary>
        public const int DefaultCapacity = 256;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#else
        static bool _enabled = false;
#endif

        static AesirEventTraceSnapshot[] _traces = new AesirEventTraceSnapshot[DefaultCapacity];
        static int _nextWriteIndex;

        /// <summary>
        /// 是否采集诊断数据。默认：编辑器与 Development Build 为 true，正式包为 false。
        /// 可在运行时切换；关闭后不再产生新快照，已有快照保留至 <see cref="ClearTraces" />。
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// 环形缓冲容量。设置时会重建缓冲并保留最近 <c>min(原数量, 新容量)</c> 条快照。
        /// </summary>
        public static int TraceCapacity
        {
            get => _traces.Length;
            set
            {
                var capacity = Mathf.Max(1, value);
                if (capacity == _traces.Length)
                {
                    return;
                }

                var keep = Mathf.Min(TraceCount, capacity);
                var next = new AesirEventTraceSnapshot[capacity];
                for (var i = 0; i < keep; i++)
                {
                    next[i] = GetRecentTraceAt(i);
                }

                _traces = next;
                TraceCount = keep;
                _nextWriteIndex = keep % capacity;
            }
        }

        /// <summary>当前缓冲中的快照数量。</summary>
        public static int TraceCount { get; private set; }

        /// <summary>
        /// 新快照产生时触发。订阅此事件以实现实时监视面板；回调在主线程分发结束时同步调用。
        /// <para>
        /// 回调抛出的异常不会传播回事件分发（由门面隔离并记录），避免诊断设施影响业务。
        /// </para>
        /// </summary>
        public static event Action<AesirEventTraceSnapshot> TraceProduced;

        /// <summary>
        /// 取最近的链路快照（最新在前）。<paramref name="max" /> 小于 0 时返回全部。
        /// 返回的是新分配的数组（冷路径，供面板刷新调用）。
        /// </summary>
        public static AesirEventTraceSnapshot[] GetRecentTraces(int max = -1)
        {
            var count = max < 0 ? TraceCount : Mathf.Min(max, TraceCount);
            if (count <= 0)
            {
                return Array.Empty<AesirEventTraceSnapshot>();
            }

            var result = new AesirEventTraceSnapshot[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = GetRecentTraceAt(i);
            }

            return result;
        }

        /// <summary>清空所有链路快照。</summary>
        public static void ClearTraces()
        {
            TraceCount = 0;
            _nextWriteIndex = 0;
        }

        /// <summary>
        /// 取所有已注册事件类型的订阅汇总（冷路径，供监视面板一次性拉取）。
        /// </summary>
        public static AesirEventSubscriptionSnapshot[] GetSubscriptions()
        {
            if (!EventModule.TryGetExisting(out var module))
            {
                return Array.Empty<AesirEventSubscriptionSnapshot>();
            }

            var result = new AesirEventSubscriptionSnapshot[module.Bindings.Count];
            var index = 0;
            foreach (var pair in module.Bindings)
            {
                result[index++] = new AesirEventSubscriptionSnapshot(
                    pair.Key, pair.Key.Name, pair.Value.Count);
            }

            return result;
        }

        /// <summary>
        /// 取指定事件类型的订阅者明细（冷路径）。事件类型无订阅时返回空数组。
        /// </summary>
        public static AesirSubscriberSnapshot[] GetSubscribers(Type eventType)
        {
            if (eventType == null || !EventModule.TryGetExisting(out var module))
            {
                return Array.Empty<AesirSubscriberSnapshot>();
            }

            if (!module.Bindings.TryGetValue(eventType, out var list))
            {
                return Array.Empty<AesirSubscriberSnapshot>();
            }

            var result = new AesirSubscriberSnapshot[list.Count];
            FillSubscriberSnapshots(list, result);
            return result;
        }

        /// <summary>指定事件类型的订阅者总数。</summary>
        public static int GetSubscriberCount(Type eventType)
        {
            if (eventType == null || !EventModule.TryGetExisting(out var module))
            {
                return 0;
            }

            return module.Bindings.TryGetValue(eventType, out var list) ? list.Count : 0;
        }

        /// <summary>指定事件类型是否有任何订阅者。</summary>
        public static bool HasSubscribers(Type eventType) => GetSubscriberCount(eventType) > 0;

        #region 供 EventModule 调用的采集入口（internal）

        internal static void RecordRaised(AesirEventArgs eventArgs,
            Type eventType,
            int invokedCount,
            int totalSubscriberCount,
            float executionMs)
        {
            if (!Enabled)
            {
                return;
            }

            var snapshot = new AesirEventTraceSnapshot(AesirEventTraceKind.Raised, eventType.Name, eventType,
                DescribeActor(eventArgs.Sender), eventArgs.Sender, invokedCount, totalSubscriberCount,
                executionMs, eventArgs.IsCancelled, eventArgs.Filters?.Count ?? 0);
            Publish(snapshot);
        }

        internal static void RecordBound(BindingInfo binding) =>
            RecordBindingChange(AesirEventTraceKind.SubscriberBound, binding);

        internal static void RecordUnbound(BindingInfo binding) =>
            RecordBindingChange(AesirEventTraceKind.SubscriberUnbound, binding);

        #endregion

        static void RecordBindingChange(AesirEventTraceKind kind, BindingInfo binding)
        {
            if (!Enabled)
            {
                return;
            }

            var eventType = binding.EventType;
            var snapshot = new AesirEventTraceSnapshot(kind, eventType.Name, eventType,
                DescribeActor(binding.Subscriber), binding.Subscriber, 0, 0, 0f, false, 0);
            Publish(snapshot);
        }

        static void Publish(AesirEventTraceSnapshot snapshot)
        {
            _traces[_nextWriteIndex] = snapshot;
            _nextWriteIndex = (_nextWriteIndex + 1) % _traces.Length;
            if (TraceCount < _traces.Length)
            {
                TraceCount++;
            }

            var handlers = TraceProduced;
            if (handlers == null)
            {
                return;
            }

            try
            {
                handlers(snapshot);
            }
            catch (Exception e)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag, $"诊断快照监听回调抛出异常：{e.Message}");
            }
        }

        /// <summary>取倒数第 <paramref name="newestOffset" /> 条快照（0 = 最新）。</summary>
        static AesirEventTraceSnapshot GetRecentTraceAt(int newestOffset)
        {
            var index = (_nextWriteIndex - 1 - newestOffset + _traces.Length) % _traces.Length;
            return _traces[index];
        }

        static void FillSubscriberSnapshots(List<BindingInfo> list, AesirSubscriberSnapshot[] target)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var binding = list[i];
                target[i] = new AesirSubscriberSnapshot(binding.Subscriber, DescribeActor(binding.Subscriber),
                    binding.MemberName, binding.Priority,
                    !AesirEventUtility.IsObjectUnityNull(binding.Subscriber));
            }
        }

        /// <summary>
        /// 参与者名称缓存（弱引用键，不阻止参与者被回收）。
        /// 名称在首次采集时缓存：诊断开启时避免每事件一次 go.name（interop）+ 字符串拼接分配；
        /// 运行期改名不反映（诊断展示首次观察到的名字）。
        /// </summary>
        static ConditionalWeakTable<object, string> _actorNames = new ConditionalWeakTable<object, string>();

        /// <summary>
        /// 生成可展示的参与者名称：Unity 对象取 GameObject 名 + 组件类型，普通对象取类型名。
        /// 已缓存的对象直接复用；null 与已销毁对象不缓存（返回"（已销毁）"）。
        /// </summary>
        static string DescribeActor(object actor)
        {
            if (actor == null)
            {
                return "（已销毁）";
            }

            if (_actorNames.TryGetValue(actor, out var cached))
            {
                return cached;
            }

            if (AesirEventUtility.IsObjectUnityNull(actor))
            {
                return "（已销毁）";
            }

            var name = BuildActorName(actor);
            _actorNames.Add(actor, name);
            return name;
        }

        static string BuildActorName(object actor)
        {
            if (AesirEventUtility.TryGetGameObject(actor, out var go))
            {
                return actor is Component component ? $"{go.name}({component.GetType().Name})" : go.name;
            }

            if (actor is ScriptableObject so)
            {
                return so.name;
            }

            return actor.GetType().Name;
        }

        /// <summary>
        /// 域加载时重置静态状态：清空环形缓冲与静态事件订阅，兼容关闭 Domain Reload 的 Play 模式。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _traces = new AesirEventTraceSnapshot[DefaultCapacity];
            TraceCount = 0;
            _nextWriteIndex = 0;
            _actorNames = new ConditionalWeakTable<object, string>();
            TraceProduced = null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Enabled = true;
#else
            _enabled = false;
#endif
        }
    }
}
