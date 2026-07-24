using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// AesirArchitecture 内部日志工具。
    /// <para>
    /// 所有架构模块的日志输出应走此工具，以醒目的颜色和 [AesirArchitecture] 标识区分来源。
    /// Log/Warning 通过 [Conditional] 在打包时自动剔除；Error 始终保留。
    /// </para>
    /// </summary>
    public static class AesirArchitectureDebug
    {
        const string Tag = "<color=#00FF88><b>[AesirArchitecture]</b></color>";
        const string TagWarning = "<color=#FFA500><b>[AesirArchitecture]</b></color>";
        const string TagError = "<color=#FF4444><b>[AesirArchitecture]</b></color>";
        const string TagTest = "<color=#00BFFF><b>[AesirArchitectureTest]</b></color>";

        /// <summary>
        /// Error 级别的富文本标签，供异常消息复用以保持控制台输出风格一致。
        /// </summary>
        public const string ErrorTag = TagError;

        /// <summary>
        /// 输出 Log 级别消息
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            Debug.Log($"{Tag} {message}");
        }

        /// <summary>
        /// 输出 Log 级别消息，附带来源标识
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(object source, string message)
        {
            Debug.Log($"{Tag}<color=#00FF88>[{source}]</color> {message}");
        }

        /// <summary>
        /// 输出 Warning 级别消息
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"{TagWarning} {message}");
        }

        /// <summary>
        /// 输出 Warning 级别消息，附带来源标识
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object source, string message)
        {
            Debug.LogWarning($"{TagWarning}<color=#FFA500>[{source}]</color> {message}");
        }

        /// <summary>
        /// 输出 Error 级别消息
        /// </summary>
        public static void LogError(string message)
        {
            Debug.LogError($"{TagError} {message}");
        }

        /// <summary>
        /// 输出 Error 级别消息，附带来源标识
        /// </summary>
        public static void LogError(object source, string message)
        {
            Debug.LogError($"{TagError}<color=#FF4444>[{source}]</color> {message}");
        }

        /// <summary>
        /// 输出单元测试日志消息。
        /// <para>
        /// 仅在定义了 UNITY_INCLUDE_TESTS 的程序集中生效，非测试构建自动剔除调用。
        /// </para>
        /// </summary>
        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void LogTestInfo(string message)
        {
            Debug.Log($"{TagTest} {message}");
        }

        /// <summary>
        /// 输出单元测试日志消息，附带来源标识
        /// </summary>
        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void LogTestInfo(object source, string message)
        {
            Debug.Log($"{TagTest}<color=#00BFFF>[{source}]</color> {message}");
        }
    }
}
