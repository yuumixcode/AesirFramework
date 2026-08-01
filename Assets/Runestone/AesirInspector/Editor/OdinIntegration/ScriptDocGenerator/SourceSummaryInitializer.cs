#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 在 OdinIntegration 程序集加载时注入基于 OdinSourceFileHelper 的 Summary 解析器，
    /// 从源代码的 XML <c>/// &lt;summary&gt;</c> 注释中读取成员摘要。
    /// </summary>
    [InitializeOnLoad]
    public static class SourceSummaryInitializer
    {
        static readonly Dictionary<Type, Dictionary<string, string>> _cache =
            new Dictionary<Type, Dictionary<string, string>>();

        static SourceSummaryInitializer()
        {
            MemberData.SummaryResolver = ResolveSummaryFromSource;
        }

        static string ResolveSummaryFromSource(MemberInfo member)
        {
            var declaringType = member.DeclaringType;
            if (declaringType == null)
            {
                if (member is Type type)
                    declaringType = type;
                else
                    return null;
            }

            if (!_cache.TryGetValue(declaringType, out var memberSummaries))
            {
                memberSummaries = SourceSummaryParser.ParseSummariesForType(declaringType);
                _cache[declaringType] = memberSummaries;
            }

            if (memberSummaries == null)
                return null;

            var memberName = member is Type t ? t.Name : member.Name;
            return memberSummaries.TryGetValue(memberName, out var summary) ? summary : null;
        }
    }
}
#endif
