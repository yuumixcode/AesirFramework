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

using System.Diagnostics;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 Path 路径的编辑器安全工具类。仅在编辑器阶段可用，打包后调用自动剔除。
    /// </summary>
    [Summary("关于 Path 路径的编辑器安全工具类。仅在编辑器阶段可用，打包后调用自动剔除。")]
    public static class PathSafeEditorUtility
    {
        /// <summary>
        /// 确保 Assets 目录下的相对路径的文件夹存在，如果不存在则递归创建。仅在编辑器阶段可用，打包后自动剔除。
        /// </summary>
        [Summary("确保 Assets 目录下的相对路径的文件夹存在，如果不存在则递归创建。")]
        [Conditional("UNITY_EDITOR")]
        public static void EnsureDirectoryExists(string relativePath)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var fullPath = PathUtility.ToUnityPath(relativePath.Trim());
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
#endif
        }
    }
}
