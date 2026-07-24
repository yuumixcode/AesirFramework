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
    public enum AesirArchitectureLifeCyclePhase
    {
        /// <summary>
        /// 逻辑帧开始：在 PlayerLoop.Update 之前执行，架构优先运算
        /// </summary>
        BeforeUpdate = 1,

        /// <summary>
        /// 逻辑帧结束：在 PlayerLoop.PostLateUpdate 之后执行，读取当前帧所有状态
        /// </summary>
        AfterUpdate = 2
    }

    /// <summary>
    /// 基于 PlayerLoop 的生命周期钩子系统，无需 MonoBehaviour 即可接入游戏级帧回调。
    /// <para>
    /// 通过 <see cref="Register" /> 注册回调，order 越小越先执行；系统自动在域加载时注入 PlayerLoop。
    /// </para>
    /// </summary>
    public static class AesirArchitecturePlayerLoop
    {
        static readonly Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>> Hooks =
            new Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>>();

        static readonly List<Action> DelayedCommands = new List<Action>();
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
        public static void Register(AesirArchitectureLifeCyclePhase phase, Action callback, int order = 0)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (_invoking)
            {
                DelayedCommands.Add(() => AddHook(phase, callback, order));
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
        public static void Unregister(AesirArchitectureLifeCyclePhase phase, Action callback)
        {
            if (_invoking)
            {
                DelayedCommands.Add(() => RemoveHook(phase, callback));
            }
            else
            {
                RemoveHook(phase, callback);
            }
        }

        /// <summary>
        /// 清空所有回调
        /// </summary>
        public static void Reset()
        {
            Hooks.Clear();
            DelayedCommands.Clear();
            _sortDirty = false;
            _nextInsertionIndex = 0;
        }

        /// <summary>
        /// 获取指定阶段的已注册回调数量
        /// </summary>
        public static int GetHookCount(AesirArchitectureLifeCyclePhase phase) =>
            Hooks.TryGetValue(phase, out var list) ? list.Count : 0;

        /// <summary>
        /// BeforeUpdate 阶段的 PlayerLoop 回调入口，供测试直接触发
        /// </summary>
        internal static void OnBeforeUpdate() => InvokeHooks(AesirArchitectureLifeCyclePhase.BeforeUpdate);

        /// <summary>
        /// AfterUpdate 阶段的 PlayerLoop 回调入口，供测试直接触发
        /// </summary>
        internal static void OnAfterUpdate() => InvokeHooks(AesirArchitectureLifeCyclePhase.AfterUpdate);

        static void AddHook(AesirArchitectureLifeCyclePhase phase, Action callback, int order)
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

        static void RemoveHook(AesirArchitectureLifeCyclePhase phase, Action callback)
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

        static void ExecuteDelayedCommands()
        {
            for (var i = 0; i < DelayedCommands.Count; i++)
            {
                DelayedCommands[i]();
            }

            DelayedCommands.Clear();
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

        static void InvokeHooks(AesirArchitectureLifeCyclePhase phase)
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
                    try
                    {
                        list[i].Callback.Invoke();
                    }
                    catch (Exception e)
                    {
                        AesirArchitectureDebug.LogError(nameof(AesirArchitecturePlayerLoop), e.ToString());
                    }
                }
            }
            finally
            {
                _invoking = false;
                if (DelayedCommands.Count > 0)
                {
                    ExecuteDelayedCommands();
                }
            }
        }

        /// <summary>
        /// PlayerLoop 子系统 type 标识，在 Update 之前执行
        /// </summary>
        struct AesirArchitectureScriptRunBeforeUpdate { }

        /// <summary>
        /// PlayerLoop 子系统 type 标识，在 PostLateUpdate 之后执行
        /// </summary>
        struct AesirArchitectureScriptRunAfterUpdate { }

        struct HookEntry
        {
            public Action Callback;
            public int Order;
            public long InsertionIndex;
        }
    }
}
