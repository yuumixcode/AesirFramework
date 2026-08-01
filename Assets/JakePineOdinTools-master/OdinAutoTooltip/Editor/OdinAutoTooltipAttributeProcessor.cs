// Copyright (c) 2026 Jake Pine
// SPDX-License-Identifier: MIT
// This software is provided "as is", without warranty of any kind. Use at your own risk.

#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Applies <see cref="TooltipAttribute"/> to Odin inspector members from XML <c>&lt;summary&gt;</c> doc comments
/// in source. Skips members that already have <see cref="TooltipAttribute"/> or
/// <see cref="PropertyTooltipAttribute"/>.
/// </summary>
public class OdinAutoTooltipAttributeProcessor : OdinAttributeProcessor<object>
{
    // --- Options (edit defaults here or assign at runtime) ---

    /// <summary>
    /// Master switch. When false, ProcessChildMemberAttributes returns immediately — no source
    /// parsing, no XML reading, no cache population. Zero overhead per repaint.
    /// </summary>
    public static readonly bool ENABLED = true;

    /// <summary>
    /// When true, reads <c>/// &lt;summary&gt;</c> doc comments from source and uses them as tooltips.
    /// </summary>
    public static bool USE_SUMMARIES = true;

    /// <summary>
    /// When true, uses the member name as the tooltip when no summary is found (or when
    /// <see cref="USE_SUMMARIES"/> is false). When false, members without a summary get no auto-tooltip.
    /// </summary>
    public static bool USE_MEMBER_NAME = true;

    /// <summary>
    /// Optional project hook. When set, return true to skip auto-tooltip for a member
    /// (e.g. compact inline foldout header fields where a tooltip icon would break layout).
    /// </summary>
    public static Func<InspectorProperty, MemberInfo, List<Attribute>, bool> ShouldSkipMember;

    // Cache: declaring type -> (member name -> summary)
    private static readonly Dictionary<Type, Dictionary<string, string>> summaryCache = new Dictionary<Type, Dictionary<string, string>>();

    static OdinAutoTooltipAttributeProcessor()
    {
        UnityEditor.AssemblyReloadEvents.afterAssemblyReload += () => summaryCache.Clear();
    }

    public override void ProcessChildMemberAttributes(InspectorProperty property, MemberInfo member, List<Attribute> attributes)
    {
        if (!ENABLED || (!USE_SUMMARIES && !USE_MEMBER_NAME))
            return;

        // Check if TooltipAttribute or PropertyTooltipAttribute is already applied
        if (!attributes.OfType<TooltipAttribute>().Any() && !attributes.OfType<PropertyTooltipAttribute>().Any())
        {
            if (ShouldSkipMember != null && ShouldSkipMember(property, member, attributes))
                return;

            string tooltip = ResolveTooltipText(member);
            if (string.IsNullOrEmpty(tooltip))
                return;

            attributes.Add(new TooltipAttribute(tooltip));
        }
    }

    private static string ResolveTooltipText(MemberInfo member)
    {
        string summary = USE_SUMMARIES ? GetSummaryFromSource(member) : null;
        if (!string.IsNullOrWhiteSpace(summary))
            return summary;

        if (USE_MEMBER_NAME)
            return member.Name;

        return null;
    }

    private static string GetSummaryFromSource(MemberInfo member)
    {
        Type declaringType = member.DeclaringType;
        if (declaringType == null)
            return null;

        if (!summaryCache.TryGetValue(declaringType, out Dictionary<string, string> memberSummaries))
        {
            // Cache the result even when null. ParseSummariesForType returns null whenever the source file
            // cannot be found — which is the case for every compiled-only type (Unity built-in components,
            // third-party DLLs, etc.). Without caching null, each repaint re-runs AssetDatabase.FindAssets and
            // file reads for those types, which is very costly in windows that draw many components at once
            // (e.g. PinnedInspectorWindow, which renders all components on a GameObject).
            memberSummaries = ParseSummariesForType(declaringType);
            summaryCache[declaringType] = memberSummaries;
        }

        if (memberSummaries != null && memberSummaries.TryGetValue(member.Name, out string summary))
            return summary;

        return null;
    }

    private static Dictionary<string, string> ParseSummariesForType(Type type)
    {
        string[] lines = OdinSourceFileHelper.GetSourceLines(type);
        if (lines == null)
            return null;

        string typeKey = OdinSourceFileHelper.GetTypeKey(type);
        if (OdinSourceFileHelper.TryGetTypeBodyRange(lines, typeKey, out int bodyStartIndex, out int bodyEndIndex))
        {
            string[] scopedLines = new string[bodyEndIndex - bodyStartIndex + 1];
            Array.Copy(lines, bodyStartIndex, scopedLines, 0, scopedLines.Length);
            return ExtractSummaries(scopedLines);
        }

        return ExtractSummaries(lines);
    }

    private static readonly Regex summaryContentRegex = new Regex(
        @"<summary>\s*(.*?)\s*</summary>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex xmlTagRegex = new Regex(
        @"<see\s+cref=""([^""]*)""\s*/?>|<[^>]+>",
        RegexOptions.Compiled);

    private static string StripXmlTags(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        // Replace <see cref="Type"/> with just the type name (last segment after dot)
        text = xmlTagRegex.Replace(text, m =>
        {
            if (m.Groups[1].Success)
            {
                string cref = m.Groups[1].Value;
                int dot = cref.LastIndexOf('.');
                return dot >= 0 ? cref.Substring(dot + 1) : cref;
            }
            return string.Empty;
        });
        // Collapse any double spaces left behind
        return Regex.Replace(text, @"  +", " ").Trim();
    }

    private static Dictionary<string, string> ExtractSummaries(string[] lines)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        int lineIndex = 0;

        while (lineIndex < lines.Length)
        {
            string trimmed = lines[lineIndex].TrimStart();

            if (!StartsSummaryDocComment(trimmed))
            {
                lineIndex++;
                continue;
            }

            List<string> summaryLines = CollectSummaryDocLines(lines, ref lineIndex);
            if (summaryLines.Count == 0)
                continue;

            lineIndex = SkipMemberPreambleLines(lines, lineIndex);

            while (lineIndex < lines.Length && string.IsNullOrWhiteSpace(lines[lineIndex]))
                lineIndex++;

            if (lineIndex >= lines.Length)
                break;

            string memberName = OdinSourceFileHelper.ExtractMemberName(lines[lineIndex].Trim());
            if (memberName == null)
                continue;

            string summary = ParseSummaryText(summaryLines);
            if (!string.IsNullOrWhiteSpace(summary))
                result[memberName] = summary;
        }

        return result;
    }

    private static bool StartsSummaryDocComment(string trimmedLine)
    {
        if (!trimmedLine.StartsWith("///", StringComparison.Ordinal))
            return false;

        string docContent = trimmedLine.Substring(3).TrimStart();
        return docContent.StartsWith("<summary>", StringComparison.Ordinal)
            || docContent.StartsWith("<summary ", StringComparison.Ordinal);
    }

    private static List<string> CollectSummaryDocLines(string[] lines, ref int lineIndex)
    {
        List<string> summaryLines = new List<string>();

        while (lineIndex < lines.Length)
        {
            string line = lines[lineIndex].TrimStart();
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

    private static int SkipMemberPreambleLines(string[] lines, int lineIndex)
    {
        while (lineIndex < lines.Length)
        {
            string attrLine = lines[lineIndex].TrimStart();
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

    private static string ParseSummaryText(List<string> summaryLines)
    {
        string fullSummary = string.Join(" ", summaryLines);
        Match match = summaryContentRegex.Match(fullSummary);
        string summary = match.Success
            ? match.Groups[1].Value.Trim()
            : fullSummary.Replace("<summary>", string.Empty).Replace("</summary>", string.Empty).Trim();
        summary = StripXmlTags(summary);
        return string.IsNullOrWhiteSpace(summary) ? null : summary;
    }
}
#endif
