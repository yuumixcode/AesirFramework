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
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 MonoScript 的编辑器安全工具类。仅在编辑器阶段可用，打包后调用返回默认值。
    /// </summary>
    [Summary("关于 MonoScript 的编辑器安全工具类。仅在编辑器阶段可用，打包后调用返回默认值。")]
    public static class MonoScriptSafeEditorUtility
    {
        /// <summary>
        /// 在项目中根据脚本文件名称查找脚本文件，并在编辑器中选择。仅在编辑器阶段可用，打包后自动剔除。
        /// </summary>
        [Summary("在项目中根据脚本文件名称查找脚本文件，并在编辑器中选择。")]
        [Conditional("UNITY_EDITOR")]
        public static void SelectMonoScript(string scriptName)
        {
#if UNITY_EDITOR
            Selection.activeObject = GetMonoScript(scriptName);
#endif
        }
#if UNITY_EDITOR
        /// <summary>
        /// 在项目中根据脚本文件名称查找脚本文件，返回找到的 MonoScript。仅在编辑器阶段可用，打包后返回 null。
        /// </summary>
        [Summary("在项目中根据脚本文件名称查找脚本文件，返回找到的 MonoScript。")]
        public static MonoScript GetMonoScript(string scriptName)
        {
            MonoScript foundMonoScript = null;
            var scriptAssetPath = FindScriptPath(scriptName);
            if (!string.IsNullOrWhiteSpace(scriptAssetPath))
            {
                foundMonoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
            }

            return foundMonoScript;
        }
#endif
        /// <summary>
        /// 在项目中根据脚本文件名称查找脚本文件，返回脚本文件路径。仅在编辑器阶段可用，打包后返回 string.Empty
        /// </summary>
        [Summary("在项目中根据脚本文件名称查找脚本文件，返回脚本文件路径。")]
        public static string FindScriptPath(string scriptName)
        {
#if UNITY_EDITOR
            var scriptAssetPath = AssetDatabase.FindAssets("t:MonoScript " + scriptName)
                .Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            return !string.IsNullOrWhiteSpace(scriptAssetPath) ? scriptAssetPath : null;
#else
            return string.Empty;
#endif
        }
    }
}
