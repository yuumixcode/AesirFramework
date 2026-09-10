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

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 源文件查找与成员名提取工具。
    /// 查找链路：<see cref="ScriptAssemblyFilter" /> 程序集过滤 →
    /// AssetDatabase 按名搜索 + <see cref="MonoScript.GetClass()" /> 验证（单遍，partial 全收集）→
    /// <see cref="ProjectScriptIndex" /> 内容索引兜底（文件名与类型名不一致的场景，全项目仅扫描一次）。
    /// 非 partial 场景返回唯一匹配；partial 类型返回全部声明文件。
    /// </summary>
    public static class SourceFileAnalyzerUtility
    {
        static readonly Dictionary<Type, string[]> _sourcePathsCache = new Dictionary<Type, string[]>();

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

        // 这些关键字不可能出现在成员声明的行首——命中即视为语句行，直接放弃提取，
        // 防止悬空 /// 文档被错误归属到局部变量（如 var x = 1; 提取出 "x"）
        static readonly HashSet<string> _statementStarterKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "var", "using", "return", "if", "else", "while", "for", "foreach", "do", "switch",
            "case", "break", "continue", "throw", "new", "yield", "await", "lock", "goto"
        };

        static SourceFileAnalyzerUtility() => AssemblyReloadEvents.afterAssemblyReload += ClearCache;

        /// <summary>
        /// 清空所有缓存。
        /// </summary>
        public static void ClearCache()
        {
            _sourcePathsCache.Clear();
        }

        /// <summary>
        /// 获取类型对应的源文件条目数组（路径 + 代码内容）。
        /// 兼容包装：每次现读文件内容，不在静态缓存中驻留行数组（内容由解析结果缓存承接）。
        /// </summary>
        public static SourceFileEntry[] GetSourceFiles(Type type)
        {
            var paths = FindSourceFilePaths(type);
            if (paths.Length == 0)
            {
                return Array.Empty<SourceFileEntry>();
            }

            var entries = new List<SourceFileEntry>(paths.Length);
            foreach (var path in paths)
            {
                try
                {
                    var fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        entries.Add(new SourceFileEntry(path, File.ReadAllLines(fullPath)));
                    }
                }
                catch
                {
                    // IO 异常忽略，跳过该文件
                }
            }

            return entries.ToArray();
        }

        /// <summary>
        /// 查找类型对应的源文件相对路径（Assets/ 开头），结果按类型缓存。
        /// 引擎模块 / 预编译 DLL 类型直接返回空数组（不可能存在项目源码）。
        /// </summary>
        public static string[] FindSourceFilePaths(Type type)
        {
            if (type == null)
            {
                return Array.Empty<string>();
            }

            if (_sourcePathsCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var paths = FindSourceFilePathsUncached(type);
            _sourcePathsCache[type] = paths;
            return paths;
        }

        static string[] FindSourceFilePathsUncached(Type type)
        {
            // 前置程序集过滤：非脚本程序集（引擎模块、预编译 DLL）不可能存在项目源文件，
            // 直接短路，避免其触发昂贵的项目级内容扫描
            if (!ScriptAssemblyFilter.IsScriptAssembly(type.Assembly))
            {
                return Array.Empty<string>();
            }

            // 嵌套类型以最外层声明类型名为搜索名；泛型类型去掉 arity 后缀
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

            var results = new List<string>();

            // 第一轮（合并历史 Round 1/2）：单遍 AssetDatabase 搜索。
            // GetClass 验证收集全部匹配（含 partial 分部与文件名不一致的部分）；历史实现因
            // partial 判定恒真导致两轮全量 Load，此处收敛为单遍。
            // GetClass == null 的未编译脚本按文件名精确一致回退采信。
            foreach (var guid in AssetDatabase.FindAssets($"{typeName} t:MonoScript"))
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
                    if (TryGetExistingFullPath(path, out _) && !results.Contains(path))
                    {
                        results.Add(path);
                    }
                }
            }

            if (results.Count > 0)
            {
                return results.ToArray();
            }

            // 第二轮：项目级类型声明索引（历史 Round 3/4 的每类型全项目扫描收敛为一次全局索引）。
            // 文件名与类型名不一致、接口与他类同文件等场景在此命中；
            // 期望命名空间非空时按索引内的命名空间集合过滤，排除其他命名空间的同名类型。
            if (ProjectScriptIndex.TryGetTypePaths(typeName, out var candidates))
            {
                var expectedNamespace = searchType.Namespace;
                foreach (var candidate in candidates)
                {
                    if (results.Contains(candidate))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(expectedNamespace) &&
                        !ProjectScriptIndex.FileDeclaresNamespace(candidate, expectedNamespace))
                    {
                        continue;
                    }

                    if (TryGetExistingFullPath(candidate, out _))
                    {
                        results.Add(candidate);
                    }
                }
            }

            return results.Count > 0 ? results.ToArray() : Array.Empty<string>();
        }

        static bool TryGetExistingFullPath(string assetPath, out string fullPath)
        {
            fullPath = null;
            try
            {
                fullPath = Path.GetFullPath(assetPath);
                return File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
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

            // 行首语句关键字守卫：局部语句（var/return/if...）不可能构成成员声明
            var firstWordEnd = 0;
            while (firstWordEnd < line.Length &&
                   (char.IsLetterOrDigit(line[firstWordEnd]) || line[firstWordEnd] == '_'))
            {
                firstWordEnd++;
            }

            if (firstWordEnd > 0 && _statementStarterKeywords.Contains(line.Substring(0, firstWordEnd)))
            {
                return null;
            }

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
                !_declarationKeywords.Contains(genericMethodMatch.Groups[1].Value) &&
                IsValidIdentifier(genericMethodMatch.Groups[1].Value))
            {
                return genericMethodMatch.Groups[1].Value;
            }

            // 通用成员声明：修饰符 + 类型 + 成员名 + 终止符（{ ; = ( 之一）
            // 适用于大多数单行声明，如 "public int Count;" "public void Foo() { }"
            // 注意：表达式体 "=> " 中的 = 也会被此正则匹配，因此必须放在泛型方法正则之后
            var match = _memberDeclRegex.Match(line);
            if (match.Success && IsValidIdentifier(match.Groups[1].Value))
            {
                return match.Groups[1].Value;
            }

            // 简单匹配：任意标识符后跟 { ; = ( 之一
            // 作为通用正则的补充，捕获未被前者匹配的边缘情况
            var simpleMatch = Regex.Match(line, @"(\w+)\s*[{;=\(]");
            if (simpleMatch.Success && !_declarationKeywords.Contains(simpleMatch.Groups[1].Value) &&
                IsValidIdentifier(simpleMatch.Groups[1].Value))
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
    }
}
#endif
