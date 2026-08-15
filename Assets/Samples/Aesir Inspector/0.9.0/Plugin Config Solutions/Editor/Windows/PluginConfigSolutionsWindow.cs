using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Samples.PluginConfig.Editor
{
    public class PluginConfigSolutionsWindow : OdinMenuEditorWindow
    {
        PluginConfigSolutionIntroduction _introduction;
        ScriptableSingletonInPreferencesSample _preferencesSampleConfig;
        ScriptableSingletonInProjectSample _projectSampleConfig;
        PluginConfigRuntimeOnEditorSample _runtimeOnEditorSampleConfig;

        protected override void OnEnable()
        {
            base.OnEnable();
            _preferencesSampleConfig = ScriptableSingletonInPreferencesSample.instance;
            _projectSampleConfig = ScriptableSingletonInProjectSample.instance;
            _runtimeOnEditorSampleConfig = PluginConfigRuntimeOnEditorSample.Instance;
            _introduction = new PluginConfigSolutionIntroduction();
            _introduction.OnEnable();
            MenuWidth = 220f;
            WindowPadding = new Vector4(10f, 10f, 10f, 10f);
        }

        [MenuItem(AesirInspectorMenuItems.SamplePluginConfigSolutions, false,
            AesirInspectorMenuItems.SamplePluginConfigSolutionsOrder)]
        public static void Open()
        {
            var window = GetWindow<PluginConfigSolutionsWindow>();
            window.titleContent =
                new GUIContent(AesirInspectorMenuItems.SamplePluginConfigSolutionsWindowName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900, 700);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var menuStyle = new OdinMenuStyle
            {
                Height = 30,
                Offset = 20.00f,
                IndentAmount = 15.00f,
                IconSize = 16.00f,
                IconOffset = 0.00f,
                NotSelectedIconAlpha = 0.70f,
                IconPadding = 3.00f,
                TriangleSize = 16.00f,
                TrianglePadding = 0.00f,
                AlignTriangleLeft = true,
                Borders = true,
                BorderPadding = 13.00f,
                BorderAlpha = 0.50f,
                SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
                SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
            };

            var tree = new OdinMenuTree(false, menuStyle)
            {
                { "简介", _introduction },
                { "ScriptableSingleton/Project 级别", _projectSampleConfig },
                { "ScriptableSingleton/Preferences 级别", _preferencesSampleConfig },
                { "Runtime On Editor 配置方案", _runtimeOnEditorSampleConfig }
            };
            tree.Config.DrawSearchToolbar = true;
            tree.EnumerateTree().AddThumbnailIcons().SortMenuItemsByName();
            return tree;
        }
    }
}
