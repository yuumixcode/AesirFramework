namespace Runestone.AesirModules.Samples.Events
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
