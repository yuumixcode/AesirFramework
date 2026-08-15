using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MenuItemViewer 可视化面板
    /// </summary>
    public class MenuItemViewerSO : ScriptableObject, IAesirInspectorReset
    {
        static readonly string ConfigName =
            typeof(MenuItemViewerSO).GetNiceFullName();

        public static BilingualData ToolMenuPath = new BilingualData("菜单项检查器", "MenuItemViewer");

        public static MenuItemViewerSO Instance =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<MenuItemViewerSO>(ConfigName,
                AesirInspectorPaths.MiniToolsAssetsFolderPath, "MenuItemViewer");

        #region Event Functions

        void OnEnable()
        {
            bilingualHeaderControl = new BilingualHeaderControl("MenuItem 查看器", "MenuItem Viewer",
                "查看项目内的 MenuItem 的信息，便于规划菜单项",
                "View the information of MenuItems within the project to facilitate menu item planning",
                AesirInspectorWebLinks.GitUrl);
        }

        #endregion

        #region IAesirInspectorReset Members

        public void AesirInspectorReset()
        {
            assemblyFilter = null;
            menuItemInfos = null;
        }

        #endregion

        [PropertySpace(8, 8)]
        [BilingualButton("搜集项目所有菜单项，排除筛选项", "Collect MenuItems Exclude Filter", ButtonSizes.Large)]
        public void CollectMenuItems()
        {
            menuItemInfos = MenuItemViewerController.GetAllMenuItems(assemblyFilter);
        }

        #region Serialized Fields

        public BilingualHeaderControl bilingualHeaderControl;

        [PropertySpace]
        [SerializeReference]
        [BilingualTitle("剔除特定程序集的菜单项", "Exclude MenuItems from Specific Assembly")]
        [HideLabel]
        public IAssemblyFilter assemblyFilter;

        [PropertyOrder(10)]
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        public List<MenuItemInfo> menuItemInfos;

        #endregion
    }
}
