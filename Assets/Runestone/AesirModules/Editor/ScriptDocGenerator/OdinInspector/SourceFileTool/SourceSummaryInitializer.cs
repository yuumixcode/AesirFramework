#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 在编辑器程序集（Runestone.AesirModules.ScriptDocGenerator.Editor）加载时注入 Summary 解析器。
    /// 优先检查 [Summary] 特性；若无则从源代码的 XML <c>/// &lt;summary&gt;</c> 注释中读取。
    /// 使用全限定键（AssemblyName.Namespace.TypeName.MemberName）避免跨程序集同名类型冲突。
    /// 查询键链：嵌套类型规范键（FullName，+ 转 .）→ 扁平旧键（Namespace.最内层类型）；
    /// 方法/构造函数附参数后缀：参数类型键 → 参数计数键 → 无后缀键；
    /// 重载全部失配时按"该方法名下 distinct summary 恰好 1 个"回退（宁可缺不可错）。
    /// </summary>
    [InitializeOnLoad]
    public static class SourceSummaryInitializer
    {
        // Type → 合并后的结构化文档（键带 Type 所在程序集名前缀）
        static readonly Dictionary<Type, ParsedSourceDoc> _typeDocCache =
            new Dictionary<Type, ParsedSourceDoc>();

        // 文件路径 → 未加前缀的结构化文档；同文件多类型共享一次解析（历史实现按类型重复解析整文件）
        static readonly Dictionary<string, ParsedSourceDoc> _fileDocCache =
            new Dictionary<string, ParsedSourceDoc>();

        static SourceSummaryInitializer()
        {
            MemberData.SummaryResolver = ResolveSummary;
            MemberData.ParamSummariesResolver = ResolveParamSummaries;
            MemberData.ReturnsSummaryResolver = ResolveReturnsSummary;
        }

        static string ResolveSummary(MemberInfo member)
        {
            // Step 1: 优先检查 [Summary] 特性
            var attr = member?.GetCustomAttribute<SummaryAttribute>();
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

            var doc = GetTypeDoc(declaringType);
            if (doc.Summaries.Count == 0)
            {
                return null;
            }

            // 注意：对 Type 自身，用 member 的 FullName（嵌套类型需用自身全名，而非 DeclaringType 的）
            var keyType = member is Type ? (Type)member : declaringType;
            var cores = BuildKeyCores(keyType);
            var shortKey = member.Name;

            if (member is Type)
            {
                foreach (var core in cores)
                {
                    if (doc.Summaries.TryGetValue(core, out var summary))
                    {
                        return summary;
                    }
                }

                return doc.Summaries.TryGetValue(shortKey, out var fallbackSummary)
                    ? fallbackSummary
                    : null;
            }

            if (member is MethodBase methodBase)
            {
                // 方法用 member.Name；构造函数用 "#ctor"（与解析器发射的别名键一致）
                var baseName = member is ConstructorInfo ? "#ctor" : member.Name;
                var paramTypeSuffix = "(" + GetParameterTypeNames(methodBase) + ")";
                var paramCountSuffix = "(" + methodBase.GetParameters().Length + ")";

                foreach (var core in cores)
                {
                    if (doc.Summaries.TryGetValue(core + "." + baseName + paramTypeSuffix,
                            out var summary))
                    {
                        return summary;
                    }

                    if (doc.Summaries.TryGetValue(core + "." + baseName + paramCountSuffix,
                            out var summaryByCount))
                    {
                        return summaryByCount;
                    }

                    if (doc.Summaries.TryGetValue(core + "." + baseName, out var summaryWithoutParams))
                    {
                        return summaryWithoutParams;
                    }
                }

                // 构造函数的自然名键（类型名，解析器对构造函数同时发射两种键）；
                // 覆盖解析器 #ctor 检测启发式未命中的边缘情况
                if (member is ConstructorInfo)
                {
                    var ctorName = StripArity(declaringType.Name);
                    foreach (var core in cores)
                    {
                        if (doc.Summaries.TryGetValue(core + "." + ctorName + paramTypeSuffix,
                                out var summary))
                        {
                            return summary;
                        }

                        if (doc.Summaries.TryGetValue(core + "." + ctorName + paramCountSuffix,
                                out var summaryByCount))
                        {
                            return summaryByCount;
                        }

                        if (doc.Summaries.TryGetValue(core + "." + ctorName,
                                out var summaryWithoutParams))
                        {
                            return summaryWithoutParams;
                        }
                    }
                }

                // 唯一 summary 回退：该方法名/构造名下 distinct summary 恰好 1 个才取，
                // 多个则放弃——宁可缺，不可错
                return GetUniqueOverloadSummary(doc, cores, baseName);
            }

            // 属性 / 字段 / 事件（含索引器 "Item"）
            foreach (var core in cores)
            {
                if (doc.Summaries.TryGetValue(core + "." + member.Name, out var summary))
                {
                    return summary;
                }
            }

            return doc.Summaries.TryGetValue(shortKey, out var fallback) ? fallback : null;
        }

        static string GetUniqueOverloadSummary(ParsedSourceDoc doc, string[] cores, string baseName)
        {
            string unique = null;
            var distinctCount = 0;
            foreach (var kv in doc.Summaries)
            {
                foreach (var core in cores)
                {
                    if (!kv.Key.StartsWith(core + "." + baseName + "(", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (unique == null)
                    {
                        unique = kv.Value;
                        distinctCount = 1;
                    }
                    else if (unique != kv.Value)
                    {
                        distinctCount = 2;
                    }

                    break;
                }
            }

            return distinctCount == 1 ? unique : null;
        }

        static IReadOnlyDictionary<string, string> ResolveParamSummaries(MethodInfo method)
        {
            if (method == null)
            {
                return null;
            }

            var doc = GetTypeDoc(method.DeclaringType);
            if (doc.ParamSummaries.Count == 0)
            {
                return null;
            }

            var cores = BuildKeyCores(method.DeclaringType);
            var paramTypeSuffix = "(" + GetParameterTypeNames(method) + ")";
            var paramCountSuffix = "(" + method.GetParameters().Length + ")";
            foreach (var core in cores)
            {
                if (doc.ParamSummaries.TryGetValue(core + "." + method.Name + paramTypeSuffix,
                        out var params1))
                {
                    return params1;
                }

                if (doc.ParamSummaries.TryGetValue(core + "." + method.Name + paramCountSuffix,
                        out var params2))
                {
                    return params2;
                }

                if (doc.ParamSummaries.TryGetValue(core + "." + method.Name, out var params3))
                {
                    return params3;
                }
            }

            return null;
        }

        static string ResolveReturnsSummary(MethodInfo method)
        {
            if (method == null)
            {
                return null;
            }

            var doc = GetTypeDoc(method.DeclaringType);
            if (doc.ReturnsSummaries.Count == 0)
            {
                return null;
            }

            var cores = BuildKeyCores(method.DeclaringType);
            var paramTypeSuffix = "(" + GetParameterTypeNames(method) + ")";
            var paramCountSuffix = "(" + method.GetParameters().Length + ")";
            foreach (var core in cores)
            {
                if (doc.ReturnsSummaries.TryGetValue(core + "." + method.Name + paramTypeSuffix,
                        out var returns))
                {
                    return returns;
                }

                if (doc.ReturnsSummaries.TryGetValue(core + "." + method.Name + paramCountSuffix,
                        out var returnsByCount))
                {
                    return returnsByCount;
                }

                if (doc.ReturnsSummaries.TryGetValue(core + "." + method.Name,
                        out var returnsWithoutParams))
                {
                    return returnsWithoutParams;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取类型的合并文档：定位其全部源文件（缓存），按文件缓存解析结果（同文件多类型
        /// 只解析一次），以类型所在程序集名为键前缀合并后按类型缓存。
        /// 缓存仅驻留字符串字典，不驻留源文件行数组。
        /// </summary>
        static ParsedSourceDoc GetTypeDoc(Type type)
        {
            if (type == null)
            {
                return new ParsedSourceDoc();
            }

            if (_typeDocCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var merged = new ParsedSourceDoc();
            var paths = SourceFileAnalyzerUtility.FindSourceFilePaths(type);
            var prefix = type.Assembly.GetName().Name + ".";
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (!_fileDocCache.TryGetValue(path, out var fileDoc))
                {
                    fileDoc = LoadFileDoc(path);
                    _fileDocCache[path] = fileDoc;
                }

                merged.MergeWithPrefix(fileDoc, prefix);
            }

            _typeDocCache[type] = merged;
            return merged;
        }

        static ParsedSourceDoc LoadFileDoc(string path)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                if (!File.Exists(fullPath))
                {
                    return new ParsedSourceDoc();
                }

                return SourceScanner.Scan(File.ReadAllLines(fullPath));
            }
            catch
            {
                return new ParsedSourceDoc();
            }
        }

        /// <summary>
        /// 构造类型级键核心（含程序集前缀）。
        /// 规范键：FullName 中嵌套分隔符 + 替换为 .，与解析器类型栈生成的链一致；
        /// 扁平旧键：命名空间 + 最内层类型名（解析器对嵌套类型额外发射的历史格式，保留一版兼容）。
        /// </summary>
        static string[] BuildKeyCores(Type type)
        {
            var assemblyName = type.Assembly.GetName().Name;
            var fullName = type.FullName ?? type.Name;
            var backtickIndex = fullName.IndexOf('`');
            if (backtickIndex >= 0)
            {
                fullName = fullName.Substring(0, backtickIndex);
            }

            var canonical = assemblyName + "." + fullName.Replace('+', '.');
            var legacyName = (string.IsNullOrEmpty(type.Namespace) ? "" : type.Namespace + ".") +
                             StripArity(type.Name);
            var legacy = assemblyName + "." + legacyName;
            return canonical == legacy ? new[] { canonical } : new[] { canonical, legacy };
        }

        /// <summary>
        /// 从方法/构造函数中获取参数类型名列表，与源码声明的格式对齐。
        /// 例如 void DoSomething(int count, string name) → "int, string"
        /// </summary>
        static string GetParameterTypeNames(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
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

        static string StripArity(string name)
        {
            var backtick = name.IndexOf('`');
            return backtick >= 0 ? name.Substring(0, backtick) : name;
        }

        /// <summary>
        /// 清空所有缓存（供外部调用）。
        /// </summary>
        public static void ClearCache()
        {
            _typeDocCache.Clear();
            _fileDocCache.Clear();
            SourceFileAnalyzerUtility.ClearCache();
        }
    }
}
#endif
