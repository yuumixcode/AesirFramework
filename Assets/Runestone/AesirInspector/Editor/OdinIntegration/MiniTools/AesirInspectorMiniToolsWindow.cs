using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Aesir Inspector Mini Tools 窗口，整合常用编辑器小工具入口
    /// </summary>
    public class AesirInspectorMiniToolsWindow : OdinMenuEditorWindow
    {
        static readonly BilingualData SyntaxHighlighterMenuName =
            new BilingualData("语法高亮处理器", "Syntax Highlighter");

        static readonly BilingualData MenuItemViewerMenuName = new BilingualData("菜单项查看器", "MenuItem Viewer");

        static object _lastSelection;

        OdinSyntaxHighlighterPanelSO _highlighterPanel;
        MenuItemViewerSO _menuItemViewer;
        OdinMenuStyle _menuStyle;

        protected override void OnEnable()
        {
            base.OnEnable();
            _highlighterPanel = OdinSyntaxHighlighterPanelSO.Instance;
            _menuItemViewer = MenuItemViewerSO.Instance;
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
        /// 打开 Aesir Inspector Mini Tools 窗口
        /// </summary>
        [MenuItem(AesirInspectorMenuItems.MiniTools, false, AesirInspectorMenuItems.MiniToolsOrder)]
        public static void Open()
        {
            var window = GetWindow<AesirInspectorMiniToolsWindow>();
            window.titleContent = new GUIContent("Aesir Inspector Mini Tools");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900, 800);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false, _menuStyle)
            {
                { SyntaxHighlighterMenuName, _highlighterPanel },
                { MenuItemViewerMenuName, _menuItemViewer }
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
