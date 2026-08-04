namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Aesir Inspector 所有 MenuItem 菜单路径和优先级的统一管理。
    /// Unity 中 MenuItem 的顺序由 priority 参数（一个整数）决定，核心规则是：数字越小，位置越靠上。若不设置，默认值为 1000。
    /// 父菜单的 priority 由首次被编译的子菜单项决定。
    /// </summary>
    public static class AesirInspectorMenuItems
    {
        #region Menu Roots

        public const string ToolsAesirRoot = "Tools/Aesir";
        public const string ToolsAesirInspectorRoot = "Tools/Aesir/Inspector";

        /// <summary>
        /// Assets 上下文菜单中 Script Doc Generator 的根路径。
        /// </summary>
        public const string AssetsScriptDocGeneratorRoot = "Assets/Aesir Inspector/Script Doc Generator";

        /// <summary>
        /// Assets 上下文菜单中 Process Summary 的根路径。
        /// </summary>
        public const string AssetsProcessSummaryRoot = "Assets/Aesir Inspector/Process Summary";

        #endregion

        #region Tools Menu

        /// <summary>
        /// 打开 Getting Started 窗口的菜单路径。
        /// </summary>
        public const string GettingStarted = ToolsAesirRoot + "/Getting Started";

        /// <summary>
        /// Getting Started 菜单项优先级。
        /// </summary>
        public const int GettingStartedOrder = -980;

        /// <summary>
        /// Getting Started 窗口标题。
        /// </summary>
        public const string GettingStartedWindowName = "Getting Started";

        /// <summary>
        /// 打开 Preferences 窗口的菜单路径。
        /// </summary>
        public const string Preferences = ToolsAesirInspectorRoot + "/Preferences";

        /// <summary>
        /// Preferences 菜单项优先级。
        /// </summary>
        public const int PreferencesOrder = -880;

        /// <summary>
        /// Preferences 窗口标题。
        /// </summary>
        public const string PreferencesWindowName = "Preferences";

        /// <summary>
        /// 打开 Attribute Overview Pro 窗口的菜单路径。
        /// </summary>
        public const string AttributeOverviewPro = ToolsAesirInspectorRoot + "/Attribute Overview Pro";

        /// <summary>
        /// Attribute Overview Pro 菜单项优先级。
        /// </summary>
        public const int AttributeOverviewProOrder = -900;

        /// <summary>
        /// 打开 Script Doc Generator 窗口的菜单路径。
        /// </summary>
        public const string ScriptDocGenerator = ToolsAesirInspectorRoot + "/Script Doc Generator";

        /// <summary>
        /// Script Doc Generator 菜单项优先级。
        /// </summary>
        public const int ScriptDocGeneratorOrder = -895;

        /// <summary>
        /// 打开 Mini Tools 窗口的菜单路径。
        /// </summary>
        public const string MiniTools = ToolsAesirInspectorRoot + "/Mini Tools";

        /// <summary>
        /// Mini Tools 菜单项优先级。
        /// </summary>
        public const int MiniToolsOrder = -885;

        /// <summary>
        /// 打开 Extension Package Manager 窗口的菜单路径。
        /// </summary>
        public const string ExtensionPackageManager = ToolsAesirInspectorRoot + "/Extension Package Manager";

        /// <summary>
        /// Extension Package Manager 菜单项优先级。
        /// </summary>
        public const int ExtensionPackageManagerOrder = -890;

        /// <summary>
        /// 打开 Plugin Config Solutions 示例窗口的菜单路径。
        /// </summary>
        public const string SamplePluginConfigSolutions =
            ToolsAesirInspectorRoot + "/Samples/Plugin Config Solutions";

        /// <summary>
        /// Plugin Config Solutions 示例菜单项优先级。
        /// </summary>
        public const int SamplePluginConfigSolutionsOrder = -800;

        /// <summary>
        /// Plugin Config Solutions 示例窗口标题。
        /// </summary>
        public const string SamplePluginConfigSolutionsWindowName = "Plugin Config Solutions";

        /// <summary>
        /// 打开 RuntimeInitializeLoadType 示例窗口的菜单路径。
        /// </summary>
        public const string SampleRuntimeInitializeOnLoad =
            ToolsAesirInspectorRoot + "/Samples/RuntimeInitializeLoadType";

        /// <summary>
        /// RuntimeInitializeLoadType 示例菜单项优先级。
        /// </summary>
        public const int SampleRuntimeInitializeOnLoadOrder = -795;

        /// <summary>
        /// RuntimeInitializeLoadType 示例窗口标题。
        /// </summary>
        public const string SampleRuntimeInitializeOnLoadWindowName = "RuntimeInitializeLoadType";

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
