using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 脚本文档生成器窗口，以 OdinMenuEditorWindow 布局展示四种工作模式。
    /// </summary>
    public class ScriptDocGeneratorWindow : OdinMenuEditorWindow
    {
        const string WindowName = "Script Doc Generator";

        static readonly BilingualData SingleScriptMenuName =
            new BilingualData("单脚本", "Single Script");

        static readonly BilingualData MultipleScriptsMenuName =
            new BilingualData("多脚本", "Multiple Scripts");

        static readonly BilingualData SingleAssemblyMenuName =
            new BilingualData("单程序集", "Single Assembly");

        static readonly BilingualData MultipleAssembliesMenuName =
            new BilingualData("多程序集", "Multiple Assemblies");

        SingleScriptPanelSO _singleScriptPanel;
        MultipleScriptsPanelSO _multipleScriptsPanel;
        SingleAssemblyPanelSO _singleAssemblyPanel;
        MultipleAssembliesPanelSO _multipleAssembliesPanel;

        static readonly string PanelsPath =
            AesirInspectorPaths.EditorDefaultResourcesPath + "/ScriptDocGenerator/Panels";

        OdinMenuStyle _menuStyle;

        static object _lastSelection;

        static SingleScriptPanelSO GetSingleScriptPanel() =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<SingleScriptPanelSO>(
                "SingleScriptPanel", PanelsPath, "SingleScriptPanel");

        static MultipleScriptsPanelSO GetMultipleScriptsPanel() =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<MultipleScriptsPanelSO>(
                "MultipleScriptsPanel", PanelsPath, "MultipleScriptsPanel");

        static SingleAssemblyPanelSO GetSingleAssemblyPanel() =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<SingleAssemblyPanelSO>(
                "SingleAssemblyPanel", PanelsPath, "SingleAssemblyPanel");

        static MultipleAssembliesPanelSO GetMultipleAssembliesPanel() =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<MultipleAssembliesPanelSO>(
                "MultipleAssembliesPanel", PanelsPath, "MultipleAssembliesPanel");

        protected override void OnEnable()
        {
            base.OnEnable();

            _singleScriptPanel = GetSingleScriptPanel();
            _multipleScriptsPanel = GetMultipleScriptsPanel();
            _singleAssemblyPanel = GetSingleAssemblyPanel();
            _multipleAssembliesPanel = GetMultipleAssembliesPanel();

            MenuWidth = 230f;
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

            ScriptDocGeneratorPanelBase.ToastRequested -= ShowToast;
            ScriptDocGeneratorPanelBase.ToastRequested += ShowToast;

            AesirInspectorLanguageSettingsSO.LanguageChanged -= CustomRebuild;
            AesirInspectorLanguageSettingsSO.LanguageChanged += CustomRebuild;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ScriptDocGeneratorPanelBase.ToastRequested -= ShowToast;
            AesirInspectorLanguageSettingsSO.LanguageChanged -= CustomRebuild;
            _lastSelection = null;
        }

        [MenuItem(AesirInspectorMenuItems.ScriptDocGenerator, false,
            AesirInspectorMenuItems.ScriptDocGeneratorOrder)]
        public static void OpenWindow()
        {
            var window = GetWindow<ScriptDocGeneratorWindow>();
            window.titleContent = new GUIContent(WindowName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 800);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false, _menuStyle)
            {
                { SingleScriptMenuName, _singleScriptPanel },
                { MultipleScriptsMenuName, _multipleScriptsPanel },
                { SingleAssemblyMenuName, _singleAssemblyPanel },
                { MultipleAssembliesMenuName, _multipleAssembliesPanel }
            };
            tree.Config.DrawSearchToolbar = false;
            tree.EnumerateTree().SortMenuItemsByName();
            return tree;
        }

        void CustomRebuild()
        {
            _lastSelection = MenuTree.Selection.SelectedValue;
            ForceMenuTreeRebuild();
            TrySelectMenuItemWithObject(_lastSelection);
        }

        new void ShowToast(ToastPosition position, SdfIconType icon, string message, Color color, float duration)
        {
            ShowNotification(new GUIContent(message), duration);
        }
    }
}
