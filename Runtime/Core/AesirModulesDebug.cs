using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Runestone.AesirArchitecture
{
    public static class AesirModulesDebug
    {
        public const string UIModuleTag = "[UIModule]";
        public const string SceneModuleTag = "[SceneModule]";
        public const string ObjectBinderTag = "[ObjectBinder]";
        public const string AesirModulesTag = "[AesirModules]";

        static string GetColoredTag(Tags tagCategory, string tag)
        {
            return tagCategory switch
            {
                Tags.Info => "<color=#00FF88><b>" + tag + "</b></color>",
                Tags.Warning => "<color=#FFA500><b>" + tag + "</b></color>",
                Tags.Error => "<color=#FF4444><b>" + tag + "</b></color>",
                Tags.Test => "<color=#00BFFF><b>" + tag + "</b></color>",
                _ => throw new ArgumentOutOfRangeException(nameof(tagCategory), tagCategory, null)
            };
        }

        /// <summary>
        /// 输出 Log 级别消息
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string prefixTag, string message)
        {
            Debug.Log($"{GetColoredTag(Tags.Info, prefixTag)} {message}");
        }

        /// <summary>
        /// 输出 Log 级别消息，附带来源标识
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(object source, string prefixTag, string message)
        {
            Debug.Log($"{GetColoredTag(Tags.Info, prefixTag)} <color=#00FF88>[{source}]</color> {message}");
        }

        /// <summary>
        /// 输出 Warning 级别消息
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string prefixTag, string message)
        {
            Debug.LogWarning($"{GetColoredTag(Tags.Warning, prefixTag)} {message}");
        }

        /// <summary>
        /// 输出 Warning 级别消息，附带来源标识
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object source, string prefixTag, string message)
        {
            Debug.LogWarning(
                $"{GetColoredTag(Tags.Warning, prefixTag)} <color=#FFA500>[{source}]</color> {message}");
        }

        /// <summary>
        /// 输出 Error 级别消息
        /// </summary>
        public static void LogError(string prefixTag, string message)
        {
            Debug.LogError($"{GetColoredTag(Tags.Error, prefixTag)} {message}");
        }

        /// <summary>
        /// 输出 Error 级别消息，附带来源标识
        /// </summary>
        public static void LogError(object source, string prefixTag, string message)
        {
            Debug.LogError(
                $"{GetColoredTag(Tags.Error, prefixTag)} <color=#FF4444>[{source}]</color> {message}");
        }

        /// <summary>
        /// 输出单元测试日志消息。
        /// <para>
        /// 仅在定义了 UNITY_INCLUDE_TESTS 的程序集中生效，非测试构建自动剔除调用。
        /// </para>
        /// </summary>
        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void LogTestInfo(string prefixTag, string message)
        {
            Debug.Log($"{GetColoredTag(Tags.Test, prefixTag)} {message}");
        }

        /// <summary>
        /// 输出单元测试日志消息，附带来源标识
        /// </summary>
        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void LogTestInfo(object source, string prefixTag, string message)
        {
            Debug.Log($"{GetColoredTag(Tags.Test, prefixTag)} <color=#00BFFF>[{source}]</color> {message}");
        }

        enum Tags
        {
            Info,
            Warning,
            Error,
            Test
        }
    }
}
