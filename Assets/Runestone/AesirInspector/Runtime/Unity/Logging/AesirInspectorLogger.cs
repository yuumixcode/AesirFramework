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
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 日志工具。编译后自动剔除，Console 双击可跳转到调用方。
    /// </summary>
    [Summary("Aesir Inspector 日志工具")]
    public static class AesirInspectorLogger
    {
        /// <summary>
        /// 输出信息日志，前缀 <c>[Aesir Inspector]</c> 显示为绿色。
        /// </summary>
        [Summary("输出信息日志")]
        [Conditional("UNITY_EDITOR")]
        public static void Info(string message)
        {
            if (!AesirInspectorLoggerSettings.IsInfoEnabled)
            {
                return;
            }

            Debug.Log($"<color=#00FF00>[Aesir Inspector]</color> {message}");
        }

        /// <summary>
        /// 输出警告日志，前缀 <c>[Aesir Inspector]</c> 显示为黄色。
        /// </summary>
        [Summary("输出警告日志")]
        [Conditional("UNITY_EDITOR")]
        public static void Warning(string message)
        {
            if (!AesirInspectorLoggerSettings.IsWarningEnabled)
            {
                return;
            }

            Debug.LogWarning($"<color=#FFFF00>[Aesir Inspector]</color> {message}");
        }

        /// <summary>
        /// 输出错误日志，前缀 <c>[Aesir Inspector]</c> 显示为红色。
        /// </summary>
        [Summary("输出错误日志")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_EDITOR")]
        public static void Error(string message)
        {
            Debug.LogError($"<color=#FF0000>[Aesir Inspector]</color> {message}");
        }

        /// <summary>
        /// 输出自定义前缀的信息日志，前缀显示为绿色。
        /// </summary>
        [Summary("输出自定义前缀的信息日志")]
        [Conditional("UNITY_EDITOR")]
        public static void Info(string prefix, string message)
        {
            if (!AesirInspectorLoggerSettings.IsInfoEnabled)
            {
                return;
            }

            Debug.Log($"<color=#00FF00>[{prefix}]</color> {message}");
        }

        /// <summary>
        /// 输出自定义前缀的警告日志，前缀显示为黄色。
        /// </summary>
        [Summary("输出自定义前缀的警告日志")]
        [Conditional("UNITY_EDITOR")]
        public static void Warning(string prefix, string message)
        {
            if (!AesirInspectorLoggerSettings.IsWarningEnabled)
            {
                return;
            }

            Debug.LogWarning($"<color=#FFFF00>[{prefix}]</color> {message}");
        }

        /// <summary>
        /// 输出自定义前缀的错误日志，前缀显示为红色。
        /// </summary>
        [Summary("输出自定义前缀的错误日志")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_EDITOR")]
        public static void Error(string prefix, string message)
        {
            Debug.LogError($"<color=#FF0000>[{prefix}]</color> {message}");
        }
    }
}
