#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirModules.Samples.Events.KeyPress
{
    /// <summary>
    /// 按键按下事件。
    /// </summary>
    public class KeyPressedEvent : AesirEventArgs
    {
        /// <summary>
        /// 被按下的键。
        /// </summary>
        public UnityEngine.KeyCode Key;
    }
}
#endif
