using System;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Odin 包装层的核心桥接接口。由 OdinIntegration 程序集实现，在无 Odin 环境下使用 DefaultOdinBridge。
    /// </summary>
    [Summary("Odin 包装层的核心桥接接口。由 OdinIntegration 程序集实现，在无 Odin 环境下使用 DefaultOdinBridge。")]
    public interface IOdinBridge
    {
        bool IsAvailable { get; }
        string GetFriendlyName(Type type);
        string GetFriendlyFullName(Type type);
        string GetGenericConstraintsString(Type type, bool useFullTypeNames = false);
    }
}
