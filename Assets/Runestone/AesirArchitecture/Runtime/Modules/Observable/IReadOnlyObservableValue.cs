using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 只读可观察属性接口。
    /// <para>View 层通过此接口添加监听，不能修改值。</para>
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    /// <remarks>
    /// 这是 View 层使用的只读接口，只能订阅变更不能修改值。
    /// <para>
    /// <see cref="AddListenerAndInvoke" /> 在添加监听后立即触发一次当前值，适用于 View 初始化时同步显示。
    /// </para>
    /// <para>
    /// <see cref="InvokeEvent" /> 强制触发通知，用于值未变但需要刷新的场景。
    /// </para>
    /// </remarks>
    /// <seealso cref="IObservableValue{T}" />
    public interface IReadOnlyObservableValue<out T>
    {
        /// <summary>
        /// 获取当前值
        /// </summary>
        T Value { get; }

        /// <summary>
        /// 添加监听者。回调参数为新值。
        /// </summary>
        /// <param name="callback">值变更时调用的回调函数，参数为变更后的新值。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听，避免手动管理生命周期。</returns>
        AutoRemoveListenerHandle AddListener(Action<T> callback);

        /// <summary>
        /// 移除监听者。
        /// </summary>
        /// <param name="callback">先前通过 <see cref="AddListener" /> 注册的回调函数。</param>
        void RemoveListener(Action<T> callback);

        /// <summary>
        /// 添加监听并立即触发一次当前值，用于初始化时同步监听方状态。
        /// </summary>
        /// <param name="callback">值变更时调用的回调函数，参数为变更后的新值。</param>
        /// <returns>返回一个 <see cref="AutoRemoveListenerHandle" />，释放后自动移除监听。</returns>
        AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback);

        /// <summary>
        /// 触发值变更通知，用于强制刷新监听方状态。
        /// </summary>
        void InvokeEvent();
    }
}
