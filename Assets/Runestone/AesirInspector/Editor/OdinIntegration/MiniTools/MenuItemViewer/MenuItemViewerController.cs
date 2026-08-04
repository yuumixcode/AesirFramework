using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MenuItemViewer 逻辑控制类，使用反射获取所有程序集中的 UnityEditor.MenuItem 特性
    /// </summary>
    public static class MenuItemViewerController
    {
        public static List<MenuItemInfo> GetAllMenuItems(IAssemblyFilter assemblyFilter = null)
        {
            var menuItems = new List<MenuItemInfo>();
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allAssemblies = loadedAssemblies.Where(assembly =>
                assemblyFilter == null || !assemblyFilter.ShouldFilterOut(assembly)).ToList();
            AesirInspectorDebug.Info($"收集 {allAssemblies.Count} 个程序集中的 MenuItem");
            foreach (var assembly in allAssemblies)
            {
                ProcessAssembly(assembly, menuItems);
            }

            menuItems.Sort((a, b) =>
            {
                var pathComparison = string.Compare(a.MenuPath, b.MenuPath, StringComparison.Ordinal);
                return pathComparison != 0 ? pathComparison : a.Priority.CompareTo(b.Priority);
            });
            AesirInspectorDebug.Info($"一共发现 {menuItems.Count} 个 MenuItem");
            return menuItems;
        }

        static void ProcessAssembly(Assembly assembly, List<MenuItemInfo> menuItems)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var methods =
                    type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (var method in methods)
                {
                    if (method.GetCustomAttributes(typeof(MenuItem), false) is MenuItem[]
                        {
                            Length: > 0
                        } menuItemAttributes)
                    {
                        menuItems.AddRange(menuItemAttributes.Select(menuItem =>
                            new MenuItemInfo(menuItem, method)));
                    }
                }
            }
        }
    }
}
