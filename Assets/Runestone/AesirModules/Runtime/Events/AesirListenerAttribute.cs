using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件订阅者特性。标记在方法上，表示该方法监听指定类型的 <see cref="AesirEventArgs" />。
    /// <para>
    /// 用法示例：
    /// <code>
    /// [AesirListener]
    /// private void OnKeyPressed(OnKeyPressed e) { ... }
    /// 
    /// [AesirListener(typeof(OnKeyPressed))]
    /// private void OnKeyPressed() { ... }
    /// 
    /// [AesirListener(SubscriberPriority.Essential)]
    /// private void OnKeyPressed(OnKeyPressed e) { ... }
    /// 
    /// [AesirListener(typeof(OnKeyPressed), SubscriberPriority.Cleanup)]
    /// private void OnKeyPressedCleanup() { ... }
    /// </code>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AesirListenerAttribute : Attribute
    {
        /// <summary>
        /// 创建默认特性实例。事件类型从方法第一个参数推断，优先级默认 High。
        /// </summary>
        public AesirListenerAttribute() { }

        /// <summary>
        /// 创建特性实例并显式指定监听的事件类型。
        /// </summary>
        /// <param name="eventType">事件类型，必须继承自 <see cref="AesirEventArgs" />。</param>
        public AesirListenerAttribute(Type eventType)
        {
            if (eventType == null || !typeof(AesirEventArgs).IsAssignableFrom(eventType))
            {
                throw new ArgumentException("事件类型必须继承自 AesirEventArgs。", nameof(eventType));
            }

            EventType = eventType;
        }

        /// <summary>
        /// 创建特性实例并指定优先级。事件类型从方法第一个参数推断。
        /// </summary>
        /// <param name="priority">订阅优先级。</param>
        public AesirListenerAttribute(SubscriberPriority priority) => Priority = priority;

        /// <summary>
        /// 创建特性实例并显式指定监听的事件类型和优先级。
        /// </summary>
        /// <param name="eventType">事件类型，必须继承自 <see cref="AesirEventArgs" />。</param>
        /// <param name="priority">订阅优先级。</param>
        public AesirListenerAttribute(Type eventType, SubscriberPriority priority)
        {
            if (eventType == null || !typeof(AesirEventArgs).IsAssignableFrom(eventType))
            {
                throw new ArgumentException("事件类型必须继承自 AesirEventArgs。", nameof(eventType));
            }

            EventType = eventType;
            Priority = priority;
        }

        /// <summary>
        /// 显式指定监听的事件类型。为 null 时从方法第一个参数推断。
        /// </summary>
        public Type EventType { get; set; }

        /// <summary>
        /// 订阅优先级。决定订阅者在分阶段分发中的执行阶段。
        /// 默认 <see cref="SubscriberPriority.High" />。
        /// </summary>
        public SubscriberPriority Priority { get; set; } = SubscriberPriority.High;
    }
}
