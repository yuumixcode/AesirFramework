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
using System.Linq;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Path 路径字符串工具类，提供路径相关的操作方法
    /// </summary>
    [Summary("Path 路径字符串工具类，提供路径相关的操作方法")]
    public static class PathUtility
    {
        /// <summary>
        /// 将路径中的反斜杠替换为正斜杠
        /// </summary>
        [Summary("将路径中的反斜杠替换为正斜杠")]
        public static string ToUnityPath(string path) =>
            string.IsNullOrEmpty(path) ? string.Empty : path.Replace("\\", "/");

        /// <summary>
        /// 尝试获取完整路径中以目标字符串结尾的子路径，匹配最后一个出现的目标字符串。如果没有找到，返回 false
        /// </summary>
        [Summary("尝试获取完整路径中以目标字符串结尾的子路径，匹配最后一个出现的目标字符串。如果没有找到，返回 false")]
        public static bool TryGetSubPathWithSpecialEnd(string fullRelativePath,
            string endWithString,
            out string subPath)
        {
            if (string.IsNullOrWhiteSpace(fullRelativePath) || string.IsNullOrWhiteSpace(endWithString))
            {
                AesirInspectorLogger.Error("路径或目标字符串不能为空！");
                subPath = string.Empty;
                return false;
            }

            fullRelativePath = ToUnityPath(fullRelativePath);
            if (!fullRelativePath.StartsWith("Assets"))
            {
                AesirInspectorLogger.Error("完整路径不是以 Assets 开头的，需要使用相对路径。");
                subPath = string.Empty;
                return false;
            }

            var splits = fullRelativePath.Split('/');
            var finalIndex = Array.LastIndexOf(splits, endWithString);
            if (finalIndex == -1)
            {
                AesirInspectorLogger.Error($"完整路径中没有找到能够以 {endWithString} 为结尾的子路径。");
                subPath = string.Empty;
                return false;
            }

            subPath = string.Join("/", splits.Take(finalIndex + 1));
            return true;
        }

        /// <summary>
        /// 合并两个路径字符串并规范化为 Unity 格式
        /// </summary>
        [Summary("合并两个路径字符串并规范化为 Unity 格式")]
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
