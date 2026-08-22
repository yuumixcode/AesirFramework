using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Samples.PluginConfig.Editor
{
    [Serializable]
    public class PluginConfigSolutionIntroduction
    {
        public BilingualHeaderControl header;

        [PropertySpace(10)]
        [DisplayAsString(false, 14, TextAlignment.Left, true)]
        [HideLabel]
        [ShowIf(
            "@" + nameof(AesirInspectorLanguageSettingsSO) + "." +
            nameof(AesirInspectorLanguageSettingsSO.CurrentIsChinese), false)]
        public string descriptionChinese = @"配置类需求：
1. 是否需要跨项目调用？
2. 是否需要便于查看和调试？
3. 是否需要 Editor 程序集调用？
4. 是否需要 Runtime 程序集可以调用？
5. 是否需要构建后的程序运行时调用？";

        [PropertySpace(10)]
        [DisplayAsString(false, 14, TextAlignment.Left, true)]
        [HideLabel]
        [ShowIf(
            "@" + nameof(AesirInspectorLanguageSettingsSO) + "." +
            nameof(AesirInspectorLanguageSettingsSO.CurrentIsEnglish), false)]
        public string descriptionEnglish = @"Configuration requirements:
1. Cross-project access needed?
2. Easy to inspect and debug?
3. Editor assembly access needed?
4. Runtime assembly access needed?
5. Post-build runtime access needed?";

        [TableList(HideToolbar = true, AlwaysExpanded = true, IsReadOnly = true, ShowIndexLabels = true)]
        public List<UsageSceneSection> usageScenes;

        public void OnEnable()
        {
            header = new BilingualHeaderControl("插件配置解决方案", "Plugin Config Solutions",
                "Unity 插件或项目通常需要设计配置类，根据需求选择不同解决方案。",
                "Unity plugins or projects typically need to design configuration classes, choosing different solutions based on requirements.");
            usageScenes = new List<UsageSceneSection>
            {
                new UsageSceneSection
                {
                    usageScenario = "1,2,3 → PreferencesFolder",
                    chineseDescription = "支持跨项目调用，Unity 编辑器内方便查看，仅 Editor 程序集调用",
                    solution = "ScriptableSingleton + [FilePath(Location.PreferencesFolder)]"
                },
                new UsageSceneSection
                {
                    usageScenario = "1,3 → EditorPrefs",
                    chineseDescription = "支持跨项目调用，不需要在 Unity 编辑器中直接查看和调试，仅 Editor 程序集调用",
                    solution = "EditorPrefs (key-value, binary)"
                },
                new UsageSceneSection
                {
                    usageScenario = "2,3 → ProjectFolder",
                    chineseDescription = "不需要跨项目调用，Unity 编辑器内方便查看，仅 Editor 程序集调用",
                    solution = "ScriptableSingleton + [FilePath(Location.ProjectFolder)]"
                },
                new UsageSceneSection
                {
                    usageScenario = "2,3,4 → Editor Default Resources",
                    chineseDescription = "不需要跨项目调用，Runtime 和 Editor 程序集均可调用，便于查看和调试，不需要构建后调用",
                    solution = "ScriptableObject + EditorBuildSettings (Assets/Editor Default Resources/)"
                },
                new UsageSceneSection
                {
                    usageScenario = "2,3,4,5 → Settings + Preloaded",
                    chineseDescription = "不需要跨项目调用，Runtime 和 Editor 程序集均可调用，便于查看和调试，需要构建后调用",
                    solution =
                        "ScriptableObject + EditorBuildSettings + PlayerSettings.SetPreloadedAssets (Assets/Settings/)"
                }
            };
        }
    }

    [Serializable]
    public class UsageSceneSection
    {
        [DisplayAsString(12, TextAlignment.Left)]
        [GUIColor("green")]
        public string usageScenario;

        [DisplayAsString(12, TextAlignment.Left)]
        public string chineseDescription;

        [DisplayAsString(12, TextAlignment.Left)]
        public string solution;
    }
}
