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
    [Summary("MenuItemViewer 逻辑控制类，使用反射获取所有程序集中的 UnityEditor.MenuItem 特性")]
    public static class MenuItemViewerController
    {
        [Summary("获取经过筛选的程序集中的所有 MenuItem 特性信息")]
        public static List<MenuItemInfo> GetAllMenuItems(IAssemblyFilter assemblyFilter = null)
        {
            var menuItems = new List<MenuItemInfo>();
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allAssemblies = loadedAssemblies.Where(assembly =>
                assemblyFilter == null || !assemblyFilter.ShouldFilterOut(assembly)).ToList();
            AesirInspectorLogger.Info($"收集 {allAssemblies.Count} 个程序集中的 MenuItem");
            foreach (var assembly in allAssemblies)
            {
                ProcessAssembly(assembly, menuItems);
            }

            menuItems.Sort((a, b) =>
            {
                var pathComparison = string.Compare(a.MenuPath, b.MenuPath, StringComparison.Ordinal);
                return pathComparison != 0 ? pathComparison : a.Priority.CompareTo(b.Priority);
            });
            AesirInspectorLogger.Info($"一共发现 {menuItems.Count} 个 MenuItem");
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
