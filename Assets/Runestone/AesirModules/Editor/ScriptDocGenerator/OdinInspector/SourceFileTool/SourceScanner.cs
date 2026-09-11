#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 单遍字符级状态机源码扫描器：
    /// 第一遍净化——字符串（普通/逐字）、字符字面量、行注释、块注释内容置空，产出净化行，
    /// 使后续正则天然免疫字符串/注释里的假类型声明、假命名空间与假花括号；
    /// 第二遍扫描——命名空间栈（块式/文件级）+ 类型栈（花括号深度配对）单次前向扫描，
    /// 将 /// 文档注释块关联到其后的声明行，产出结构化 <see cref="ParsedSourceDoc" />。
    /// </summary>
    public static class SourceScanner
    {
        static readonly Regex _typeDeclRegex = new Regex(@"\b(class|struct|enum|interface|record)\s+(\w+)",
            RegexOptions.Compiled);

        static readonly Regex _recordKeywordRegex = new Regex(
            @"\brecord\s+(?:struct|class)\s+(\w+)", RegexOptions.Compiled);

        static readonly Regex _namespaceRegex = new Regex(@"^\s*namespace\s+([\w.]+)", RegexOptions.Compiled);

        static readonly Regex _leadingAttributesRegex =
            new Regex(@"^(\s*\[.*?\]\s*)+", RegexOptions.Compiled);

        static readonly Regex _summaryTagRegex = new Regex(@"<summary>\s*(.*?)\s*</summary>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _returnsTagRegex = new Regex(@"<returns>\s*(.*?)\s*</returns>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _remarksTagRegex = new Regex(@"<remarks>\s*(.*?)\s*</remarks>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _valueTagRegex = new Regex(@"<value>\s*(.*?)\s*</value>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _paramTagRegex = new Regex(
            @"<param\s+name=(?<q>[""'])(?<name>[\w@.]+)\k<q>\s*>\s*(?<value>.*?)\s*</param>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _typeParamTagRegex = new Regex(
            @"<typeparam\s+name=(?<q>[""'])(?<name>[\w@.]+)\k<q>\s*>\s*(?<value>.*?)\s*</typeparam>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // XML 标签清理：保留有语义的内容（cref/paramref 名称、<c> 文本），<para> 分段，其余标签删除
        static readonly Regex _xmlTagRegex = new Regex(
            @"<see\s+cref=(?<cq>[""'])(?<cref>[^""']*)\k<cq>\s*/?>" +
            @"|<see\s+langword=(?<lq>[""'])(?<langword>[^""']*)\k<lq>\s*/?>" +
            @"|<paramref\s+name=(?<pq>[""'])(?<paramref>[^""']*)\k<pq>\s*/?>" +
            @"|<typeparamref\s+name=(?<tq>[""'])(?<typeparamref>[^""']*)\k<tq>\s*/?>" +
            @"|<c>(?<inline>.*?)</c>" + @"|<code>(?<code>.*?)</code>" + @"|<para\s*/?>" + @"|</para>" +
            @"|<[^>]+>", RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _multiSpaceRegex = new Regex(@"  +", RegexOptions.Compiled);

        static readonly Regex _paraPaddingRegex = new Regex(@" *\n *", RegexOptions.Compiled);

        static readonly Regex _indexerRegex = new Regex(@"\bthis\s*\[", RegexOptions.Compiled);

        static readonly Regex _conversionOperatorRegex =
            new Regex(@"\b(implicit|explicit)\s+operator\b", RegexOptions.Compiled);

        static readonly Regex _symbolOperatorRegex =
            new Regex(@"\boperator\s*(?<sym>[+\-*/%&|^!~<>=]+)", RegexOptions.Compiled);

        static readonly Regex _wordOperatorRegex =
            new Regex(@"\boperator\s+(?<word>true|false)\b", RegexOptions.Compiled);

        // 构造函数声明中，成员名之前的合法 token 只能是访问/静态修饰符（或无）
        static readonly HashSet<string> _ctorModifierWords = new HashSet<string>(StringComparer.Ordinal)
        {
            "public", "private", "protected", "internal", "static", "extern", "unsafe"
        };

        /// <summary>
        /// 单遍扫描源码行数组，返回结构化文档注释结果。
        /// parseDocs 为 false 时跳过 /// 文档块的收集与解析（仅收集类型/命名空间声明，供项目级索引用）。
        /// </summary>
        public static ParsedSourceDoc Scan(string[] lines, bool parseDocs = true)
        {
            var result = new ParsedSourceDoc();
            if (lines == null || lines.Length == 0)
            {
                return result;
            }

            var count = lines.Length;
            var sanitized = new string[count];
            var isDocLine = new bool[count];
            var docContent = new string[count];
            var sanitizerState = new SanitizerState();
            for (var i = 0; i < count; i++)
            {
                sanitized[i] = SanitizeLine(sanitizerState, lines[i], out isDocLine[i], out docContent[i]);
            }

            var nsStack = new Stack<Frame>();
            var typeStack = new Stack<Frame>();
            string fileScopedNamespace = null;
            var depth = 0;

            var docBuffer = parseDocs ? new List<string>() : null;
            var attrText = new StringBuilder();
            var attrDepth = 0;
            var previousWasDocLine = false;

            for (var i = 0; i < count; i++)
            {
                var depthBefore = depth;
                var line = sanitized[i];

                if (isDocLine[i])
                {
                    if (parseDocs)
                    {
                        // 文档块之间被空行/预处理/特性行打断时，丢弃前一块（与 C# 编译器语义一致）
                        if (!previousWasDocLine && docBuffer.Count > 0)
                        {
                            docBuffer.Clear();
                        }

                        docBuffer.Add(docContent[i]);
                    }

                    previousWasDocLine = true;
                    continue;
                }

                previousWasDocLine = false;
                var trimmedStart = line.TrimStart();

                // 预处理指令：不参与结构状态、不作为声明目标
                if (trimmedStart.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                // 空行：仅推进结构状态，不打断待关联文档块
                if (string.IsNullOrWhiteSpace(line))
                {
                    UpdateStructure(result, line, sanitized, i, ref depth, nsStack, ref fileScopedNamespace,
                        typeStack);
                    continue;
                }

                // 特性行（特性可跨行）：累积括号平衡后剥出声明残余，支持 [Attr] 与声明同行/多行特性
                if (docBuffer is { Count: > 0 } &&
                    (attrDepth > 0 || trimmedStart.StartsWith("[", StringComparison.Ordinal)))
                {
                    attrDepth += CountChar(line, '[') - CountChar(line, ']');
                    attrText.Append(' ').Append(trimmedStart);
                    if (attrDepth <= 0)
                    {
                        var remainder = _leadingAttributesRegex.Replace(attrText.ToString(), "").Trim();
                        attrText.Clear();
                        // 必须先于 UpdateStructure 解析文档：后者会把该行声明的类型压栈，
                        // 否则类型链会把自身重复一层
                        if (remainder.Length > 0)
                        {
                            ResolveDoc(result, docBuffer, remainder,
                                CurrentNamespace(nsStack, fileScopedNamespace), typeStack, depthBefore,
                                sanitized, i);
                            docBuffer.Clear();
                        }
                    }

                    UpdateStructure(result, line, sanitized, i, ref depth, nsStack, ref fileScopedNamespace,
                        typeStack);
                    continue;
                }

                if (docBuffer is { Count: > 0 })
                {
                    ResolveDoc(result, docBuffer, line, CurrentNamespace(nsStack, fileScopedNamespace),
                        typeStack, depthBefore, sanitized, i);
                    docBuffer.Clear();
                }

                UpdateStructure(result, line, sanitized, i, ref depth, nsStack, ref fileScopedNamespace,
                    typeStack);
            }

            return result;
        }

        /// <summary>
        /// 从文档注释行列表（已移除 /// 前缀）中提取 summary 纯文本。
        /// </summary>
        public static string ParseSummaryTextFromLines(List<string> summaryLines)
        {
            if (summaryLines == null || summaryLines.Count == 0)
            {
                return null;
            }

            var joined = string.Join(" ", summaryLines);
            var match = _summaryTagRegex.Match(joined);
            var summary = match.Success
                ? match.Groups[1].Value.Trim()
                : joined.Replace("<summary>", string.Empty).Replace("</summary>", string.Empty).Trim();
            return string.IsNullOrWhiteSpace(summary) ? null : CleanDocText(summary);
        }

        /// <summary>
        /// 清理 XML 标签：cref/paramref/typeparamref/langword 替换为名称，&lt;c&gt;/&lt;code&gt; 保留内容，
        /// &lt;para&gt; 分段为换行，其余标签删除，折叠多余空格，最后解码 XML 实体。
        /// </summary>
        public static string StripXmlTags(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return _xmlTagRegex.Replace(text, match =>
            {
                if (match.Groups["cref"].Success)
                {
                    var cref = match.Groups["cref"].Value;
                    var dot = cref.LastIndexOf('.');
                    return dot >= 0 ? cref.Substring(dot + 1) : cref;
                }

                if (match.Groups["langword"].Success)
                {
                    return match.Groups["langword"].Value;
                }

                if (match.Groups["paramref"].Success)
                {
                    return match.Groups["paramref"].Value;
                }

                if (match.Groups["typeparamref"].Success)
                {
                    return match.Groups["typeparamref"].Value;
                }

                if (match.Groups["inline"].Success)
                {
                    return match.Groups["inline"].Value;
                }

                if (match.Groups["code"].Success)
                {
                    return match.Groups["code"].Value;
                }

                if (match.Value.StartsWith("<para", StringComparison.Ordinal) || match.Value == "</para>")
                {
                    return "\n";
                }

                return string.Empty;
            });
        }

        sealed class Frame
        {
            public readonly int BodyDepth;
            public readonly string Name;
            public bool Entered;

            public Frame(string name, int bodyDepth)
            {
                Name = name;
                BodyDepth = bodyDepth;
            }
        }

        sealed class SanitizerState
        {
            public bool InBlockComment;
            public bool InVerbatimString;
        }

        sealed class SourceDocText
        {
            public readonly Dictionary<string, string> Params = new Dictionary<string, string>();
            public readonly Dictionary<string, string> TypeParams = new Dictionary<string, string>();
            public string Remarks;
            public string Returns;
            public string Summary;
            public string Value;

            public bool IsEmpty =>
                Summary == null && Returns == null && Remarks == null && Value == null && Params.Count == 0 &&
                TypeParams.Count == 0;
        }

        #region 净化器

        /// <summary>
        /// 字符级净化一行：字符串/字符字面量内容与注释内容置空（保留引号字符以维持 token 结构）。
        /// 行首（跳过空白）为 /// 且处于代码态时识别为 XML 文档注释行，原样返回由上层收集。
        /// </summary>
        static string SanitizeLine(SanitizerState state,
            string line,
            out bool isDocLine,
            out string docContent)
        {
            isDocLine = false;
            docContent = null;
            if (line == null)
            {
                return string.Empty;
            }

            if (!state.InBlockComment && !state.InVerbatimString)
            {
                var firstChar = 0;
                while (firstChar < line.Length && char.IsWhiteSpace(line[firstChar]))
                {
                    firstChar++;
                }

                if (firstChar + 2 < line.Length && line[firstChar] == '/' && line[firstChar + 1] == '/' &&
                    line[firstChar + 2] == '/')
                {
                    isDocLine = true;
                    docContent = line.Substring(firstChar + 3).Trim();
                    return line;
                }
            }

            var chars = line.ToCharArray();
            var i = 0;
            while (i < chars.Length)
            {
                if (state.InBlockComment)
                {
                    if (chars[i] == '*' && i + 1 < chars.Length && chars[i + 1] == '/')
                    {
                        chars[i] = ' ';
                        chars[i + 1] = ' ';
                        i += 2;
                        state.InBlockComment = false;
                    }
                    else
                    {
                        chars[i] = ' ';
                        i++;
                    }

                    continue;
                }

                if (state.InVerbatimString)
                {
                    if (chars[i] == '"')
                    {
                        if (i + 1 < chars.Length && chars[i + 1] == '"')
                        {
                            // 逐字字符串的 "" 转义
                            chars[i] = ' ';
                            chars[i + 1] = ' ';
                            i += 2;
                        }
                        else
                        {
                            chars[i] = ' ';
                            i++;
                            state.InVerbatimString = false;
                        }
                    }
                    else
                    {
                        chars[i] = ' ';
                        i++;
                    }

                    continue;
                }

                var c = chars[i];
                if (c == '/' && i + 1 < chars.Length)
                {
                    if (chars[i + 1] == '/')
                    {
                        for (; i < chars.Length; i++)
                        {
                            chars[i] = ' ';
                        }

                        break;
                    }

                    if (chars[i + 1] == '*')
                    {
                        chars[i] = ' ';
                        chars[i + 1] = ' ';
                        i += 2;
                        state.InBlockComment = true;
                        continue;
                    }
                }

                if (c == '"')
                {
                    if (IsVerbatimQuote(line, i))
                    {
                        state.InVerbatimString = true;
                        i++;
                    }
                    else
                    {
                        // 普通字符串：内容置空；行尾未闭合视为非法代码，状态不跨行
                        i++;
                        while (i < chars.Length)
                        {
                            if (chars[i] == '\\')
                            {
                                chars[i] = ' ';
                                if (i + 1 < chars.Length)
                                {
                                    chars[i + 1] = ' ';
                                    i++;
                                }

                                i++;
                                continue;
                            }

                            if (chars[i] == '"')
                            {
                                break;
                            }

                            chars[i] = ' ';
                            i++;
                        }

                        // 跳过闭合引号，避免其被再次当作开引号
                        if (i < chars.Length)
                        {
                            i++;
                        }
                    }

                    continue;
                }

                if (c == '\'')
                {
                    i++;
                    while (i < chars.Length)
                    {
                        if (chars[i] == '\\')
                        {
                            chars[i] = ' ';
                            if (i + 1 < chars.Length)
                            {
                                chars[i + 1] = ' ';
                                i++;
                            }

                            i++;
                            continue;
                        }

                        if (chars[i] == '\'')
                        {
                            break;
                        }

                        chars[i] = ' ';
                        i++;
                    }

                    // 跳过闭合引号，避免其被再次当作开引号
                    if (i < chars.Length)
                    {
                        i++;
                    }

                    continue;
                }

                i++;
            }

            return new string(chars);
        }

        static bool IsVerbatimQuote(string line, int quoteIndex)
        {
            var j = quoteIndex - 1;
            if (j < 0)
            {
                return false;
            }

            if (line[j] == '@')
            {
                return true;
            }

            // $@" 或 @$" 插值逐字字符串
            return line[j] == '$' && j - 1 >= 0 && line[j - 1] == '@';
        }

        #endregion

        #region 结构扫描

        static void UpdateStructure(ParsedSourceDoc result,
            string line,
            string[] sanitized,
            int index,
            ref int depth,
            Stack<Frame> nsStack,
            ref string fileScopedNamespace,
            Stack<Frame> typeStack)
        {
            var nsMatch = _namespaceRegex.Match(line);
            if (nsMatch.Success)
            {
                var nsName = nsMatch.Groups[1].Value;
                var rest = line.Substring(nsMatch.Index + nsMatch.Length).TrimStart();
                bool isBlock;
                if (rest.StartsWith("{", StringComparison.Ordinal))
                {
                    isBlock = true;
                }
                else if (rest.StartsWith(";", StringComparison.Ordinal))
                {
                    isBlock = false;
                }
                else
                {
                    // 花括号可能在后续行：向后找第一个非空行判定块式/文件级
                    isBlock = false;
                    for (var j = index + 1; j < sanitized.Length; j++)
                    {
                        var next = sanitized[j].Trim();
                        if (next.Length == 0)
                        {
                            continue;
                        }

                        isBlock = next.StartsWith("{", StringComparison.Ordinal);
                        break;
                    }
                }

                if (isBlock)
                {
                    var current = CurrentNamespace(nsStack, fileScopedNamespace);
                    var fullName = string.IsNullOrEmpty(current) ? nsName : current + "." + nsName;
                    nsStack.Push(new Frame(fullName, depth + 1));
                    result.DeclaredNamespaces.Add(fullName);
                }
                else
                {
                    fileScopedNamespace = nsName;
                    result.DeclaredNamespaces.Add(nsName);
                }
            }

            if (TryExtractTypeDeclaration(line, out var typeName))
            {
                result.DeclaredTypeNames.Add(typeName);
                // 以 ; 结尾的无体声明（如位置记录 record Foo(int X);）不入栈——其内部不会再有嵌套成员
                if (!line.TrimEnd().EndsWith(";", StringComparison.Ordinal))
                {
                    typeStack.Push(new Frame(typeName, depth + 1));
                }
            }

            // 统计花括号深度，同时跟踪本行内的最大深度（用于判定栈顶帧是否"已进入体"）
            var runningDepth = depth;
            var maxDepth = depth;
            foreach (var c in line)
            {
                if (c == '{')
                {
                    runningDepth++;
                    if (runningDepth > maxDepth)
                    {
                        maxDepth = runningDepth;
                    }
                }
                else if (c == '}')
                {
                    runningDepth--;
                }
            }

            depth = runningDepth;

            // 本行花括号进入栈顶帧的体深度时标记已入体
            if (typeStack.Count > 0 && maxDepth >= typeStack.Peek().BodyDepth)
            {
                typeStack.Peek().Entered = true;
            }

            if (nsStack.Count > 0 && maxDepth >= nsStack.Peek().BodyDepth)
            {
                nsStack.Peek().Entered = true;
            }

            // 弹栈：已入体帧在深度回落到体之下时弹出；
            // 未入体帧（花括号在后续行的声明）仅在深度回落到声明层之下时视为放弃弹出，
            // 避免声明行本身（花括号未出现）被立即弹栈导致上下文全部丢失
            while (typeStack.Count > 0 && depth < typeStack.Peek().BodyDepth &&
                   (typeStack.Peek().Entered || depth < typeStack.Peek().BodyDepth - 1))
            {
                typeStack.Pop();
            }

            while (nsStack.Count > 0 && depth < nsStack.Peek().BodyDepth &&
                   (nsStack.Peek().Entered || depth < nsStack.Peek().BodyDepth - 1))
            {
                nsStack.Pop();
            }
        }

        static bool TryExtractTypeDeclaration(string line, out string typeName)
        {
            var match = _typeDeclRegex.Match(line);
            if (!match.Success)
            {
                typeName = null;
                return false;
            }

            typeName = match.Groups[2].Value;
            // record struct Foo / record class Foo：首个匹配捕获到的是 struct/class 关键字
            if (match.Groups[1].Value == "record" && (typeName == "struct" || typeName == "class"))
            {
                var recordMatch = _recordKeywordRegex.Match(line);
                if (recordMatch.Success)
                {
                    typeName = recordMatch.Groups[1].Value;
                }
            }

            return true;
        }

        static string CurrentNamespace(Stack<Frame> nsStack, string fileScopedNamespace) =>
            nsStack.Count > 0 ? nsStack.Peek().Name : fileScopedNamespace ?? string.Empty;

        #endregion

        #region 文档关联

        static void ResolveDoc(ParsedSourceDoc result,
            List<string> docBuffer,
            string declText,
            string namespaceFqn,
            Stack<Frame> typeStack,
            int depthBefore,
            string[] sanitized,
            int declIndex)
        {
            var doc = ParseDocText(docBuffer);
            if (doc == null)
            {
                return;
            }

            if (TryExtractTypeDeclaration(declText, out var typeName))
            {
                var chain = typeStack.Select(frame => frame.Name).Reverse().ToList();
                chain.Add(typeName);
                foreach (var core in BuildTypeCores(namespaceFqn, chain))
                {
                    AddDoc(result, doc, core);
                }

                return;
            }

            if (typeStack.Count == 0)
            {
                return;
            }

            var frame = typeStack.Peek();
            // 只有直接位于类型体深度的声明才是成员；更深的花括号（方法体内）视为局部语句，丢弃文档
            if (depthBefore != frame.BodyDepth)
            {
                return;
            }

            var fullDecl = CollectFullDeclaration(sanitized, declIndex);
            var paramTypes = ExtractParameterTypes(fullDecl);
            var paramCount = paramTypes?.Count ?? 0;

            string memberName;
            var operatorName = TryGetOperatorName(declText, paramCount);
            if (operatorName != null)
            {
                memberName = operatorName;
            }
            else if (_indexerRegex.IsMatch(declText))
            {
                // 索引器：与反射端 PropertyInfo.Name（默认 "Item"）对齐
                memberName = "Item";
            }
            else
            {
                memberName = SourceFileAnalyzerUtility.ExtractMemberName(declText);
            }

            if (string.IsNullOrEmpty(memberName))
            {
                return;
            }

            var isCtor = operatorName == null && !_indexerRegex.IsMatch(declText) &&
                         IsCtorDeclaration(declText, memberName, frame.Name);

            // 参数后缀：参数类型键（与历史格式一致）+ 参数计数键（对源码书写格式免疫）
            List<string> suffixes;
            if (paramTypes == null)
            {
                suffixes = new List<string> { "" };
            }
            else
            {
                suffixes = new List<string>
                {
                    "(" + string.Join(", ", paramTypes) + ")",
                    "(" + paramTypes.Count + ")"
                };
            }

            var enclosing = typeStack.Select(f => f.Name).Reverse().ToList();
            var cores = BuildTypeCores(namespaceFqn, enclosing).ToList();
            foreach (var core in cores)
            {
                foreach (var suffix in suffixes)
                {
                    AddDoc(result, doc, core + "." + memberName + suffix);
                }

                if (isCtor)
                {
                    foreach (var suffix in suffixes)
                    {
                        AddDoc(result, doc, core + "." + "#ctor" + suffix);
                    }
                }
            }
        }

        static void AddDoc(ParsedSourceDoc result, SourceDocText doc, string key)
        {
            if (doc.Summary != null)
            {
                result.Summaries[key] = doc.Summary;
            }

            if (doc.Returns != null)
            {
                result.ReturnsSummaries[key] = doc.Returns;
            }

            if (doc.Remarks != null)
            {
                result.RemarksSummaries[key] = doc.Remarks;
            }

            if (doc.Value != null)
            {
                result.ValueSummaries[key] = doc.Value;
            }

            if (doc.Params.Count > 0)
            {
                result.ParamSummaries[key] = doc.Params;
            }

            if (doc.TypeParams.Count > 0)
            {
                result.TypeParamSummaries[key] = doc.TypeParams;
            }
        }

        static IEnumerable<string> BuildTypeCores(string namespaceFqn, List<string> chain)
        {
            var joined = string.Join(".", chain);
            yield return string.IsNullOrEmpty(namespaceFqn) ? joined : namespaceFqn + "." + joined;

            // 嵌套类型额外发射扁平旧键（Namespace.Inner），保留一版兼容
            if (chain.Count > 1)
            {
                var legacy = string.IsNullOrEmpty(namespaceFqn)
                    ? chain[chain.Count - 1]
                    : namespaceFqn + "." + chain[chain.Count - 1];
                yield return legacy;
            }
        }

        static SourceDocText ParseDocText(List<string> docLines)
        {
            var joined = string.Join(" ", docLines);
            var doc = new SourceDocText();

            var summaryMatch = _summaryTagRegex.Match(joined);
            if (summaryMatch.Success)
            {
                doc.Summary = CleanDocText(summaryMatch.Groups[1].Value.Trim());
            }
            else
            {
                var fallback = joined.Replace("<summary>", string.Empty).Replace("</summary>", string.Empty)
                    .Trim();
                if (fallback.Length > 0 && !fallback.StartsWith("<param") &&
                    !fallback.StartsWith("<returns") && !fallback.StartsWith("<remarks") &&
                    !fallback.StartsWith("<typeparam") && !fallback.StartsWith("<value"))
                {
                    doc.Summary = CleanDocText(fallback);
                }
            }

            var returnsMatch = _returnsTagRegex.Match(joined);
            if (returnsMatch.Success)
            {
                doc.Returns = CleanDocText(returnsMatch.Groups[1].Value.Trim());
            }

            var remarksMatch = _remarksTagRegex.Match(joined);
            if (remarksMatch.Success)
            {
                doc.Remarks = CleanDocText(remarksMatch.Groups[1].Value.Trim());
            }

            var valueMatch = _valueTagRegex.Match(joined);
            if (valueMatch.Success)
            {
                doc.Value = CleanDocText(valueMatch.Groups[1].Value.Trim());
            }

            foreach (Match match in _paramTagRegex.Matches(joined))
            {
                var text = CleanDocText(match.Groups["value"].Value.Trim());
                if (text != null)
                {
                    doc.Params[match.Groups["name"].Value] = text;
                }
            }

            foreach (Match match in _typeParamTagRegex.Matches(joined))
            {
                var text = CleanDocText(match.Groups["value"].Value.Trim());
                if (text != null)
                {
                    doc.TypeParams[match.Groups["name"].Value] = text;
                }
            }

            return doc.IsEmpty ? null : doc;
        }

        static string CleanDocText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var text = StripXmlTags(raw);
            text = _multiSpaceRegex.Replace(text, " ");
            text = _paraPaddingRegex.Replace(text, "\n");
            text = DecodeXmlEntities(text);
            text = text.Trim();
            return text.Length == 0 ? null : text;
        }

        static string DecodeXmlEntities(string text) => text.Replace("&lt;", "<").Replace("&gt;", ">")
            .Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&nbsp;", " ").Replace("&amp;", "&");

        static bool IsCtorDeclaration(string declText, string memberName, string typeName)
        {
            if (memberName != typeName)
            {
                return false;
            }

            var match = Regex.Match(declText, @"\b" + Regex.Escape(memberName) + @"\s*\(");
            if (!match.Success)
            {
                return false;
            }

            var before = declText.Substring(0, match.Index).TrimEnd();
            if (before.Length == 0)
            {
                return true;
            }

            // 取最后一个词（对空白字符种类免疫：空格/Tab 混排均可）
            var lastWordMatch = Regex.Match(before, @"(\w+)$");
            return lastWordMatch.Success && _ctorModifierWords.Contains(lastWordMatch.Groups[1].Value);
        }

        static string TryGetOperatorName(string declText, int paramCount)
        {
            var conversionMatch = _conversionOperatorRegex.Match(declText);
            if (conversionMatch.Success)
            {
                return conversionMatch.Groups[1].Value == "implicit" ? "op_Implicit" : "op_Explicit";
            }

            var wordMatch = _wordOperatorRegex.Match(declText);
            if (wordMatch.Success)
            {
                return wordMatch.Groups["word"].Value == "true" ? "op_True" : "op_False";
            }

            var symbolMatch = _symbolOperatorRegex.Match(declText);
            if (!symbolMatch.Success)
            {
                return null;
            }

            var symbol = symbolMatch.Groups["sym"].Value;
            return symbol switch
            {
                "+" => paramCount == 1 ? "op_UnaryPlus" : "op_Addition",
                "-" => paramCount == 1 ? "op_UnaryNegation" : "op_Subtraction",
                "++" => "op_Increment",
                "--" => "op_Decrement",
                "!" => "op_LogicalNot",
                "~" => "op_OnesComplement",
                "*" => "op_Multiply",
                "/" => "op_Division",
                "%" => "op_Modulus",
                "&" => "op_BitwiseAnd",
                "|" => "op_BitwiseOr",
                "^" => "op_ExclusiveOr",
                "<<" => "op_LeftShift",
                ">>" => "op_RightShift",
                "==" => "op_Equality",
                "!=" => "op_Inequality",
                "<" => "op_LessThan",
                ">" => "op_GreaterThan",
                "<=" => "op_LessThanOrEqual",
                ">=" => "op_GreaterThanOrEqual",
                "&&" => "op_LogicalAnd",
                "||" => "op_LogicalOr",
                _ => null
            };
        }

        /// <summary>
        /// 如果声明行包含未闭合的 (（参数跨多行），则从净化行数组中向前收集直到括号平衡。
        /// </summary>
        static string CollectFullDeclaration(string[] sanitizedLines, int startIndex)
        {
            var line = sanitizedLines[startIndex];
            var trimmed = line.TrimStart();

            var openParen = trimmed.IndexOf('(');
            if (openParen < 0)
            {
                return line;
            }

            var depth = 0;
            var hasClose = false;
            foreach (var c in trimmed)
            {
                if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        hasClose = true;
                        break;
                    }
                }
            }

            if (hasClose)
            {
                return line;
            }

            var sb = new StringBuilder(line);
            for (var j = startIndex + 1; j < sanitizedLines.Length; j++)
            {
                sb.Append(' ').Append(sanitizedLines[j].Trim());
                foreach (var c in sanitizedLines[j])
                {
                    if (c == '(')
                    {
                        depth++;
                    }
                    else if (c == ')')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            return sb.ToString();
                        }
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 从声明文本中提取参数类型列表（不含参数名）。返回 null 表示不是方法声明；
        /// 空列表表示无参方法。逗号按泛型/数组/括号深度顶层切分。
        /// </summary>
        static List<string> ExtractParameterTypes(string declLine)
        {
            var trimmed = declLine.TrimStart();

            var openParen = trimmed.IndexOf('(');
            if (openParen < 0)
            {
                return null;
            }

            var depth = 0;
            var closeParen = -1;
            for (var i = openParen; i < trimmed.Length; i++)
            {
                if (trimmed[i] == '(')
                {
                    depth++;
                }
                else if (trimmed[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeParen = i;
                        break;
                    }
                }
            }

            if (closeParen < 0)
            {
                return null;
            }

            var paramSection = trimmed.Substring(openParen + 1, closeParen - openParen - 1).Trim();
            if (string.IsNullOrEmpty(paramSection))
            {
                return new List<string>();
            }

            var typeNames = new List<string>();
            foreach (var param in SplitTopLevel(paramSection))
            {
                var p = param.Trim();
                if (p.Length == 0)
                {
                    continue;
                }

                p = Regex.Replace(p, @"^(this\s+|params\s+|ref\s+|out\s+|in\s+)+", "");

                var eqIndex = p.IndexOf('=');
                if (eqIndex >= 0)
                {
                    p = p.Substring(0, eqIndex).Trim();
                }

                var tokens = p.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length >= 2)
                {
                    typeNames.Add(string.Join(" ", tokens, 0, tokens.Length - 1).Trim());
                }
                else if (tokens.Length == 1)
                {
                    typeNames.Add(tokens[0]);
                }
            }

            return typeNames;
        }

        static List<string> SplitTopLevel(string section)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            var depth = 0;
            foreach (var c in section)
            {
                switch (c)
                {
                    case '<':
                    case '[':
                    case '(':
                        depth++;
                        current.Append(c);
                        break;
                    case '>':
                    case ']':
                    case ')':
                        if (depth > 0)
                        {
                            depth--;
                        }

                        current.Append(c);
                        break;
                    case ',':
                        if (depth == 0)
                        {
                            parts.Add(current.ToString());
                            current.Clear();
                        }
                        else
                        {
                            current.Append(c);
                        }

                        break;
                    default:
                        current.Append(c);
                        break;
                }
            }

            parts.Add(current.ToString());
            return parts;
        }

        static int CountChar(string line, char target)
        {
            var count = 0;
            foreach (var c in line)
            {
                if (c == target)
                {
                    count++;
                }
            }

            return count;
        }

        #endregion
    }
}
#endif
