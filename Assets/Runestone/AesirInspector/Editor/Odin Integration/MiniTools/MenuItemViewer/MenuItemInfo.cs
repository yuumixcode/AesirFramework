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
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 存储 UnityEditor.MenuItem 特性的参数信息
    /// </summary>
    [Summary("存储 UnityEditor.MenuItem 特性的参数信息")]
    [Serializable]
    public class MenuItemInfo : ISearchFilterable
    {
        public MenuItemInfo(MenuItem menuItem, MethodInfo method)
        {
            MenuPath = menuItem.menuItem;
            IsValidateFunction = menuItem.validate;
            Priority = menuItem.priority;
            Method = method;
            Assembly = method.DeclaringType?.Assembly;
            ClassName = method.DeclaringType?.Name;
            MethodName = method.Name;
            FullMethodSignature = $"{ClassName}.{MethodName}()";
        }

        [Summary("菜单项")]
        [ShowInInspector]
        [EnableGUI]
        [DisplayAsString]
        [BilingualText("菜单项", "Menu Path")]
        public string MenuPath { get; }

        [Summary("优先级")]
        [ShowInInspector]
        [EnableGUI]
        [DisplayAsString]
        [BilingualText("优先级", "Priority")]
        public int Priority { get; }

        [Summary("是否是验证方法")]
        public bool IsValidateFunction { get; set; }

        [Summary("所属方法")]
        public MethodInfo Method { get; set; }

        [Summary("所属程序集")]
        public Assembly Assembly { get; set; }

        [Summary("所属类名")]
        public string ClassName { get; set; }

        [Summary("方法名")]
        public string MethodName { get; set; }

        [Summary("完整的方法签名")]
        public string FullMethodSignature { get; set; }

        #region ISearchFilterable Members

        [Summary("ISearchFilterable 接口方法，自定义搜索匹配规则")]
        public bool IsMatch(string searchString) =>
            MenuPath.ToLower().Contains(searchString.ToLower()) ||
            MethodName.ToLower().Contains(searchString.ToLower());

        #endregion
    }
}
