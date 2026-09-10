#if UNITY_EDITOR
using System.Collections.Generic;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 单个源文件（或合并后的类型级视图）的结构化 XML 文档注释解析结果。
    /// 键为全限定键（无程序集前缀，合并时按需添加）：
    /// 类型级 <c>Namespace.TypeName</c>；成员级 <c>Namespace.TypeName.MemberName</c>，
    /// 方法与构造函数附参数后缀——参数类型键 <c>Member(int, string)</c>、参数计数键 <c>Member(2)</c>，
    /// 参数列表提取失败时退化为无后缀键。构造函数额外发射 <c>#ctor</c> 键。
    /// </summary>
    public sealed class ParsedSourceDoc
    {
        /// <summary>
        /// summary 文本字典（&lt;summary&gt; 标签）。
        /// </summary>
        public Dictionary<string, string> Summaries { get; } = new Dictionary<string, string>();

        /// <summary>
        /// 参数级文档字典（&lt;param&gt; 标签），成员键 → 参数名 → 文本。
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> ParamSummaries { get; } =
            new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// 返回值文档字典（&lt;returns&gt; 标签）。
        /// </summary>
        public Dictionary<string, string> ReturnsSummaries { get; } = new Dictionary<string, string>();

        /// <summary>
        /// 备注文档字典（&lt;remarks&gt; 标签）。
        /// </summary>
        public Dictionary<string, string> RemarksSummaries { get; } = new Dictionary<string, string>();

        /// <summary>
        /// 属性值文档字典（&lt;value&gt; 标签）。
        /// </summary>
        public Dictionary<string, string> ValueSummaries { get; } = new Dictionary<string, string>();

        /// <summary>
        /// 泛型参数文档字典（&lt;typeparam&gt; 标签），成员键 → 参数名 → 文本。
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> TypeParamSummaries { get; } =
            new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// 该文件内声明的全部类型名（含嵌套类型的简单名，供项目级索引使用）。
        /// </summary>
        public HashSet<string> DeclaredTypeNames { get; } = new HashSet<string>();

        /// <summary>
        /// 该文件内声明的全部命名空间全名（块式命名空间按嵌套层级合并）。
        /// </summary>
        public HashSet<string> DeclaredNamespaces { get; } = new HashSet<string>();

        /// <summary>
        /// 将另一份解析结果按程序集键前缀合并进来（后写入覆盖同键，与历史行为一致）。
        /// </summary>
        public void MergeWithPrefix(ParsedSourceDoc other, string prefix)
        {
            if (other == null)
            {
                return;
            }

            foreach (var kv in other.Summaries)
            {
                Summaries[prefix + kv.Key] = kv.Value;
            }

            foreach (var kv in other.ReturnsSummaries)
            {
                ReturnsSummaries[prefix + kv.Key] = kv.Value;
            }

            foreach (var kv in other.RemarksSummaries)
            {
                RemarksSummaries[prefix + kv.Key] = kv.Value;
            }

            foreach (var kv in other.ValueSummaries)
            {
                ValueSummaries[prefix + kv.Key] = kv.Value;
            }

            foreach (var kv in other.ParamSummaries)
            {
                ParamSummaries[prefix + kv.Key] = kv.Value;
            }

            foreach (var kv in other.TypeParamSummaries)
            {
                TypeParamSummaries[prefix + kv.Key] = kv.Value;
            }

            foreach (var name in other.DeclaredTypeNames)
            {
                DeclaredTypeNames.Add(name);
            }

            foreach (var name in other.DeclaredNamespaces)
            {
                DeclaredNamespaces.Add(name);
            }
        }
    }
}
#endif
