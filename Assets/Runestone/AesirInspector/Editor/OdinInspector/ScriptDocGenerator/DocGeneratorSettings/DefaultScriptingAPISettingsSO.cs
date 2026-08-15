using System.Globalization;
using System.Linq;
using System.Text;
using Sirenix.Utilities;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 默认中文 API 文档生成设置
    /// </summary>
    public class DefaultScriptingAPISettingsSO : DocGeneratorSettingsSO
    {
        const string GeneratorSettingsPath = AesirInspectorPaths.EditorDefaultResourcesPath +
                                             "/ScriptDocGenerator/GeneratorSettings";

        static readonly string ConfigName = typeof(DefaultScriptingAPISettingsSO).GetNiceFullName();

        public static DefaultScriptingAPISettingsSO Instance =>
            ScriptableObjectSafeEditorUtility
                .GetOrCreateEditorScriptableObject<DefaultScriptingAPISettingsSO>(ConfigName,
                    GeneratorSettingsPath, "DefaultCnScriptingAPI");

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
            var sb = new StringBuilder();
            if (eventDataArray.Length <= 1)
            {
                return sb;
            }

            var hasAPI = false;
            var hasInheritedAndApiEvent = false;
            var hasNoInheritedAndApiEvent = false;

            foreach (var eventData in eventDataArray)
            {
                if (hasAPI && hasNoInheritedAndApiEvent && hasInheritedAndApiEvent)
                {
                    break;
                }

                eventData.TryAsIMemberData(out var memberData);
                if (!hasAPI && eventData.IsApiMember())
                {
                    hasAPI = true;
                }

                if (!eventData.IsApiMember())
                {
                    continue;
                }

                if (hasInheritedAndApiEvent && hasNoInheritedAndApiEvent)
                {
                    continue;
                }

                if (memberData.IsFromInheritance)
                {
                    hasInheritedAndApiEvent = true;
                }
                else
                {
                    hasNoInheritedAndApiEvent = true;
                }
            }

            if (!hasAPI)
            {
                return sb;
            }

            sb.AppendLine("## 事件");
            sb.AppendLine();
            if (hasNoInheritedAndApiEvent)
            {
                sb.AppendLine("### 声明的事件");
                sb.AppendLine();
                sb.AppendLine("| 事件名称 | 注释 |");
                sb.AppendLine("| :--- | :--- | ");
                foreach (var eventData in eventDataArray)
                {
                    eventData.TryAsIMemberData(out var memberData);
                    if (!eventData.IsApiMember() || memberData.IsFromInheritance)
                    {
                        continue;
                    }

                    sb.AppendLine($"| `{eventData.Signature}` | {memberData.SummaryAttributeValue} |");
                }

                sb.AppendLine();
            }

            if (!hasInheritedAndApiEvent)
            {
                return sb;
            }

            sb.AppendLine("### 继承的事件");
            sb.AppendLine();
            sb.AppendLine("| 事件签名 | 注释 | 声明事件的类 |");
            sb.AppendLine("| :--- | :--- | :--- |");
            foreach (var eventData in eventDataArray)
            {
                eventData.TryAsIMemberData(out var memberData);
                if (!eventData.IsApiMember() || !memberData.IsFromInheritance)
                {
                    continue;
                }

                sb.AppendLine(
                    $"| `{eventData.Signature}` | {memberData.SummaryAttributeValue} | `{memberData.DeclaringType}` |");
            }

            sb.AppendLine();

            return sb;
        }

        static StringBuilder CreateMethodsContent(IMethodData[] methodDataArray)
        {
            var sb = new StringBuilder();
            if (methodDataArray.Length <= 1)
            {
                return sb;
            }

            var hasApiMember = false;
            var hasOperatorAndApiMethod = false;
            var hasInheritAndNoOperatorAndApiMethod = false;
            var hasNoInheritAndNoOperatorAndApiMethod = false;
            foreach (var methodData in methodDataArray)
            {
                if (hasApiMember && hasNoInheritAndNoOperatorAndApiMethod &&
                    hasInheritAndNoOperatorAndApiMethod && hasOperatorAndApiMethod)
                {
                    break;
                }

                if (!hasApiMember && methodData.IsApiMember())
                {
                    hasApiMember = true;
                }

                if (!methodData.IsApiMember())
                {
                    continue;
                }

                if (!hasOperatorAndApiMethod && methodData.IsOperator)
                {
                    hasOperatorAndApiMethod = true;
                }

                if (methodData.IsOperator)
                {
                    continue;
                }

                if (hasNoInheritAndNoOperatorAndApiMethod && hasInheritAndNoOperatorAndApiMethod)
                {
                    continue;
                }

                methodData.TryAsIMemberData(out var memberData);
                if (memberData.IsFromInheritance)
                {
                    hasInheritAndNoOperatorAndApiMethod = true;
                }
                else
                {
                    hasNoInheritAndNoOperatorAndApiMethod = true;
                }
            }

            if (!hasApiMember)
            {
                return sb;
            }

            sb.AppendLine("## 方法");
            sb.AppendLine();
            sb.AppendLine("### 所有方法签名总览");
            sb.AppendLine();
            sb.AppendLine("| 方法完整签名 |");
            sb.AppendLine("| :--- | ");
            foreach (var methodData in methodDataArray)
            {
                if (!methodData.IsApiMember())
                {
                    continue;
                }

                sb.AppendLine($"| `{methodData.Signature}` |");
            }

            sb.AppendLine();
            if (hasNoInheritAndNoOperatorAndApiMethod)
            {
                sb.AppendLine("### 声明的普通方法");
                sb.AppendLine();
                sb.AppendLine("| 普通方法名称 | 注释 |");
                sb.AppendLine("| :--- | :--- | ");
                var filteredMethodDataArray = methodDataArray.Where(m =>
                    m.IsApiMember() && !m.IsOperator && m.TryAsIMemberData(out var memberData) &&
                    !memberData.IsFromInheritance);
                foreach (var methodData in filteredMethodDataArray)
                {
                    methodData.TryAsIMemberData(out var memberData);
                    sb.AppendLine(
                        $"| `{methodData.SignatureWithoutParameters}` | {memberData.SummaryAttributeValue} |");
                }

                sb.AppendLine();
            }

            if (hasInheritAndNoOperatorAndApiMethod)
            {
                sb.AppendLine("### 继承的普通方法");
                sb.AppendLine();
                sb.AppendLine("| 普通方法名称 | 注释 | 声明方法的类 |");
                sb.AppendLine("| :--- | :--- | :--- |");
                var filteredMethodDataArray = methodDataArray.Where(m =>
                    m.IsApiMember() && !m.IsOperator && m.TryAsIMemberData(out var memberData) &&
                    memberData.IsFromInheritance);
                foreach (var methodData in filteredMethodDataArray)
                {
                    methodData.TryAsIMemberData(out var memberData);
                    sb.AppendLine(
                        $"| `{methodData.SignatureWithoutParameters}` | {memberData.SummaryAttributeValue} | `{memberData.DeclaringType}` |");
                }

                sb.AppendLine();
            }

            if (!hasOperatorAndApiMethod)
            {
                return sb;
            }

            sb.AppendLine("### 运算符特殊方法");
            sb.AppendLine();
            sb.AppendLine("| 方法签名 | 注释 | 声明方法的类 |");
            sb.AppendLine("| :--- | :--- | :--- |");
            var filteredOperators = methodDataArray.Where(m => m.IsApiMember() && m.IsOperator);
            foreach (var methodData in filteredOperators)
            {
                methodData.TryAsIMemberData(out var memberData);
                sb.AppendLine(
                    $"| `{methodData.Signature}` | {memberData.SummaryAttributeValue} | `{memberData.DeclaringType}` |");
            }

            sb.AppendLine();
            return sb;
        }

        static StringBuilder CreatePropertiesContent(IPropertyData[] propertyDataArray)
        {
            var sb = new StringBuilder();
            if (propertyDataArray.Length <= 0)
            {
                return sb;
            }

            var hasAPI = false;
            var hasInheritedAndApiProperty = false;
            var hasNoInheritedAndApiProperty = false;

            foreach (var propertyData in propertyDataArray)
            {
                if (hasAPI && hasNoInheritedAndApiProperty && hasInheritedAndApiProperty)
                {
                    break;
                }

                if (!hasAPI && propertyData.IsApiMember())
                {
                    hasAPI = true;
                }

                propertyData.TryAsIMemberData(out var memberData);
                if (hasInheritedAndApiProperty && hasNoInheritedAndApiProperty)
                {
                    continue;
                }

                if (memberData.IsFromInheritance)
                {
                    hasInheritedAndApiProperty = true;
                }
                else
                {
                    hasNoInheritedAndApiProperty = true;
                }
            }

            if (!hasAPI)
            {
                return sb;
            }

            sb.AppendLine("## 属性");
            sb.AppendLine();
            if (hasNoInheritedAndApiProperty)
            {
                sb.AppendLine("### 声明的属性");
                sb.AppendLine();
                sb.AppendLine("| 属性签名 | 注释 |");
                sb.AppendLine("| :--- | :--- |");
                foreach (var propertyData in propertyDataArray)
                {
                    propertyData.TryAsIMemberData(out var memberData);
                    if (!propertyData.IsApiMember() || memberData.IsFromInheritance)
                    {
                        continue;
                    }

                    sb.AppendLine($"| `{propertyData.Signature}` | {memberData.SummaryAttributeValue} |");
                }

                sb.AppendLine();
            }

            if (!hasInheritedAndApiProperty)
            {
                return sb;
            }

            sb.AppendLine("### 继承的属性");
            sb.AppendLine();
            sb.AppendLine("| 属性签名 | 注释 | 声明属性的类 | ");
            sb.AppendLine("| :--- | :--- | :--- |");
            foreach (var propertyData in propertyDataArray)
            {
                propertyData.TryAsIMemberData(out var memberData);
                if (!propertyData.IsApiMember() || !memberData.IsFromInheritance)
                {
                    continue;
                }

                sb.AppendLine(
                    $"| `{propertyData.Signature}` | {memberData.SummaryAttributeValue} | `{memberData.DeclaringType}` |");
            }

            sb.AppendLine();

            return sb;
        }

        static StringBuilder CreateFieldsContent(IFieldData[] fieldDataArray)
        {
            var sb = new StringBuilder();
            if (fieldDataArray.Length <= 0)
            {
                return sb;
            }

            var hasAPI = false;
            var hasConstAndApiField = false;
            var hasNoConstAndNoInheritedAndApiField = false;
            var hasNoConstAndInheritedAndApiField = false;

            foreach (var fieldData in fieldDataArray)
            {
                if (hasAPI && hasConstAndApiField && hasNoConstAndNoInheritedAndApiField &&
                    hasNoConstAndInheritedAndApiField)
                {
                    break;
                }

                if (!hasAPI && fieldData.IsApiMember())
                {
                    hasAPI = true;
                }

                if (!fieldData.IsApiMember())
                {
                    continue;
                }

                if (!hasConstAndApiField && fieldData.IsConstant)
                {
                    hasConstAndApiField = true;
                }

                if (fieldData.IsConstant)
                {
                    continue;
                }

                if (hasNoConstAndNoInheritedAndApiField && hasNoConstAndInheritedAndApiField)
                {
                    continue;
                }

                fieldData.TryAsIMemberData(out var memberData);
                if (memberData.IsFromInheritance)
                {
                    hasNoConstAndInheritedAndApiField = true;
                }
                else
                {
                    hasNoConstAndNoInheritedAndApiField = true;
                }
            }

            if (!hasAPI)
            {
                return sb;
            }

            sb.AppendLine("## 字段");
            sb.AppendLine();
            if (hasConstAndApiField)
            {
                sb.AppendLine("### 常量字段");
                sb.AppendLine();
                sb.AppendLine("| 字段完整签名 | 注释 |");
                sb.AppendLine("| :--- | :--- |");
                foreach (var fieldData in fieldDataArray)
                {
                    fieldData.TryAsIMemberData(out var memberData);
                    if (!fieldData.IsApiMember() && !fieldData.IsConstant)
                    {
                        continue;
                    }

                    sb.AppendLine($"| `{fieldData.Signature}` | {memberData.SummaryAttributeValue} |");
                }

                sb.AppendLine();
            }

            if (hasNoConstAndNoInheritedAndApiField)
            {
                sb.AppendLine("### 声明的普通字段");
                sb.AppendLine();
                sb.AppendLine("| 字段名称 | 注释 | ");
                sb.AppendLine("| :--- | :--- | ");
                foreach (var fieldData in fieldDataArray)
                {
                    fieldData.TryAsIMemberData(out var memberData);
                    if (!fieldData.IsApiMember() || memberData.IsFromInheritance || fieldData.IsConstant)
                    {
                        continue;
                    }

                    sb.AppendLine($"| `{fieldData.Signature}` | {memberData.SummaryAttributeValue} |");
                }

                sb.AppendLine();
            }

            if (hasNoConstAndInheritedAndApiField)
            {
                sb.AppendLine("### 继承的普通字段");
                sb.AppendLine();
                sb.AppendLine("| 字段名称 | 注释 | 声明字段的类 |");
                sb.AppendLine("| :--- | :--- | :--- |");
                foreach (var fieldData in fieldDataArray)
                {
                    fieldData.TryAsIMemberData(out var memberData);
                    if (!fieldData.IsApiMember() || !memberData.IsFromInheritance || fieldData.IsConstant)
                    {
                        continue;
                    }

                    sb.AppendLine(
                        $"| `{fieldData.Signature}` | {memberData.SummaryAttributeValue} | `{memberData.DeclaringType}` |");
                }

                sb.AppendLine();
            }

            return sb;
        }
    }
}
