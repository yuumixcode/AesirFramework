using Runestone.AesirModules;

namespace Runestone.AesirModules.Samples.Events
{
    /// <summary>
    /// 按键按下事件参数。
    /// </summary>
    public class OnKeyPressed : AesirEventArgs
    {
        /// <summary>
        /// 被按下的键。
        /// </summary>
        public UnityEngine.KeyCode Key;
    }
}
