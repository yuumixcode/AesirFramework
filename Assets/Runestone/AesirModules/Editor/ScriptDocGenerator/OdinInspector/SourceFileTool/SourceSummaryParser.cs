#if UNITY_EDITOR
using System.Collections.Generic;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 源码 XML 文档注释解析门面。实际解析由 <see cref="SourceScanner" /> 单遍状态机完成：
    /// 字符串/逐字字符串/注释感知净化 + 命名空间栈 + 类型栈，支持全限定键（含嵌套类型规范键
    /// 与扁平旧键）、方法参数类型键与参数计数键、构造函数 <c>#ctor</c> 键。
    /// </summary>
    public static class SourceSummaryParser
    {
        /// <summary>
        /// 解析多个源文件条目中的 summary 注释，返回全限定键 → summary 字典。
        /// 键格式：<c>Namespace.TypeName</c>（类型级）或
        /// <c>Namespace.TypeName.MemberName</c>（成员级）；方法与构造函数附参数后缀——
        /// 参数类型键 <c>Member(int, string)</c>（历史格式）、参数计数键 <c>Member(2)</c>；
        /// 参数提取失败时为无后缀键；构造函数额外发射 <c>#ctor</c> 键。
        /// assemblyName 非空时作为键前缀，避免不同程序集中同名命名空间+类型名的键冲突。
        /// </summary>
        public static Dictionary<string, string> ParseSummaries(SourceFileEntry[] entries,
            string assemblyName = null) =>
            ParseDocComments(entries, assemblyName).Summaries;

        /// <summary>
        /// 解析多个源文件条目中的全部 XML 文档标签（summary/param/returns/remarks/value/typeparam），
        /// 返回结构化解析结果，键规则与 <see cref="ParseSummaries" /> 一致。
        /// </summary>
        public static ParsedSourceDoc ParseDocComments(SourceFileEntry[] entries, string assemblyName = null)
        {
            var result = new ParsedSourceDoc();
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

                result.MergeWithPrefix(SourceScanner.Scan(entry.sourceLines), prefix);
            }

            return result;
        }

        /// <summary>
        /// 从文档注释行列表（已移除 /// 前缀）中提取 summary 纯文本，清理 XML 标签与实体。
        /// </summary>
        public static string ParseSummaryText(List<string> summaryLines) =>
            SourceScanner.ParseSummaryTextFromLines(summaryLines);

        /// <summary>
        /// 清理 XML 标签：将 <c>&lt;see cref="A.B"/&gt;</c> 替换为 B，paramref/typeparamref/langword
        /// 替换为名称，&lt;c&gt;/&lt;code&gt; 保留内容，&lt;para&gt; 分段，移除其他 XML 标签，折叠多余空格。
        /// </summary>
        public static string StripXmlTags(string text) => SourceScanner.StripXmlTags(text);
    }
}
#endif
