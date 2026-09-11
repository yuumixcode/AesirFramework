#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 在编辑器程序集（Runestone.AesirModules.ScriptDocGenerator.Editor）加载时注入 XML 文档注释解析器：
    /// summary / param / returns / remarks / value / typeparam 六种标签全部接入 <see cref="MemberData" /> 对应解析委托。
    /// Summary 优先检查 [Summary] 特性；若无则从源代码的 XML <c>/// &lt;summary&gt;</c> 注释中读取。
    /// 使用全限定键（AssemblyName.Namespace.TypeName.MemberName）避免跨程序集同名类型冲突。
    /// 查询键链：嵌套类型规范键（FullName，+ 转 .）→ 扁平旧键（Namespace.最内层类型）；
    /// 方法/构造函数附参数后缀：参数类型键 → 参数计数键 → 无后缀键；
    /// 重载全部失配时按"该方法名下 distinct 文本恰好 1 个"回退（宁可缺不可错）。
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
            MemberData.RemarksResolver = ResolveRemarks;
            MemberData.ValueResolver = ResolveValue;
            MemberData.TypeParamsResolver = ResolveTypeParamSummaries;
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
            return ResolveMemberDocText(member, doc => doc.Summaries);
        }

        static string ResolveRemarks(MemberInfo member) =>
            ResolveMemberDocText(member, doc => doc.RemarksSummaries);

        static string ResolveValue(MemberInfo member) =>
            ResolveMemberDocText(member, doc => doc.ValueSummaries);

        static string ResolveReturnsSummary(MethodInfo method) =>
            ResolveMemberDocText(method, doc => doc.ReturnsSummaries);

        /// <summary>
        /// 按 member 种类在全限定键链上查找文本类文档（summary/remarks/returns/value 共用）：
        /// 类型成员用嵌套类型规范键链 + 短名键回退；方法/构造函数用参数后缀键链 + 构造函数自然名键 +
        /// 唯一重载文本回退；其余成员用成员键 + 短名键。
        /// </summary>
        static string ResolveMemberDocText(MemberInfo member,
            Func<ParsedSourceDoc, Dictionary<string, string>> dictSelector)
        {
            if (member == null)
            {
                return null;
            }

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
            var dict = dictSelector(doc);
            if (dict.Count == 0)
            {
                return null;
            }

            if (member is Type)
            {
                // 对 Type 自身，用 member 的 FullName（嵌套类型需用自身全名，而非 DeclaringType 的）
                foreach (var core in BuildKeyCores((Type)member))
                {
                    if (dict.TryGetValue(core, out var value))
                    {
                        return value;
                    }
                }

                return dict.TryGetValue(member.Name, out var typeFallback) ? typeFallback : null;
            }

            if (member is MethodBase methodBase)
            {
                // 方法用 member.Name；构造函数用 "#ctor"（与解析器发射的别名键一致）
                var baseName = member is ConstructorInfo ? "#ctor" : member.Name;
                if (TryGetMethodDocText(dict, methodBase, declaringType, baseName, out var byMethod))
                {
                    return byMethod;
                }

                // 构造函数的自然名键（类型名，解析器对构造函数同时发射两种键）；
                // 覆盖解析器 #ctor 检测启发式未命中的边缘情况
                if (member is ConstructorInfo && TryGetMethodDocText(dict, methodBase, declaringType,
                        StripArity(declaringType.Name), out var byCtorName))
                {
                    return byCtorName;
                }

                return GetUniqueOverloadDocText(dict, BuildKeyCores(declaringType), baseName);
            }

            // 属性 / 字段 / 事件（含索引器 "Item"）
            foreach (var core in BuildKeyCores(declaringType))
            {
                if (dict.TryGetValue(core + "." + member.Name, out var value))
                {
                    return value;
                }
            }

            return dict.TryGetValue(member.Name, out var memberFallback) ? memberFallback : null;
        }

        /// <summary>
        /// 方法/构造函数的参数后缀键链查找：参数类型键 → 参数计数键 → 无后缀键。
        /// </summary>
        static bool TryGetMethodDocText(Dictionary<string, string> dict,
            MethodBase methodBase,
            Type declaringType,
            string baseName,
            out string value)
        {
            var paramTypeSuffix = "(" + GetParameterTypeNames(methodBase) + ")";
            var paramCountSuffix = "(" + methodBase.GetParameters().Length + ")";
            foreach (var core in BuildKeyCores(declaringType))
            {
                if (dict.TryGetValue(core + "." + baseName + paramTypeSuffix, out value))
                {
                    return true;
                }

                if (dict.TryGetValue(core + "." + baseName + paramCountSuffix, out value))
                {
                    return true;
                }

                if (dict.TryGetValue(core + "." + baseName, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// 唯一重载文本回退：该方法名/构造名下 distinct 文本恰好 1 个才取，多个则放弃——宁可缺，不可错
        /// </summary>
        static string GetUniqueOverloadDocText(Dictionary<string, string> dict,
            string[] cores,
            string baseName)
        {
            string unique = null;
            var distinctCount = 0;
            foreach (var kv in dict)
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

        static IReadOnlyDictionary<string, string> ResolveParamSummaries(MethodBase methodBase)
        {
            if (methodBase == null)
            {
                return null;
            }

            var declaringType = methodBase.DeclaringType;
            if (declaringType == null)
            {
                return null;
            }

            var doc = GetTypeDoc(declaringType);
            if (doc.ParamSummaries.Count == 0)
            {
                return null;
            }

            // 方法用 member.Name；构造函数用 "#ctor" 并以自然名键兜底（与解析器发射的两种键一致）
            var baseNames = methodBase is ConstructorInfo
                ? new[] { "#ctor", StripArity(declaringType.Name) }
                : new[] { methodBase.Name };
            var paramTypeSuffix = "(" + GetParameterTypeNames(methodBase) + ")";
            var paramCountSuffix = "(" + methodBase.GetParameters().Length + ")";
            foreach (var baseName in baseNames)
            {
                foreach (var core in BuildKeyCores(declaringType))
                {
                    if (doc.ParamSummaries.TryGetValue(core + "." + baseName + paramTypeSuffix,
                            out var params1))
                    {
                        return params1;
                    }

                    if (doc.ParamSummaries.TryGetValue(core + "." + baseName + paramCountSuffix,
                            out var params2))
                    {
                        return params2;
                    }

                    if (doc.ParamSummaries.TryGetValue(core + "." + baseName, out var params3))
                    {
                        return params3;
                    }
                }
            }

            return null;
        }

        static IReadOnlyDictionary<string, string> ResolveTypeParamSummaries(Type type)
        {
            if (type == null)
            {
                return null;
            }

            var doc = GetTypeDoc(type);
            if (doc.TypeParamSummaries.Count == 0)
            {
                return null;
            }

            foreach (var core in BuildKeyCores(type))
            {
                if (doc.TypeParamSummaries.TryGetValue(core, out var typeParams))
                {
                    return typeParams;
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
        /// 清空所有缓存。每次类型分析前由 <see cref="ScriptDocGeneratorUtility" /> 调用，
        /// 保证分析总是读取磁盘上最新的 XML 注释（此前仅靠域重载失效，存在同域内旧缓存窗口）。
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
