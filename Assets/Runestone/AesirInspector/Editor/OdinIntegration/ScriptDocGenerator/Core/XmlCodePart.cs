using System;
using System.Text.RegularExpressions;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// XML 注释部分和代码块的组合。
    /// </summary>
    [Serializable]
    public class XmlCodePart
    {
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
        /// 从 xml 中提取 Summary 的内容。
        /// </summary>
        public string SummaryValue
        {
            get
            {
                var match = Regex.Match(xml, "/// <summary>(.*?)</summary>", RegexOptions.Singleline);
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
        /// 获取 SummaryAttribute 的代码文本。
        /// </summary>
        public string SummaryAttributeText
        {
            get
            {
                if (string.IsNullOrEmpty(SummaryValue))
                {
                    return string.Empty;
                }

                var indent = Regex.Match(xml, @"^\s*").Value;
                var attr = nameof(SummaryAttribute).Replace("Attribute", "");
                return indent + "[" + attr + "(\"" + SummaryValue + "\")]\n";
            }
        }

        /// <summary>
        /// 删除了 summary 标签部分的 xml。
        /// </summary>
        public string RemovedSummaryXml
        {
            get
            {
                var processedXml = xml;
                var match = Regex.Match(xml, @"/// <summary>(.*?)</summary>", RegexOptions.Singleline);
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
        /// 获取删除了 SummaryAttribute 的代码。
        /// </summary>
        public string GetReplaceAllOutput() =>
            xml + LeadingPreprocessorLines + RemoveAllSummaryAttributeCode;

        /// <summary>
        /// 获取同步 Summary 后的代码。
        /// </summary>
        public string GetSyncOutput() =>
            xml + LeadingPreprocessorLines + SummaryAttributeText + RemovedFirstSummaryAttributeCode;

        /// <summary>
        /// 获取替换了 summary 标签的代码。
        /// </summary>
        public string GetReplaceOutput() =>
            RemovedSummaryXml + LeadingPreprocessorLines + SummaryAttributeText +
            RemovedFirstSummaryAttributeCode;

        static bool IsPreprocessorDirective(string line) => line.TrimStart().StartsWith("#");
    }
}
