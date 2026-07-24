using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 自动移除监听调用器基类。维护监听句柄列表，子类在特定生命周期事件中调用 <see cref="RemoveAllListeners" /> 批量移除。
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class RemoveListenerTrigger : AesirMonoBehaviour
    {
        readonly RemoveListenerHandleCollection _handles = new RemoveListenerHandleCollection();

        /// <summary>
        /// 添加监听句柄，使其在调用条件满足时自动移除
        /// </summary>
        public void AddRemoveListenerHandle(AutoRemoveListenerHandle handle)
        {
            _handles.Add(handle);
        }

        /// <summary>
        /// 移除所有已注册的监听并清空列表
        /// </summary>
        protected void RemoveAllListeners() => _handles.RemoveAllListeners();
    }
}
