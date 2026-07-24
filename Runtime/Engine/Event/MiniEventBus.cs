using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 静态泛型事件总线。利用 CLR 泛型静态字段隔离消除 Dictionary 查找。
    /// <para>每个事件类型 T 拥有独立的监听列表。</para>
    /// <para>推荐使用结构体实现 IEventArgs 以避免 Invoke 时的装箱与 GC 开销。</para>
    /// </summary>
    /// <typeparam name="T">
    /// 事件参数类型，须实现 IEventArgs（推荐结构体）
    /// </typeparam>
    public static class MiniEventBus<T> where T : IEventArgs
    {
        static readonly List<Action<T>> Listeners = new List<Action<T>>();
        static readonly List<Action> NoArgListeners = new List<Action>();
        static readonly List<Action> DelayedCommands = new List<Action>();
        static bool _invoking;

        static MiniEventBus()
        {
            ResetStaticsAssistant.Register(() =>
            {
                Listeners.Clear();
                NoArgListeners.Clear();
                DelayedCommands.Clear();
                _invoking = false;
            });
        }

        /// <summary>
        /// 注册带参监听，返回可自动移除的监听句柄。
        /// <para>
        /// 若在 Invoke 迭代中调用，注册会延迟到迭代结束后执行。
        /// </para>
        /// </summary>
        public static AutoRemoveListenerHandle AddListener(Action<T> listener)
        {
            if (_invoking)
            {
                DelayedCommands.Add(() => Listeners.Add(listener));
            }
            else
            {
                Listeners.Add(listener);
            }

            return new AutoRemoveListenerHandle(() => RemoveListener(listener));
        }

        /// <summary>
        /// 注册无参监听，返回可自动移除的监听句柄。
        /// <para>
        /// 若在 Invoke 迭代中调用，注册会延迟到迭代结束后执行。
        /// </para>
        /// </summary>
        public static AutoRemoveListenerHandle AddListener(Action listener)
        {
            if (_invoking)
            {
                DelayedCommands.Add(() => NoArgListeners.Add(listener));
            }
            else
            {
                NoArgListeners.Add(listener);
            }

            return new AutoRemoveListenerHandle(() => RemoveListener(listener));
        }

        /// <summary>
        /// 移除带参监听。
        /// <para>
        /// 若在 Invoke 迭代中调用，移除会延迟到迭代结束后执行。
        /// </para>
        /// </summary>
        public static void RemoveListener(Action<T> listener)
        {
            if (_invoking)
            {
                DelayedCommands.Add(() => Listeners.Remove(listener));
            }
            else
            {
                Listeners.Remove(listener);
            }
        }

        /// <summary>
        /// 移除无参监听。
        /// <para>
        /// 若在 Invoke 迭代中调用，移除会延迟到迭代结束后执行。
        /// </para>
        /// </summary>
        public static void RemoveListener(Action listener)
        {
            if (_invoking)
            {
                DelayedCommands.Add(() => NoArgListeners.Remove(listener));
            }
            else
            {
                NoArgListeners.Remove(listener);
            }
        }

        /// <summary>
        /// 发布事件，同时通知带参和无参监听。通过 _invoking 标志保护迭代，迭代期间的增删操作会延迟执行。
        /// </summary>
        public static void Invoke(T args)
        {
            _invoking = true;
            try
            {
                for (var i = 0; i < Listeners.Count; i++)
                {
                    Listeners[i].Invoke(args);
                }

                for (var i = 0; i < NoArgListeners.Count; i++)
                {
                    NoArgListeners[i].Invoke();
                }
            }
            finally
            {
                _invoking = false;
                ExecuteDelayedCommands();
            }
        }

        static void ExecuteDelayedCommands()
        {
            if (DelayedCommands.Count == 0)
            {
                return;
            }

            for (var i = 0; i < DelayedCommands.Count; i++)
            {
                DelayedCommands[i]();
            }

            DelayedCommands.Clear();
        }

        /// <summary>
        /// 清空当前事件类型的所有监听
        /// </summary>
        public static void Clear()
        {
            Listeners.Clear();
            NoArgListeners.Clear();
            DelayedCommands.Clear();
        }
    }
}
