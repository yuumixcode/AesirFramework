using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 静态变量重置助手，用于运行时阶段自动重置静态变量，兼容 Disable Domain Reload。
    /// 关闭 Domain Reload 时静态回调列表不会重置，所以每次启动时均可调用重置方法。
    /// </summary>
    /// <remarks>
    /// <para><b>背景</b>：Unity 默认在进入 Play Mode 时进行域重置（Domain Reload），
    /// 会清空所有静态字段的状态。但关闭 Domain Reload（即 "Enter Play Mode Options" 中的 "Reload Domain" 未勾选）后，
    /// 静态字段不会自动重置，上一次 Play Mode 的残留数据可能导致意外的行为或错误。</para>
    /// <para><b>解决方案</b>：此助手通过 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c>
    /// 在域加载的子系统注册阶段自动触发 <see cref="ResetStaticsAll"/>，
    /// 遍历并执行所有通过 <see cref="Register"/> 注册的重置回调，手动将静态字段恢复到初始状态。</para>
    /// </remarks>
    public static class ResetStaticsAssistant
    {
        static readonly List<Action> ResetStaticsCallbacks = new List<Action>();

        /// <summary>
        /// 注册静态变量重置回调，在 Domain Reload 时自动调用
        /// </summary>
        /// <param name="callback">静态变量重置回调，在域加载时自动执行，应将相关静态字段重置为初始值</param>
        /// <remarks>
        /// 注册的回调会在每次域加载时由 <see cref="ResetStaticsAll"/> 自动执行，无需手动调用。
        /// 适用于重置任何在 Disable Domain Reload 模式下不会自动清空的静态字段。
        /// </remarks>
        public static void Register(Action callback)
        {
            ResetStaticsCallbacks.Add(callback);
        }

        /// <summary>
        /// 执行所有已注册的静态变量重置回调
        /// </summary>
        /// <remarks>
        /// 此方法由 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c> 自动触发，
        /// 在 Unity 进入 Play Mode 或域重载时自动执行，无需手动调用。
        /// </remarks>
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
