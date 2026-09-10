namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// Script Doc Generator 所有 MenuItem 菜单路径和优先级的统一管理。
    /// Unity 中 MenuItem 的顺序由 priority 参数（一个整数）决定，核心规则是：数字越小，位置越靠上。若不设置，默认值为 1000。
    /// 父菜单的 priority 由首次被编译的子菜单项决定。
    /// </summary>
    public static class ScriptDocGeneratorMenuPaths
    {
        #region Menu Roots

        /// <summary>
        /// Assets 上下文菜单中 Script Doc Generator 的根路径。
        /// </summary>
        public const string AssetsScriptDocGeneratorRoot = "Assets/Script Doc Generator";

        /// <summary>
        /// Assets 上下文菜单中 Process Summary 的根路径。
        /// </summary>
        public const string AssetsProcessSummaryRoot = AssetsScriptDocGeneratorRoot + "/Process Summary";

        #endregion

        #region Tools Menu

        /// <summary>
        /// 打开 Script Doc Generator 窗口的菜单路径。
        /// </summary>
        public const string ScriptDocGenerator = "Tools/Aesir/Script Doc Generator";

        /// <summary>
        /// Script Doc Generator 菜单项优先级。
        /// </summary>
        public const int ScriptDocGeneratorOrder = -895;

        /// <summary>
        /// 打开 Script Doc Generator UI Toolkit 窗口的菜单路径。
        /// </summary>
        public const string ScriptDocGeneratorUIToolkit = "Tools/Aesir/Script Doc Generator (UI Toolkit)";

        /// <summary>
        /// Script Doc Generator UI Toolkit 菜单项优先级。
        /// </summary>
        public const int ScriptDocGeneratorUIToolkitOrder = -894;

        #endregion

        #region Assets Context Menu

        /// <summary>
        /// 同步 XML Summary 注释到 SummaryAttribute 的菜单路径。
        /// </summary>
        public const string ProcessSummarySync = AssetsProcessSummaryRoot + "/Sync";

        /// <summary>
        /// Process Summary Sync 菜单项优先级。
        /// Script Doc Generator 末尾 124，+11 产生分割线。
        /// </summary>
        public const int ProcessSummarySyncOrder = -28;

        /// <summary>
        /// 用 SummaryAttribute 替换 XML Summary 注释的菜单路径。
        /// </summary>
        public const string ProcessSummaryReplace = AssetsProcessSummaryRoot + "/Replace";

        /// <summary>
        /// Process Summary Replace 菜单项优先级。
        /// </summary>
        public const int ProcessSummaryReplaceOrder = -25;

        /// <summary>
        /// 移除所有 SummaryAttribute 的菜单路径。
        /// </summary>
        public const string ProcessSummaryRemove = AssetsProcessSummaryRoot + "/Remove";

        /// <summary>
        /// Process Summary Remove 菜单项优先级。
        /// </summary>
        public const int ProcessSummaryRemoveOrder = -23;

        #endregion
    }
}
