using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 确保 UI 模块配置资产存在：编辑模式域加载后，Resources 兜底路径缺失且项目中无同类型资产时，
    /// 自动创建 <see cref="UIModuleConfigSO" /> 至 <c>Assets/Resources/UIModuleConfig/</c>，
    /// 免去用户手动创建资产的前置步骤（配置调整不依赖预放置 [UIModule]）。
    /// </summary>
    /// <remarks>
    /// 创建逻辑与 Scene 模块共用 <see cref="AesirSingletonAssetInitializer" />（单一真源）。
    /// </remarks>
    static class UIModuleConfigAssetInitializer
    {
        /// <summary>资产固定创建目录（相对项目根）。</summary>
        internal const string AssetDirectory = "Assets/Resources/UIModuleConfig";

        /// <summary>资产固定创建路径（相对项目根）。</summary>
        internal const string AssetPath = AssetDirectory + "/UIModuleConfig.asset";

        [InitializeOnLoadMethod]
        static void ScheduleEnsureConfigAsset()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // 域加载期 AssetDatabase 未就绪（CreateAsset 导入会失败），推迟到域加载完成后执行
            EditorApplication.delayCall += EnsureConfigAsset;
        }

        static void EnsureConfigAsset() =>
            AesirSingletonAssetInitializer.EnsureSingletonAsset<UIModuleConfigSO>(AssetPath,
                UIModuleConfigSO.ResourcePath, AssetDirectory, AesirModulesDebug.UIModuleTag);
    }
}
