using System;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Mono 生命周期事件扩展方法集合。
    /// </summary>
    /// <remarks>
    /// 提供 <see cref="MonoBehaviour" /> 和 <see cref="GameObject" /> 的 <c>AddListener</c> 扩展方法，
    /// 内部委托给 <see cref="MonoLifecycleProxy.Instance" /> 单例。
    /// <para>
    /// 返回的 <see cref="AutoRemoveListenerHandle" /> 可配合 <see cref="RemoveListenerExtensions" /> 使用，
    /// 也可手动调用 <see cref="AutoRemoveListenerHandle.Dispose" /> 移除监听。
    /// </para>
    /// </remarks>
    /// <seealso cref="MonoLifecycleProxy" />
    /// <seealso cref="MonoLifecycleEvent" />
    public static class MonoLifecycleProxyExtensions
    {
        /// <summary>
        /// 添加生命周期事件监听。
        /// </summary>
        /// <param name="mono">监听所依附的 MonoBehaviour</param>
        /// <param name="evt">要监听的生命周期事件类型</param>
        /// <param name="callback">事件触发时执行的回调委托</param>
        /// <param name="order">执行优先级，值越小越先执行；同 order 时按注册顺序执行</param>
        /// <returns>用于后续自动移除该监听的句柄</returns>
        public static AutoRemoveListenerHandle RegisterCustomLifecycle(this MonoBehaviour mono,
            MonoLifecycleEvent evt,
            Action callback,
            int order = 0) =>
            MonoLifecycleProxy.Instance.AddListener(evt, callback, order);

        /// <summary>
        /// 添加生命周期事件监听。
        /// </summary>
        /// <param name="go">监听所依附的 GameObject</param>
        /// <param name="evt">要监听的生命周期事件类型</param>
        /// <param name="callback">事件触发时执行的回调委托</param>
        /// <param name="order">执行优先级，值越小越先执行；同 order 时按注册顺序执行</param>
        /// <returns>用于后续自动移除该监听的句柄</returns>
        public static AutoRemoveListenerHandle RegisterCustomLifecycle(this GameObject go,
            MonoLifecycleEvent evt,
            Action callback,
            int order = 0) =>
            MonoLifecycleProxy.Instance.AddListener(evt, callback, order);

        /// <summary>
        /// 移除生命周期事件监听。
        /// </summary>
        /// <param name="mono">监听所依附的 MonoBehaviour</param>
        /// <param name="evt">目标生命周期事件类型</param>
        /// <param name="callback">要移除的回调委托，必须与注册时传入的实例相同</param>
        public static void UnregisterCustomLifecycle(this MonoBehaviour mono,
            MonoLifecycleEvent evt,
            Action callback)
        {
            MonoLifecycleProxy.Instance.RemoveListener(evt, callback);
        }

        /// <summary>
        /// 移除生命周期事件监听。
        /// </summary>
        /// <param name="go">监听所依附的 GameObject</param>
        /// <param name="evt">目标生命周期事件类型</param>
        /// <param name="callback">要移除的回调委托，必须与注册时传入的实例相同</param>
        public static void UnregisterCustomLifecycle(this GameObject go,
            MonoLifecycleEvent evt,
            Action callback)
        {
            MonoLifecycleProxy.Instance.RemoveListener(evt, callback);
        }

        /// <summary>
        /// 快捷注册（MonoBehaviour 专用）。扫描实现的所有 ICustomXXX 接口，
        /// 将对应方法自动注册到匹配的生命周期事件中，并在 GameObject 销毁时自动取消订阅。
        /// </summary>
        /// <param name="mono">实现了任意 ICustomXXX 接口的 MonoBehaviour</param>
        public static void RegisterCustomLifecycle(this MonoBehaviour mono)
        {
            MonoLifecycleProxy.Register(mono);
        }

        /// <summary>
        /// 快捷注册（任意对象）。扫描实现的所有 ICustomXXX 接口，
        /// 将对应方法自动注册到匹配的生命周期事件中。
        /// </summary>
        /// <param name="obj">实现了任意 ICustomXXX 接口的对象（MonoBehaviour 或纯 C# 类均可）</param>
        /// <returns>组合句柄，Dispose 时一次性移除本次注册的所有监听</returns>
        /// <remarks>
        /// 适用于非 MonoBehaviour 的纯 C# 类。调用方负责在适当时机 Dispose 返回的句柄以取消订阅。
        /// </remarks>
        public static AutoRemoveListenerHandle RegisterCustomLifecycle(this object obj) =>
            MonoLifecycleProxy.Register(obj);
    }
}
