#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 从源代码的 XML <c>/// &lt;summary&gt;</c> 注释中解析成员摘要。
    /// 使用 XmlSummaryTool 式分块 + 块注释状态跟踪，避免 <c>/* */</c> 内的假 <c>///</c> 误判。
    /// 支持全限定键（Namespace.TypeName.MemberName）避免跨类型同名成员冲突。
    /// </summary>
    public static class SourceSummaryParser
    {
        static readonly Regex _summaryContentRegex = new Regex(@"<summary>\s*(.*?)\s*</summary>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _xmlTagRegex = new Regex(
            @"<see\s+cref=""([^""]*)""\s*/?>|<[^>]+>", RegexOptions.Compiled);

        static readonly Regex _multiSpaceRegex = new Regex(@"  +", RegexOptions.Compiled);

        static readonly Regex _typeDeclRegex = new Regex(
            @"\b(class|struct|enum|interface)\s+(\w+)", RegexOptions.Compiled);

        static readonly Regex _namespaceRegex = new Regex(@"^\s*namespace\s+([\w.]+)", RegexOptions.Compiled);

        /// <summary>
        /// 解析多个源文件条目中的 summary 注释，返回全限定键 → summary 字典。
        /// 键格式：<c>AssemblyName.Namespace.TypeName</c>（类型级）或
        /// <c>AssemblyName.Namespace.TypeName.MemberName</c>（成员级）。
        /// 程序集名前缀避免不同程序集中同名命名空间+类型名的键冲突。
        /// </summary>
        public static Dictionary<string, string> ParseSummaries(SourceFileEntry[] entries,
            string assemblyName = null)
        {
            var result = new Dictionary<string, string>();
            if (entries == null)
            {
                return result;
            }

            var prefix = string.IsNullOrEmpty(assemblyName) ? "" : assemblyName + ".";

            foreach (var entry in entries)
            {
                if (entry?.sourceLines == null)
                {
                    continue;
                }

                var summaries = ParseSummariesFromLines(entry.sourceLines, prefix);
                foreach (var kv in summaries)
                {
                    result[kv.Key] = kv.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// 从源代码行数组中提取所有 summary 注释，关联到全限定键。
        /// </summary>
        static Dictionary<string, string> ParseSummariesFromLines(string[] lines, string keyPrefix = "")
        {
            var result = new Dictionary<string, string>();
            var inBlockComment = false;
            var currentNamespace = string.Empty;

            var i = 0;
            while (i < lines.Length)
            {
                // 更新块注释状态
                inBlockComment = UpdateBlockCommentState(lines[i], inBlockComment);

                // 更新命名空间上下文
                UpdateNamespaceContext(lines[i], ref currentNamespace);

                if (inBlockComment || !IsXmlDocLine(lines[i]))
                {
                    i++;
                    continue;
                }

                // 收集 XML 注释行
                var xmlLines = new List<string>();
                while (i < lines.Length)
                {
                    inBlockComment = UpdateBlockCommentState(lines[i], inBlockComment);
                    if (inBlockComment)
                    {
                        i++;
                        continue;
                    }

                    var trimmed = lines[i].TrimStart();
                    if (!trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        break;
                    }

                    xmlLines.Add(trimmed.Substring(3).Trim());
                    i++;
                }

                if (xmlLines.Count == 0)
                {
                    continue;
                }

                // 跳过预处理指令、特性行、空行
                while (i < lines.Length)
                {
                    inBlockComment = UpdateBlockCommentState(lines[i], inBlockComment);
                    if (inBlockComment)
                    {
                        i++;
                        continue;
                    }

                    var trimmed = lines[i].TrimStart();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") ||
                        trimmed.StartsWith("[", StringComparison.Ordinal))
                    {
                        i++;
                        continue;
                    }

                    break;
                }

                if (i >= lines.Length)
                {
                    break;
                }

                // 提取成员名或类型名
                var declLine = lines[i];
                var typeMatch = _typeDeclRegex.Match(declLine.TrimStart());
                string key;
                if (typeMatch.Success)
                {
                    // 类型自身的 summary
                    var typeName = typeMatch.Groups[2].Value;
                    key = keyPrefix + (string.IsNullOrEmpty(currentNamespace)
                        ? typeName
                        : currentNamespace + "." + typeName);
                }
                else
                {
                    // 成员的 summary
                    var memberName = SourceFileAnalyzerUtility.ExtractMemberName(declLine);
                    if (memberName == null)
                    {
                        continue;
                    }

                    // 查找当前行所在的类型上下文
                    var currentType = FindCurrentType(lines, i);
                    if (currentType == null)
                    {
                        continue;
                    }

                    key = keyPrefix + (string.IsNullOrEmpty(currentNamespace)
                        ? currentType + "." + memberName
                        : currentNamespace + "." + currentType + "." + memberName);

                    // 对方法声明，提取参数列表附加到键中，区分重载方法
                    // 例如 DoSomething(int) / DoSomething(string, int)
                    // 仅当声明行中包含 ( 时才尝试提取，避免误匹配属性和字段
                    // 如果 ( 和 ) 不在同一行（参数跨多行），则向前收集行直到找到匹配的 )
                    var fullDeclLine = CollectFullDeclaration(lines, i);
                    var paramPart = ExtractParameterTypesFromDecl(fullDeclLine);
                    if (paramPart != null)
                    {
                        key += "(" + paramPart + ")";
                    }
                }

                var summary = ParseSummaryText(xmlLines);
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    result[key] = summary;
                }
            }

            return result;
        }

        /// <summary>
        /// 从当前行向前搜索最近的类型声明，返回类型名。
        /// </summary>
        static string FindCurrentType(string[] lines, int currentIndex)
        {
            for (var i = currentIndex; i >= 0; i--)
            {
                var match = _typeDeclRegex.Match(lines[i].TrimStart());
                if (match.Success)
                {
                    return match.Groups[2].Value;
                }
            }

            return null;
        }

        /// <summary>
        /// 如果声明行包含未闭合的 (（参数跨多行），则向前收集行直到括号匹配，
        /// 返回完整的声明文本。如果单行已闭合或不包含 (，直接返回原行。
        /// </summary>
        static string CollectFullDeclaration(string[] lines, int startIndex)
        {
            var line = lines[startIndex];
            var trimmed = line.TrimStart();

            // 如果不包含 (，不是方法声明，直接返回
            var openParen = trimmed.IndexOf('(');
            if (openParen < 0)
                return line;

            // 检查括号是否已闭合
            var depth = 0;
            var hasClose = false;
            foreach (var c in trimmed)
            {
                if (c == '(') depth++;
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0) { hasClose = true; break; }
                }
            }

            // 单行已闭合
            if (hasClose)
                return line;

            // 跨行收集直到括号闭合
            var sb = new StringBuilder(line);
            for (var j = startIndex + 1; j < lines.Length; j++)
            {
                sb.Append(" " + lines[j].Trim());
                foreach (var c in lines[j])
                {
                    if (c == '(') depth++;
                    else if (c == ')')
                    {
                        depth--;
                        if (depth == 0) { return sb.ToString(); }
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 从方法声明行中提取参数类型列表（不含参数名），用逗号分隔。
        /// 例如 "public void DoSomething(int count, string name)" → "int, string"
        /// 如果声明行无括号或参数为空，返回空字符串表示无参数方法。
        /// 返回 null 表示不是方法声明（如属性、字段）。
        /// </summary>
        static string ExtractParameterTypesFromDecl(string declLine)
        {
            var trimmed = declLine.TrimStart();

            // 必须包含 ( 才是方法声明
            var openParen = trimmed.IndexOf('(');
            if (openParen < 0)
            {
                return null;
            }

            // 查找匹配的闭括号（处理嵌套括号，如泛型方法 Method<T>(List<T> param)）
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

            // 无参数方法
            if (string.IsNullOrEmpty(paramSection))
            {
                return "";
            }

            // 提取每个参数的类型部分（跳过修饰符 this/params/ref/out/in 和参数名）
            var paramParts = paramSection.Split(',');
            var typeNames = new List<string>();
            foreach (var param in paramParts)
            {
                var p = param.Trim();
                if (string.IsNullOrEmpty(p))
                {
                    continue;
                }

                // 移除修饰符
                p = Regex.Replace(p, @"^(this\s+|params\s+|ref\s+|out\s+|in\s+)+", "");

                // 移除默认值（= ...）
                var eqIdx = p.IndexOf('=');
                if (eqIdx >= 0)
                {
                    p = p.Substring(0, eqIdx).Trim();
                }

                // 参数格式为 "Type name" 或 "Type[] name" 等，最后一个标识符是参数名
                // 取除最后一个标识符外的部分作为类型
                var tokens = p.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length >= 2)
                {
                    // 类型是除最后一个 token 外的所有部分
                    var typePart = string.Join(" ", tokens, 0, tokens.Length - 1);
                    typeNames.Add(typePart.Trim());
                }
                else if (tokens.Length == 1)
                {
                    // 只有一个 token，可能是委托类型参数（如 Action<int>）
                    typeNames.Add(tokens[0]);
                }
            }

            return string.Join(", ", typeNames);
        }

        /// <summary>
        /// 更新块注释状态。如果行中包含 <c>/*</c> 或 <c>*/</c>，更新状态。
        /// </summary>
        static bool UpdateBlockCommentState(string line, bool inBlockComment)
        {
            if (string.IsNullOrEmpty(line))
            {
                return inBlockComment;
            }

            // 先移除行内的 // 注释（不影响块注释状态）
            var code = StripLineComment(line);

            for (var i = 0; i < code.Length - 1; i++)
            {
                if (inBlockComment)
                {
                    if (code[i] == '*' && code[i + 1] == '/')
                    {
                        inBlockComment = false;
                        i++;
                    }
                }
                else
                {
                    if (code[i] == '/' && code[i + 1] == '*')
                    {
                        inBlockComment = true;
                        i++;
                    }
                }
            }

            return inBlockComment;
        }

        /// <summary>
        /// 移除行尾的 // 注释（不处理字符串内的 //）。
        /// </summary>
        static string StripLineComment(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return line ?? string.Empty;
            }

            var inString = false;
            var stringChar = '\0';
            for (var i = 0; i < line.Length - 1; i++)
            {
                var c = line[i];
                if (inString)
                {
                    if (c == '\\')
                    {
                        i++;
                        continue;
                    }

                    if (c == stringChar)
                    {
                        inString = false;
                    }

                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    inString = true;
                    stringChar = c;
                    continue;
                }

                if (c == '/' && line[i + 1] == '/')
                {
                    return line[..i];
                }
            }

            return line;
        }

        /// <summary>
        /// 更新命名空间上下文。
        /// </summary>
        static void UpdateNamespaceContext(string line, ref string currentNamespace)
        {
            var match = _namespaceRegex.Match(line);
            if (match.Success)
            {
                currentNamespace = match.Groups[1].Value;
            }
        }

        /// <summary>
        /// 判断一行是否为 XML 文档注释行（以 /// 开头）。
        /// </summary>
        static bool IsXmlDocLine(string line) =>
            line != null && line.TrimStart().StartsWith("///", StringComparison.Ordinal);

        /// <summary>
        /// 从 summary 注释行列表中提取纯文本。
        /// </summary>
        public static string ParseSummaryText(List<string> summaryLines)
        {
            var fullSummary = string.Join(" ", summaryLines);
            var match = _summaryContentRegex.Match(fullSummary);
            var summary = match.Success
                ? match.Groups[1].Value.Trim()
                : fullSummary.Replace("<summary>", string.Empty).Replace("</summary>", string.Empty).Trim();
            return string.IsNullOrWhiteSpace(summary) ? null : StripXmlTags(summary);
        }

        /// <summary>
        /// 清理 XML 标签：将 <c>&lt;see cref="A.B"/&gt;</c> 替换为 B，移除其他 XML 标签，折叠多余空格。
        /// </summary>
        public static string StripXmlTags(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            text = _xmlTagRegex.Replace(text, m =>
            {
                if (!m.Groups[1].Success)
                {
                    return string.Empty;
                }

                var cref = m.Groups[1].Value;
                var dot = cref.LastIndexOf('.');
                return dot >= 0 ? cref.Substring(dot + 1) : cref;
            });

            return _multiSpaceRegex.Replace(text, " ").Trim();
        }
    }
}
#endif
