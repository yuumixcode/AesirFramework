using System;
using System.Collections.Generic;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件参数基类。所有自定义事件参数继承此类，作为事件数据载体在 <see cref="EventModule" /> 中传递。
    /// <para>
    /// 注意：<see cref="AesirEventArgs" /> 本身不持有监听者，仅作为参数实例。
    /// 订阅管理由 <see cref="EventModule" /> 的 <c>BindingRegistry</c> 负责。
    /// </para>
    /// <para>
    /// 通过 <see cref="WithFilter" /> / <see cref="WithFilters" /> 可链式声明订阅者过滤器，
    /// 实现"只让特定范围的订阅者收到"的精确投递。
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class AesirEventArgs : ICloneable
    {
        /// <summary>
        /// 订阅者过滤器列表。通过 <see cref="WithFilter" /> / <see cref="WithFilters" /> 链式添加，
        /// 由 <see cref="EventModule" /> 在分发时逐订阅者检查，全部通过才投递。
        /// <para>
        /// [NonSerialized]：过滤器是运行时投递策略，不参与序列化。
        /// 未添加任何过滤器时保持 null，不为空列表分配内存。
        /// </para>
        /// </summary>
        [NonSerialized]
        List<ISubscriberFilter> _filters;

        /// <summary>
        /// 事件发布者。由 <see cref="EventModule" /> 在分发时写入。
        /// </summary>
        public object Sender { get; private set; }

        /// <summary>
        /// 只读过滤器列表视图。未添加任何过滤器时为 null。
        /// </summary>
        public IReadOnlyList<ISubscriberFilter> Filters => _filters;

        /// <summary>
        /// 分发时供 <see cref="EventModule" /> 直接迭代的具体列表，避免接口枚举装箱。
        /// </summary>
        internal List<ISubscriberFilter> FilterList => _filters;

        /// <summary>
        /// 创建事件参数的浅拷贝。供用户在需要隔离分发实例时手动调用。
        /// 基于 <see cref="object.MemberwiseClone" /> 实现：值类型字段会被独立复制，
        /// 但引用类型字段（如数组、<see cref="System.Collections.Generic.List{T}" />、自定义类等）
        /// 仅复制引用，克隆体与原实例会共享同一个底层对象。
        /// 若事件参数子类包含可变的引用类型字段，且需要保证各订阅者互不影响，
        /// 应在该子类中重写本方法以实现深拷贝。
        /// 过滤器列表 <see cref="Filters" /> 同样仅复制引用，克隆体与原实例共享同一过滤器列表。
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
        /// 链式添加一个订阅者过滤器。多次调用依次叠加，分发时全部过滤器通过才投递。
        /// 过滤器随事件参数实例存在：缓存复用的参数实例（如 AesirEventArgsSO）会保留已添加的过滤器。
        /// </summary>
        /// <param name="filter">过滤器实例。</param>
        /// <returns>当前事件参数实例（支持链式调用）。</returns>
        public AesirEventArgs WithFilter(ISubscriberFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _filters ??= new List<ISubscriberFilter>(1);
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// 链式添加一组订阅者过滤器。
        /// </summary>
        /// <param name="filters">过滤器实例数组。</param>
        /// <returns>当前事件参数实例（支持链式调用）。</returns>
        public AesirEventArgs WithFilters(params ISubscriberFilter[] filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            if (filters.Length == 0)
            {
                return this;
            }

            _filters ??= new List<ISubscriberFilter>(filters.Length);
            _filters.AddRange(filters);
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
