using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 只读可观察属性接口。
    /// <para>View 层通过此接口添加监听，不能修改值。</para>
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    public interface IReadOnlyObservableValue<out T>
    {
        /// <summary>
        /// 获取当前值
        /// </summary>
        T Value { get; }

        /// <summary>
        /// 添加监听者。回调参数为新值。
        /// </summary>
        AutoRemoveListenerHandle AddListener(Action<T> callback);

        /// <summary>
        /// 移除监听者。
        /// </summary>
        void RemoveListener(Action<T> callback);

        /// <summary>
        /// 添加监听并立即触发一次当前值，用于初始化时同步监听方状态。
        /// </summary>
        AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback);

        /// <summary>
        /// 触发值变更通知，用于强制刷新监听方状态。
        /// </summary>
        void InvokeEvent();
    }
}
