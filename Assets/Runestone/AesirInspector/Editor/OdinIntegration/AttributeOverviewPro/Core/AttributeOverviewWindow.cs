using System.Linq;
using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Aesir Inspector 特性总览编辑器窗口，以可搜索的树形菜单展示所有已注册的特性面板。
    /// </summary>
    public class AttributeOverviewWindow : OdinMenuEditorWindow
    {
        static AttributeOverviewWindow _window;
        AttributeOverviewDatabaseSO _databaseSO;

        /// <summary>
        /// 打开 Attribute Overview 窗口。
        /// </summary>
        [MenuItem(AesirInspectorMenuItems.AttributeOverviewPro, false,
            AesirInspectorMenuItems.AttributeOverviewProOrder)]
        public static void OpenWindow()
        {
            _window = GetWindow<AttributeOverviewWindow>("Attribute Overview Pro");
            _window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1050, 750);
            _window.Show();
        }

        protected override void Initialize()
        {
            _databaseSO = AttributeOverviewDatabaseSO.Instance;
            WindowPadding = new Vector4(15, 15, 15, 5);
            MenuWidth = 230f;
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree
            {
                Config =
                {
                    DrawSearchToolbar = true,
                    SearchTerm = ""
                },
                DefaultMenuStyle = new OdinMenuStyle
                {
                    Height = 24
                }
            };

            tree.Config.SearchFunction = menuItem =>
            {
                var str = menuItem.Name.ToLower().Replace(" ", "");
                var searchStr = tree.Config.SearchTerm.ToLower().Replace(" ", "");
                return str.Contains(searchStr);
            };

            if (_databaseSO == null || _databaseSO.AttributePanelArrayMap == null)
            {
                return tree;
            }

            // 按照分类数组映射的顺序添加，以保证 Essentials 在首位
            var categories = new[]
            {
                nameof(AesirAttributeCategory.Essentials),
                nameof(AesirAttributeCategory.Buttons),
                nameof(AesirAttributeCategory.Collections),
                nameof(AesirAttributeCategory.Groups),
                nameof(AesirAttributeCategory.Conditionals),
                nameof(AesirAttributeCategory.Numbers),
                nameof(AesirAttributeCategory.TypeSpecifics),
                nameof(AesirAttributeCategory.Validation),
                nameof(AesirAttributeCategory.Misc),
                nameof(AesirAttributeCategory.Meta),
                nameof(AesirAttributeCategory.Unity),
                nameof(AesirAttributeCategory.Debug)
            };

            foreach (var category in categories)
            {
                if (!_databaseSO.AttributePanelArrayMap.TryGetValue(category, out var panels) ||
                    panels == null)
                {
                    continue;
                }

                // 分类内部按显示名称排序
                var sortedPanels = panels
                    .Where(p => p != null && p.BilingualHeaderControl?.headerName != null)
                    .OrderBy(p => p.BilingualHeaderControl.headerName.ChineseDisplay).ToArray();

                foreach (var panel in sortedPanels)
                {
                    var menuName = panel.BilingualHeaderControl.headerName.ChineseDisplay;
                    var path = category + "/" + menuName;
                    tree.AddObjectAtPath(path, panel);
                }
            }

            return tree;
        }
    }
}
