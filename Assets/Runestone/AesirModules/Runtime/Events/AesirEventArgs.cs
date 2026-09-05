using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件参数基类。所有自定义事件参数继承此类，作为事件数据载体在 <see cref="EventModule" /> 中传递。
    /// <para>
    /// 注意：<see cref="AesirEventArgs" /> 本身不持有监听者，仅作为参数实例。
    /// 订阅管理由 <see cref="EventModule" /> 的 <c>BindingRegistry</c> 负责。
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class AesirEventArgs : ICloneable
    {
        /// <summary>
        /// 事件发布者。由 <see cref="EventModule" /> 在分发时写入。
        /// </summary>
        public object Sender { get; private set; }

        /// <summary>
        /// 创建事件参数的浅拷贝。供用户在需要隔离分发实例时手动调用。
        /// 基于 <see cref="object.MemberwiseClone" /> 实现：值类型字段会被独立复制，
        /// 但引用类型字段（如数组、<see cref="System.Collections.Generic.List{T}" />、自定义类等）
        /// 仅复制引用，克隆体与原实例会共享同一个底层对象。
        /// 若事件参数子类包含可变的引用类型字段，且需要保证各订阅者互不影响，
        /// 应在该子类中重写本方法以实现深拷贝。
        /// </summary>
        /// <returns>事件参数的克隆实例。</returns>
        public virtual object Clone() => MemberwiseClone();

        /// <summary>
        /// 设置事件发布者。
        /// </summary>
        /// <param name="sender">发布者对象。</param>
        /// <returns>当前事件参数实例（支持链式调用）。</returns>
        public AesirEventArgs SetSender(object sender)
        {
            Sender = sender;
            return this;
        }

        /// <summary>
        /// 使用已设置的发布者触发事件。需先通过 <see cref="SetSender" /> 设置发布者。
        /// </summary>
        public void Invoke()
        {
            EventModule.InvokeEvent(Sender, this);
        }

        /// <summary>
        /// 使用指定发布者触发事件。
        /// </summary>
        /// <param name="sender">发布者对象。</param>
        public void Invoke(object sender)
        {
            EventModule.InvokeEvent(sender, this);
        }
    }
}
