namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件订阅者优先级。4 档排序：First → High → Medium → Last。
    /// <para>
    /// Medium 为 Script 订阅（<c>AddListener&lt;T&gt;</c>）默认值；High 需显式指定。
    /// First / Last 用于前置校验与收尾清理——这两档不受 <see cref="AesirEventArgs.Cancel" /> 取消传播影响。
    /// </para>
    /// </summary>
    public enum SubscriberPriority
    {
        /// <summary>前。比所有默认档位更早执行；不受取消传播影响，用于前置校验。</summary>
        First,

        /// <summary>高优先级。需显式指定。</summary>
        High,

        /// <summary>中优先级。Script 订阅 <c>AddListener&lt;T&gt;</c> 默认值。</summary>
        Medium,

        /// <summary>最后。在所有订阅者之后执行；不受取消传播影响，用于收尾/清理。</summary>
        Last
    }
}
