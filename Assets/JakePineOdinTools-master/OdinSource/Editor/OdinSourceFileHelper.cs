// Copyright (c) 2026 Jake Pine
// SPDX-License-Identifier: MIT
// This software is provided "as is", without warranty of any kind. Use at your own risk.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Shared source-file utilities for Jake Pine Odin editor plugins.
/// Locates the .cs file for a type and extracts member declaration lines from raw text.
/// </summary>
public static class OdinSourceFileHelper
{
    private static readonly Dictionary<Type, string[]> sourceLinesCache = new Dictionary<Type, string[]>();

    // Lazy full-project type-name → absolute file path index. Built once on the first miss of the
    // name-based search, then reused for all subsequent lookups. Cleared on assembly reload.
    private static Dictionary<string, string> typeToFileIndex = null;

    private static readonly Regex typeDefinitionRegex = new Regex(
        @"\b(class|struct|enum|interface)\s+(\w+)",
        RegexOptions.Compiled);

    private static readonly Regex memberDeclRegex = new Regex(
        @"(?:public|private|protected|internal|\s|static|readonly|const|volatile|new|override|virtual|abstract|sealed|async|partial)*\s+\S+\s+(\w+)\s*[{;=\(]",
        RegexOptions.Compiled);

    private static readonly Regex leadingAttributesRegex = new Regex(
        @"^(\s*\[.*?\]\s*)+",
        RegexOptions.Compiled);

    private static readonly HashSet<string> declarationKeywords = new HashSet<string>(StringComparer.Ordinal)
    {
        "class", "struct", "enum", "interface", "namespace",
        "if", "else", "while", "for", "foreach", "return", "using",
        "get", "set", "public", "private", "protected", "internal",
        "static", "readonly", "void", "new", "override", "virtual",
        "abstract", "sealed", "async", "partial", "event"
    };

    static OdinSourceFileHelper()
    {
        UnityEditor.AssemblyReloadEvents.afterAssemblyReload += ClearCache;
    }

    public static void ClearCache()
    {
        sourceLinesCache.Clear();
        typeToFileIndex = null;
    }

    public static string[] GetSourceLines(Type type)
    {
        if (type == null)
        {
            return null;
        }

        if (sourceLinesCache.TryGetValue(type, out string[] cachedLines))
        {
            return cachedLines;
        }

        string sourceFilePath = FindSourceFile(type);
        if (sourceFilePath == null)
        {
            return null;
        }

        try
        {
            string[] lines = File.ReadAllLines(sourceFilePath);
            sourceLinesCache[type] = lines;
            return lines;
        }
        catch
        {
            return null;
        }
    }

    public static string FindSourceFile(Type type)
    {
        Type searchType = type;
        while (searchType.DeclaringType != null)
        {
            searchType = searchType.DeclaringType;
        }

        string typeName = searchType.Name;
        int backtick = typeName.IndexOf('`');
        if (backtick >= 0)
        {
            typeName = typeName.Substring(0, backtick);
        }

        string[] guids = AssetDatabase.FindAssets($"{typeName} t:MonoScript");
        string preferredFileName = typeName + ".cs";
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (!path.EndsWith(preferredFileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (monoScript != null)
            {
                Type scriptClass = monoScript.GetClass();
                if (scriptClass == searchType)
                {
                    return Path.GetFullPath(path);
                }

                if (scriptClass == null && monoScript.name == typeName)
                {
                    return Path.GetFullPath(path);
                }
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
                {
                    return Path.GetFullPath(path);
                }

                if (scriptClass == null && monoScript.name == typeName)
                {
                    return Path.GetFullPath(path);
                }
            }
        }

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath))
            {
                continue;
            }

            string content = File.ReadAllText(fullPath);
            foreach (Match match in typeDefinitionRegex.Matches(content))
            {
                if (match.Groups[2].Value == typeName)
                {
                    return fullPath;
                }
            }
        }

        // Last resort: full-project index. Built once and cached so subsequent lookups are O(1).
        Dictionary<string, string> index = GetOrBuildTypeIndex();
        if (index.TryGetValue(typeName, out string indexedPath))
            return indexedPath;

        return null;
    }

    private static Dictionary<string, string> GetOrBuildTypeIndex()
    {
        if (typeToFileIndex != null)
            return typeToFileIndex;

        typeToFileIndex = new Dictionary<string, string>(StringComparer.Ordinal);
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
                foreach (Match match in typeDefinitionRegex.Matches(content))
                {
                    string name = match.Groups[2].Value;
                    // First file wins — consistent with the rest of FindSourceFile.
                    if (!typeToFileIndex.ContainsKey(name))
                        typeToFileIndex[name] = fullPath;
                }
            }
            catch { }
        }
        return typeToFileIndex;
    }

    public static string GetTypeKey(Type type)
    {
        if (type == null)
        {
            return null;
        }

        List<string> parts = new List<string>();
        Type current = type;
        while (current != null)
        {
            string name = current.Name;
            int backtick = name.IndexOf('`');
            if (backtick >= 0)
            {
                name = name.Substring(0, backtick);
            }

            parts.Insert(0, name);
            current = current.DeclaringType;
        }

        return string.Join(".", parts);
    }

    public static string ExtractMemberName(string declarationLine)
    {
        if (string.IsNullOrWhiteSpace(declarationLine))
        {
            return null;
        }

        // Always parse a comment- and string-free copy. Comments or string literals can contain any
        // characters ([ ] ( ) ; = , { } etc.) that would otherwise confuse the structural regexes below.
        string sanitized = StripStringsAndComment(declarationLine);
        string line = leadingAttributesRegex.Replace(sanitized, "").TrimStart();

        Match enumMatch = Regex.Match(line, @"^\s*(\w+)\s*[,=]");
        if (enumMatch.Success)
        {
            string enumName = enumMatch.Groups[1].Value;
            if (!declarationKeywords.Contains(enumName))
            {
                return enumName;
            }
        }

        Match match = memberDeclRegex.Match(line);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        Match simpleMatch = Regex.Match(line, @"(\w+)\s*[{;=\(]");
        if (simpleMatch.Success)
        {
            string name = simpleMatch.Groups[1].Value;
            if (!declarationKeywords.Contains(name))
            {
                return name;
            }
        }

        return null;
    }

    public static bool IsFieldDeclarationLine(string declarationLine)
    {
        if (string.IsNullOrWhiteSpace(declarationLine))
        {
            return false;
        }

        // Strip comments and string-literal contents up front so none of the checks below can be fooled
        // by characters that merely appear inside a comment or a string (e.g. "x; // note (inclusive)"
        // or `string s = "get; set;";`).
        string sanitized = StripStringsAndComment(declarationLine);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return false;
        }

        string line = leadingAttributesRegex.Replace(sanitized, "").TrimStart();

        // A '(' that appears before any '=' is a parameter list — i.e. a method/indexer signature, including
        // expression-bodied members like `public int Foo() => 5;`. A field can only have '(' after '=' (a
        // call in its initializer, e.g. `public int x = Make();`).
        int parenIndex = line.IndexOf('(');
        int equalsIndex = line.IndexOf('=');
        if (parenIndex >= 0 && (equalsIndex < 0 || parenIndex < equalsIndex))
        {
            return false;
        }

        // Expression-bodied property `public int X => expr;` is a member body, not a field. Distinguish it from
        // a field whose initializer contains a lambda (`public Func<int,int> f = x => x * 2;`): the field has a
        // real assignment '=' before the '=>' arrow, the expression-bodied member does not.
        int arrowIndex = line.IndexOf("=>", StringComparison.Ordinal);
        if (arrowIndex >= 0 && !HasAssignmentBefore(line, arrowIndex))
        {
            return false;
        }

        // Property accessors mark a property, not a field.
        if (line.Contains(" get;") || line.Contains(" set;") || line.Contains(" get ") || line.Contains(" set ")
            || line.Contains("{get") || line.Contains("{ get"))
        {
            return false;
        }

        string memberName = ExtractMemberName(declarationLine);
        return !string.IsNullOrEmpty(memberName);
    }

    // True if a standalone assignment '=' (not part of ==, !=, <=, >= or =>) appears before <limit>.
    private static bool HasAssignmentBefore(string line, int limit)
    {
        for (int i = 0; i < limit && i < line.Length; i++)
        {
            if (line[i] != '=')
            {
                continue;
            }

            char prev = i > 0 ? line[i - 1] : '\0';
            char next = i + 1 < line.Length ? line[i + 1] : '\0';
            if (next != '=' && next != '>' && prev != '=' && prev != '!' && prev != '<' && prev != '>')
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// True if the line declares a property or method rather than a field. Batch attributes are propagated onto
    /// these members too (e.g. inspector-visible properties and <c>[Button]</c> methods), not just fields.
    /// </summary>
    public static bool IsPropertyOrMethodDeclarationLine(string declarationLine)
    {
        if (string.IsNullOrWhiteSpace(declarationLine))
        {
            return false;
        }

        string sanitized = StripStringsAndComment(declarationLine);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return false;
        }

        string line = leadingAttributesRegex.Replace(sanitized, "").TrimStart();
        if (line.Length == 0)
        {
            return false;
        }

        // Type declarations (nested class/struct/enum/interface/namespace) are not members we propagate onto.
        if (typeDefinitionRegex.IsMatch(line) || line.StartsWith("namespace", StringComparison.Ordinal))
        {
            return false;
        }

        if (string.IsNullOrEmpty(ExtractMemberName(declarationLine)))
        {
            return false;
        }

        // Method or indexer: a '(' parameter list that comes before any '=' (a signature, not an initializer
        // call). Covers both block-bodied and expression-bodied methods.
        int parenIndex = line.IndexOf('(');
        int equalsIndex = line.IndexOf('=');
        if (parenIndex >= 0 && (equalsIndex < 0 || parenIndex < equalsIndex))
        {
            return true;
        }

        // Expression-bodied property: '=>' that is not a field initializer lambda.
        int arrowIndex = line.IndexOf("=>", StringComparison.Ordinal);
        if (arrowIndex >= 0 && !HasAssignmentBefore(line, arrowIndex))
        {
            return true;
        }

        // Property with accessors.
        if (line.Contains(" get;") || line.Contains(" set;") || line.Contains(" get ") || line.Contains(" set ")
            || line.Contains("{get") || line.Contains("{ get"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the index of the line on which a member declaration starting at <paramref name="declStart"/> ends:
    /// the line carrying its terminating ';' (field, expression-bodied, or auto member) or the matching '}' that
    /// closes its '{ }' body (block-bodied property or method). Lets the parser skip over member bodies so their
    /// inner statements are never mistaken for fields.
    /// </summary>
    public static int FindMemberEndLine(string[] lines, int declStart)
    {
        if (lines == null || declStart < 0 || declStart >= lines.Length)
        {
            return declStart;
        }

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
                    {
                        return i;
                    }
                }
                else if (ch == ';' && !seenBrace && depth == 0)
                {
                    return i;
                }
            }
        }

        return lines.Length - 1;
    }

    public static bool TryGetTypeBodyRange(string[] lines, string typeKey, out int bodyStartIndex, out int bodyEndIndex)
    {
        bodyStartIndex = -1;
        bodyEndIndex = -1;

        if (lines == null || string.IsNullOrEmpty(typeKey))
        {
            return false;
        }

        string[] typeNames = typeKey.Split('.');
        int searchLine = 0;

        for (int partIndex = 0; partIndex < typeNames.Length; partIndex++)
        {
            string typeName = typeNames[partIndex];
            int declarationLine = -1;

            for (int i = searchLine; i < lines.Length; i++)
            {
                Match match = typeDefinitionRegex.Match(StripStringsAndComment(lines[i]));
                if (match.Success && match.Groups[2].Value == typeName)
                {
                    declarationLine = i;
                    break;
                }
            }

            if (declarationLine < 0)
            {
                return false;
            }

            int openBraceLine = FindOpenBraceLine(lines, declarationLine);
            if (openBraceLine < 0)
            {
                return false;
            }

            int closeBraceLine = FindMatchingCloseBrace(lines, openBraceLine);
            if (closeBraceLine < 0)
            {
                return false;
            }

            if (partIndex == typeNames.Length - 1)
            {
                bodyStartIndex = openBraceLine;
                bodyEndIndex = closeBraceLine;
                return true;
            }

            searchLine = openBraceLine + 1;
            if (searchLine > closeBraceLine)
            {
                return false;
            }
        }

        return false;
    }

    public static int FindOpenBraceLine(string[] lines, int declarationLine)
    {
        if (lines == null || declarationLine < 0 || declarationLine >= lines.Length)
        {
            return -1;
        }

        if (StripStringsAndComment(lines[declarationLine]).Contains("{"))
        {
            return declarationLine;
        }

        for (int i = declarationLine + 1; i < lines.Length; i++)
        {
            if (StripStringsAndComment(lines[i]).Contains("{"))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Splits a source line into code and trailing comment, ignoring // that appears inside string literals.
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
                codePart = line.Substring(0, i).TrimEnd();
                commentPart = line.Substring(i);
                return;
            }
        }

        codePart = line;
        commentPart = string.Empty;
    }

    public static int FindMatchingCloseBrace(string[] lines, int openBraceLineIndex)
    {
        int depth = 0;
        for (int i = openBraceLineIndex; i < lines.Length; i++)
        {
            // Count only structural braces — ignore any '{' or '}' inside string literals or // comments
            // (e.g. `string s = "a } b";` or `// closes with }`).
            string code = StripStringsAndComment(lines[i]);
            for (int c = 0; c < code.Length; c++)
            {
                if (code[c] == '{')
                {
                    depth++;
                }
                else if (code[c] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Net change in brace depth from structural <c>{</c> and <c>}</c> on a single line (string literals and
    /// <c>//</c> comments ignored).
    /// </summary>
    public static int GetNetBraceDepthChange(string line)
    {
        string code = StripStringsAndComment(line);
        int depth = 0;
        for (int i = 0; i < code.Length; i++)
        {
            if (code[i] == '{')
            {
                depth++;
            }
            else if (code[i] == '}')
            {
                depth--;
            }
        }

        return depth;
    }

    /// <summary>
    /// Returns the line with all string/char literal contents and any trailing <c>//</c> comment removed, so the
    /// remaining structural characters (<c>{ } [ ] ( ) ; = ,</c>) can be scanned safely. Single-line scope only:
    /// multi-line block comments (<c>/* */</c>) and multi-line verbatim strings (<c>@"..."</c> spanning lines) are
    /// not tracked across lines — keep braces balanced inside those if you use them.
    /// </summary>
    public static string StripStringsAndComment(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return line ?? string.Empty;
        }

        StringBuilder builder = new StringBuilder(line.Length);
        bool inString = false;
        char stringChar = '\0';
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inString)
            {
                if (c == '\\' && i + 1 < line.Length)
                {
                    i++; // skip the escaped character
                    continue;
                }

                if (c == stringChar)
                {
                    inString = false;
                }

                continue; // drop the literal's contents
            }

            if (c == '"' || c == '\'')
            {
                inString = true;
                stringChar = c;
                continue;
            }

            if (c == '/' && i + 1 < line.Length && line[i + 1] == '/')
            {
                break; // rest of the line is a comment
            }

            builder.Append(c);
        }

        return builder.ToString();
    }
}
#endif
