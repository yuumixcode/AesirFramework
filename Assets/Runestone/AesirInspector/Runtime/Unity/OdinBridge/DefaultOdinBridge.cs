using System;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 无 Odin 环境下的默认 IOdinBridge 实现，返回合理的降级值。
    /// </summary>
    [Summary("无 Odin 环境下的默认 IOdinBridge 实现，返回合理的降级值。")]
    public class DefaultOdinBridge : IOdinBridge
    {
        public bool IsAvailable => false;
        public string GetFriendlyName(Type type) => type.Name;
        public string GetFriendlyFullName(Type type) => type.FullName;
        public string GetGenericConstraintsString(Type type, bool _) => string.Empty;
    }
}
