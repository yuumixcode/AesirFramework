using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 自动移除监听调用器基类。维护监听句柄列表，子类在特定生命周期事件中调用 <see cref="RemoveAllListeners" /> 批量移除。
    /// </summary>
    /// <remarks>
    /// 作为抽象基类，本身不绑定任何 Unity 生命周期回调。子类通过在 <c>OnDestroy</c>、<c>OnDisable</c> 等回调中
    /// 调用 <see cref="RemoveAllListeners"/> 来触发批量清理，从而将"何时移除监听"的策略交由子类决定。
    /// </remarks>
    public abstract class RemoveListenerTrigger : MonoBehaviour
    {
        readonly RemoveListenerHandleCollection _handles = new RemoveListenerHandleCollection();

        /// <summary>
        /// 添加监听句柄，使其在调用条件满足时自动移除
        /// </summary>
        /// <param name="handle">要注册的自动移除监听句柄，封装了目标监听与移除委托</param>
        public void AddRemoveListenerHandle(AutoRemoveListenerHandle handle)
        {
            _handles.Add(handle);
        }

        /// <summary>
        /// 移除所有已注册的监听并清空列表
        /// </summary>
        /// <remarks>
        /// 将批量清理委托给 <see cref="RemoveListenerHandleCollection.RemoveAllListeners"/> 执行，
        /// 确保所有已注册的监听句柄按统一流程被正确移除并释放引用。
        /// </remarks>
        protected void RemoveAllListeners() => _handles.RemoveAllListeners();
    }
}
