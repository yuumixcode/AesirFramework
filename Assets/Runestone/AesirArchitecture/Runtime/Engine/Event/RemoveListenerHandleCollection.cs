using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 监听句柄集合。管理 <see cref="AutoRemoveListenerHandle" /> 句柄的添加与批量移除，
    /// 供 <see cref="RemoveListenerTrigger" /> 和 <see cref="RemoveListenerOnSceneUnloadedTrigger" /> 复用。
    /// </summary>
    public sealed class RemoveListenerHandleCollection
    {
        readonly List<AutoRemoveListenerHandle> _handles = new List<AutoRemoveListenerHandle>();

        /// <summary>
        /// 添加监听句柄，使其在调用条件满足时自动移除
        /// </summary>
        public void Add(AutoRemoveListenerHandle handle)
        {
            _handles.Add(handle);
        }

        /// <summary>
        /// 移除所有已注册的监听并清空列表
        /// </summary>
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
