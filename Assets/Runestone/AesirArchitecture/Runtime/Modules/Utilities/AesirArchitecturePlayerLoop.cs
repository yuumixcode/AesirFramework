using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 游戏级生命周期阶段，对应 PlayerLoop 子系统插入点
    /// </summary>
    /// <remarks>
    /// 各阶段对应的 PlayerLoop 插入位置（按执行顺序）：
    /// <list type="bullet">
    /// <item><see cref="BeforeUpdate"/>：通过 <c>PlayerLoopUtility.InsertSystemBefore&lt;Update&gt;</c> 注入到
    /// <c>PlayerLoop.Update</c> 子系统之前，确保架构逻辑在每帧 Update 阶段开始前执行。</item>
    /// <item><see cref="AfterUpdate"/>：通过 <c>PlayerLoopUtility.InsertSystemAfter&lt;PostLateUpdate&gt;</c> 注入到
    /// <c>PlayerLoop.PostLateUpdate</c> 子系统之后，确保架构逻辑在每帧所有更新完成后执行，可读取当前帧的最终状态。</item>
    /// </list>
    /// </remarks>
    public enum AesirArchitectureLifecyclePhase
    {
        /// <summary>
        /// 逻辑帧开始：在 PlayerLoop.Update 之前执行，架构优先运算
        /// </summary>
        BeforeUpdate = 0,

        /// <summary>
        /// 逻辑帧结束：在 PlayerLoop.PostLateUpdate 之后执行，读取当前帧所有状态
        /// </summary>
        AfterUpdate = 1
    }

    /// <summary>
    /// 基于 PlayerLoop 的生命周期钩子系统，无需 MonoBehaviour 即可接入游戏级帧回调。
    /// <para>
    /// 通过 <see cref="Register" /> 注册回调，order 越小越先执行；系统自动在域加载时注入 PlayerLoop。
    /// </para>
    /// <para>
    /// <b>注入自愈</b>：PlayerLoop 注入可能被第三方 SDK 用其缓存的副本调用 <c>PlayerLoop.SetPlayerLoop</c> 覆盖，
    /// 导致钩子静默失效。框架通过 <see cref="EnsureInjected" /> 自愈：域加载时、每次 <see cref="Register" /> 时、
    /// 以及 <see cref="MonoLifecycleProxy" /> 运行期间周期性检测并补插缺失的注入点；用户也可手动调用。
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para><b>待处理命令机制</b>：在遍历回调执行期间，如果有 <see cref="Register"/> 或 <see cref="Unregister"/> 调用，
    /// 不会直接修改回调集合（否则会抛出 <see cref="InvalidOperationException"/>），
    /// 而是将操作缓存到 <c>PendingCommands</c> 列表中，待当前遍历结束后统一执行。</para>
    /// <para><b>稳定排序机制</b>：回调列表使用 <c>Order</c> 字段进行优先级排序，<c>Order</c> 越小越先执行。
    /// 当多个回调的 <c>Order</c> 相同时，使用 <c>InsertionIndex</c>（插入顺序自增序号）作为次级排序键，
    /// 确保相同优先级的回调按注册顺序执行，排序结果稳定可预期。</para>
    /// <para><b>域加载安全</b>：通过 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c>
    /// 在 Unity 的子系统注册阶段自动注入 PlayerLoop，该阶段早于场景加载和脚本初始化，
    /// 确保在 Disable Domain Reload 模式下也能正确重建钩子系统。</para>
    /// </remarks>
    public static class AesirArchitecturePlayerLoop
    {
        static readonly Dictionary<AesirArchitectureLifecyclePhase, List<HookEntry>> Hooks =
            new Dictionary<AesirArchitectureLifecyclePhase, List<HookEntry>>();

        static readonly List<Action> PendingCommands = new List<Action>();
        static bool _invoking;
        static bool _sortDirty;
        static long _nextInsertionIndex;

        /// <summary>
        /// 自动初始化：在域加载时将自定义子系统注入 PlayerLoop
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
            Reset();
            EnsureInjected();
        }

        /// <summary>
        /// 确保两个注入点存在于当前 PlayerLoop。已存在时为空操作，缺失时重新注入。
        /// </summary>
        /// <remarks>
        /// PlayerLoop 注入的自愈入口，幂等可重复调用。第三方 SDK 若使用其缓存的 PlayerLoop 副本调用
        /// <c>PlayerLoop.SetPlayerLoop</c>，会连同框架注入的两个子系统一起抹掉，
        /// 导致 <see cref="AesirArchitectureLifecyclePhase.BeforeUpdate"/> /
        /// <see cref="AesirArchitectureLifecyclePhase.AfterUpdate"/> 钩子静默失效。
        /// 此方法通过 <see cref="PlayerLoopUtility.ContainsSystem{TTarget}"/> 检测后仅补插缺失的子系统，
        /// 并保留当前 PlayerLoop 中第三方已有的其他修改。调用时机：
        /// <list type="bullet">
        /// <item><see cref="Initialize"/> 在域加载时调用；</item>
        /// <item><see cref="Register"/> 每次注册回调时调用（注册即自愈）；</item>
        /// <item><see cref="MonoLifecycleProxy"/> 运行期间周期性调用（运行中自愈）；</item>
        /// <item>用户在已知第三方 SDK 修改 PlayerLoop 后也可手动调用。</item>
        /// </list>
        /// </remarks>
        public static void EnsureInjected()
        {
            // 两个注入点各自独立检查，避免一个缺失导致另一个也跳过
            if (!PlayerLoopUtility.ContainsSystem<AesirArchitectureScriptRunBeforeUpdate>())
            {
                PlayerLoopUtility.InsertSystemBefore<Update>(new PlayerLoopSystem
                {
                    type = typeof(AesirArchitectureScriptRunBeforeUpdate),
                    updateDelegate = OnBeforeUpdate
                });
            }

            if (!PlayerLoopUtility.ContainsSystem<AesirArchitectureScriptRunAfterUpdate>())
            {
                PlayerLoopUtility.InsertSystemAfter<PostLateUpdate>(new PlayerLoopSystem
                {
                    type = typeof(AesirArchitectureScriptRunAfterUpdate),
                    updateDelegate = OnAfterUpdate
                });
            }
        }

        /// <summary>
        /// 注册回调，order 越小越先执行，默认 0。
        /// <para>
        /// 回调持有者销毁前必须调用 <see cref="Unregister" /> 注销；若未注销，回调将永久残留并阻止目标对象被回收。
        /// </para>
        /// </summary>
        /// <param name="phase">目标生命周期阶段，决定回调在哪一帧阶段执行</param>
        /// <param name="callback">每帧执行的回调委托，必须为非空委托实例</param>
        /// <param name="order">执行优先级，值越小越先执行；同 order 时按注册顺序执行</param>
        public static void Register(AesirArchitectureLifecyclePhase phase, Action callback, int order = 0)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // 注册即自愈：若注入点已被第三方 SDK 覆盖，此处补插缺失的子系统（幂等，已注入时仅为两次树遍历检测）
            EnsureInjected();

            if (_invoking)
            {
                PendingCommands.Add(() => AddHook(phase, callback, order));
            }
            else
            {
                AddHook(phase, callback, order);
            }
        }

        /// <summary>
        /// 注销回调。
        /// <para>
        /// 必须传入注册时的同一委托实例，匿名函数无法通过此方法注销。
        /// </para>
        /// </summary>
        /// <param name="phase">目标生命周期阶段</param>
        /// <param name="callback">要注销的回调委托，必须与注册时传入的实例相同</param>
        /// <remarks>
        /// 若在回调遍历期间调用此方法，注销操作不会立即执行，而是被缓存到待处理命令列表中，
        /// 待当前阶段所有回调遍历结束后才统一执行，以避免遍历期间修改集合导致异常。
        /// </remarks>
        public static void Unregister(AesirArchitectureLifecyclePhase phase, Action callback)
        {
            if (_invoking)
            {
                PendingCommands.Add(() => RemoveHook(phase, callback));
            }
            else
            {
                RemoveHook(phase, callback);
            }
        }

        /// <summary>
        /// 清空所有回调
        /// </summary>
        /// <remarks>
        /// 此方法在 <see cref="Initialize"/> 中调用，确保域重载后清空旧的回调数据和待处理命令，
        /// 防止 Disable Domain Reload 模式下残留的静态状态导致回调重复执行或引用已销毁的对象。
        /// </remarks>
        public static void Reset()
        {
            Hooks.Clear();
            PendingCommands.Clear();
            _sortDirty = false;
            _nextInsertionIndex = 0;
        }

        /// <summary>
        /// 获取指定阶段的已注册回调数量
        /// </summary>
        public static int GetHookCount(AesirArchitectureLifecyclePhase phase) =>
            Hooks.TryGetValue(phase, out var list) ? list.Count : 0;

        /// <summary>
        /// BeforeUpdate 阶段的 PlayerLoop 回调入口，供测试直接触发
        /// </summary>
        internal static void OnBeforeUpdate() => InvokeHooks(AesirArchitectureLifecyclePhase.BeforeUpdate);

        /// <summary>
        /// AfterUpdate 阶段的 PlayerLoop 回调入口，供测试直接触发
        /// </summary>
        internal static void OnAfterUpdate() => InvokeHooks(AesirArchitectureLifecyclePhase.AfterUpdate);

        static void AddHook(AesirArchitectureLifecyclePhase phase, Action callback, int order)
        {
            if (!Hooks.TryGetValue(phase, out var list))
            {
                list = new List<HookEntry>();
                Hooks[phase] = list;
            }

            list.Add(new HookEntry
                { Callback = callback, Order = order, InsertionIndex = _nextInsertionIndex++ });
            _sortDirty = true;
        }

        static void RemoveHook(AesirArchitectureLifecyclePhase phase, Action callback)
        {
            if (!Hooks.TryGetValue(phase, out var list))
            {
                return;
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Callback == callback)
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        static void ExecutePendingCommands()
        {
            for (var i = 0; i < PendingCommands.Count; i++)
            {
                PendingCommands[i]();
            }

            PendingCommands.Clear();
        }

        static void EnsureSorted()
        {
            if (!_sortDirty)
            {
                return;
            }

            foreach (var kvp in Hooks)
            {
                kvp.Value.Sort((a, b) =>
                {
                    var res = a.Order.CompareTo(b.Order);
                    return res != 0 ? res : a.InsertionIndex.CompareTo(b.InsertionIndex);
                });
            }

            _sortDirty = false;
        }

        static void InvokeHooks(AesirArchitectureLifecyclePhase phase)
        {
            if (!Hooks.TryGetValue(phase, out var list) || list.Count == 0)
            {
                return;
            }

            EnsureSorted();
            _invoking = true;
            try
            {
                for (var i = 0; i < list.Count; i++)
                {
                    // 回调异常直接向上传播（fail-fast，Unity 会捕获 PlayerLoop 内异常并记日志）；
                    // finally 保证 _invoking 复位与延迟命令执行，不因异常卡死注册状态机
                    list[i].Callback.Invoke();
                }
            }
            finally
            {
                _invoking = false;
                if (PendingCommands.Count > 0)
                {
                    ExecutePendingCommands();
                }
            }
        }

        /// <summary>
        /// PlayerLoop 子系统 type 标识，在 Update 之前执行
        /// </summary>
        /// <remarks>
        /// 此空结构体仅作为 <c>PlayerLoopSystem.type</c> 的类型标识使用，
        /// 让 <c>PlayerLoopUtility.ContainsSystem&lt;T&gt;</c> 能够检测自定义子系统是否已注入，
        /// 避免重复注入。不包含任何运行时逻辑。
        /// </remarks>
        struct AesirArchitectureScriptRunBeforeUpdate { }

        /// <summary>
        /// PlayerLoop 子系统 type 标识，在 PostLateUpdate 之后执行
        /// </summary>
        /// <remarks>
        /// 此空结构体仅作为 <c>PlayerLoopSystem.type</c> 的类型标识使用，
        /// 让 <c>PlayerLoopUtility.ContainsSystem&lt;T&gt;</c> 能够检测自定义子系统是否已注入，
        /// 避免重复注入。不包含任何运行时逻辑。
        /// </remarks>
        struct AesirArchitectureScriptRunAfterUpdate { }

        /// <summary>
        /// 回调条目，记录单个生命周期回调及其排序信息
        /// </summary>
        /// <remarks>
        /// <see cref="InsertionIndex"/> 是一个自增的序号，在每次 <c>AddHook</c> 时分配。
        /// 当多个条目的 <see cref="Order"/> 相同时，使用 <c>InsertionIndex</c> 作为次级排序键，
        /// 确保相同优先级的回调按注册先后顺序执行，实现稳定排序。
        /// </remarks>
        struct HookEntry
        {
            public Action Callback;
            public int Order;
            public long InsertionIndex;
        }
    }
}
