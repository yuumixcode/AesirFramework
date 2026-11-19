namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// UI 模块配置资产的显式创建入口：在 Resources 兜底路径缺失且项目中无同类型资产时，
    /// 创建 <see cref="UIModuleConfigSO" /> 至 <c>Assets/Resources/UIModuleConfig/UIModuleConfig.asset</c>。
    /// </summary>
    /// <remarks>
    /// 包导入时<b>不</b>自动创建——第三方包不应在导入瞬间写入用户工程。由菜单
    /// <c>Tools/Aesir/Modules/Create Module Config Assets</c> 显式调用；未创建时运行时以内存默认配置兜底。
    /// 创建逻辑与 Scene 模块共用 <see cref="AesirSingletonAssetInitializer" />（单一真源）。
    /// </remarks>
    internal static class UIModuleConfigAssetInitializer
    {
        /// <summary>资产固定创建目录（相对项目根）。</summary>
        internal const string AssetDirectory = "Assets/Resources/UIModuleConfig";

        /// <summary>资产固定创建路径（相对项目根）。</summary>
        internal const string AssetPath = AssetDirectory + "/UIModuleConfig.asset";

        /// <summary>显式创建 UI 模块配置资产（幂等：已存在时跳过）。</summary>
        internal static void EnsureConfigAsset() =>
            AesirSingletonAssetInitializer.EnsureSingletonAsset<UIModuleConfigSO>(AssetPath,
                UIModuleConfigSO.ResourcePath, AssetDirectory, AesirModulesDebug.UIModuleTag);
    }
}
