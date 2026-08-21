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
    /// <remarks>
    /// 编译行为差异：
    /// <list type="bullet">
    ///     <item>
    ///     <c>Log</c> 与 <c>LogWarning</c> 系列方法标注了 <c>[Conditional("UNITY_EDITOR")]</c>，
    ///     在非编辑器构建时编译器会自动移除所有调用点，不会产生任何运行时开销。
    ///     </item>
    ///     <item>
    ///     <c>LogError</c> 系列方法未使用 <c>[Conditional]</c> 特性，在所有构建中均保留，
    ///     因为错误日志在生产环境中同样需要可见，以便定位问题。
    ///     </item>
    /// </list>
    /// </remarks>
    public static class AesirArchitectureDebug
    {
        const string Tag = "<color=#00FF88><b>[AesirArchitecture]</b></color>";
        const string TagWarning = "<color=#FFA500><b>[AesirArchitecture]</b></color>";
        const string TagError = "<color=#FF4444><b>[AesirArchitecture]</b></color>";
        const string TagTest = "<color=#00BFFF><b>[AesirArchitectureTest]</b></color>";

        /// <summary>
        /// Error 级别的富文本标签，供异常消息复用以保持控制台输出风格一致。
        /// </summary>
        /// <remarks>
        /// 此常量供 <c>CapabilityExtensions</c> 的异常消息复用，
        /// 确保抛出的异常在控制台中与直接调用 <see cref="LogError(string)" /> 保持一致的 <c>[AesirArchitecture]</c> 标识风格，
        /// 便于开发者快速识别错误来源。
        /// </remarks>
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
        /// <param name="source">日志来源标识，通常传入 <c>nameof(TypeName)</c> 以便快速定位日志产生的模块</param>
        /// <param name="message">日志内容</param>
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
        /// <param name="source">日志来源标识，通常传入 <c>nameof(TypeName)</c> 以便快速定位日志产生的模块</param>
        /// <param name="message">警告内容</param>
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
        /// <param name="source">日志来源标识，通常传入 <c>nameof(TypeName)</c> 以便快速定位日志产生的模块</param>
        /// <param name="message">错误内容</param>
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
        /// <remarks>
        /// 该方法使用 <c>[Conditional("UNITY_INCLUDE_TESTS")]</c> 特性，
        /// 仅在包含测试代码的程序集编译时保留调用。在正式发布构建中 Unity 不会定义 <c>UNITY_INCLUDE_TESTS</c>，
        /// 所有调用点都会被编译器自动移除，确保测试日志不会泄漏到生产环境。
        /// </remarks>
        /// <param name="message">测试日志内容</param>
        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void LogTestInfo(string message)
        {
            Debug.Log($"{TagTest} {message}");
        }

        /// <summary>
        /// 输出单元测试日志消息，附带来源标识
        /// </summary>
        /// <param name="source">日志来源标识，通常传入 <c>nameof(TypeName)</c> 以便快速定位日志产生的模块</param>
        /// <param name="message">测试日志内容</param>
        /// <remarks>
        /// 该方法使用 <c>[Conditional("UNITY_INCLUDE_TESTS")]</c> 特性，
        /// 仅在包含测试代码的程序集编译时保留调用，正式发布构建中自动移除。
        /// </remarks>
        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void LogTestInfo(object source, string message)
        {
            Debug.Log($"{TagTest}<color=#00BFFF>[{source}]</color> {message}");
        }
    }
}
