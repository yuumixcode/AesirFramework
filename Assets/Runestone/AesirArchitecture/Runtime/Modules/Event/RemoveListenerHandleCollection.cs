using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 监听句柄集合。管理 <see cref="AutoRemoveListenerHandle" /> 句柄的添加与批量移除，
    /// 供 <see cref="RemoveListenerTrigger" /> 和 <see cref="RemoveListenerOnSceneUnloadedTrigger" /> 复用。
    /// </summary>
    /// <remarks>
    /// 作为 <see cref="RemoveListenerTrigger"/> 和 <see cref="RemoveListenerOnSceneUnloadedTrigger"/>
    /// 的共享底层实现，统一管理多个 <see cref="AutoRemoveListenerHandle"/> 的批量移除。
    /// 通过将句柄收集到同一集合中，在生命周期事件触发时一次性调用 <see cref="RemoveAllListeners"/>
    /// 即可完成全部监听的清理，无需逐个手动移除。
    /// </remarks>
    public sealed class RemoveListenerHandleCollection
    {
        readonly List<AutoRemoveListenerHandle> _handles = new List<AutoRemoveListenerHandle>();

        /// <summary>
        /// 添加监听句柄，使其在调用条件满足时自动移除
        /// </summary>
        /// <param name="handle">要纳入批量管理的自动移除监听句柄</param>
        public void Add(AutoRemoveListenerHandle handle)
        {
            _handles.Add(handle);
        }

        /// <summary>
        /// 移除所有已注册的监听并清空列表
        /// </summary>
        /// <remarks>
        /// 遍历集合中的每个 <see cref="AutoRemoveListenerHandle"/> 调用其 <see cref="AutoRemoveListenerHandle.Dispose"/>，
        /// 执行各监听的注销回调，随后清空内部列表。调用后集合恢复为空状态，可继续接收新的句柄。
        /// </remarks>
        public void RemoveAllListeners()
        {
            foreach (var handle in _handles)
            {
                handle.Dispose();
            }

            _handles.Clear();
        }
    }
}
