using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Sirenix.Utilities;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 默认中文 API 文档生成设置
    /// </summary>
    /// <remarks>
    /// 纯 Markdown 输出（无 Front Matter / 无 div 包裹 / 无锚点详情）。
    /// 成员章节的分组核心（API 过滤 → 常量/声明/继承/运算符分组 → 固定顺序）与
    /// Zensical 生成器共享 <see cref="MemberGrouper" />，本类只声明各章节的表头、
    /// 列形与名称选择器（每节约 10 行配置），不再手写三旗标探测循环。
    /// </remarks>
    public class DefaultScriptingAPISettingsSO : DocGeneratorSettingsSO
    {
        static readonly string ConfigName = typeof(DefaultScriptingAPISettingsSO).GetNiceFullName();

        public static DefaultScriptingAPISettingsSO Instance =>
            ScriptDocGeneratorEditorUtility.GetOrCreateEditorScriptableObject<DefaultScriptingAPISettingsSO>(
                ConfigName, ScriptDocGeneratorPaths.GeneratorSettingsFolderPath, "DefaultCnScriptingAPI");

        public override string GetGeneratedDocumentation(ITypeData data)
        {
            var sb = new StringBuilder();
            sb.Append(CreateIntroductionContent(data));
            sb.Append(CreateConstructorsContent(data.RuntimeReflectedConstructorsData));
            sb.Append(CreateEventsContent(data.RuntimeReflectedEventsData));
            sb.Append(CreateMethodsContent(data.RuntimeReflectedMethodsData));
            sb.Append(CreatePropertiesContent(data.RuntimeReflectedPropertiesData));
            sb.Append(CreateFieldsContent(data.RuntimeReflectedFieldsData));
            return sb.ToString();
        }

        static StringBuilder CreateIntroductionContent(ITypeData typeData)
        {
            typeData.TryAsIMemberData(out var memberData);
            var sb = new StringBuilder();
            sb.AppendLine("# `" + memberData.Name + "`");
            sb.AppendLine();
            sb.AppendLine("## 介绍");
            sb.AppendLine();
            sb.Append("- 种类: `");
            var typeCategory = typeData.TypeCategory.ToString().ToLower(CultureInfo.CurrentCulture);
            if (typeData.IsStatic)
            {
                sb.Append("static " + typeCategory);
            }
            else if (typeData.IsAbstract && typeData.TypeCategory != TypeCategory.Interface)
            {
                sb.Append("abstract " + typeCategory);
            }
            else
            {
                sb.Append(typeCategory);
            }

            sb.AppendLine("`");
            sb.AppendLine($"- 所在程序集: `{typeData.AssemblyName}`");
            if (!string.IsNullOrWhiteSpace(typeData.NamespaceName))
            {
                sb.AppendLine($"- 所在命名空间: `{typeData.NamespaceName}`");
            }

            if (typeData.ReferenceWebLinkArray.Length >= 1)
            {
                for (var i = 0; i < typeData.ReferenceWebLinkArray.Length; i++)
                {
                    sb.AppendLine($"- 参考链接 [{i + 1}] : {typeData.ReferenceWebLinkArray[i]}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("``` csharp");
            sb.AppendLine(typeData.FullDeclarationWithAttributes);
            sb.AppendLine("```");
            if (string.IsNullOrEmpty(memberData.SummaryAttributeValue))
            {
                sb.AppendLine();
                return sb;
            }

            sb.AppendLine();
            sb.AppendLine("### 注释");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(memberData.SummaryAttributeValue))
            {
                sb.AppendLine("- " + memberData.SummaryAttributeValue);
            }

            sb.AppendLine();
            return sb;
        }

        /// <summary>
        /// 向分组表格追加成员行：两列（名称 | 注释）或三列（追加声明类型列）。
        /// </summary>
        static void AppendRow(StringBuilder sb,
            IDerivedMemberData member,
            string signature,
            bool withDeclaringType)
        {
            member.TryAsIMemberData(out var memberData);
            var row = $"| `{signature}` | {memberData.SummaryAttributeValue} |";
            if (withDeclaringType)
            {
                row += $" `{memberData.DeclaringType}` |";
            }

            sb.AppendLine(row);
        }

        /// <summary>
        /// 输出一个分组的完整表格（三级标题 + 表头 + 成员行）。
        /// </summary>
        static void AppendGroupTable(StringBuilder sb,
            string sectionTitle,
            string tableHeader,
            string tableSeparator,
            List<IDerivedMemberData> items,
            Func<IDerivedMemberData, string> signatureSelector,
            bool withDeclaringType)
        {
            sb.AppendLine(sectionTitle);
            sb.AppendLine();
            sb.AppendLine(tableHeader);
            sb.AppendLine(tableSeparator);
            foreach (var member in items)
            {
                AppendRow(sb, member, signatureSelector(member), withDeclaringType);
            }

            sb.AppendLine();
        }

        static StringBuilder CreateConstructorsContent(IConstructorData[] constructorDataArray)
        {
            var sb = new StringBuilder();
            if (constructorDataArray.Length <= 0)
            {
                return sb;
            }

            sb.AppendLine("## 构造方法");
            sb.AppendLine();
            sb.AppendLine("| 构造方法签名 [仅包含公共实例方法] | 注释 |");
            sb.AppendLine("| :--- | :--- |");

            foreach (var constructorData in constructorDataArray)
            {
                var fullSignature = constructorData.Signature;
                constructorData.TryAsIMemberData(out var memberData);
                if (memberData.IsObsolete)
                {
                    fullSignature = $"`[Obsolete] {fullSignature}`";
                }

                var comment = memberData.SummaryAttributeValue;
                sb.AppendLine("| " + $"`{fullSignature}`" + " | " + comment + " |");
            }

            sb.AppendLine();
            return sb;
        }

        static StringBuilder CreateEventsContent(IEventData[] eventDataArray)
        {
            var groups = MemberGrouper.GroupApiMembers(eventDataArray, member =>
            {
                member.TryAsIMemberData(out var memberData);
                return memberData.IsFromInheritance ? MemberGroup.Inherited : MemberGroup.Declared;
            });
            var sb = new StringBuilder();
            if (groups.Count == 0)
            {
                return sb;
            }

            sb.AppendLine("## 事件");
            sb.AppendLine();
            foreach (var (group, items) in groups)
            {
                if (group == MemberGroup.Declared)
                {
                    AppendGroupTable(sb, "### 声明的事件", "| 事件名称 | 注释 |", "| :--- | :--- | ", items,
                        member => ((IEventData)member).Signature, false);
                }
                else
                {
                    AppendGroupTable(sb, "### 继承的事件", "| 事件签名 | 注释 | 声明事件的类 |", "| :--- | :--- | :--- |",
                        items, member => ((IEventData)member).Signature, true);
                }
            }

            return sb;
        }

        static StringBuilder CreateMethodsContent(IMethodData[] methodDataArray)
        {
            var sb = new StringBuilder();
            var apiMethods = methodDataArray.Where(m => m.IsApiMember()).ToList();
            if (apiMethods.Count == 0)
            {
                return sb;
            }

            sb.AppendLine("## 方法");
            sb.AppendLine();
            sb.AppendLine("### 所有方法签名总览");
            sb.AppendLine();
            sb.AppendLine("| 方法完整签名 |");
            sb.AppendLine("| :--- | ");
            foreach (var methodData in apiMethods)
            {
                sb.AppendLine($"| `{methodData.Signature}` |");
            }

            sb.AppendLine();

            var groups = MemberGrouper.GroupApiMembers(apiMethods, member =>
            {
                var methodData = (IMethodData)member;
                if (methodData.IsOperator)
                {
                    return MemberGroup.Operator;
                }

                member.TryAsIMemberData(out var memberData);
                return memberData.IsFromInheritance ? MemberGroup.Inherited : MemberGroup.Declared;
            });

            foreach (var (group, items) in groups)
            {
                switch (group)
                {
                    case MemberGroup.Declared:
                        AppendGroupTable(sb, "### 声明的普通方法", "| 普通方法名称 | 注释 |", "| :--- | :--- | ", items,
                            member => ((IMethodData)member).SignatureWithoutParameters, false);
                        break;
                    case MemberGroup.Inherited:
                        AppendGroupTable(sb, "### 继承的普通方法", "| 普通方法名称 | 注释 | 声明方法的类 |",
                            "| :--- | :--- | :--- |", items,
                            member => ((IMethodData)member).SignatureWithoutParameters, true);
                        break;
                    case MemberGroup.Operator:
                        AppendGroupTable(sb, "### 运算符特殊方法", "| 方法签名 | 注释 | 声明方法的类 |",
                            "| :--- | :--- | :--- |", items, member => ((IMethodData)member).Signature, true);
                        break;
                }
            }

            return sb;
        }

        static StringBuilder CreatePropertiesContent(IPropertyData[] propertyDataArray)
        {
            var groups = MemberGrouper.GroupApiMembers(propertyDataArray, member =>
            {
                member.TryAsIMemberData(out var memberData);
                return memberData.IsFromInheritance ? MemberGroup.Inherited : MemberGroup.Declared;
            });
            var sb = new StringBuilder();
            if (groups.Count == 0)
            {
                return sb;
            }

            sb.AppendLine("## 属性");
            sb.AppendLine();
            foreach (var (group, items) in groups)
            {
                if (group == MemberGroup.Declared)
                {
                    AppendGroupTable(sb, "### 声明的属性", "| 属性签名 | 注释 |", "| :--- | :--- |", items,
                        member => ((IPropertyData)member).Signature, false);
                }
                else
                {
                    AppendGroupTable(sb, "### 继承的属性", "| 属性签名 | 注释 | 声明属性的类 | ", "| :--- | :--- | :--- |",
                        items, member => ((IPropertyData)member).Signature, true);
                }
            }

            return sb;
        }

        static StringBuilder CreateFieldsContent(IFieldData[] fieldDataArray)
        {
            var groups = MemberGrouper.GroupApiMembers(fieldDataArray, member =>
            {
                var fieldData = (IFieldData)member;
                if (fieldData.IsConstant)
                {
                    return MemberGroup.Constant;
                }

                member.TryAsIMemberData(out var memberData);
                return memberData.IsFromInheritance ? MemberGroup.Inherited : MemberGroup.Declared;
            });
            var sb = new StringBuilder();
            if (groups.Count == 0)
            {
                return sb;
            }

            sb.AppendLine("## 字段");
            sb.AppendLine();
            foreach (var (group, items) in groups)
            {
                switch (group)
                {
                    case MemberGroup.Constant:
                        AppendGroupTable(sb, "### 常量字段", "| 字段完整签名 | 注释 |", "| :--- | :--- |", items,
                            member => ((IFieldData)member).Signature, false);
                        break;
                    case MemberGroup.Declared:
                        AppendGroupTable(sb, "### 声明的普通字段", "| 字段名称 | 注释 | ", "| :--- | :--- | ", items,
                            member => ((IFieldData)member).Signature, false);
                        break;
                    case MemberGroup.Inherited:
                        AppendGroupTable(sb, "### 继承的普通字段", "| 字段名称 | 注释 | 声明字段的类 |",
                            "| :--- | :--- | :--- |", items, member => ((IFieldData)member).Signature, true);
                        break;
                }
            }

            return sb;
        }
    }
}
