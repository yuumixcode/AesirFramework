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

using System.Text;
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 层级工具类，提供 Transform 层级路径相关的操作方法
    /// </summary>
    [Summary("层级工具类，提供 Transform 层级路径相关的操作方法")]
    public static class HierarchyUtility
    {
        /// <summary>
        /// 获取物体在层级中的完整路径
        /// </summary>
        [Summary("获取物体在层级中的完整路径")]
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
        [Summary("获取子物体相对于父物体的路径")]
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
                AesirInspectorLogger.Warning($"{child.name} 不是 {parent.name} 的子物体");
                return null;
            }

            return path;
        }

        /// <summary>
        /// 获取子物体相对于父物体的路径（基于路径字符串）
        /// </summary>
        [Summary("获取子物体相对于父物体的路径（基于路径字符串）")]
        public static string GetRelativePath(string parentPath, string childPath)
        {
            if (string.IsNullOrEmpty(parentPath))
            {
                AesirInspectorLogger.Error("父物体路径为空");
                return "ParentPath == null";
            }

            if (childPath == parentPath)
            {
                return string.Empty;
            }

            if (!childPath.StartsWith(parentPath + "/"))
            {
                AesirInspectorLogger.Error("路径错误，并不是子物体");
                return null;
            }

            return childPath.Substring(parentPath.Length + 1);
        }

        /// <summary>
        /// 递归查找深层子物体
        /// </summary>
        [Summary("递归查找深层子物体")]
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
