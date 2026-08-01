using System;
using System.Reflection;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration
{
    /// <summary>
    /// 通过反射封装 Odin 内部语法高亮处理器的静态工具类。
    /// </summary>
    public static class OdinCodeHighlighter
    {
        static readonly Type SyntaxHighlighterType = Type.GetType(
            "Sirenix.OdinInspector.Editor.Examples.SyntaxHighlighter," + "Sirenix.OdinInspector.Editor," +
            "Version=1.0.0.0," + "Culture=neutral," + "PublicKeyToken=null");

        static readonly MethodInfo ParseMethod =
            SyntaxHighlighterType?.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// 代码预览区域背景色。
        /// </summary>
        public static Color BackgroundColor { get; } = new Color(0.118f, 0.118f, 0.118f, 1f);

        /// <summary>
        /// 代码文本默认颜色。
        /// </summary>
        public static Color TextColor { get; } = new Color(0.863f, 0.863f, 0.863f, 1f);

        /// <summary>
        /// 对代码文本应用语法高亮，返回包含富文本标记的结果。
        /// </summary>
        public static string ApplyHighlighting(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return code;
            }

            if (ParseMethod != null)
            {
                try
                {
                    return ParseMethod.Invoke(null, new object[] { code }) as string ?? code;
                }
                catch (Exception e)
                {
                    // 忽略高亮异常，返回原始内容。插值字符串 ($"...") 可能导致 Odin 语法高亮器崩溃
                    Debug.LogWarning($"[AesirCodeHighlighter] 语法高亮解析失败 (通常是因为不支持插值字符串等 C# 新语法): {e.Message}");
                    return code;
                }
            }

            Debug.LogError("[AesirCodeHighlighter] 无法获取 Odin SyntaxHighlighter.Parse 方法");
            return code;
        }
    }
}
