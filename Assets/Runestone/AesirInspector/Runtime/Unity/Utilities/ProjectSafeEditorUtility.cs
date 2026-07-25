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

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Diagnostics;
using Object = UnityEngine.Object;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 Project 操作的编辑器安全工具类。仅在编辑器阶段可用，打包后自动剔除。
    /// </summary>
    [Summary("关于 Project 操作的编辑器安全工具类。仅在编辑器阶段可用，打包后自动剔除。")]
    public static class ProjectSafeEditorUtility
    {
        /// <summary>
        /// Ping 项目中的任何资源，可以是文件夹路径。传入相对路径。仅在编辑器阶段可用，打包后自动剔除。
        /// </summary>
        [Summary("Ping 项目中的任何资源，可以是文件夹路径。传入相对路径。")]
        [Conditional("UNITY_EDITOR")]
        public static void PingAndSelectAsset(string relativePath)
        {
#if UNITY_EDITOR
            if (!relativePath.StartsWith("Assets"))
            {
                AesirInspectorLogger.Error("相对路径必须以 Assets 开头");
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
#endif
        }
    }
}
