using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

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
        /// 从 xml 中提取 Summary 的内容（压缩空白、剥离子标签、解码 XML 实体）。
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
                    // 解码 XML 实体（与 EscapeForXml 互逆）——未解码会让实体文本（如 &lt;）原样进特性，
                    // 回写时再次转义成 &amp;lt;（双重转义）
                    cleanedSummaryContent = DecodeXmlEntities(cleanedSummaryContent);
                    return cleanedSummaryContent;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 代码块中属于"XML 注释归属成员（块内首成员）"的 [Summary] 特性文本内容（已反转义）。
        /// 不存在、无法解析或首个 [Summary] 属于块内非首成员时为 null。
        /// </summary>
        public string SummaryAttributeContent
        {
            get
            {
                return GetSummaryAttribution(out var content) == SummaryAttribution.FirstMember
                    ? content
                    : null;
            }
        }

        /// <summary>
        /// 块内首成员声明之后是否存在任何 [Summary] 特性（属于块内其他成员）。
        /// 此时工具不能安全判断归属——Sync/Replace/Remove 三模式均跳过本块并告警
        /// （fail-closed，避免误删/误注非首成员的特性）。
        /// </summary>
        public bool HasNonFirstMemberSummaryAttribute
        {
            get
            {
                var targetCode = CodeAfterLeadingPreprocessor;
                return SummaryAttributeRegex.Match(targetCode, FindFirstDeclarationOffset(targetCode))
                    .Success;
            }
        }

        /// <summary>
        /// [Summary] 特性的归属分析结果。
        /// </summary>
        enum SummaryAttribution
        {
            /// <summary>块内无 [Summary] 特性。</summary>
            None,

            /// <summary>首个 [Summary] 位于首成员声明之前——属于 XML 注释的归属成员。</summary>
            FirstMember,

            /// <summary>首个 [Summary] 位于首成员声明之后——属于块内其他成员。</summary>
            NonFirstMember
        }

        /// <summary>
        /// 分析首个 [Summary] 特性的归属：位于首成员声明行之前才属于 XML 注释的归属成员。
        /// </summary>
        SummaryAttribution GetSummaryAttribution(out string content)
        {
            content = null;
            var targetCode = CodeAfterLeadingPreprocessor;
            var match = SummaryAttributeRegex.Match(targetCode);
            if (!match.Success)
            {
                return SummaryAttribution.None;
            }

            if (match.Index < FindFirstDeclarationOffset(targetCode))
            {
                content = UnescapeAttributeContent(match.Groups["content"].Value);
                return SummaryAttribution.FirstMember;
            }

            return SummaryAttribution.NonFirstMember;
        }

        /// <summary>
        /// 首成员声明的起始偏移：从块首跳过空白行、预处理行、特性行与单行注释行后的第一行。
        /// 块注释（/* */）不做跨行解析——按声明处理（保守：其后的特性判为非首成员，fail-closed）。
        /// </summary>
        static int FindFirstDeclarationOffset(string targetCode)
        {
            var offset = 0;
            while (offset < targetCode.Length)
            {
                var lineEnd = targetCode.IndexOf('\n', offset);
                var line = lineEnd < 0
                    ? targetCode.Substring(offset)
                    : targetCode.Substring(offset, lineEnd - offset);
                var trimmed = line.TrimStart();
                if (trimmed.Length == 0 || trimmed.StartsWith("#") || trimmed.StartsWith("[") ||
                    trimmed.StartsWith("//"))
                {
                    offset = lineEnd < 0 ? targetCode.Length : lineEnd + 1;
                    continue;
                }

                return offset;
            }

            return targetCode.Length;
        }

        /// <summary>
        /// 单遍反转义特性文本：\x → x（与 <see cref="EscapeSummaryText" /> 互逆）。
        /// </summary>
        static string UnescapeAttributeContent(string escaped)
        {
            if (escaped.IndexOf('\\') < 0)
            {
                return escaped;
            }

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

        /// <summary>
        /// 压缩连续空白并 Trim（与 <see cref="SummaryValue" /> 的最终规范化一致），用于幂等比较。
        /// </summary>
        static string NormalizeSummaryText(string text) => Regex.Replace(text, @"\s+", " ").Trim();

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
                    // 摘要块整块摘除后，其紧邻的空 /// 行成为无内容的孤儿注释行，一并清除；
                    // 只清剩余注释区的首尾——标签之间的空 /// 行是用户的段落排版，保留
                    processedXml = Regex.Replace(processedXml, @"^(?:[ \t]*///[ \t]*(?:\r?\n|$))+", "");
                    processedXml = Regex.Replace(processedXml, @"(?:\r?\n)?[ \t]*///[ \t]*(?:\r?\n)*$", "\n");
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
            // 缩进取行首空白但不含换行（\s 含 \n，xml 以空行开头时会得到跨行"缩进"导致注入行错位）
            var indent = Regex.Match(xml, @"^[ \t]*").Value;
            return indent + "[Summary(\"" + EscapeSummaryText(content) + "\")]\n";
        }

        /// <summary>
        /// 以指定文本回写 XML 的 summary 标签内容（单行形式，保留原缩进），用于特性优先的双向对齐。
        /// </summary>
        public string GetSummaryAlignedXml(string content) =>
            SummaryBlockRegex.Replace(xml,
                match => match.Groups["prefix"].Value + "<summary>" + EscapeForXml(content) + "</summary>");

        /// <summary>
        /// 获取删除了 SummaryAttribute 的代码。块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。
        /// </summary>
        public string GetReplaceAllOutput()
        {
            if (HasNonFirstMemberSummaryAttribute)
            {
                LogNonFirstMemberWarning(nameof(GetReplaceAllOutput));
                return xml + code;
            }

            return xml + LeadingPreprocessorLines + RemoveAllSummaryAttributeCode;
        }

        /// <summary>
        /// 获取同步 Summary 后的代码（双向对齐，[Summary] 特性优先）：
        /// 已有可解析的特性时特性为权威内容——与 XML 不一致则以特性文本回写 XML summary，代码保持原样；
        /// 无特性时维持原有行为——以 XML 内容生成特性，插在前导预处理指令之后。
        /// 块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。
        /// </summary>
        public string GetSyncOutput()
        {
            if (HasNonFirstMemberSummaryAttribute)
            {
                LogNonFirstMemberWarning(nameof(GetSyncOutput));
                return xml + code;
            }

            if (SummaryAttributeContent != null)
            {
                // 幂等比较：两侧做同等空白压缩，仅空白差异不触发回写
                if (XmlHasSummaryTag && NormalizeSummaryText(SummaryAttributeContent) != SummaryValue)
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
        /// 块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。
        /// </summary>
        public string GetReplaceOutput()
        {
            if (HasNonFirstMemberSummaryAttribute)
            {
                LogNonFirstMemberWarning(nameof(GetReplaceOutput));
                return xml + code;
            }

            var content = PreferredSummaryContent;
            if (content == null)
            {
                return xml + code;
            }

            return RemovedSummaryXml + LeadingPreprocessorLines + GetSummaryAttributeText(content) +
                   RemovedFirstSummaryAttributeCode;
        }

        /// <summary>
        /// 块内非首成员持有 [Summary] 时的跳过告警（fail-closed——不乱改用户代码）。
        /// </summary>
        static void LogNonFirstMemberWarning(string operation) =>
            Debug.LogWarning("[ScriptDocGenerator] " + operation +
                             "：检测到代码块内非首成员持有 [Summary] 特性，无法安全判断归属，已跳过该代码块。" +
                             "请为该成员补充独立的 XML 注释块，或手动调整 [Summary] 位置。");

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

        /// <summary>
        /// 解码 XML 文本实体。&amp;amp; 必须最后解码——与 <see cref="EscapeForXml" /> 的转义顺序互逆：
        /// 先解码 &amp;amp; 会把源码中的字面实体文本（如 &amp;amp;lt;，表示字符串 "&lt;"）错误地二次解码成 &lt;。
        /// </summary>
        static string DecodeXmlEntities(string text) =>
            text.Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'")
                .Replace("&amp;", "&");

        static bool IsPreprocessorDirective(string line) => line.TrimStart().StartsWith("#");
    }
}
