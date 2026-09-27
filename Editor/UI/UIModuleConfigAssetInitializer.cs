using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 确保 UI 模块配置资产存在：编辑模式域加载后，Resources 兜底路径缺失且项目中无同类型资产时，
    /// 自动创建 <see cref="UIModuleConfigSO" /> 至 <c>Assets/Resources/UIModuleConfig/</c>，
    /// 免去用户手动创建资产的前置步骤（配置调整不依赖预放置 [UIModule]）。
    /// </summary>
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

        static void EnsureConfigAsset()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // Resources 兜底路径已命中，资产就绪
            if (Resources.Load<UIModuleConfigSO>(UIModuleConfigSO.ResourcePath) != null)
            {
                return;
            }

            // 项目中已有同类型资产（含被移出 Resources 的形态，视为用户主动放弃 Resources 加载）：
            // 尊重现状，不强制搬回也不重复创建
            if (AssetDatabase.FindAssets("t:" + nameof(UIModuleConfigSO)).Length > 0)
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(AssetDirectory))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "UIModuleConfig");
            }

            var config = ScriptableObject.CreateInstance<UIModuleConfigSO>();
            AssetDatabase.CreateAsset(config, AssetPath);
            AssetDatabase.SaveAssets();
            AesirModulesDebug.Log(AesirModulesDebug.UIModuleTag,
                "已自动创建 UI 模块配置资产（Resources 兜底路径）：" + AssetPath);
        }
    }
}
