#if UNITY_EDITOR
using TJGenerators.Config;

namespace TJGenerators.Utils
{
    /// <summary>
    /// 生成请求来源标识。通过 HTTP 头 <see cref="HeaderName"/>（与 "source" 并列）上报，
    /// 让后端区分本次生成是由编辑器 UI 面板发起，还是由 AI custom tool（agent）发起。
    /// </summary>
    public static class GenerationRequestOrigin
    {
        /// <summary>请求头名称。</summary>
        public const string HeaderName = "fromMethod";

        /// <summary>包版本请求头名称，与 fromMethod 并列上报。</summary>
        public const string PackageVersionHeaderName = "X-Package-Version";

        /// <summary>Agent 会话 ID 请求头名称，用于按 session 分组查询任务。</summary>
        public const string SessionIdHeaderName = "X-Session-Id";

        /// <summary>编辑器 UI 面板发起的生成。</summary>
        public const string Ui = "ui";

        /// <summary>AI custom tool（agent）发起的生成。</summary>
        public const string Agent = "agent";

        /// <summary>
        /// 获取当前包版本号，通过 PackageManager API 动态读取。
        /// 获取失败时返回空字符串（后端会将空版本归入"未知"分组）。
        /// </summary>
        public static string GetPackageVersion()
        {
            try
            {
                // 优先通过 Assembly 查找（对注册到 Package Manager 的包有效）
                var info = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(ConfigManager).Assembly);
                if (info != null && !string.IsNullOrEmpty(info.version))
                    return info.version;

                // 本地包安装时 FindForAssembly 可能返回 null，
                // 通过包内已知资源路径回退查找
                info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(
                    "Packages/cn.tuanjie.ai.generators/package.json");
                if (info != null && !string.IsNullOrEmpty(info.version))
                    return info.version;

                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}
#endif
