using System;
using System.Linq;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Path 路径字符串工具类，提供路径相关的操作方法
    /// </summary>
    public static class PathUtility
    {
        /// <summary>
        /// 将路径中的反斜杠替换为正斜杠
        /// </summary>
        public static string ToUnityPath(string path) =>
            string.IsNullOrEmpty(path) ? string.Empty : path.Replace("\\", "/");

        /// <summary>
        /// 尝试获取完整路径中以目标字符串结尾的子路径，匹配最后一个出现的目标字符串。如果没有找到，返回 false
        /// </summary>
        public static bool TryGetSubPathWithSpecialEnd(string fullRelativePath,
            string endWithString,
            out string subPath)
        {
            if (string.IsNullOrWhiteSpace(fullRelativePath) || string.IsNullOrWhiteSpace(endWithString))
            {
                AesirInspectorDebug.Error("路径或目标字符串不能为空！");
                subPath = string.Empty;
                return false;
            }

            fullRelativePath = ToUnityPath(fullRelativePath);
            if (!fullRelativePath.StartsWith("Assets"))
            {
                AesirInspectorDebug.Error("完整路径不是以 Assets 开头的，需要使用相对路径。");
                subPath = string.Empty;
                return false;
            }

            var splits = fullRelativePath.Split('/');
            var finalIndex = Array.LastIndexOf(splits, endWithString);
            if (finalIndex == -1)
            {
                AesirInspectorDebug.Error($"完整路径中没有找到能够以 {endWithString} 为结尾的子路径。");
                subPath = string.Empty;
                return false;
            }

            subPath = string.Join("/", splits.Take(finalIndex + 1));
            return true;
        }

        /// <summary>
        /// 合并两个路径字符串并规范化为 Unity 格式
        /// </summary>
        public static string CombinePath(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return b;
            }

            if (string.IsNullOrEmpty(b))
            {
                return a;
            }

            a = ToUnityPath(a).TrimEnd('/');
            b = ToUnityPath(b).TrimStart('/');
            return a + "/" + b;
        }
    }
}
