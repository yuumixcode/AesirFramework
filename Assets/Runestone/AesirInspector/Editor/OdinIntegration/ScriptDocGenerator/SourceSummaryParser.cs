#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 从源代码的 XML <c>/// &lt;summary&gt;</c> 注释中解析成员摘要。
    /// 供 <see cref="SourceSummaryInitializer"/> 和 <see cref="OdinAutoTooltipAttributeProcessor"/> 共享。
    /// </summary>
    public static class SourceSummaryParser
    {
        static readonly Regex _summaryContentRegex = new Regex(
            @"<summary>\s*(.*?)\s*</summary>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _xmlTagRegex = new Regex(
            @"<see\s+cref=""([^""]*)""\s*/?>|<[^>]+>",
            RegexOptions.Compiled);

        static readonly Regex _typeDeclRegex = new Regex(
            @"\b(class|struct|enum|interface)\s+(\w+)",
            RegexOptions.Compiled);

        static readonly Regex _multiSpaceRegex = new Regex(@"  +", RegexOptions.Compiled);

        /// <summary>
        /// 解析一个类型中所有成员的 summary 注释，返回 (成员名 → summary) 字典。
        /// </summary>
        public static Dictionary<string, string> ParseSummariesForType(Type type)
        {
            var lines = OdinSourceFileHelper.GetSourceLines(type);
            if (lines == null)
                return null;

            var typeKey = OdinSourceFileHelper.GetTypeKey(type);
            if (OdinSourceFileHelper.TryGetTypeBodyRange(lines, typeKey, out var bodyStart, out var bodyEnd))
            {
                var scopedLines = new string[bodyEnd - bodyStart + 1];
                Array.Copy(lines, bodyStart, scopedLines, 0, scopedLines.Length);
                return ExtractSummaries(scopedLines);
            }

            return ExtractSummaries(lines);
        }

        /// <summary>
        /// 从源代码行数组中提取所有 summary 注释，关联到紧随其后的成员声明。
        /// </summary>
        public static Dictionary<string, string> ExtractSummaries(string[] lines)
        {
            var result = new Dictionary<string, string>();
            int i = 0;

            while (i < lines.Length)
            {
                if (!StartsSummaryDocComment(lines[i].TrimStart()))
                {
                    i++;
                    continue;
                }

                var summaryLines = CollectSummaryDocLines(lines, ref i);
                if (summaryLines.Count == 0)
                    continue;

                i = SkipMemberPreambleLines(lines, i);

                while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
                    i++;

                if (i >= lines.Length)
                    break;

                var memberName = ResolveMemberName(lines[i]);
                if (memberName == null)
                    continue;

                var summary = ParseSummaryText(summaryLines);
                if (!string.IsNullOrWhiteSpace(summary))
                    result[memberName] = summary;
            }

            return result;
        }

        /// <summary>
        /// 清理 XML 标签：将 <c>&lt;see cref="A.B"/&gt;</c> 替换为 B，移除其他 XML 标签，折叠多余空格。
        /// </summary>
        public static string StripXmlTags(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = _xmlTagRegex.Replace(text, m =>
            {
                if (!m.Groups[1].Success)
                    return string.Empty;
                var cref = m.Groups[1].Value;
                var dot = cref.LastIndexOf('.');
                return dot >= 0 ? cref.Substring(dot + 1) : cref;
            });

            return _multiSpaceRegex.Replace(text, " ").Trim();
        }

        /// <summary>
        /// 从 summary 注释行列表中提取纯文本。
        /// </summary>
        public static string ParseSummaryText(List<string> summaryLines)
        {
            var fullSummary = string.Join(" ", summaryLines);
            var match = _summaryContentRegex.Match(fullSummary);
            var summary = match.Success
                ? match.Groups[1].Value.Trim()
                : fullSummary
                    .Replace("<summary>", string.Empty)
                    .Replace("</summary>", string.Empty)
                    .Trim();
            return string.IsNullOrWhiteSpace(summary) ? null : StripXmlTags(summary);
        }

        /// <summary>
        /// 判断一行是否以 <c>/// &lt;summary&gt;</c> 开头。
        /// </summary>
        public static bool StartsSummaryDocComment(string trimmedLine)
        {
            if (!trimmedLine.StartsWith("///", StringComparison.Ordinal))
                return false;

            var docContent = trimmedLine.Substring(3).TrimStart();
            return docContent.StartsWith("<summary>", StringComparison.Ordinal)
                || docContent.StartsWith("<summary ", StringComparison.Ordinal);
        }

        /// <summary>
        /// 从当前行开始收集连续的 <c>///</c> 注释行，直到遇到 <c>&lt;/summary&gt;</c> 或非注释行。
        /// </summary>
        public static List<string> CollectSummaryDocLines(string[] lines, ref int lineIndex)
        {
            var summaryLines = new List<string>();

            while (lineIndex < lines.Length)
            {
                var line = lines[lineIndex].TrimStart();
                if (!line.StartsWith("///", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(line))
                    break;

                if (line.StartsWith("///", StringComparison.Ordinal))
                    summaryLines.Add(line.Substring(3).Trim());

                lineIndex++;

                if (line.Contains("</summary>", StringComparison.Ordinal))
                    break;
            }

            while (lineIndex < lines.Length && lines[lineIndex].TrimStart().StartsWith("///", StringComparison.Ordinal))
                lineIndex++;

            return summaryLines;
        }

        /// <summary>
        /// 跳过成员声明前的预处理指令、注释和特性行。
        /// </summary>
        public static int SkipMemberPreambleLines(string[] lines, int lineIndex)
        {
            while (lineIndex < lines.Length)
            {
                var attrLine = lines[lineIndex].TrimStart();

                if (attrLine.StartsWith("#"))
                {
                    lineIndex++;
                    continue;
                }

                if (attrLine.StartsWith("//", StringComparison.Ordinal) && !attrLine.StartsWith("[", StringComparison.Ordinal))
                {
                    lineIndex++;
                    continue;
                }

                if (attrLine.StartsWith("[", StringComparison.Ordinal))
                {
                    if (OdinSourceFileHelper.IsFieldDeclarationLine(attrLine))
                        break;

                    lineIndex++;
                    continue;
                }

                break;
            }

            return lineIndex;
        }

        static string ResolveMemberName(string declarationLine)
        {
            var trimmed = declarationLine.TrimStart();

            // 类型自身的 summary：声明行就是 type definition，用类型名作为 key
            var typeMatch = _typeDeclRegex.Match(trimmed);
            if (typeMatch.Success && typeMatch.Index < 5)
                return typeMatch.Groups[2].Value;

            return OdinSourceFileHelper.ExtractMemberName(declarationLine.Trim());
        }
    }
}
#endif
