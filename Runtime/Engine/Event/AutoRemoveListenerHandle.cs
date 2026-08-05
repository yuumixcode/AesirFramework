using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 自动移除监听句柄。包装注销回调。
    /// </summary>
    /// <remarks>
    /// 实现 <see cref="IDisposable"/> 的 struct，配合 using 语句可在作用域结束时自动移除监听，
    /// 避免因忘记调用 RemoveListener 而导致的内存泄漏。
    /// <para>
    /// 内部持有 <see cref="Action"/> 回调，<see cref="Dispose"/> 后将回调置空，
    /// 因此重复调用 Dispose 是安全的，不会抛出异常。
    /// </para>
    /// <para>
    /// 该句柄由 <see cref="MiniEvent.AddListener"/> 和
    /// <c>ObservableValue&lt;T&gt;.AddListener</c> 返回。
    /// </para>
    /// </remarks>
    public struct AutoRemoveListenerHandle : IDisposable
    {
        Action _callback;

        /// <summary>
        /// 创建移除监听句柄，传入注销回调
        /// </summary>
        /// <param name="removeListenerCallback">移除监听时执行的回调，通常为从事件中注销监听者的委托</param>
        public AutoRemoveListenerHandle(Action removeListenerCallback) => _callback = removeListenerCallback;

        /// <summary>
        /// 执行移除监听，重复调用安全
        /// </summary>
        /// <remarks>
        /// 调用内部回调后将其置空，确保重复调用 <see cref="Dispose"/> 不会再次触发注销逻辑，
        /// 也不会抛出 <see cref="NullReferenceException"/>。这使得在 using 语句和手动调用
        /// 混合使用时无需额外判空。
        /// </remarks>
        public void Dispose()
        {
            _callback?.Invoke();
            _callback = null;
        }
    }
}
