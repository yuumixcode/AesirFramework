// R3 扩展辅助：上游 Shims.cs 的等价实现（netstandard 无 CancellationToken.UnsafeRegister）。
// 语义：注册回调时不捕获同步上下文（与 R3 内部的线程模型一致）。

#nullable enable
using System;
using System.Threading;

namespace Runestone.AesirArchitecture.R3
{
    /// <summary>
    /// <see cref="CancellationToken" /> 的辅助扩展。
    /// </summary>
    internal static class CancellationTokenExtensions
    {
        /// <summary>
        /// 注册取消回调，不捕获 <see cref="SynchronizationContext" />。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <param name="callback">取消时调用的回调。</param>
        /// <param name="state">传给回调的状态对象。</param>
        /// <returns>可用于解除注册的句柄。</returns>
        public static CancellationTokenRegistration UnsafeRegister(
            this CancellationToken cancellationToken,
            Action<object> callback,
            object state)
        {
            return cancellationToken.Register(callback, state, useSynchronizationContext: false);
        }
    }
}
