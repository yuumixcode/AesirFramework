using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// C# 脚本的 XML 中的 Summary 注释的处理器。
    /// </summary>
    [Serializable]
    public class XmlSummaryTool
    {
        public enum ProcessMode
        {
            SyncSummary,
            ReplaceSummary,
            RemoveSummary
        }

        /// <summary>
        /// 原始源代码内容。
        /// </summary>
        public string sourceScriptText;

        /// <summary>
        /// 源代码按行分割后的列表。
        /// </summary>
        public List<string> sourceScriptLines;

        /// <summary>
        /// 第一个 XML 文档注释之前的所有代码行。
        /// </summary>
        public List<string> headerLines;

        /// <summary>
        /// 第一个 XML 文档注释的行号索引，从这一行开始处理 XML 文档注释。
        /// </summary>
        public int firstXmlCommentLineIndex = -1;

        /// <summary>
        /// XML 文档注释与代码块的组合列表，代码块是可能包含多个成员的。
        /// </summary>
        public List<XmlCodePart> xmlCodeParts = new List<XmlCodePart>();

        public XmlSummaryTool(string sourceScript)
        {
            sourceScriptText = sourceScript ?? throw new ArgumentNullException(nameof(sourceScript));
            InitializeSourceLines();
        }

        /// <summary>
        /// 获取第一个 XML 文档注释之前的所有代码行组成的字符串。
        /// </summary>
        public string HeaderScript => string.Join("\n", headerLines);

        /// <summary>
        /// 解析源脚本，将其分解为头部部分和 XML 文档注释与代码块的组合列表。
        /// </summary>
        public XmlSummaryTool ParseSourceScript()
        {
            ExtractHeaderLines();
            CreateXmlCodeParts();
            return this;
        }

        /// <summary>
        /// 获取处理后的完整脚本内容。
        /// </summary>
        public string GetProcessedSourceScript(ProcessMode processMode)
        {
            var processedScript = GetProcessedHeaderScript() + "\n";
            if (xmlCodeParts.Count > 0)
            {
                processedScript = xmlCodeParts.Aggregate(processedScript, (current, xmlCodePart) =>
                {
                    return processMode switch
                    {
                        ProcessMode.SyncSummary => current + xmlCodePart.GetSyncOutput(),
                        ProcessMode.ReplaceSummary => current + xmlCodePart.GetReplaceOutput(),
                        ProcessMode.RemoveSummary => current + xmlCodePart.GetReplaceAllOutput(),
                        _ => current
                    };
                });
            }

            processedScript = Regex.Replace(processedScript, @"(^\s*$\r?\n)", "\n", RegexOptions.Multiline);
            return processedScript;
        }

        void InitializeSourceLines()
        {
            sourceScriptLines = sourceScriptText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();
        }

        string GetProcessedHeaderScript()
        {
            var headerScript = HeaderScript;
            var match = Regex.Match(headerScript, @"namespace\s+([\w.]+)");
            if (match.Success)
            {
                var namespaceName = match.Groups[1].Value;
                if (namespaceName == typeof(SummaryAttribute).Namespace)
                {
                    return headerScript;
                }
            }

            if (!headerScript.Contains("using " + typeof(SummaryAttribute).Namespace + ";"))
            {
                headerScript = "using " + typeof(SummaryAttribute).Namespace + ";\n" + headerScript;
            }

            return headerScript;
        }

        /// <summary>
        /// 提取第一个 XML 文档注释之前的所有代码行，并标记第一个 XML 文档注释的行号索引。
        /// </summary>
        void ExtractHeaderLines()
        {
            headerLines = new List<string>();
            for (var i = 0; i < sourceScriptLines.Count; i++)
            {
                var line = sourceScriptLines[i];
                if (!IsXmlDocumentationLine(line))
                {
                    headerLines.Add(line);
                }
                else
                {
                    firstXmlCommentLineIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// 生成 XML 文档注释和代码块组合的列表。
        /// </summary>
        void CreateXmlCodeParts()
        {
            if (firstXmlCommentLineIndex == -1)
            {
                Debug.LogWarning("未在代码中发现 XML 文档注释（以 /// 开头的注释）");
                return;
            }

            xmlCodeParts = new List<XmlCodePart>();
            var currentXmlStartLine = firstXmlCommentLineIndex;
            while (currentXmlStartLine < sourceScriptLines.Count)
            {
                var (xmlComment, nextStartLine) = ExtractXmlCommentBlock(currentXmlStartLine);
                if (string.IsNullOrEmpty(xmlComment))
                {
                    break;
                }

                var (codeBlock, newXmlStartLine) = ExtractCodeBlock(nextStartLine);
                xmlCodeParts.Add(new XmlCodePart(xmlComment, codeBlock));
                currentXmlStartLine = newXmlStartLine;
            }
        }

        (string xmlComment, int nextStartLine) ExtractXmlCommentBlock(int startLine)
        {
            var xmlComment = string.Empty;
            var currentLine = startLine;
            for (var i = startLine; i < sourceScriptLines.Count; i++)
            {
                var line = sourceScriptLines[i];
                if (IsXmlDocumentationLine(line))
                {
                    xmlComment += line + "\n";
                    currentLine = i + 1;
                }
                else
                {
                    break;
                }
            }

            return (xmlComment, currentLine);
        }

        (string codeBlock, int nextXmlStartLine) ExtractCodeBlock(int startLine)
        {
            var codeBlock = string.Empty;
            var nextXmlStartLine = startLine;
            for (var i = startLine; i < sourceScriptLines.Count; i++)
            {
                var line = sourceScriptLines[i];
                if (i == sourceScriptLines.Count - 1)
                {
                    codeBlock += line;
                    nextXmlStartLine = i + 1;
                    break;
                }

                if (!IsXmlDocumentationLine(line))
                {
                    codeBlock += line + "\n";
                }
                else
                {
                    nextXmlStartLine = i;
                    break;
                }
            }

            return (codeBlock, nextXmlStartLine);
        }

        static bool IsXmlDocumentationLine(string line) => line.Trim().StartsWith("///");
    }
}
