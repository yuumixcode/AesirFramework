#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirModules.Samples.Events.Filters
{
    /// <summary>
    /// 警报事件。配合 <c>WithTag("Player")</c> + <see cref="InsideCollider2D" /> 过滤器发布，
    /// 只有"Tag 为 Player 且位于警报圈（发布者的 Collider2D 范围）内"的订阅者收到。
    /// </summary>
    public class AlarmEvent : AesirEventArgs
    {
        /// <summary>
        /// 警报内容。
        /// </summary>
        public string Message;
    }
}
#endif
