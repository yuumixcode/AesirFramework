using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 无参数简单事件，提供自动移除监听的功能。
    /// </summary>
    /// <remarks>
    /// 基于 <see cref="Action" /> 委托的轻量级事件实现。不使用 <see cref="List{T}" /> 存储监听者，
    /// 而是直接通过 <c>+=</c> / <c>-=</c> 操作委托，实现零分配的监听管理。
    /// <para>
    /// <see cref="AddListener" /> 返回 <see cref="AutoRemoveListenerHandle" />，
    /// 支持使用 using 语句在作用域结束时自动移除监听，或通过
    /// <see cref="RemoveListenerExtensions" /> 绑定到 Unity 生命周期事件。
    /// </para>
    /// <para>
    /// <see cref="GetListeners" /> 返回当前委托调用列表，可用于调试或检查已注册的监听者数量。
    /// </para>
    /// <para>
    /// 与 C# <c>event</c> 关键字的区别：<see cref="MiniEvent" /> 提供 <see cref="Dispose" /> 方法，
    /// 可主动清空所有委托引用，适合在响应式系统中随宿主对象一起释放资源，
    /// 而 C# event 没有内置的清空机制。
    /// </para>
    /// </remarks>
    public sealed class MiniEvent : IDisposable
    {
        Action _eventListeners;

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        /// <remarks>
        /// 将内部委托置空，断开对所有监听者的引用，防止因监听者长期存活而导致的内存泄漏。
        /// 调用后所有已注册的监听者将不再被通知，但不会触发各监听者的移除逻辑——
        /// 如需逐个移除，应使用 <see cref="RemoveListener" /> 或通过 <see cref="AutoRemoveListenerHandle" />。
        /// </remarks>
        public void Dispose()
        {
            _eventListeners = null;
        }

        /// <summary>
        /// 添加监听者，并返回可自动移除的监听句柄
        /// </summary>
        /// <param name="listener">要添加的事件监听委托</param>
        /// <returns>用于后续自动移除该监听的句柄</returns>
        public AutoRemoveListenerHandle AddListener(Action listener)
        {
            _eventListeners += listener;
            return new AutoRemoveListenerHandle(() => RemoveListener(listener));
        }

        /// <summary>
        /// 移除监听者
        /// </summary>
        /// <param name="listener">要移除的事件监听委托</param>
        public void RemoveListener(Action listener) => _eventListeners -= listener;

        /// <summary>
        /// 调用事件，通知所有监听者
        /// </summary>
        /// <remarks>
        /// 直接多播调用，零分配。异常语义与原生 C# 事件一致：某个监听者抛出异常会中断后续监听者的执行
        /// 并向上传播（fail-fast）——监听回调不应抛异常属框架约定，业务异常应在回调内部自行处理。
        /// </remarks>
        public void Invoke()
        {
            _eventListeners?.Invoke();
        }

        /// <summary>
        /// 获取当前所有已注册的委托列表
        /// </summary>
        /// <returns>委托数组；无监听者时返回空数组</returns>
        public Delegate[] GetListeners() =>
            _eventListeners?.GetInvocationList() ?? Array.Empty<Delegate>();
    }

    /// <summary>
    /// 单参事件
    /// </summary>
    /// <typeparam name="T">事件参数类型</typeparam>
    /// <remarks>
    /// 基于 <see cref="Action{T}" /> 委托的轻量级事件实现。不使用 <see cref="List{T}" /> 存储监听者，
    /// 而是直接通过 <c>+=</c> / <c>-=</c> 操作委托，实现零分配的监听管理。
    /// <para>
    /// <see cref="AddListener" /> 返回 <see cref="AutoRemoveListenerHandle" />，
    /// 支持使用 using 语句在作用域结束时自动移除监听，或通过
    /// <see cref="RemoveListenerExtensions" /> 绑定到 Unity 生命周期事件。
    /// </para>
    /// <para>
    /// <see cref="GetListeners" /> 返回当前委托调用列表，可用于调试或检查已注册的监听者数量。
    /// </para>
    /// <para>
    /// 与 C# <c>event</c> 关键字的区别：<see cref="MiniEvent{T}" /> 提供 <see cref="Dispose" /> 方法，
    /// 可主动清空所有委托引用，适合在响应式系统中随宿主对象一起释放资源，
    /// 而 C# event 没有内置的清空机制。
    /// </para>
    /// </remarks>
    public sealed class MiniEvent<T> : IDisposable
    {
        Action<T> _eventListeners;

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        /// <remarks>
        /// 将内部委托置空，断开对所有监听者的引用，防止因监听者长期存活而导致的内存泄漏。
        /// 调用后所有已注册的监听者将不再被通知，但不会触发各监听者的移除逻辑——
        /// 如需逐个移除，应使用 <see cref="RemoveListener" /> 或通过 <see cref="AutoRemoveListenerHandle" />。
        /// </remarks>
        public void Dispose()
        {
            _eventListeners = null;
        }

        /// <summary>
        /// 添加监听者，返回可自动移除的监听句柄
        /// </summary>
        /// <param name="listener">要添加的事件监听委托</param>
        /// <returns>用于后续自动移除该监听的句柄</returns>
        public AutoRemoveListenerHandle AddListener(Action<T> listener)
        {
            _eventListeners += listener;
            return new AutoRemoveListenerHandle(() => RemoveListener(listener));
        }

        /// <summary>
        /// 移除监听者
        /// </summary>
        /// <param name="listener">要移除的事件监听委托</param>
        public void RemoveListener(Action<T> listener) => _eventListeners -= listener;

        /// <summary>
        /// 调用事件，通知所有监听者
        /// </summary>
        /// <param name="t">传递给监听者的事件参数</param>
        /// <remarks>
        /// 直接多播调用，零分配。异常语义与原生 C# 事件一致：某个监听者抛出异常会中断后续监听者的执行
        /// 并向上传播（fail-fast）——监听回调不应抛异常属框架约定，业务异常应在回调内部自行处理。
        /// </remarks>
        public void Invoke(T t)
        {
            _eventListeners?.Invoke(t);
        }

        /// <summary>
        /// 获取当前所有已注册的委托列表
        /// </summary>
        /// <returns>委托数组；无监听者时返回空数组</returns>
        public Delegate[] GetListeners() =>
            _eventListeners?.GetInvocationList() ?? Array.Empty<Delegate>();
    }
}
