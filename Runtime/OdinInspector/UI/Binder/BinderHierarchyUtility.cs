using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 场景层级路径工具类，用于 Object Binder 计算物体在层级中的路径。
    /// <para>
    /// 提供绝对路径和相对路径两种计算方式：
    /// - 绝对路径：从场景根物体到目标的完整路径，用于 <see cref="BinderAssistant" /> 和 <see cref="BinderTag" /> 的路径标识。
    /// - 相对路径：子物体相对于父物体的路径，用于生成脚本中 <c>transform.Find()</c> 的参数。
    /// </para>
    /// </summary>
    internal static class BinderHierarchyUtility
    {
        /// <summary>
        /// 获取物体在场景层级中的绝对路径（从根物体到目标，以 <c>/</c> 分隔）。
        /// </summary>
        /// <param name="trans">目标 Transform。</param>
        /// <returns>绝对路径字符串，如 <c>Canvas/Panel/Button</c>。</returns>
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
        /// 获取子物体相对于父物体的路径。
        /// <para>
        /// 通过对比两条绝对路径的公共前缀，截取子路径部分。
        /// 若子路径不以父路径为前缀则报错（说明二者不是父子关系）。
        /// </para>
        /// </summary>
        /// <param name="parentPath">父物体的绝对路径。</param>
        /// <param name="childPath">子物体的绝对路径。</param>
        /// <returns>相对路径字符串（如 <c>Panel/Button</c>）；若不是父子关系则返回 null。</returns>
        public static string GetRelativePath(string parentPath, string childPath)
        {
            var parentPathArray = parentPath.Split('/');
            var childPathArray = childPath.Split('/');
            var targetPathList = new List<string>();

            // 校验：父路径必须是子路径的前缀，否则二者不是父子关系
            if (parentPathArray.Where((path, i) => childPathArray[i] != path).Any())
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag, "路径错误，并不是子物体");
                return null;
            }

            // 截取父路径之后的路径段作为相对路径
            for (var i = parentPathArray.Length; i < childPathArray.Length; i++)
            {
                targetPathList.Add(childPathArray[i]);
            }

            return string.Join("/", targetPathList);
        }
    }
}
