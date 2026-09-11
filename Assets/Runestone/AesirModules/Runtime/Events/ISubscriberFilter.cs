namespace Runestone.AesirModules
{
    /// <summary>
    /// 订阅者过滤器策略接口。发布者通过 <see cref="AesirEventArgs.WithFilter" /> 声明过滤器，
    /// <see cref="EventModule" /> 分发时对每个订阅者逐个检查，全部过滤器通过才投递。
    /// <para>
    /// 约定：过滤器无法解析对象（如发布者不是场景对象、订阅者不是 GameObject/Component）时
    /// 按"不通过"处理（fail-closed），避免过滤条件失效导致事件意外扩散。
    /// </para>
    /// </summary>
    public interface ISubscriberFilter
    {
        /// <summary>
        /// 判断订阅者是否应收到本次事件。
        /// </summary>
        /// <param name="eventArgs">正在分发的事件参数，可通过 <see cref="AesirEventArgs.Sender" /> 获取发布者。</param>
        /// <param name="subscriber">待检查的订阅者对象。</param>
        /// <param name="priority">该订阅绑定的优先级档位。</param>
        /// <returns>true 表示投递给该订阅者；false 表示拦截。</returns>
        bool ShouldReceive(AesirEventArgs eventArgs, object subscriber, SubscriberPriority priority);
    }
}
