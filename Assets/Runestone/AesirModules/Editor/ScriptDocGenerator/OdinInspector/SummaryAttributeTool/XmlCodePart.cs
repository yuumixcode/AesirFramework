using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// XML 注释部分和代码块的组合。
    /// </summary>
    [Serializable]
    public class XmlCodePart
    {
        /// <summary>
        /// [Summary] 特性文本匹配。内容组支持转义引号，形如 [Summary("...")]；
        /// 拼接字符串实参（[Summary("a" + "b")]）等无法安全解析的形式不匹配——
        /// 此时按"无特性"处理，回退 XML 内容，宁可回退也不猜错。
        /// </summary>
        static readonly Regex SummaryAttributeRegex = new Regex(
            @"\[Summary\(""(?<content>(?:\\.|[^""\\])*)""\)]", RegexOptions.Compiled);

        /// <summary>
        /// XML summary 标签块匹配（捕获行首缩进与 /// 前缀），用于按特性内容回写。
        /// </summary>
        static readonly Regex SummaryBlockRegex = new Regex(@"(?<prefix>^[ \t]*///\s*)<summary>.*?</summary>",
            RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// 注释部分的源代码，以 /// 开头。
        /// </summary>
        public string xml;

        /// <summary>
        /// 不以注释开头的代码块，除了注释对应的成员外，可能包含多个成员。
        /// </summary>
        public string code;

        public XmlCodePart(string xml, string code)
        {
            this.xml = xml;
            this.code = code;
        }

        /// <summary>
        /// code 开头的连续预处理指令行（如 #if、#elif、#else），确保添加 [Summary] 时位于条件编译块内部。
        /// </summary>
        public string LeadingPreprocessorLines
        {
            get
            {
                if (string.IsNullOrEmpty(code))
                {
                    return string.Empty;
                }

                var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var count = 0;
                while (count < lines.Length && IsPreprocessorDirective(lines[count]))
                {
                    count++;
                }

                return count > 0 ? string.Join("\n", lines, 0, count) + "\n" : string.Empty;
            }
        }

        /// <summary>
        /// code 去掉开头预处理指令行后的内容。
        /// </summary>
        public string CodeAfterLeadingPreprocessor
        {
            get
            {
                var leading = LeadingPreprocessorLines;
                return leading.Length > 0 ? code.Substring(leading.Length) : code;
            }
        }

        /// <summary>
        /// XML 注释块中是否包含 summary 标签。
        /// </summary>
        public bool XmlHasSummaryTag => Regex.IsMatch(xml, @"///\s*<summary>");

        /// <summary>
        /// 从 xml 中提取 Summary 的内容。
        /// </summary>
        public string SummaryValue
        {
            get
            {
                var match = Regex.Match(xml, @"///\s*<summary>(.*?)</summary>", RegexOptions.Singleline);
                if (match.Success)
                {
                    var summaryContent = match.Groups[1].Value.Trim();
                    // 移除 XML 子标签（如 <param>, <returns> 等）
                    var cleanedSummaryContent = Regex.Replace(summaryContent, "<[^>]+>", "");
                    // 移除多余的注释符号（///）
                    cleanedSummaryContent = Regex.Replace(cleanedSummaryContent, @"^\s*///\s*", "",
                        RegexOptions.Multiline);
                    // 移除空行
                    cleanedSummaryContent = Regex.Replace(cleanedSummaryContent, @"^\s*$\r?\n", "",
                        RegexOptions.Multiline);
                    // 压缩连续的空白字符
                    cleanedSummaryContent = Regex.Replace(cleanedSummaryContent, @"\s+", " ").Trim();
                    return cleanedSummaryContent;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 代码块中首个 [Summary] 特性的文本内容（已反转义）。不存在或无法解析时为 null。
        /// </summary>
        public string SummaryAttributeContent
        {
            get
            {
                var match = SummaryAttributeRegex.Match(CodeAfterLeadingPreprocessor);
                if (!match.Success)
                {
                    return null;
                }

                var escaped = match.Groups["content"].Value;
                if (escaped.IndexOf('\\') < 0)
                {
                    return escaped;
                }

                // 单遍反转义：\x → x（与转义逻辑互逆，先查后替换避免 \\ 误拆）
                var sb = new StringBuilder(escaped.Length);
                for (var i = 0; i < escaped.Length; i++)
                {
                    if (escaped[i] == '\\' && i + 1 < escaped.Length)
                    {
                        i++;
                    }

                    sb.Append(escaped[i]);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// 首选 Summary 内容：[Summary] 特性优先（特性为权威源），无特性或特性无法解析时回退 XML summary。
        /// </summary>
        public string PreferredSummaryContent =>
            SummaryAttributeContent ?? (string.IsNullOrEmpty(SummaryValue) ? null : SummaryValue);

        /// <summary>
        /// 删除了 summary 标签部分的 xml。
        /// </summary>
        public string RemovedSummaryXml
        {
            get
            {
                var processedXml = xml;
                var match = Regex.Match(xml, @"///\s*<summary>(.*?)</summary>", RegexOptions.Singleline);
                if (match.Success)
                {
                    processedXml = xml.Replace(match.Value, "");
                    processedXml = Regex.Replace(processedXml, @"^\s*$\r?\n", "", RegexOptions.Multiline);
                }

                return processedXml;
            }
        }

        /// <summary>
        /// 删除了第一个 [Summary()] 部分的代码块（不含开头预处理指令行）。
        /// </summary>
        public string RemovedFirstSummaryAttributeCode
        {
            get
            {
                var targetCode = CodeAfterLeadingPreprocessor;
                var attr = nameof(SummaryAttribute).Replace("Attribute", "");
                var match = Regex.Match(targetCode,
                    @"(?m)(?:^|\s)\s*\[" + attr + @"\(""(?<content>[\s\S]*?)""\)\]", RegexOptions.Multiline);
                if (match.Success)
                {
                    targetCode = targetCode.Replace(match.Value, "");
                    targetCode = Regex.Replace(targetCode, @"^\s*$\r?\n", "", RegexOptions.Multiline);
                }

                return targetCode;
            }
        }

        /// <summary>
        /// 删除了所有 [Summary()] 部分的代码块（不含开头预处理指令行）。
        /// </summary>
        public string RemoveAllSummaryAttributeCode
        {
            get
            {
                var targetCode = CodeAfterLeadingPreprocessor;
                var attr = nameof(SummaryAttribute).Replace("Attribute", "");
                targetCode = Regex.Replace(targetCode,
                    @"(?m)(?:^|\s)\s*\[" + attr + @"\(""(?<content>[\s\S]*?)""\)\]", "",
                    RegexOptions.Multiline);
                targetCode = Regex.Replace(targetCode, @"^\s*$\r?\n", "", RegexOptions.Multiline);
                return targetCode;
            }
        }

        /// <summary>
        /// 获取以指定内容生成的 [Summary] 特性行（内容经 C# 转义，缩进与 XML 注释块一致）。
        /// </summary>
        public string GetSummaryAttributeText(string content)
        {
            var indent = Regex.Match(xml, @"^\s*").Value;
            return indent + "[Summary(\"" + EscapeSummaryText(content) + "\")]\n";
        }

        /// <summary>
        /// 以指定文本回写 XML 的 summary 标签内容（单行形式，保留原缩进），用于特性优先的双向对齐。
        /// </summary>
        public string GetSummaryAlignedXml(string content) =>
            SummaryBlockRegex.Replace(xml,
                match => match.Groups["prefix"].Value + "<summary>" + EscapeForXml(content) + "</summary>");

        /// <summary>
        /// 获取删除了 SummaryAttribute 的代码。
        /// </summary>
        public string GetReplaceAllOutput() =>
            xml + LeadingPreprocessorLines + RemoveAllSummaryAttributeCode;

        /// <summary>
        /// 获取同步 Summary 后的代码（双向对齐，[Summary] 特性优先）：
        /// 已有可解析的特性时特性为权威内容——与 XML 不一致则以特性文本回写 XML summary，代码保持原样；
        /// 无特性时维持原有行为——以 XML 内容生成特性，插在前导预处理指令之后。
        /// </summary>
        public string GetSyncOutput()
        {
            if (SummaryAttributeContent != null)
            {
                if (XmlHasSummaryTag && SummaryAttributeContent != SummaryValue)
                {
                    return GetSummaryAlignedXml(SummaryAttributeContent) + code;
                }

                return xml + code;
            }

            if (string.IsNullOrEmpty(SummaryValue))
            {
                return xml + code;
            }

            return xml + LeadingPreprocessorLines + GetSummaryAttributeText(SummaryValue) +
                   RemovedFirstSummaryAttributeCode;
        }

        /// <summary>
        /// 获取替换了 summary 标签的代码：内容首选 [Summary] 特性（特性为权威源），无特性时取 XML；
        /// 移除 XML summary 标签，并在前导预处理指令之后写入单行 [Summary] 特性。
        /// </summary>
        public string GetReplaceOutput()
        {
            var content = PreferredSummaryContent;
            if (content == null)
            {
                return xml + code;
            }

            return RemovedSummaryXml + LeadingPreprocessorLines + GetSummaryAttributeText(content) +
                   RemovedFirstSummaryAttributeCode;
        }

        /// <summary>
        /// 转义文本中会破坏 C# 字符串字面量的字符：先转义反斜杠，再转义双引号。
        /// </summary>
        static string EscapeSummaryText(string content) =>
            content.Replace("\\", "\\\\").Replace("\"", "\\\"");

        /// <summary>
        /// 转义会破坏 XML 文本节点的字符（&amp; &lt; &gt;）。
        /// </summary>
        static string EscapeForXml(string text) =>
            text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        static bool IsPreprocessorDirective(string line) => line.TrimStart().StartsWith("#");
    }
}
