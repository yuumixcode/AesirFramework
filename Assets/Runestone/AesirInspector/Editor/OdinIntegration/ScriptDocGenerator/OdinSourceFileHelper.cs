//
// 本文件提取自 JakePineOdinTools 项目 (MIT License, Copyright (c) 2026 Jake Pine)
// https://github.com/JakePineGames/JakePineOdinTools
// ----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
 /// <summary>
 /// 共享源文件解析工具，为 Odin 编辑器插件提供 .cs 文件定位与成员声明行提取能力。
 /// </summary>
 public static class OdinSourceFileHelper
 {
 static readonly Dictionary<Type, string[]> _sourceLinesCache = new Dictionary<Type, string[]>();

 // 延迟构建的全局 type-name → 绝对文件路径索引，首次未命中时构建，后续复用。程序集重载时清空。
 static Dictionary<string, string> _typeToFileIndex;

 static readonly Regex _typeDefinitionRegex = new Regex(
 @"\b(class|struct|enum|interface)\s+(\w+)",
 RegexOptions.Compiled);

 static readonly Regex _memberDeclRegex = new Regex(
 @"(?:public|private|protected|internal|\s|static|readonly|const|volatile|new|override|virtual|abstract|sealed|async|partial)*\s+\S+\s+(\w+)\s*[{;=\(]",
 RegexOptions.Compiled);

 static readonly Regex _leadingAttributesRegex = new Regex(
 @"^(\s*\[.*?\]\s*)+",
 RegexOptions.Compiled);

 static readonly HashSet<string> _declarationKeywords = new HashSet<string>(StringComparer.Ordinal)
 {
 "class", "struct", "enum", "interface", "namespace",
 "if", "else", "while", "for", "foreach", "return", "using",
 "get", "set", "public", "private", "protected", "internal",
 "static", "readonly", "void", "new", "override", "virtual",
 "abstract", "sealed", "async", "partial", "event"
 };

 static OdinSourceFileHelper()
 {
 AssemblyReloadEvents.afterAssemblyReload += ClearCache;
 }

 /// <summary>
 /// 清空所有缓存（源文件行缓存与类型索引）。
 /// </summary>
 public static void ClearCache()
 {
 _sourceLinesCache.Clear();
 _typeToFileIndex = null;
 }

 /// <summary>
 /// 获取类型对应的 .cs 源文件行数组，结果会被缓存。
 /// </summary>
 public static string[] GetSourceLines(Type type)
 {
 if (type == null)
 return null;

 if (_sourceLinesCache.TryGetValue(type, out string[] cachedLines))
 return cachedLines;

 string sourceFilePath = FindSourceFile(type);
 if (sourceFilePath == null)
 return null;

 try
 {
 string[] lines = File.ReadAllLines(sourceFilePath);
 _sourceLinesCache[type] = lines;
 return lines;
 }
 catch
 {
 return null;
 }
 }

 /// <summary>
 /// 通过 AssetDatabase 查找类型对应的 .cs 源文件绝对路径。
 /// </summary>
 public static string FindSourceFile(Type type)
 {
 Type searchType = type;
 while (searchType.DeclaringType != null)
 searchType = searchType.DeclaringType;

 string typeName = searchType.Name;
 int backtick = typeName.IndexOf('`');
 if (backtick >= 0)
 typeName = typeName.Substring(0, backtick);

 string[] guids = AssetDatabase.FindAssets($"{typeName} t:MonoScript");
 string preferredFileName = typeName + ".cs";
 for (int i = 0; i < guids.Length; i++)
 {
 string path = AssetDatabase.GUIDToAssetPath(guids[i]);
 if (!path.EndsWith(preferredFileName, StringComparison.OrdinalIgnoreCase))
 continue;

 MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
 if (monoScript != null)
 {
 Type scriptClass = monoScript.GetClass();
 if (scriptClass == searchType)
 return Path.GetFullPath(path);
 if (scriptClass == null && monoScript.name == typeName)
 return Path.GetFullPath(path);
 }
 }

 for (int i = 0; i < guids.Length; i++)
 {
 string path = AssetDatabase.GUIDToAssetPath(guids[i]);
 MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
 if (monoScript != null)
 {
 Type scriptClass = monoScript.GetClass();
 if (scriptClass == searchType)
 return Path.GetFullPath(path);
 if (scriptClass == null && monoScript.name == typeName)
 return Path.GetFullPath(path);
 }
 }

 for (int i = 0; i < guids.Length; i++)
 {
 string path = AssetDatabase.GUIDToAssetPath(guids[i]);
 string fullPath = Path.GetFullPath(path);
 if (!File.Exists(fullPath))
 continue;

 string content = File.ReadAllText(fullPath);
 foreach (Match match in _typeDefinitionRegex.Matches(content))
 {
 if (match.Groups[2].Value == typeName)
 return fullPath;
 }
 }

 // 最后手段：全局索引，仅构建一次并缓存，后续查找为 O(1)。
 Dictionary<string, string> index = GetOrBuildTypeIndex();
 if (index.TryGetValue(typeName, out string indexedPath))
 return indexedPath;

 return null;
 }

 static Dictionary<string, string> GetOrBuildTypeIndex()
 {
 if (_typeToFileIndex != null)
 return _typeToFileIndex;

 _typeToFileIndex = new Dictionary<string, string>(StringComparer.Ordinal);
 string[] allGuids = AssetDatabase.FindAssets("t:MonoScript");
 for (int i = 0; i < allGuids.Length; i++)
 {
 string assetPath = AssetDatabase.GUIDToAssetPath(allGuids[i]);
 string fullPath = Path.GetFullPath(assetPath);
 if (!File.Exists(fullPath))
 continue;
 try
 {
 string content = File.ReadAllText(fullPath);
 foreach (Match match in _typeDefinitionRegex.Matches(content))
 {
 string name = match.Groups[2].Value;
 if (!_typeToFileIndex.ContainsKey(name))
 _typeToFileIndex[name] = fullPath;
 }
 }
 catch { }
 }

 return _typeToFileIndex;
 }

 /// <summary>
 /// 获取类型的嵌套层级键（如 "Outer.Inner"），用于定位源文件中的类型体范围。
 /// </summary>
 public static string GetTypeKey(Type type)
 {
 if (type == null)
 return null;

 var parts = new List<string>();
 Type current = type;
 while (current != null)
 {
 string name = current.Name;
 int backtick = name.IndexOf('`');
 if (backtick >= 0)
 name = name.Substring(0, backtick);

 parts.Insert(0, name);
 current = current.DeclaringType;
 }

 return string.Join(".", parts);
 }

 /// <summary>
 /// 从声明行中提取成员名称。
 /// </summary>
 public static string ExtractMemberName(string declarationLine)
 {
 if (string.IsNullOrWhiteSpace(declarationLine))
 return null;

 // 始终解析去除了注释和字符串的副本，避免注释或字符串字面量中的字符干扰结构化正则。
 string sanitized = StripStringsAndComment(declarationLine);
 string line = _leadingAttributesRegex.Replace(sanitized, "").TrimStart();

 Match enumMatch = Regex.Match(line, @"^\s*(\w+)\s*[,=]");
 if (enumMatch.Success)
 {
 string enumName = enumMatch.Groups[1].Value;
 if (!_declarationKeywords.Contains(enumName))
 return enumName;
 }

 Match match = _memberDeclRegex.Match(line);
 if (match.Success)
 return match.Groups[1].Value;

 Match simpleMatch = Regex.Match(line, @"(\w+)\s*[{;=\(]");
 if (simpleMatch.Success)
 {
 string name = simpleMatch.Groups[1].Value;
 if (!_declarationKeywords.Contains(name))
 return name;
 }

 return null;
 }

 /// <summary>
 /// 判断声明行是否为字段声明（而非属性、方法或类型声明）。
 /// </summary>
 public static bool IsFieldDeclarationLine(string declarationLine)
 {
 if (string.IsNullOrWhiteSpace(declarationLine))
 return false;

 string sanitized = StripStringsAndComment(declarationLine);
 if (string.IsNullOrWhiteSpace(sanitized))
 return false;

 string line = _leadingAttributesRegex.Replace(sanitized, "").TrimStart();

 // '(' 在 '=' 之前出现的是方法/索引器签名（含表达式体成员），字段只可能在 '=' 之后出现 '('（初始化器调用）。
 int parenIndex = line.IndexOf('(');
 int equalsIndex = line.IndexOf('=');
 if (parenIndex >= 0 && (equalsIndex < 0 || parenIndex < equalsIndex))
 return false;

 // 表达式体属性 `public int X => expr;` 不是字段。区分字段初始化器含 lambda 的情况：
 // 字段在 '=>' 之前有真正的赋值 '='，表达式体成员没有。
 int arrowIndex = line.IndexOf("=>", StringComparison.Ordinal);
 if (arrowIndex >= 0 && !HasAssignmentBefore(line, arrowIndex))
 return false;

 // 属性访问器标记这不是字段。
 if (line.Contains(" get;") || line.Contains(" set;") || line.Contains(" get ") || line.Contains(" set ")
 || line.Contains("{get") || line.Contains("{ get"))
 return false;

 string memberName = ExtractMemberName(declarationLine);
 return !string.IsNullOrEmpty(memberName);
 }

 /// <summary>
 /// 判断声明行是否为属性或方法声明（而非字段）。
 /// </summary>
 public static bool IsPropertyOrMethodDeclarationLine(string declarationLine)
 {
 if (string.IsNullOrWhiteSpace(declarationLine))
 return false;

 string sanitized = StripStringsAndComment(declarationLine);
 if (string.IsNullOrWhiteSpace(sanitized))
 return false;

 string line = _leadingAttributesRegex.Replace(sanitized, "").TrimStart();
 if (line.Length == 0)
 return false;

 if (_typeDefinitionRegex.IsMatch(line) || line.StartsWith("namespace", StringComparison.Ordinal))
 return false;

 if (string.IsNullOrEmpty(ExtractMemberName(declarationLine)))
 return false;

 int parenIndex = line.IndexOf('(');
 int equalsIndex = line.IndexOf('=');
 if (parenIndex >= 0 && (equalsIndex < 0 || parenIndex < equalsIndex))
 return true;

 int arrowIndex = line.IndexOf("=>", StringComparison.Ordinal);
 if (arrowIndex >= 0 && !HasAssignmentBefore(line, arrowIndex))
 return true;

 if (line.Contains(" get;") || line.Contains(" set;") || line.Contains(" get ") || line.Contains(" set ")
 || line.Contains("{get") || line.Contains("{ get"))
 return true;

 return false;
 }

 /// <summary>
 /// 返回成员声明的结束行索引（终止 ';' 所在行，或匹配 '}' 闭合的行）。
 /// </summary>
 public static int FindMemberEndLine(string[] lines, int declStart)
 {
 if (lines == null || declStart < 0 || declStart >= lines.Length)
 return declStart;

 int depth = 0;
 bool seenBrace = false;
 for (int i = declStart; i < lines.Length; i++)
 {
 string code = StripStringsAndComment(lines[i]);
 for (int c = 0; c < code.Length; c++)
 {
 char ch = code[c];
 if (ch == '{')
 {
 depth++;
 seenBrace = true;
 }
 else if (ch == '}')
 {
 depth--;
 if (seenBrace && depth <= 0)
 return i;
 }
 else if (ch == ';' && !seenBrace && depth == 0)
 {
 return i;
 }
 }
 }

 return lines.Length - 1;
 }

 /// <summary>
 /// 尝试获取类型体在源文件中的行范围（开括号行到匹配的闭括号行）。
 /// </summary>
 public static bool TryGetTypeBodyRange(string[] lines, string typeKey, out int bodyStartIndex, out int bodyEndIndex)
 {
 bodyStartIndex = -1;
 bodyEndIndex = -1;

 if (lines == null || string.IsNullOrEmpty(typeKey))
 return false;

 string[] typeNames = typeKey.Split('.');
 int searchLine = 0;

 for (int partIndex = 0; partIndex < typeNames.Length; partIndex++)
 {
 string typeName = typeNames[partIndex];
 int declarationLine = -1;

 for (int i = searchLine; i < lines.Length; i++)
 {
 Match match = _typeDefinitionRegex.Match(StripStringsAndComment(lines[i]));
 if (match.Success && match.Groups[2].Value == typeName)
 {
 declarationLine = i;
 break;
 }
 }

 if (declarationLine < 0)
 return false;

 int openBraceLine = FindOpenBraceLine(lines, declarationLine);
 if (openBraceLine < 0)
 return false;

 int closeBraceLine = FindMatchingCloseBrace(lines, openBraceLine);
 if (closeBraceLine < 0)
 return false;

 if (partIndex == typeNames.Length - 1)
 {
 bodyStartIndex = openBraceLine;
 bodyEndIndex = closeBraceLine;
 return true;
 }

 searchLine = openBraceLine + 1;
 if (searchLine > closeBraceLine)
 return false;
 }

 return false;
 }

 /// <summary>
 /// 从声明行开始查找第一个包含 '{' 的行。
 /// </summary>
 public static int FindOpenBraceLine(string[] lines, int declarationLine)
 {
 if (lines == null || declarationLine < 0 || declarationLine >= lines.Length)
 return -1;

 if (StripStringsAndComment(lines[declarationLine]).Contains("{"))
 return declarationLine;

 for (int i = declarationLine + 1; i < lines.Length; i++)
 {
 if (StripStringsAndComment(lines[i]).Contains("{"))
 return i;
 }

 return -1;
 }

 /// <summary>
 /// 将源代码行拆分为代码部分和尾随注释部分，忽略字符串字面量内的 //。
 /// </summary>
 public static void SplitCodeAndComment(string line, out string codePart, out string commentPart)
 {
 if (string.IsNullOrEmpty(line))
 {
 codePart = line;
 commentPart = string.Empty;
 return;
 }

 bool inString = false;
 char stringChar = '\0';
 for (int i = 0; i < line.Length - 1; i++)
 {
 char c = line[i];
 if (inString)
 {
 if (c == '\\')
 {
 i++;
 continue;
 }

 if (c == stringChar)
 inString = false;

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
 codePart = line.Substring(0, i).TrimEnd();
 commentPart = line.Substring(i);
 return;
 }
 }

 codePart = line;
 commentPart = string.Empty;
 }

 /// <summary>
 /// 从开括号行开始查找匹配的闭括号行索引。
 /// </summary>
 public static int FindMatchingCloseBrace(string[] lines, int openBraceLineIndex)
 {
 int depth = 0;
 for (int i = openBraceLineIndex; i < lines.Length; i++)
 {
 string code = StripStringsAndComment(lines[i]);
 for (int c = 0; c < code.Length; c++)
 {
 if (code[c] == '{')
 depth++;
 else if (code[c] == '}')
 {
 depth--;
 if (depth == 0)
 return i;
 }
 }
 }

 return -1;
 }

 /// <summary>
 /// 返回单行中结构性 '{' 和 '}' 的净深度变化（忽略字符串字面量和 // 注释中的内容）。
 /// </summary>
 public static int GetNetBraceDepthChange(string line)
 {
 string code = StripStringsAndComment(line);
 int depth = 0;
 for (int i = 0; i < code.Length; i++)
 {
 if (code[i] == '{')
 depth++;
 else if (code[i] == '}')
 depth--;
 }

 return depth;
 }

 /// <summary>
 /// 返回去除了字符串/字符字面量内容和尾随 // 注释后的行，使剩余的结构性字符可安全扫描。仅处理单行。
 /// </summary>
 public static string StripStringsAndComment(string line)
 {
 if (string.IsNullOrEmpty(line))
 return line ?? string.Empty;

 var builder = new StringBuilder(line.Length);
 bool inString = false;
 char stringChar = '\0';
 for (int i = 0; i < line.Length; i++)
 {
 char c = line[i];
 if (inString)
 {
 if (c == '\\' && i + 1 < line.Length)
 {
 i++;
 continue;
 }

 if (c == stringChar)
 inString = false;

 continue;
 }

 if (c == '"' || c == '\'')
 {
 inString = true;
 stringChar = c;
 continue;
 }

 if (c == '/' && i + 1 < line.Length && line[i + 1] == '/')
 break;

 builder.Append(c);
 }

 return builder.ToString();
 }

 static bool HasAssignmentBefore(string line, int limit)
 {
 for (int i = 0; i < limit && i < line.Length; i++)
 {
 if (line[i] != '=')
 continue;

 char prev = i > 0 ? line[i - 1] : '\0';
 char next = i + 1 < line.Length ? line[i + 1] : '\0';
 if (next != '=' && next != '>' && prev != '=' && prev != '!' && prev != '<' && prev != '>')
 return true;
 }

 return false;
 }
 }
}
#endif
