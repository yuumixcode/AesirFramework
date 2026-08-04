using System.Diagnostics;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 日志工具。编译后自动剔除，Console 双击可跳转到调用方。
    /// </summary>
    public static class AesirInspectorDebug
    {
        /// <summary>
        /// 输出信息日志，前缀 <c>[Aesir Inspector]</c> 显示为绿色。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Info(string message)
        {
            if (!AesirInspectorDebugSettings.IsInfoEnabled)
            {
                return;
            }

            Debug.Log($"<color=#00FF00>[Aesir Inspector]</color> {message}");
        }

        /// <summary>
        /// 输出警告日志，前缀 <c>[Aesir Inspector]</c> 显示为黄色。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Warning(string message)
        {
            if (!AesirInspectorDebugSettings.IsWarningEnabled)
            {
                return;
            }

            Debug.LogWarning($"<color=#FFFF00>[Aesir Inspector]</color> {message}");
        }

        /// <summary>
        /// 输出错误日志，前缀 <c>[Aesir Inspector]</c> 显示为红色。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_EDITOR")]
        public static void Error(string message)
        {
            Debug.LogError($"<color=#FF0000>[Aesir Inspector]</color> {message}");
        }

        /// <summary>
        /// 输出自定义前缀的信息日志，前缀显示为绿色。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Info(string prefix, string message)
        {
            if (!AesirInspectorDebugSettings.IsInfoEnabled)
            {
                return;
            }

            Debug.Log($"<color=#00FF00>[{prefix}]</color> {message}");
        }

        /// <summary>
        /// 输出自定义前缀的警告日志，前缀显示为黄色。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Warning(string prefix, string message)
        {
            if (!AesirInspectorDebugSettings.IsWarningEnabled)
            {
                return;
            }

            Debug.LogWarning($"<color=#FFFF00>[{prefix}]</color> {message}");
        }

        /// <summary>
        /// 输出自定义前缀的错误日志，前缀显示为红色。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_EDITOR")]
        public static void Error(string prefix, string message)
        {
            Debug.LogError($"<color=#FF0000>[{prefix}]</color> {message}");
        }
    }
}
