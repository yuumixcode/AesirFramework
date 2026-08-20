using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 静态变量重置助手（仅泛型类使用）。用于运行时阶段自动重置泛型类中的静态变量，兼容 Disable Domain Reload。
    /// 关闭 Domain Reload 时静态回调列表不会重置，所以每次启动时均可调用重置方法。
    /// </summary>
    /// <remarks>
    /// <para><b>适用范围</b>：仅泛型类（如 <c>AbstractContext&lt;T&gt;</c>）需要本助手——泛型类中的
    /// <c>[RuntimeInitializeOnLoadMethod]</c> 会被 Unity 静默跳过（不执行也不报错，Unity 2022.3 实测），
    /// 无法在自身内部声明域重载重置入口，只能通过本助手在非泛型的中心位置注册重置回调。
    /// 非泛型类不要使用本助手，直接在类内声明
    /// <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c> 重置方法即可。</para>
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
        /// <para><b>约定</b>：重置回调中禁止调用 <see cref="Register"/>——
        /// 遍历回调列表时动态添加会导致 <c>InvalidOperationException: Collection was modified</c>。
        /// 当前全部注册均在静态构造函数中完成（一次性、非动态），不触发此问题。</para>
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

        /// <summary>
        /// 手动执行所有已注册的静态变量重置回调。仅供单元测试隔离静态单例状态使用。
        /// </summary>
        /// <remarks>
        /// EditMode 测试在同一域内重复运行时不会触发域重载，已注册的静态单例（如
        /// <c>AbstractContext&lt;T&gt;._instance</c>）会跨测试运行残留，导致依赖"首次访问创建"的用例失败。
        /// 测试夹具应在 <c>SetUp</c> 中调用此方法恢复静态字段初始状态。
        /// <para>以 <c>[Conditional("UNITY_INCLUDE_TESTS")]</c> 标记，非测试构建中所有调用点自动剔除。</para>
        /// </remarks>
        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void ResetForTests()
        {
            ResetStaticsAll();
        }
    }
}
