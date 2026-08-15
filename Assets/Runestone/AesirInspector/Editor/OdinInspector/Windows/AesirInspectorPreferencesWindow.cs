using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Aesir Inspector 偏好设置窗口，集中管理核心设置项
    /// </summary>
    public class AesirInspectorPreferencesWindow : OdinMenuEditorWindow
    {
        static readonly BilingualData InspectorLanguageSettingsMenuName =
            new BilingualData("Inspector 语言设置", "Inspector Language Settings");

        static readonly BilingualData InspectorLoggerSettingsMenuName =
            new BilingualData("Inspector 日志设置", "Inspector Logger Settings");

        static object _lastSelection;

        AesirInspectorLanguageSettingsSO _aesirInspectorLanguageSettingsSO;
        AesirInspectorDebugSettings _aesirInspectorDebugSettings;
        OdinMenuStyle _menuStyle;

        protected override void OnEnable()
        {
            base.OnEnable();
            _aesirInspectorLanguageSettingsSO = AesirInspectorLanguageSettingsSO.Instance;
            _aesirInspectorDebugSettings = AesirInspectorDebugSettings.Instance;
            MenuWidth = 220f;
            WindowPadding = new Vector4(10f, 10f, 10f, 10f);
            _menuStyle = new OdinMenuStyle
            {
                Height = 30,
                Offset = 16.00f,
                IndentAmount = 15.00f,
                IconSize = 16.00f,
                IconOffset = 0.00f,
                NotSelectedIconAlpha = 0.85f,
                IconPadding = 3.00f,
                TriangleSize = 17.00f,
                TrianglePadding = 8.00f,
                AlignTriangleLeft = false,
                Borders = true,
                BorderPadding = 13.00f,
                BorderAlpha = 0.50f,
                SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
                SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
            };
            AesirInspectorLanguageSettingsSO.LanguageChanged -= CustomRebuild;
            AesirInspectorLanguageSettingsSO.LanguageChanged += CustomRebuild;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            AesirInspectorLanguageSettingsSO.LanguageChanged -= CustomRebuild;
            _lastSelection = null;
        }

        /// <summary>
        /// 打开 Aesir Inspector 偏好设置窗口
        /// </summary>
        [MenuItem(AesirInspectorMenuItems.Preferences, false, AesirInspectorMenuItems.PreferencesOrder)]
        public static void Open()
        {
            var window = GetWindow<AesirInspectorPreferencesWindow>();
            window.titleContent = new GUIContent(AesirInspectorMenuItems.PreferencesWindowName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false, _menuStyle)
            {
                { InspectorLanguageSettingsMenuName, _aesirInspectorLanguageSettingsSO },
                { InspectorLoggerSettingsMenuName, _aesirInspectorDebugSettings }
            };

            tree.Config.DrawSearchToolbar = true;
            tree.EnumerateTree().AddThumbnailIcons().SortMenuItemsByName();
            return tree;
        }

        void CustomRebuild()
        {
            _lastSelection = MenuTree.Selection.SelectedValue;
            ForceMenuTreeRebuild();
            TrySelectMenuItemWithObject(_lastSelection);
        }
    }
}
