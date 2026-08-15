#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 在 OdinIntegration 程序集加载时注入 Summary 解析器。
    /// 优先检查 [Summary] 特性；若无则从源代码的 XML <c>/// &lt;summary&gt;</c> 注释中读取。
    /// 使用全限定键（AssemblyName.Namespace.TypeName.MemberName）避免跨程序集同名类型冲突。
    /// </summary>
    [InitializeOnLoad]
    public static class SourceSummaryInitializer
    {
        // Type → 源文件条目数组缓存（路径 + 代码内容）
        static readonly Dictionary<Type, SourceFileEntry[]> _sourceFilesCache =
            new Dictionary<Type, SourceFileEntry[]>();

        // Type → 全限定键 → summary 字典
        static readonly Dictionary<Type, Dictionary<string, string>> _summaryCache =
            new Dictionary<Type, Dictionary<string, string>>();

        static SourceSummaryInitializer() => MemberData.SummaryResolver = ResolveSummary;

        static string ResolveSummary(MemberInfo member)
        {
            // Step 1: 优先检查 [Summary] 特性
            var attr = member.GetCustomAttribute<SummaryAttribute>();
            if (attr != null)
            {
                return attr.GetSummary();
            }

            // Step 2: 从源代码 XML 注释查找
            var declaringType = member.DeclaringType;
            if (declaringType == null)
            {
                if (member is Type type)
                {
                    declaringType = type;
                }
                else
                {
                    return null;
                }
            }

            if (!_summaryCache.TryGetValue(declaringType, out var summaries))
            {
                var entries = GetOrFindSourceFiles(declaringType);
                summaries =
                    SourceSummaryParser.ParseSummaries(entries, declaringType.Assembly.GetName().Name);
                _summaryCache[declaringType] = summaries;
            }

            if (summaries == null || summaries.Count == 0)
            {
                return null;
            }

            // 构造全限定键
            // 键格式：AssemblyName.Namespace.TypeName[.MemberName(ParamTypes)]
            // 程序集名前缀避免不同程序集中同名命名空间+类型名的键冲突
            // 注意：对 Type 自身，用 member 的 FullName（嵌套类型需用自身全名，而非 DeclaringType 的）
            var keyType = member is Type ? (Type)member : declaringType;
            var assemblyName = keyType.Assembly.GetName().Name;
            var typeFullName = keyType.FullName ?? keyType.Name;
            // Type.FullName 对泛型类型返回 "Namespace.TypeName`1"，但源码解析器存储的键是 "Namespace.TypeName"
            // 需要去掉反引号及后续 arity 数字，使查询键与存储键一致
            var backtickIndex = typeFullName.IndexOf('`');
            if (backtickIndex >= 0)
            {
                typeFullName = typeFullName.Substring(0, backtickIndex);
            }

            // 嵌套类型：Type.FullName 返回 "Namespace.OuterType+InnerType"
            // 但源码解析器把嵌套类型的 summary 存储在 "Namespace.InnerType" 键下
            // （解析器通过正则匹配 struct/class 声明，不跟踪嵌套层级）
            // 需要去掉 OuterType+ 前缀，只保留 Namespace.InnerType
            var plusIndex = typeFullName.IndexOf('+');
            if (plusIndex >= 0)
            {
                // 提取命名空间部分（最后一个 . 之前的内容）
                var lastDot = typeFullName.LastIndexOf('.', plusIndex);
                if (lastDot >= 0)
                {
                    typeFullName = typeFullName.Substring(0, lastDot + 1) +
                                   typeFullName.Substring(plusIndex + 1);
                }
                else
                {
                    typeFullName = typeFullName.Substring(plusIndex + 1);
                }
            }

            var fullKey = assemblyName + "." + typeFullName;
            string key;
            string shortKey;
            if (member is Type)
            {
                key = fullKey;
                shortKey = member.Name;
            }
            else if (member is MethodInfo methodInfo)
            {
                // 对方法成员，附加参数类型列表到键中，区分重载方法
                // 与解析器端 ExtractParameterTypesFromDecl 的格式一致：(Type1, Type2)
                var paramTypeNames = GetParameterTypeNames(methodInfo);
                key = fullKey + "." + member.Name + "(" + paramTypeNames + ")";
                shortKey = member.Name;
            }
            else
            {
                key = fullKey + "." + member.Name;
                shortKey = member.Name;
            }

            // 先用全限定键查询（带参数签名的方法键或普通成员键）
            if (summaries.TryGetValue(key, out var summary))
            {
                return summary;
            }

            // 回退到短键（不含参数签名，兼容无参数列表的旧格式或非方法成员）
            if (summaries.TryGetValue(shortKey, out summary))
            {
                return summary;
            }

            // 对方法成员，再尝试不带参数的键（兼容解析器未能提取参数列表的情况）
            if (member is MethodInfo)
            {
                var noParamKey = fullKey + "." + member.Name;
                if (summaries.TryGetValue(noParamKey, out summary))
                {
                    return summary;
                }
            }

            return null;
        }

        /// <summary>
        /// 从 MethodInfo 中获取参数类型名列表，与源码声明的格式对齐。
        /// 例如 void DoSomething(int count, string name) → "int, string"
        /// </summary>
        static string GetParameterTypeNames(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0)
            {
                return "";
            }

            var typeNames = new List<string>(parameters.Length);
            foreach (var param in parameters)
            {
                typeNames.Add(param.ParameterType.GetReadableTypeName());
            }

            return string.Join(", ", typeNames);
        }

        static SourceFileEntry[] GetOrFindSourceFiles(Type type)
        {
            if (_sourceFilesCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var entries = SourceFileAnalyzerUtility.GetSourceFiles(type);
            _sourceFilesCache[type] = entries;
            return entries;
        }

        /// <summary>
        /// 清空所有缓存（供外部调用）。
        /// </summary>
        public static void ClearCache()
        {
            _sourceFilesCache.Clear();
            _summaryCache.Clear();
            SourceFileAnalyzerUtility.ClearCache();
        }
    }
}
#endif
