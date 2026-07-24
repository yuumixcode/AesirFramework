using System.Collections.Generic;
using System.Linq;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 场景层级路径工具类，用于 Object Binder 计算物体在层级中的路径
    /// </summary>
    internal static class BinderHierarchyUtility
    {
        /// <summary>
        /// 获取物体在场景层级中的绝对路径
        /// </summary>
        public static string GetAbsolutePath(Transform trans)
        {
            var path = trans.name;
            var parent = trans.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        /// <summary>
        /// 获取子物体相对于父物体的路径
        /// </summary>
        public static string GetRelativePath(string parentPath, string childPath)
        {
            var parentPathArray = parentPath.Split('/');
            var childPathArray = childPath.Split('/');
            var targetPathList = new List<string>();

            if (parentPathArray.Where((path, i) => childPathArray[i] != path).Any())
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag, "路径错误，并不是子物体");
                return null;
            }

            for (var i = parentPathArray.Length; i < childPathArray.Length; i++)
            {
                targetPathList.Add(childPathArray[i]);
            }

            return string.Join("/", targetPathList);
        }
    }
}
