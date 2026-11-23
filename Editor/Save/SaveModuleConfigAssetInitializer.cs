namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 保存模块配置资产的显式创建入口：在 Resources 兜底路径缺失且项目中无同类型资产时，
    /// 创建 <see cref="SaveConfigSO" /> 至 <c>Assets/Resources/SaveModuleConfig/SaveModuleConfig.asset</c>。
    /// </summary>
    /// <remarks>
    /// 包导入时<b>不</b>自动创建——第三方包不应在导入瞬间写入用户工程。由菜单
    /// <c>Tools/Aesir/Modules/Create Module Config Assets</c> 显式调用；未创建时运行时以内存默认配置兜底。
    /// 创建逻辑与 UI / 场景模块共用 <see cref="AesirSingletonAssetInitializer" />（单一真源）。
    /// </remarks>
    internal static class SaveModuleConfigAssetInitializer
    {
        /// <summary>资产固定创建目录（相对项目根）。</summary>
        internal const string AssetDirectory = "Assets/Resources/SaveModuleConfig";

        /// <summary>资产固定创建路径（相对项目根）。</summary>
        internal const string AssetPath = AssetDirectory + "/SaveModuleConfig.asset";

        /// <summary>显式创建保存模块配置资产（幂等：已存在时跳过）。</summary>
        internal static void EnsureConfigAsset() =>
            AesirSingletonAssetInitializer.EnsureSingletonAsset<SaveConfigSO>(AssetPath,
                SaveConfigSO.ResourcePath, AssetDirectory, AesirModulesDebug.SaveModuleTag);
    }
}