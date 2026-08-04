using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 日志配置，存放在 Preferences 目录下。
    /// 控制普通日志和警告日志的显示开关，错误日志始终输出。
    /// </summary>
    public class AesirInspectorDebugSettings : AesirInspectorSettings<AesirInspectorDebugSettings>
    {
        [SerializeField]
        bool enableInfoLog;

        [SerializeField]
        bool enableWarningLog = true;

        /// <summary>
        /// 普通日志是否启用。Instance 为 null 时返回 false。
        /// </summary>
        public static bool IsInfoEnabled => Instance != null && Instance.enableInfoLog;

        /// <summary>
        /// 警告日志是否启用。Instance 为 null 时返回 true，确保编辑器初始化期间不丢失警告。
        /// </summary>
        public static bool IsWarningEnabled => Instance == null || Instance.enableWarningLog;
    }
}
