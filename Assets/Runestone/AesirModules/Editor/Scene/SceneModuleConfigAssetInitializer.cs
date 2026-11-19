namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 场景模块配置资产的显式创建入口：在 Resources 兜底路径缺失且项目中无同类型资产时，
    /// 创建 <see cref="SceneModuleConfigSO" /> 至 <c>Assets/Resources/SceneModuleConfig/SceneModuleConfig.asset</c>。
    /// </summary>
    /// <remarks>
    /// 包导入时<b>不</b>自动创建——第三方包不应在导入瞬间写入用户工程。由菜单
    /// <c>Tools/Aesir/Modules/Create Module Config Assets</c> 显式调用；未创建时运行时以内存默认配置兜底。
    /// 创建逻辑与 UI 模块共用 <see cref="AesirSingletonAssetInitializer" />（单一真源）。
    /// </remarks>
    internal static class SceneModuleConfigAssetInitializer
    {
        /// <summary>资产固定创建目录（相对项目根）。</summary>
        internal const string AssetDirectory = "Assets/Resources/SceneModuleConfig";

        /// <summary>资产固定创建路径（相对项目根）。</summary>
        internal const string AssetPath = AssetDirectory + "/SceneModuleConfig.asset";

        /// <summary>显式创建场景模块配置资产（幂等：已存在时跳过）。</summary>
        internal static void EnsureConfigAsset() =>
            AesirSingletonAssetInitializer.EnsureSingletonAsset<SceneModuleConfigSO>(AssetPath,
                SceneModuleConfigSO.ResourcePath, AssetDirectory, AesirModulesDebug.SceneModuleTag);
    }
}
