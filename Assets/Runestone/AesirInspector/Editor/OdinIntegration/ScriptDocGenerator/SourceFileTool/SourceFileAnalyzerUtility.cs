//
// 本文件提取自 JakePineOdinTools 项目 (MIT License, Copyright (c) 2026 Jake Pine)
// https://github.com/JakePineGames/JakePineOdinTools
// 精简版：仅保留源文件查找与成员名提取，移除花括号跟踪/类型体定位/字符串净化等复杂逻辑。
// ----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 源文件查找与成员名提取工具。通过 AssetDatabase 定位类型的 .cs 源文件，
    /// 返回 <see cref="SourceFileEntry" /> 数组（路径 + 代码内容）。
    /// </summary>
    public static class SourceFileAnalyzerUtility
    {
        static readonly Dictionary<Type, SourceFileEntry[]> _sourceFilesCache =
            new Dictionary<Type, SourceFileEntry[]>();

        static readonly Regex _typeDefinitionRegex = new Regex(
            @"\b(class|struct|enum|interface)\s+(\w+)", RegexOptions.Compiled);

        static readonly Regex _memberDeclRegex = new Regex(
            @"(?:public|private|protected|internal|\s|static|readonly|const|volatile|new|override|virtual|abstract|sealed|async|partial)*\s+\S+\s+(\w+)\s*[{;=\(]",
            RegexOptions.Compiled);

        static readonly Regex _leadingAttributesRegex =
            new Regex(@"^(\s*\[.*?\]\s*)+", RegexOptions.Compiled);

        static readonly HashSet<string> _declarationKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "class", "struct", "enum", "interface", "namespace",
            "if", "else", "while", "for", "foreach", "return", "using",
            "get", "set", "public", "private", "protected", "internal",
            "static", "readonly", "void", "new", "override", "virtual",
            "abstract", "sealed", "async", "partial", "event", "null"
        };

        static SourceFileAnalyzerUtility() => AssemblyReloadEvents.afterAssemblyReload += ClearCache;

        /// <summary>
        /// 清空所有缓存。
        /// </summary>
        public static void ClearCache()
        {
            _sourceFilesCache.Clear();
        }

        /// <summary>
        /// 获取类型对应的源文件条目数组（路径 + 代码内容），结果会被缓存。
        /// </summary>
        public static SourceFileEntry[] GetSourceFiles(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (_sourceFilesCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var entries = FindSourceFiles(type);
            _sourceFilesCache[type] = entries;
            return entries;
        }

        /// <summary>
        /// 通过 AssetDatabase 查找类型对应的 .cs 源文件。
        /// 非 partial 类型找到即停；partial 类型读取所有匹配脚本。
        /// </summary>
        public static SourceFileEntry[] FindSourceFiles(Type type)
        {
            var searchType = type;
            while (searchType.DeclaringType != null)
            {
                searchType = searchType.DeclaringType;
            }

            var typeName = searchType.Name;
            var backtick = typeName.IndexOf('`');
            if (backtick >= 0)
            {
                typeName = typeName[..backtick];
            }

            var isPartial = IsPartialType(searchType);
            var guids = AssetDatabase.FindAssets($"{typeName} t:MonoScript");
            var preferredFileName = typeName + ".cs";
            var results = new List<SourceFileEntry>();

            // 第一轮：精确文件名 + GetClass 验证
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(preferredFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript == null)
                {
                    continue;
                }

                var scriptClass = monoScript.GetClass();
                if (scriptClass == searchType || (scriptClass == null && monoScript.name == typeName))
                {
                    var fullPath = Path.GetFullPath(path);
                    if (!File.Exists(fullPath))
                    {
                        continue;
                    }

                    results.Add(new SourceFileEntry(path, File.ReadAllLines(fullPath)));
                    if (!isPartial)
                    {
                        return results.ToArray();
                    }
                }
            }

            // 第二轮：GetClass 验证（不要求文件名匹配）
            if (results.Count == 0 || isPartial)
            {
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (monoScript == null)
                    {
                        continue;
                    }

                    var scriptClass = monoScript.GetClass();
                    if (scriptClass == searchType || (scriptClass == null && monoScript.name == typeName))
                    {
                        var fullPath = Path.GetFullPath(path);
                        if (!File.Exists(fullPath))
                        {
                            continue;
                        }

                        if (!results.Exists(e => e.filePath == path))
                        {
                            results.Add(new SourceFileEntry(path, File.ReadAllLines(fullPath)));
                            if (!isPartial)
                            {
                                return results.ToArray();
                            }
                        }
                    }
                }
            }

            // 第三轮：内容正则匹配（在 guids 结果中搜索类型定义）
            if (results.Count == 0)
            {
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var fullPath = Path.GetFullPath(path);
                    if (!File.Exists(fullPath))
                    {
                        continue;
                    }

                    var content = File.ReadAllText(fullPath);
                    foreach (Match match in _typeDefinitionRegex.Matches(content))
                    {
                        if (match.Groups[2].Value == typeName)
                        {
                            if (!results.Exists(e => e.filePath == path))
                            {
                                results.Add(new SourceFileEntry(path, File.ReadAllLines(fullPath)));
                                if (!isPartial)
                                {
                                    return results.ToArray();
                                }
                            }
                        }
                    }
                }
            }

            // 第四轮：全局内容扫描（当文件名与类型名不匹配时，guids 可能为空）
            // 例如 Capabilities.cs 中定义了 ICanExecuteCommand，但搜索 "ICanExecuteCommand t:MonoScript" 不会返回该文件
            // 此轮扫描所有 MonoScript 文件内容，正则匹配类型定义
            if (results.Count == 0)
            {
                var allGuids = AssetDatabase.FindAssets("t:MonoScript");
                foreach (var guid in allGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var fullPath = Path.GetFullPath(path);
                    if (!File.Exists(fullPath))
                    {
                        continue;
                    }

                    // 快速预过滤：文件扩展名必须是 .cs
                    if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        var content = File.ReadAllText(fullPath);
                        foreach (Match match in _typeDefinitionRegex.Matches(content))
                        {
                            if (match.Groups[2].Value == typeName)
                            {
                                if (!results.Exists(e => e.filePath == path))
                                {
                                    results.Add(new SourceFileEntry(path, File.ReadAllLines(fullPath)));
                                    if (!isPartial)
                                    {
                                        return results.ToArray();
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return results.Count > 0 ? results.ToArray() : null;
        }

        /// <summary>
        /// 从声明行中提取成员名称。
        /// </summary>
        public static string ExtractMemberName(string declarationLine)
        {
            if (string.IsNullOrWhiteSpace(declarationLine))
            {
                return null;
            }

            // 移除行尾 // 注释，避免注释中的字符干扰后续正则匹配
            var sanitized = StripLineComment(declarationLine);
            // 移除行首特性（[Attribute]），使后续正则直接面对声明关键字
            var line = _leadingAttributesRegex.Replace(sanitized, "").TrimStart();

            // 枚举成员：以标识符开头，后跟 , 或 =（如 "None = 0," "First,"）
            // 枚举声明行不包含修饰符，与普通成员声明的格式不同，需单独处理
            var enumMatch = Regex.Match(line, @"^\s*(\w+)\s*[,=]");
            if (enumMatch.Success && !_declarationKeywords.Contains(enumMatch.Groups[1].Value))
            {
                return enumMatch.Groups[1].Value;
            }

            // 泛型方法声明：成员名后跟 <泛型参数>( ，如 RegisterModel<TModel>(...)
            // 正则 \b(\w+)\s*<[^>]+>\s*\( 要求：
            //   - 成员名后必须紧跟 <...> 再跟 (，确保匹配的是方法名而非泛型类型参数
            //   - [^>]+ 防止跨行或匹配过多内容；例如 "List<T1>" 不会被单独匹配
            // 必须优先于通用正则，否则表达式体会被误匹配：
            //   "public TModel GetModel<TModel>() where TModel : class, IModel => ..."
            //   通用正则 (\w+)\s*[{;=\(] 会先匹配到 "IModel =>" 中的 "IModel"（= 被 => 触发）
            //   而正确答案应是 "GetModel"
            var genericMethodMatch = Regex.Match(line, @"\b(\w+)\s*<[^>]+>\s*\(");
            if (genericMethodMatch.Success &&
                !_declarationKeywords.Contains(genericMethodMatch.Groups[1].Value))
            {
                return genericMethodMatch.Groups[1].Value;
            }

            // 通用成员声明：修饰符 + 类型 + 成员名 + 终止符（{ ; = ( 之一）
            // 适用于大多数单行声明，如 "public int Count;" "public void Foo() { }"
            // 注意：表达式体 "=> " 中的 = 也会被此正则匹配，因此必须放在泛型方法正则之后
            var match = _memberDeclRegex.Match(line);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // 简单匹配：任意标识符后跟 { ; = ( 之一
            // 作为通用正则的补充，捕获未被前者匹配的边缘情况
            var simpleMatch = Regex.Match(line, @"(\w+)\s*[{;=\(]");
            if (simpleMatch.Success && !_declarationKeywords.Contains(simpleMatch.Groups[1].Value))
            {
                return simpleMatch.Groups[1].Value;
            }

            // 行尾无终止符的声明：多行属性声明行尾是换行而非 { ; = (
            // 例如 "public static IContext Interface" 后跟换行的 "{ get { ... } }"
            // 按空格分割后取最后一个有效标识符作为成员名
            var words = line.Split(new[] { ' ', '<', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 2)
            {
                var lastWord = words[^1];
                if (!_declarationKeywords.Contains(lastWord) && IsValidIdentifier(lastWord))
                {
                    return lastWord;
                }
            }

            return null;
        }

        static bool IsValidIdentifier(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (!char.IsLetter(s[0]) && s[0] != '_')
            {
                return false;
            }

            foreach (var c in s)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 移除行尾的 // 注释（不处理字符串内的 //，仅用于成员名提取的简化版）。
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
                    return line[..i].TrimEnd();
                }
            }

            return line;
        }

        static bool IsPartialType(Type type) =>
            // 检查类型是否标记了 partial（通过反射无法直接获取 partial 关键字，
            // 但如果同一程序集中存在多个同名类型的 partial 声明，
            // GetFields/GetMethods 等会合并所有 partial 部分。
            // 这里用 heuristic：如果类型在多个源文件中定义，就是 partial。
            // 简化处理：始终读取所有匹配的源文件。
            true;
    }
}
#endif
