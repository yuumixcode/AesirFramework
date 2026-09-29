using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 确保场景模块配置资产存在：编辑模式域加载后，Resources 兜底路径缺失且项目中无同类型资产时，
    /// 自动创建 <see cref="SceneModuleConfigSO" /> 至 <c>Assets/Resources/SceneModuleConfig/</c>，
    /// 免去用户手动创建资产的前置步骤（配置调整不依赖预放置 [SceneModule]）。
    /// </summary>
    /// <remarks>
    /// 创建逻辑与 UI 模块共用 <see cref="AesirSingletonAssetInitializer" />（单一真源）。
    /// </remarks>
    static class SceneModuleConfigAssetInitializer
    {
        /// <summary>资产固定创建目录（相对项目根）。</summary>
        internal const string AssetDirectory = "Assets/Resources/SceneModuleConfig";

        /// <summary>资产固定创建路径（相对项目根）。</summary>
        internal const string AssetPath = AssetDirectory + "/SceneModuleConfig.asset";

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
            AesirSingletonAssetInitializer.EnsureSingletonAsset<SceneModuleConfigSO>(AssetPath,
                SceneModuleConfigSO.ResourcePath, AssetDirectory, AesirModulesDebug.SceneModuleTag);
    }
}
