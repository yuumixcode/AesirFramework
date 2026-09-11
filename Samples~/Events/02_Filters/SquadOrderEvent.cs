#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirModules.Samples.Events.Filters
{
    /// <summary>
    /// 家族命令事件。配合 <see cref="OnlySelf" /> 过滤器发布，
    /// 只有发布者自身/子树/父级链上的订阅者收到。
    /// </summary>
    public class SquadOrderEvent : AesirEventArgs
    {
        /// <summary>
        /// 命令内容。
        /// </summary>
        public string Order;
    }
}
#endif
