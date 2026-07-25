using System;
using Sirenix.Utilities;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Odin 环境下的 IOdinBridge 实现，委托给 Sirenix.Utilities 的扩展方法。
    /// </summary>
    public class OdinInspectorBridge : IOdinBridge
    {
        public bool IsAvailable => true;
        public string GetFriendlyName(Type type) => type.GetNiceName();
        public string GetFriendlyFullName(Type type) => type.GetNiceFullName();

        public string GetGenericConstraintsString(Type type, bool full) =>
            type.GetGenericConstraintsString(full);
    }

    /// <summary>
    /// 在 OdinIntegration 程序集加载时注入 OdinBridge 实现。
    /// </summary>
    [InitializeOnLoad]
    public static class OdinBridgeInitializer
    {
        static OdinBridgeInitializer() => OdinBridgeLocator.Bridge = new OdinInspectorBridge();
    }
}
