using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 存储 UnityEditor.MenuItem 特性的参数信息
    /// </summary>
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

        [ShowInInspector]
        [EnableGUI]
        [DisplayAsString]
        [BilingualText("菜单项", "Menu Path")]
        public string MenuPath { get; }

        [ShowInInspector]
        [EnableGUI]
        [DisplayAsString]
        [BilingualText("优先级", "Priority")]
        public int Priority { get; }

        public bool IsValidateFunction { get; set; }

        public MethodInfo Method { get; set; }

        public Assembly Assembly { get; set; }

        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public string FullMethodSignature { get; set; }

        #region ISearchFilterable Members

        public bool IsMatch(string searchString) =>
            MenuPath.ToLower().Contains(searchString.ToLower()) ||
            MethodName.ToLower().Contains(searchString.ToLower());

        #endregion
    }
}
