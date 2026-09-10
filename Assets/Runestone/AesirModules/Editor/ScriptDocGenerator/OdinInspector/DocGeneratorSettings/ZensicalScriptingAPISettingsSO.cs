using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.Utilities;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// Zensical 静态站点专用的 API 文档生成设置
    /// </summary>
    /// <remarks>
    /// 输出遵循 Zensical (Python-Markdown) 约定的 Markdown：
    /// YAML Front Matter、无标题 note 警示框元信息、md_in_html 表格样式包裹（.api-summary-table 等）、
    /// 显式标题锚点（{#api-xxx}）与 csharp 声明块，
    /// 配合站点侧 docs/stylesheets/api.css 呈现 DocFX 风格的 Scripting API 页面。
    /// 样式锚点约定：.api-summary-table / .api-params-table / .api-returns-table。
    /// 表格一律经 &lt;div class="..." markdown="1"&gt; 包裹注入 class —— attr_list 块级标记
    /// {: .cls } 对表格无效（Zensical 实测：class 不应用且标记原文渲染），仅标题级 {#anchor} 行内标记可用。
    /// 成员详情统一"先注释后声明"：读者先看到用途说明，再看签名。
    /// </remarks>
    public class ZensicalScriptingAPISettingsSO : DocGeneratorSettingsSO
    {
        static readonly string ConfigName = typeof(ZensicalScriptingAPISettingsSO).GetNiceFullName();

        /// <summary>
        /// Zensical 文档生成设置单例
        /// </summary>
        public static ZensicalScriptingAPISettingsSO Instance =>
            ScriptDocGeneratorEditorUtility
                .GetOrCreateEditorScriptableObject<ZensicalScriptingAPISettingsSO>(ConfigName,
                    ScriptDocGeneratorPaths.GeneratorSettingsFolderPath, "ZensicalScriptingAPI");

        /// <inheritdoc />
        public override string GetGeneratedDocumentation(ITypeData data)
        {
            var sb = new StringBuilder();
            sb.Append(CreateFrontMatter(data));
            sb.Append(CreateTypeHeaderContent(data));
            AppendConstructorsContent(sb, data.RuntimeReflectedConstructorsData);
            AppendFieldsContent(sb, data.RuntimeReflectedFieldsData);
            AppendPropertiesContent(sb, data.RuntimeReflectedPropertiesData);
            AppendEventsContent(sb, data.RuntimeReflectedEventsData);
            AppendMethodsContent(sb, data.RuntimeReflectedMethodsData);
            return sb.ToString();
        }

        static string CreateFrontMatter(ITypeData typeData)
        {
            typeData.TryAsIMemberData(out var memberData);
            var fullName = string.IsNullOrWhiteSpace(typeData.NamespaceName)
                ? memberData.Name
                : typeData.NamespaceName + "." + memberData.Name;
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine("title: " + memberData.Name);
            sb.AppendLine($"description: \"{fullName} 的 API 文档\"");
            sb.AppendLine("---");
            sb.AppendLine();
            return sb.ToString();
        }

        static StringBuilder CreateTypeHeaderContent(ITypeData typeData)
        {
            typeData.TryAsIMemberData(out var memberData);
            var sb = new StringBuilder();
            sb.AppendLine("# `" + memberData.Name + "`");
            sb.AppendLine();

            // 元信息块：无标题 note 警示框（admonition 主题原生样式，内容需 4 空格缩进）
            sb.AppendLine("!!! note \"\"");
            sb.AppendLine();
            var typeCategory = typeData.TypeCategory.ToString().ToLower(CultureInfo.InvariantCulture);
            if (typeData.IsStatic)
            {
                typeCategory = "static " + typeCategory;
            }
            else if (typeData.IsAbstract && typeData.TypeCategory != TypeCategory.Interface)
            {
                typeCategory = "abstract " + typeCategory;
            }

            sb.AppendLine($"    - **种类:** `{typeCategory}`");
            if (!string.IsNullOrWhiteSpace(typeData.NamespaceName))
            {
                sb.AppendLine($"    - **命名空间:** `{typeData.NamespaceName}`");
            }

            sb.AppendLine($"    - **程序集:** `{typeData.AssemblyName}`");
            sb.AppendLine();

            if (typeData.InheritanceChain is { Length: > 0 })
            {
                // InheritanceChain 是"近基类在前"顺序，反转为根在前并补上当前类型（DocFX 风格）
                var chain = typeData.InheritanceChain.Reverse().Select(t => $"`{t}`")
                    .Append($"`{memberData.Name}`");
                sb.AppendLine("**继承链:** " + string.Join(" → ", chain));
                sb.AppendLine();
            }

            if (typeData.InterfaceArray is { Length: > 0 })
            {
                sb.AppendLine("**实现接口:** " +
                              string.Join("，", typeData.InterfaceArray.Select(t => $"`{t}`")));
                sb.AppendLine();
            }

            if (typeData.ReferenceWebLinkArray is { Length: > 0 })
            {
                for (var i = 0; i < typeData.ReferenceWebLinkArray.Length; i++)
                {
                    sb.AppendLine($"- [参考链接 {i + 1}]({typeData.ReferenceWebLinkArray[i]})");
                }

                sb.AppendLine();
            }

            sb.AppendLine("## 声明");
            sb.AppendLine();
            sb.AppendLine("``` csharp");
            sb.AppendLine(typeData.FullDeclarationWithAttributes);
            sb.AppendLine("```");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(memberData.SummaryAttributeValue))
            {
                sb.AppendLine(memberData.SummaryAttributeValue);
                sb.AppendLine();
            }

            return sb;
        }

        static void AppendConstructorsContent(StringBuilder sb, IConstructorData[] constructorDataArray)
        {
            AppendMemberSection(sb, "构造方法", "constructor",
                constructorDataArray.Cast<IDerivedMemberData>().ToList(),
                _ => MemberGroup.Declared, true);
        }

        static void AppendFieldsContent(StringBuilder sb, IFieldData[] fieldDataArray)
        {
            AppendMemberSection(sb, "字段", "field", fieldDataArray.Cast<IDerivedMemberData>().ToList(),
                member =>
                {
                    var fieldData = (IFieldData)member;
                    if (fieldData.IsConstant)
                    {
                        return MemberGroup.Constant;
                    }

                    member.TryAsIMemberData(out var memberData);
                    return memberData.IsFromInheritance ? MemberGroup.Inherited : MemberGroup.Declared;
                }, true);
        }

        static void AppendPropertiesContent(StringBuilder sb, IPropertyData[] propertyDataArray)
        {
            AppendMemberSection(sb, "属性", "property",
                propertyDataArray.Cast<IDerivedMemberData>().ToList(),
                member =>
                {
                    member.TryAsIMemberData(out var memberData);
                    return memberData.IsFromInheritance ? MemberGroup.Inherited : MemberGroup.Declared;
                }, true);
        }

        static void AppendEventsContent(StringBuilder sb, IEventData[] eventDataArray)
        {
            AppendMemberSection(sb, "事件", "event", eventDataArray.Cast<IDerivedMemberData>().ToList(),
                member =>
                {
                    member.TryAsIMemberData(out var memberData);
                    return memberData.IsFromInheritance ? MemberGroup.Inherited : MemberGroup.Declared;
                }, true);
        }

        static void AppendMethodsContent(StringBuilder sb, IMethodData[] methodDataArray)
        {
            AppendMemberSection(sb, "方法", "method", methodDataArray.Cast<IDerivedMemberData>().ToList(),
                member =>
                {
                    var methodData = (IMethodData)member;
                    if (methodData.IsOperator)
                    {
                        return MemberGroup.Operator;
                    }

                    member.TryAsIMemberData(out var memberData);
                    return memberData.IsFromInheritance ? MemberGroup.Inherited : MemberGroup.Declared;
                }, true);
        }

        enum MemberGroup
        {
            None = 0,
            Constant,
            Declared,
            Inherited,
            Operator
        }

        static readonly MemberGroup[] GroupOrder =
            { MemberGroup.Constant, MemberGroup.Declared, MemberGroup.Inherited, MemberGroup.Operator };

        /// <summary>
        /// 通用的成员区块渲染：先输出按分组的概览表格，再输出声明成员的详情小节
        /// </summary>
        static void AppendMemberSection(StringBuilder sb, string title, string anchorCategory,
            IReadOnlyList<IDerivedMemberData> members, Func<IDerivedMemberData, MemberGroup> groupSelector,
            bool renderDetails)
        {
            if (members.Count == 0)
            {
                return;
            }

            var apiMembers = members.Where(m => m.IsApiMember()).ToList();
            if (apiMembers.Count == 0)
            {
                return;
            }

            var groups = new List<(MemberGroup Group, List<IDerivedMemberData> Items)>();
            foreach (var group in GroupOrder)
            {
                var items = apiMembers.Where(m => groupSelector(m) == group).ToList();
                if (items.Count > 0)
                {
                    groups.Add((group, items));
                }
            }

            if (groups.Count == 0)
            {
                return;
            }

            sb.AppendLine("## " + title);
            sb.AppendLine();

            var showGroupLabel = groups.Count > 1;
            foreach (var (group, items) in groups)
            {
                if (showGroupLabel)
                {
                    sb.AppendLine($"**{GetGroupLabel(group)}{title}**");
                    sb.AppendLine();
                }

                var withDeclaringType = group is MemberGroup.Inherited or MemberGroup.Operator;
                // 仅声明的成员有详情小节，概览表中的名称才可点击跳转
                var linkToDetail = renderDetails && group is MemberGroup.Constant or MemberGroup.Declared;
                // md_in_html 包裹：为表格注入样式锚点 class，api.css 据此呈现 DocFX 风格
                //（attr_list 块级标记 {: .cls } 对表格无效，必须用 div 包裹）
                sb.AppendLine("<div class=\"api-summary-table\" markdown=\"1\">");
                sb.AppendLine();

                if (withDeclaringType)
                {
                    sb.AppendLine("| 名称 | 描述 | 声明类型 |");
                    sb.AppendLine("| :--- | :--- | :--- |");
                }
                else
                {
                    sb.AppendLine("| 名称 | 描述 |");
                    sb.AppendLine("| :--- | :--- |");
                }

                foreach (var member in items)
                {
                    AppendSummaryRow(sb, anchorCategory, member, group, linkToDetail, withDeclaringType);
                }

                sb.AppendLine();
                sb.AppendLine("</div>");
                sb.AppendLine();
            }

            if (!renderDetails)
            {
                return;
            }

            foreach (var (group, items) in groups)
            {
                if (group is not (MemberGroup.Constant or MemberGroup.Declared))
                {
                    continue;
                }

                foreach (var member in items)
                {
                    AppendMemberDetail(sb, anchorCategory, member);
                }
            }
        }

        static void AppendSummaryRow(StringBuilder sb, string anchorCategory, IDerivedMemberData member,
            MemberGroup group, bool linkToDetail, bool withDeclaringType)
        {
            member.TryAsIMemberData(out var memberData);
            var displayName = GetMemberDisplayName(member, memberData);
            // 运算符签名信息量更大，概览表中直接展示完整签名
            var nameText = group == MemberGroup.Operator ? member.Signature : displayName;
            var nameCell = linkToDetail
                ? $"[`{nameText}`](#{BuildAnchorId(anchorCategory, member, memberData)})"
                : $"`{nameText}`";
            var row = $"| {nameCell} | {EscapeTableCell(memberData.SummaryAttributeValue)} |";
            if (withDeclaringType)
            {
                row += $" `{memberData.DeclaringTypeName}` |";
            }

            sb.AppendLine(row);
        }

        static void AppendMemberDetail(StringBuilder sb, string anchorCategory, IDerivedMemberData member)
        {
            member.TryAsIMemberData(out var memberData);
            var displayName = GetMemberDisplayName(member, memberData);
            var anchorId = BuildAnchorId(anchorCategory, member, memberData);

            // 显式锚点 ID：attr_list 语法，保证概览表跳转不依赖 toc slugify 规则
            sb.AppendLine($"### {displayName} {{#{anchorId}}}");
            sb.AppendLine();

            // 先注释后声明：读者先看到用途说明，再看签名
            if (!string.IsNullOrWhiteSpace(memberData.SummaryAttributeValue))
            {
                sb.AppendLine(memberData.SummaryAttributeValue);
                sb.AppendLine();
            }

            sb.AppendLine("``` csharp");
            sb.AppendLine(member.FullDeclarationWithAttributes);
            sb.AppendLine("```");
            sb.AppendLine();

            switch (member)
            {
                case IMethodData methodData:
                    AppendParametersDetail(sb, methodData.ParametersDeclaration);
                    AppendReturnDetail(sb, methodData);
                    break;
                case IConstructorData constructorData:
                    AppendParametersDetail(sb, constructorData.ParametersDeclaration);
                    break;
            }
        }

        static void AppendParametersDetail(StringBuilder sb, string parametersDeclaration)
        {
            var parameters = ParseParameters(parametersDeclaration);
            if (parameters.Count == 0)
            {
                return;
            }

            sb.AppendLine("**参数**");
            sb.AppendLine();
            sb.AppendLine("<div class=\"api-params-table\" markdown=\"1\">");
            sb.AppendLine();
            sb.AppendLine("| 名称 | 类型 |");
            sb.AppendLine("| :--- | :--- |");
            foreach (var (name, type) in parameters)
            {
                sb.AppendLine($"| `{name}` | `{type}` |");
            }

            sb.AppendLine();
            sb.AppendLine("</div>");
            sb.AppendLine();
        }

        static void AppendReturnDetail(StringBuilder sb, IMethodData methodData)
        {
            if (methodData.ReturnTypeName == "void")
            {
                return;
            }

            sb.AppendLine("**返回值**");
            sb.AppendLine();
            sb.AppendLine("<div class=\"api-returns-table\" markdown=\"1\">");
            sb.AppendLine();
            sb.AppendLine("| 类型 |");
            sb.AppendLine("| :--- |");
            sb.AppendLine($"| `{methodData.ReturnTypeName}` |");
            sb.AppendLine();
            sb.AppendLine("</div>");
            sb.AppendLine();
        }

        static string GetGroupLabel(MemberGroup group) =>
            group switch
            {
                MemberGroup.Constant => "常量",
                MemberGroup.Declared => "声明的",
                MemberGroup.Inherited => "继承的",
                MemberGroup.Operator => "运算符",
                _ => string.Empty
            };

        /// <summary>
        /// 成员的概览展示名：方法/构造方法带参数类型列表（区分重载），其余用名称
        /// </summary>
        static string GetMemberDisplayName(IDerivedMemberData member, IMemberData memberData)
        {
            switch (member)
            {
                case IMethodData methodData:
                    return $"{memberData.Name}({GetParameterTypeList(methodData.ParametersDeclaration)})";
                case IConstructorData constructorData:
                    return $"{memberData.Name}({GetParameterTypeList(constructorData.ParametersDeclaration)})";
                default:
                    return memberData.Name;
            }
        }

        static string GetParameterTypeList(string parametersDeclaration) =>
            string.Join(", ", ParseParameters(parametersDeclaration).Select(p => p.Type));

        static readonly Regex AnchorSanitizeRegex =
            new Regex("[^a-z0-9\\u4e00-\\u9fff]+", RegexOptions.Compiled);

        /// <summary>
        /// 生成与详情标题一致的锚点 ID：api-{类别}-{成员展示名}，全小写、非法字符转连字符
        /// </summary>
        static string BuildAnchorId(string anchorCategory, IDerivedMemberData member,
            IMemberData memberData) =>
            AnchorSanitizeRegex
                .Replace(anchorCategory + "-" + GetMemberDisplayName(member, memberData)
                    .ToLowerInvariant(), "-")
                .Trim('-');

        static string EscapeTableCell(string text) =>
            string.IsNullOrWhiteSpace(text) ? "—" : text.Replace("\r", " ").Replace("\n", " ").Trim();

        /// <summary>
        /// 解析参数声明字符串（如 "float force, int count = 3"）为 (名称, 类型) 列表，
        /// 顶层按逗号拆分（忽略泛型/元组/数组内部的逗号），并剥离默认值与方向修饰符
        /// </summary>
        static List<(string Name, string Type)> ParseParameters(string parametersDeclaration)
        {
            var result = new List<(string Name, string Type)>();
            if (string.IsNullOrWhiteSpace(parametersDeclaration))
            {
                return result;
            }

            foreach (var segment in SplitTopLevel(parametersDeclaration, ','))
            {
                var part = segment.Trim();
                var equalIndex = IndexOfTopLevel(part, '=');
                if (equalIndex >= 0)
                {
                    part = part.Substring(0, equalIndex).Trim();
                }

                // 剥离扩展方法 this 与方向/params 修饰符
                foreach (var keyword in new[] { "this ", "params ", "ref ", "out ", "in " })
                {
                    if (part.StartsWith(keyword, StringComparison.Ordinal))
                    {
                        part = part.Substring(keyword.Length);
                        break;
                    }
                }

                var spaceIndex = part.LastIndexOf(' ');
                if (spaceIndex < 0)
                {
                    result.Add((part, string.Empty));
                }
                else
                {
                    result.Add((part.Substring(spaceIndex + 1), part.Substring(0, spaceIndex)));
                }
            }

            return result;
        }

        static IEnumerable<string> SplitTopLevel(string text, char separator)
        {
            var depth = 0;
            var start = 0;
            for (var i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '<' or '(' or '[':
                        depth++;
                        break;
                    case '>' or ')' or ']':
                        depth--;
                        break;
                        case var c when c == separator && depth == 0:
                            yield return text.Substring(start, i - start);
                            start = i + 1;
                            break;
                }
            }

            yield return text.Substring(start);
        }

        static int IndexOfTopLevel(string text, char target)
        {
            var depth = 0;
            for (var i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '<' or '(' or '[':
                        depth++;
                        break;
                    case '>' or ')' or ']':
                        depth--;
                        break;
                        case var c when c == target && depth == 0:
                            return i;
                }
            }

            return -1;
        }

    }
}
