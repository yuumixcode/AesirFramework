using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 静态变量重置助手，用于运行时阶段自动重置静态变量，兼容 Disable Domain Reload。
    /// 关闭 Domain Reload 时静态回调列表不会重置，所以每次启动时均可调用重置方法。
    /// </summary>
    public static class ResetStaticsAssistant
    {
        static readonly List<Action> ResetStaticsCallbacks = new List<Action>();

        /// <summary>
        /// 注册静态变量重置回调，在 Domain Reload 时自动调用
        /// </summary>
        public static void Register(Action callback)
        {
            ResetStaticsCallbacks.Add(callback);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsAll()
        {
            foreach (var callback in ResetStaticsCallbacks)
            {
                callback?.Invoke();
            }
        }
    }
}
