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
using System.Text.RegularExpressions;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 正则表达式工具类，提供命名空间、类名及常用数据格式的正则校验和规范化方法
    /// </summary>
    [Summary("正则表达式工具类，提供命名空间、类名及常用数据格式的正则校验和规范化方法")]
    public static class RegexUtility
    {
        /// <summary>
        /// 匹配 XML 的 Summary，直到下一个分号、大括号
        /// </summary>
        [Summary("匹配 XML 的 Summary，直到下一个分号、大括号")]
        public static readonly Regex XmlToDeclarationEndRegex =
            new Regex(@"(\s*///\s*<summary>(.*?)</summary>\s*)(.*?)((?=;|\{))",
                RegexOptions.Singleline | RegexOptions.Multiline);

        /// <summary>
        /// 不合法的命名空间正则表达式
        /// </summary>
        [Summary("不合法的命名空间正则表达式")]
        public static readonly Regex InvalidNamespaceRegex =
            new Regex(@"([^a-zA-Z0-9._]|[\s]|::|\b(using)\b|\.{2,})");

        /// <summary>
        /// 常用的邮箱匹配正则表达式
        /// </summary>
        [Summary("常用的邮箱匹配正则表达式")]
        public static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        /// <summary>
        /// 常用的 URL 匹配正则表达式
        /// </summary>
        [Summary("常用的 URL 匹配正则表达式")]
        public static readonly Regex WebUrlRegex = new Regex(@"^(http|https)://[^\s/$.?#].[^\s]*$");

        /// <summary>
        /// 规范化命名空间
        /// </summary>
        [Summary("规范化命名空间")]
        public static string CanonicalNamespace(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var foundFirstLetter = false;
            var firstValidator = new StringBuilder();

            foreach (var c in input)
            {
                if (char.IsLetter(c))
                {
                    foundFirstLetter = true;
                    firstValidator.Append(c);
                }
                else if (foundFirstLetter)
                {
                    firstValidator.Append(c);
                }
            }

            var firstResult = firstValidator.ToString();
            return InvalidNamespaceRegex.Replace(firstResult, "");
        }

        /// <summary>
        /// 规范化脚本类名
        /// </summary>
        [Summary("规范化脚本类名")]
        public static string CanonicalScriptClassName(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                return string.Empty;
            }

            const string canonicalScriptClassNameRegex = @"([^\w\u4e00-\u9fa5])";
            className = Regex.Replace(className, canonicalScriptClassNameRegex, "");

            if (className.Length > 0)
            {
                className = char.ToUpper(className[0]) + className[1..];
            }

            return className;
        }

        /// <summary>
        /// 检查字符串是否匹配指定的正则表达式
        /// </summary>
        [Summary("检查字符串是否匹配指定的正则表达式")]
        public static bool IsMatch(string input, string pattern) =>
            !string.IsNullOrEmpty(input) && Regex.IsMatch(input, pattern);

        /// <summary>
        /// 检查字符串是否为有效的电子邮箱格式
        /// </summary>
        [Summary("检查字符串是否为有效的电子邮箱格式")]
        public static bool IsEmail(string input) =>
            !string.IsNullOrEmpty(input) && EmailRegex.IsMatch(input);
    }
}
