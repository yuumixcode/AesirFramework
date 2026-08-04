using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityTcp.Editor.Helpers
{
    /// <summary>
    /// Detects unattended editor runs (CI / UTR batchmode) and gates bridge auto-start.
    /// Set <see cref="AllowBridgeInBatchEnvVar"/> to opt in when integration tests need the bridge.
    /// </summary>
    internal static class EditorAutomationGuard
    {
        internal const string AllowBridgeInBatchEnvVar = "UNITY_TCP_ALLOW_BATCH";

        internal static bool IsAutomatedEditorRun()
            => Application.isBatchMode
               || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

        internal static bool ShouldSkipBridgeAutoStart()
            => IsAutomatedEditorRun()
               && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(AllowBridgeInBatchEnvVar));
    }
}
