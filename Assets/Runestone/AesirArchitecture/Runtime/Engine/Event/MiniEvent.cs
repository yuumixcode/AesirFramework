using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 无参数简单事件，提供自动移除监听的功能。
    /// </summary>
    public sealed class MiniEvent : IDisposable
    {
        Action _eventListeners;

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        public void Dispose()
        {
            _eventListeners = null;
        }

        /// <summary>
        /// 添加监听者，并返回可自动移除的监听句柄
        /// </summary>
        public AutoRemoveListenerHandle AddListener(Action listener)
        {
            _eventListeners += listener;
            return new AutoRemoveListenerHandle(() => RemoveListener(listener));
        }

        /// <summary>
        /// 移除监听者
        /// </summary>
        public void RemoveListener(Action listener) => _eventListeners -= listener;

        /// <summary>
        /// 调用事件，通知所有监听者
        /// </summary>
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
    public sealed class MiniEvent<T> : IDisposable
    {
        Action<T> _eventListeners;

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        public void Dispose()
        {
            _eventListeners = null;
        }

        /// <summary>
        /// 添加监听者，返回可自动移除的监听句柄
        /// </summary>
        public AutoRemoveListenerHandle AddListener(Action<T> listener)
        {
            _eventListeners += listener;
            return new AutoRemoveListenerHandle(() => RemoveListener(listener));
        }

        /// <summary>
        /// 移除监听者
        /// </summary>
        public void RemoveListener(Action<T> listener) => _eventListeners -= listener;

        /// <summary>
        /// 调用事件，通知所有监听者
        /// </summary>
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
