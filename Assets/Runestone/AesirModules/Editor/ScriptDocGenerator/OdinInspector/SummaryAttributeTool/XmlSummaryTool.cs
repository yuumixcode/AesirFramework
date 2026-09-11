using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// C# 脚本的 XML 中的 Summary 注释的处理器。
    /// 内容对齐方向（Sync/Replace）：[Summary] 特性优先——已有可解析特性时以特性文本为准（必要时回写 XML），
    /// 无特性时回退 XML summary 生成特性。仅 Remove 模式不做内容对齐。
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

        readonly string _newLine;

        public XmlSummaryTool(string sourceScript)
        {
            sourceScriptText = sourceScript ?? throw new ArgumentNullException(nameof(sourceScript));
            _newLine = DetectNewLine(sourceScriptText);
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
        /// 获取处理后的完整脚本内容。Remove 模式下若无 [Summary] 特性则原样返回（不重写格式）。
        /// </summary>
        public string GetProcessedSourceScript(ProcessMode processMode)
        {
            if (processMode == ProcessMode.RemoveSummary &&
                !xmlCodeParts.Any(p => HasSummaryAttribute(p.code)))
            {
                return sourceScriptText;
            }

            var processedScript = GetProcessedHeaderScript(processMode) + "\n";
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

            // 空行折叠仅作用于本工具改动引入的相邻空行（特性插入/删除后局部产生），不整文件重排版
            processedScript = Regex.Replace(processedScript, @"\n{3,}", "\n\n");
            return processedScript.Replace("\n", _newLine);
        }

        void InitializeSourceLines()
        {
            sourceScriptLines = sourceScriptText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();
        }

        /// <summary>
        /// 以原文件的实际行尾为准：首个出现的行尾序列决定输出行尾，无行尾的单行文件用 \n。
        /// </summary>
        static string DetectNewLine(string source)
        {
            var crlfIndex = source.IndexOf("\r\n", StringComparison.Ordinal);
            if (crlfIndex >= 0)
            {
                return "\r\n";
            }

            return source.IndexOf('\r') >= 0 ? "\r" : "\n";
        }

        static bool HasSummaryAttribute(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            return Regex.IsMatch(code, @"(?m)(?:^|\s)\s*\[Summary\(""(?<content>[\s\S]*?)""\)\]",
                RegexOptions.Multiline);
        }

        string GetProcessedHeaderScript(ProcessMode processMode)
        {
            var headerScript = HeaderScript;
            // using 仅在可能写入 [Summary] 特性的模式下注入；Remove 模式特性只减不增
            if (processMode == ProcessMode.RemoveSummary)
            {
                return headerScript;
            }

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

        /// <summary>
        /// XML 文档注释行：恰好三个斜杠开头。四斜杠（////）是普通注释，不参与分组。
        /// </summary>
        static bool IsXmlDocumentationLine(string line)
        {
            var trimmed = line.TrimStart();
            return trimmed.StartsWith("///") && (trimmed.Length == 3 || trimmed[3] != '/');
        }
    }
}
