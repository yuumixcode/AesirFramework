using System;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 按 Unity Tag 过滤订阅者。仅 Tag 匹配的订阅者收到事件。
    /// <para>
    /// 用法：<c>new OnExplosion().WithFilter(new WithTag("Enemy")).Invoke(this)</c>
    /// </para>
    /// </summary>
    public sealed class WithTag : ISubscriberFilter
    {
        readonly string _tag;

        /// <summary>
        /// 创建 Tag 过滤器。
        /// </summary>
        /// <param name="tag">目标 Tag，须已在 TagManager 中定义，否则 CompareTag 会抛异常并被分发隔离捕获。</param>
        public WithTag(string tag) => _tag = tag ?? throw new ArgumentNullException(nameof(tag));

        public bool ShouldReceive(AesirEventArgs eventArgs, object subscriber, SubscriberPriority priority) =>
            AesirEventUtility.TryGetGameObject(subscriber, out var gameObject) && gameObject.CompareTag(_tag);
    }

    /// <summary>
    /// 按优先级档位过滤。仅绑定在指定档位的订阅者收到事件，
    /// 用于"同一事件类型只通知某一档"的定向分发。
    /// </summary>
    public sealed class WithPriority : ISubscriberFilter
    {
        readonly SubscriberPriority _priority;

        /// <summary>
        /// 创建优先级过滤器。
        /// </summary>
        /// <param name="priority">仅投递的优先级档位。</param>
        public WithPriority(SubscriberPriority priority) => _priority = priority;

        public bool ShouldReceive(AesirEventArgs eventArgs, object subscriber, SubscriberPriority priority) =>
            priority == _priority;
    }

    /// <summary>
    /// 仅投递给与发布者同场景的订阅者。适配多场景叠加加载工作流，
    /// 防止持久场景中的订阅者收到临时场景的局域事件。
    /// </summary>
    public sealed class SameSceneAsEmitter : ISubscriberFilter
    {
        public bool ShouldReceive(AesirEventArgs eventArgs, object subscriber, SubscriberPriority priority) =>
            AesirEventUtility.TryGetGameObject(eventArgs.Sender, out var emitterGo) &&
            AesirEventUtility.TryGetGameObject(subscriber, out var subscriberGo) &&
            emitterGo.scene == subscriberGo.scene;
    }

    /// <summary>
    /// 仅投递给发布者自身、其子树或其父级链上的订阅者。用于"只影响自己和亲属"的局域事件
    /// （如组件通知所在层级，不波及场景中其他对象）。
    /// </summary>
    public sealed class OnlySelf : ISubscriberFilter
    {
        public bool ShouldReceive(AesirEventArgs eventArgs, object subscriber, SubscriberPriority priority)
        {
            if (!AesirEventUtility.TryGetGameObject(eventArgs.Sender, out var emitterGo) ||
                !AesirEventUtility.TryGetGameObject(subscriber, out var subscriberGo))
            {
                return false;
            }

            // IsChildOf 判断含自身，两个方向任一成立即视为同一家族
            var emitterTransform = emitterGo.transform;
            var subscriberTransform = subscriberGo.transform;
            return subscriberTransform.IsChildOf(emitterTransform) ||
                   emitterTransform.IsChildOf(subscriberTransform);
        }
    }

    /// <summary>
    /// 仅投递给位于发布者 <see cref="Collider2D" /> 范围内的订阅者。用于空间局域广播（如爆炸半径），
    /// 订阅者位置取其 <c>Transform.position</c>。发布者挂多个 Collider2D 时取第一个。
    /// </summary>
    public sealed class InsideCollider2D : ISubscriberFilter
    {
        public bool ShouldReceive(AesirEventArgs eventArgs, object subscriber, SubscriberPriority priority)
        {
            if (!AesirEventUtility.TryGetGameObject(eventArgs.Sender, out var emitterGo) ||
                !AesirEventUtility.TryGetGameObject(subscriber, out var subscriberGo))
            {
                return false;
            }

            var collider = emitterGo.GetComponent<Collider2D>();
            return collider != null && collider.OverlapPoint(subscriberGo.transform.position);
        }
    }
}
