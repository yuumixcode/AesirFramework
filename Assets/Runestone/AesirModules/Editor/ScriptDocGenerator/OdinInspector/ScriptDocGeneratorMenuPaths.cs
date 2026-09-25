namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// Script Doc Generator 所有 MenuItem 菜单路径和优先级的统一管理。
    /// Unity 中 MenuItem 的顺序由 priority 参数（一个整数）决定，核心规则是：数字越小，位置越靠上。若不设置，默认值为 1000。
    /// 父菜单的 priority 由其子菜单项 priority 的最小值决定。
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
        /// 打开 Script Doc Generator 窗口的菜单路径（Aesir Modules 包专属工具，归入 Modules 组）。
        /// </summary>
        public const string ScriptDocGenerator = "Tools/Aesir/Modules/Script Doc Generator";

        /// <summary>
        /// Script Doc Generator 菜单项优先级。
        /// 决定 Modules 组的组级排序（父菜单 priority 由子项最小值决定）：
        /// 999 位于 Architecture 组（995）之后（Check for Updates 已置底 1100）；
        /// 与相邻组差值 ≤ 10 不产生分割线，同属工具组且为组内第一项。
        /// </summary>
        public const int ScriptDocGeneratorOrder = 999;

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
