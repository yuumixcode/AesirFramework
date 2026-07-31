namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件订阅者优先级。5 档排序：Essential → High → Medium → Low → Cleanup。
    /// <para>
    /// High 为 Attribute 订阅（<c>[AesirListener]</c>）默认值，Medium 为 Script 订阅（<c>AddListener&lt;T&gt;</c>）默认值。
    /// Essential / Low / Cleanup 用于自定义特定方法的触发时机（前/后/最后）。
    /// </para>
    /// </summary>
    public enum SubscriberPriority
    {
        /// <summary>前。比所有默认档位更早执行，用于需要在常规处理前运行的逻辑。</summary>
        First,

        /// <summary>高优先级。Attribute 订阅 [AesirListener] 默认值。</summary>
        High,

        /// <summary>中优先级。Script 订阅 AddListener&lt;T&gt; 默认值。</summary>
        Medium,

        /// <summary>最后。在所有订阅者之后执行，用于收尾/清理。</summary>
        Last
    }
}
