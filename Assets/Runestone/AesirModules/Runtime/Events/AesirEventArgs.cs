using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件参数基类。所有自定义事件参数继承此类，作为事件数据载体在 <see cref="EventModule" /> 中传递。
    /// <para>
    /// 注意：<see cref="AesirEventArgs" /> 本身不持有监听者，仅作为参数实例。
    /// 订阅管理由 <see cref="EventModule" /> 的注册表负责。
    /// </para>
    /// <para>
    /// 通过 <see cref="WithFilter" /> / <see cref="WithFilters" /> 可链式声明订阅者过滤器，
    /// 实现"只让特定范围的订阅者收到"的精确投递。
    /// </para>
    /// <para>
    /// 通过 <see cref="Cancel" /> 可中止本次传播（后续 High/Medium 档订阅者不再收到），
    /// <see cref="SubscriberPriority.First" /> / <see cref="SubscriberPriority.Last" /> 档不受影响——用于前置校验与收尾清理。
    /// </para>
    /// <para>
    /// <b>分发约定</b>：同一参数实例在分发期间不得被再次发布（<see cref="EventModule" /> 会拒绝并报错）；
    /// 事件链请使用新的参数实例，或经 <c>AesirTimer.NextFrame</c> 延后发布。
    /// <see cref="SetSender" /> 是发布者侧 API，订阅者不应在分发期间调用。
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class AesirEventArgs
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
        /// 本次分发是否已被取消。由 <see cref="Cancel" /> 置位，由 <see cref="EventModule" /> 在每趟分发开始时复位。
        /// <para>
        /// 取消只影响 <see cref="SubscriberPriority.High" /> 与 <see cref="SubscriberPriority.Medium" />
        /// 档的后续订阅者；<see cref="SubscriberPriority.First" /> / <see cref="SubscriberPriority.Last" />
        /// 两档始终收到（分别承担前置校验与收尾清理）。
        /// </para>
        /// </summary>
        public bool IsCancelled { get; internal set; }

        /// <summary>
        /// 取消本次传播的订阅者。未取消时为 null。
        /// </summary>
        public object CancelledBy { get; internal set; }

        /// <summary>
        /// 是否禁止取消。为 true 时 <see cref="Cancel" /> 不生效。
        /// 由 <see cref="NonCancellable" /> 链式开启，用于"必须送达所有订阅者"的关键事件。
        /// </summary>
        public bool IsNonCancellable { get; private set; }

        /// <summary>
        /// 只读过滤器列表视图。未添加任何过滤器时为 null。
        /// </summary>
        public IReadOnlyList<ISubscriberFilter> Filters => _filters;

        /// <summary>
        /// 分发时供 <see cref="EventModule" /> 直接迭代的具体列表，避免接口枚举装箱。
        /// </summary>
        internal List<ISubscriberFilter> FilterList => _filters;

        /// <summary>
        /// 设置事件发布者。发布者侧 API：在 <see cref="Invoke()" /> 前声明发布者，
        /// 或由 <see cref="EventModule" /> 在分发开始时写入；订阅者不应在分发期间调用。
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
        /// 取消本次事件传播：在当前订阅者之后，<see cref="SubscriberPriority.High" /> 与
        /// <see cref="SubscriberPriority.Medium" /> 档尚未执行的订阅者将不再收到事件；
        /// <see cref="SubscriberPriority.First" /> / <see cref="SubscriberPriority.Last" /> 档不受影响。
        /// <para>
        /// 典型用途：输入吞噬、伤害减免链、互斥 UI 抢先响应。若事件经 <see cref="NonCancellable" />
        /// 标记为不可取消，本调用无效果（不抛异常）。
        /// </para>
        /// </summary>
        /// <param name="canceller">取消者（通常是订阅者自身），写入 <see cref="CancelledBy" /> 供诊断查看。</param>
        /// <returns>当前事件参数实例（支持链式调用）。</returns>
        public AesirEventArgs Cancel(object canceller)
        {
            if (IsNonCancellable)
            {
                return this;
            }

            IsCancelled = true;
            CancelledBy = canceller;
            return this;
        }

        /// <summary>
        /// 将事件标记为不可取消：此后 <see cref="Cancel" /> 不再生效。
        /// 用于"必须送达所有订阅者"的关键事件（如结算、存档）。
        /// </summary>
        /// <returns>当前事件参数实例（支持链式调用）。</returns>
        public AesirEventArgs NonCancellable()
        {
            IsNonCancellable = true;
            return this;
        }

        /// <summary>
        /// 复位本趟分发的取消状态。由 <see cref="EventModule" /> 在每趟分发开始时调用，
        /// 使缓存复用的参数实例（如 <see cref="AesirEventArgsSO" /> 资产内实例）不会把上一趟的取消状态带入下一趟。
        /// <see cref="IsNonCancellable" /> 属于配置，不随趟复位。
        /// </summary>
        internal void ResetCancellationState()
        {
            IsCancelled = false;
            CancelledBy = null;
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

        #region 分发侧状态与缓存（internal）

        /// <summary>
        /// 本实例是否正处于分发中。同一实例的重入发布被 <see cref="EventModule" /> 拒绝（fail-fast），
        /// 见类注释的分发约定。
        /// </summary>
        [NonSerialized]
        internal bool Dispatching;

        [NonSerialized]
        object _cachedSender;

        [NonSerialized]
        bool _senderCacheValid;

        [NonSerialized]
        GameObject _senderGameObject;

        [NonSerialized]
        Collider2D _senderCollider;

        [NonSerialized]
        bool _senderColliderResolved;

        /// <summary>
        /// 发布者的 GameObject（一趟分发内解析一次，供空间过滤器共用）。
        /// 发布者不是场景对象时为 null；分发中途被销毁时返回假 null，由调用方做 Unity null 判定（fail-closed）。
        /// </summary>
        internal GameObject SenderGameObject
        {
            get
            {
                EnsureSenderCache();
                return _senderGameObject;
            }
        }

        /// <summary>
        /// 发布者的第一个 <see cref="Collider2D" />（一趟分发内解析一次，无Collider 时为 null）。
        /// 分发中途增删/替换 Collider 按约定杜绝，缓存不做趟内失效；每趟分发开始重置，不会跨趟拿到失效引用。
        /// </summary>
        internal Collider2D SenderCollider
        {
            get
            {
                EnsureSenderCache();
                if (!_senderColliderResolved)
                {
                    _senderColliderResolved = true;
                    _senderCollider = _senderGameObject != null
                        ? _senderGameObject.GetComponent<Collider2D>()
                        : null;
                }

                return _senderCollider;
            }
        }

        /// <summary>
        /// 缓存按 Sender 引用失效：重入层使用不同参数实例，互不影响；
        /// Sender 变化（仅发布者侧合法写入）会触发重新解析，不会读到旧发布者的解析结果。
        /// </summary>
        void EnsureSenderCache()
        {
            if (_senderCacheValid && ReferenceEquals(_cachedSender, Sender))
            {
                return;
            }

            _cachedSender = Sender;
            _senderCacheValid = true;
            _senderColliderResolved = false;
            _senderCollider = null;
            AesirEventUtility.TryGetGameObject(Sender, out _senderGameObject);
        }

        /// <summary>
        /// 重置分发侧缓存。由 <see cref="EventModule" /> 在每趟分发开始时调用，
        /// 复用实例不会带入上一趟的解析结果。
        /// </summary>
        internal void ResetDispatchCaches()
        {
            _senderCacheValid = false;
            _cachedSender = null;
            _senderGameObject = null;
            _senderCollider = null;
            _senderColliderResolved = false;
        }

        #endregion
    }
}
