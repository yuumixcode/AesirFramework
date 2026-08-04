using System.Text;
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 层级工具类，提供 Transform 层级路径相关的操作方法
    /// </summary>
    public static class HierarchyUtility
    {
        /// <summary>
        /// 获取物体在层级中的完整路径
        /// </summary>
        public static string GetFullPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(transform.name);
            while (transform.parent != null)
            {
                transform = transform.parent;
                sb.Insert(0, "/");
                sb.Insert(0, transform.name);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取子物体相对于父物体的路径
        /// </summary>
        public static string GetRelativePath(Transform parent, Transform child)
        {
            if (parent == null || child == null)
            {
                return string.Empty;
            }

            if (parent == child)
            {
                return string.Empty;
            }

            var path = child.name;
            var current = child.parent;
            while (current != null && current != parent)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            if (current != parent)
            {
                AesirInspectorDebug.Warning($"{child.name} 不是 {parent.name} 的子物体");
                return null;
            }

            return path;
        }

        /// <summary>
        /// 获取子物体相对于父物体的路径（基于路径字符串）
        /// </summary>
        public static string GetRelativePath(string parentPath, string childPath)
        {
            if (string.IsNullOrEmpty(parentPath))
            {
                AesirInspectorDebug.Error("父物体路径为空");
                return "ParentPath == null";
            }

            if (childPath == parentPath)
            {
                return string.Empty;
            }

            if (!childPath.StartsWith(parentPath + "/"))
            {
                AesirInspectorDebug.Error("路径错误，并不是子物体");
                return null;
            }

            return childPath.Substring(parentPath.Length + 1);
        }

        /// <summary>
        /// 递归查找深层子物体
        /// </summary>
        public static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }

                var result = FindDeepChild(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
