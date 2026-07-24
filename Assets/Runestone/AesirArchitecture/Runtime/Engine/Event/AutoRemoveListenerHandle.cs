using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 自动移除监听句柄。包装注销回调。
    /// </summary>
    public struct AutoRemoveListenerHandle : IDisposable
    {
        Action _callback;

        /// <summary>
        /// 创建移除监听句柄，传入注销回调
        /// </summary>
        public AutoRemoveListenerHandle(Action removeListenerCallback) => _callback = removeListenerCallback;

        /// <summary>
        /// 执行移除监听，重复调用安全
        /// </summary>
        public void Dispose()
        {
            _callback?.Invoke();
            _callback = null;
        }
    }
}
