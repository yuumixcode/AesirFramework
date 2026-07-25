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

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Search;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 Hierarchy 的编辑器安全工具类，不包括预制体的 Stage 场景。仅在编辑器阶段可用，打包后调用返回默认值。
    /// </summary>
    [Summary("关于 Hierarchy 的编辑器安全工具类，不包括预制体的 Stage 场景。仅在编辑器阶段可用，打包后调用返回默认值。")]
    public static class HierarchySafeEditorUtility
    {
        /// <summary>
        /// 获取 GameObject 的绝对路径。仅在编辑器阶段可用，打包后返回 string.Empty
        /// </summary>
        [Summary("获取 GameObject 的绝对路径。仅在编辑器阶段可用，打包后返回 string.Empty")]
        public static string GetAbsolutePath(GameObject obj) => GetAbsolutePath(obj.transform);

        /// <summary>
        /// 获取 Transform 的绝对路径。仅在编辑器阶段可用，打包后返回 string.Empty
        /// </summary>
        [Summary("获取 Transform 的绝对路径。仅在编辑器阶段可用，打包后返回 string.Empty")]
        public static string GetAbsolutePath(Transform trans)
        {
#if UNITY_EDITOR
            return SearchUtils.GetHierarchyPath(trans.gameObject, false).TrimStart('/');
#else
            return string.Empty;
#endif
        }
    }
}
