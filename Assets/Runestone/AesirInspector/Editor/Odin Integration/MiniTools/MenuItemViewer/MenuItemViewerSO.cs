// ----------------------------------------------------------------------------
// MIT License
// 
// Copyright (c) 2026 Runestone - Yuumix
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MenuItemViewer 可视化面板
    /// </summary>
    [Summary("MenuItemViewer 可视化面板")]
    public class MenuItemViewerSO : ScriptableObject, IAesirInspectorReset
    {
        [Summary("EditorBuildSettings 存储引用的 Key")]
        static readonly string ConfigName =
            OdinBridgeLocator.Bridge.GetFriendlyFullName(typeof(MenuItemViewerSO));

        [Summary("菜单单项检查器的菜单路径")]
        public static BilingualData ToolMenuPath = new BilingualData("菜单项检查器", "MenuItemViewer");

        [Summary("获取 MenuItemViewerSO 单例")]
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

        [Summary("将所有字段重置为默认值")]
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
