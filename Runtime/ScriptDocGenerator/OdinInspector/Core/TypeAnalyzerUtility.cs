using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Runestone.AesirModules.ScriptDocGenerator
{
    /// <summary>
    /// 类型分析器工具类
    /// </summary>
    public static class TypeAnalyzerUtility
    {
        /// <summary>
        /// 将系统类型名称映射到其 C# 别名的字典
        /// </summary>
        public static readonly IReadOnlyDictionary<Type, string> TypeAliasMap = new Dictionary<Type, string>
        {
            { typeof(void), "void" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(byte), "byte" },
            { typeof(ushort), "ushort" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(decimal), "decimal" },
            { typeof(string), "string" },
            { typeof(char), "char" },
            { typeof(bool), "bool" },
            { typeof(float[]), "float[]" },
            { typeof(double[]), "double[]" },
            { typeof(sbyte[]), "sbyte[]" },
            { typeof(short[]), "short[]" },
            { typeof(int[]), "int[]" },
            { typeof(long[]), "long[]" },
            { typeof(byte[]), "byte[]" },
            { typeof(ushort[]), "ushort[]" },
            { typeof(uint[]), "uint[]" },
            { typeof(ulong[]), "ulong[]" },
            { typeof(decimal[]), "decimal[]" },
            { typeof(string[]), "string[]" },
            { typeof(char[]), "char[]" },
            { typeof(bool[]), "bool[]" },
            { typeof(object), "object" }
        };

        /// <summary>
        /// 获取没有后缀的 Attribute 名称
        /// </summary>
        public static string GetAttributeNameWithoutSuffix(string attributeName)
        {
            if (attributeName.EndsWith("Attribute"))
            {
                attributeName = attributeName[..^"Attribute".Length];
            }

            return attributeName;
        }

        /// <summary>
        /// 获取字段的关键字片段字符串
        /// </summary>
        public static string GetFieldKeywordSnippet(bool isConst, bool isStatic, bool isReadOnly)
        {
            var keyword = "";
            if (isConst)
            {
                keyword = "const ";
                return keyword;
            }

            switch (isStatic)
            {
                case true when isReadOnly:
                    keyword = "static readonly ";
                    break;
                case true:
                    keyword = "static ";
                    break;
                default:
                {
                    if (isReadOnly)
                    {
                        keyword = "readonly ";
                    }

                    break;
                }
            }

            return keyword;
        }

        /// <summary>
        /// 获取格式化的默认值字符串，用于生成签名
        /// </summary>
        public static string GetFormattedDefaultValue(Type memberType, object value)
        {
            if (memberType == typeof(Quaternion) || memberType == typeof(Vector3) ||
                memberType == typeof(Vector2) || memberType == typeof(Vector4))
            {
                var typeName = memberType.Name;
                var formattedValue = "new " + typeName + value;
                return formattedValue;
            }

            if (value != null && memberType.IsEnum)
            {
                var enumTypeName = memberType.Name;
                var enumName = Enum.GetName(memberType, value);
                return $"{enumTypeName}.{enumName}";
            }

            return value switch
            {
                null => "null",
                string str => $"\"{str}\"",
                bool b => b ? "true" : "false",
                float f => $"{f}f",
                char c => $"'{c}'",
                double d => $"{d}d",
                decimal m => $"{m}m",
                uint u => $"{u}u",
                long l => $"{l}L",
                ulong ul => $"{ul}ul",
                short s => $"{s}",
                ushort us => $"{us}",
                byte b2 => $"{b2}",
                sbyte sb => $"{sb}",
                _ => value.ToString()
            };
        }

        /// <summary>
        /// 获取格式化的完整特性签名字符串，返回 true 表示该特性支持格式化为完整特性签名字符串，返回 false 表示不支持。
        /// </summary>
        public static bool TryGetFormatedAttributeWithFullParameter(object attrInstance,
            out string attributeFullSignature)
        {
            var attributeFullName = GetAttributeNameWithoutSuffix(attrInstance.GetType().FullName);
            var attributeName = GetAttributeNameWithoutSuffix(attrInstance.GetType().Name);
            switch (attrInstance)
            {
                case ObsoleteAttribute obsoleteAttr:
                    attributeFullSignature = obsoleteAttr.IsError
                        ? $"[{attributeFullName}(\"{obsoleteAttr.Message}\", true)]"
                        : $"[{attributeFullName}(\"{obsoleteAttr.Message}\")]";
                    return true;
                case TooltipAttribute tooltipAttr:
                    attributeFullSignature = $"[{attributeFullName}(\"{tooltipAttr.tooltip}\")]";
                    return true;
                case RangeAttribute rangeAttr:
                    attributeFullSignature = $"[{attributeFullName}({rangeAttr.min}, {rangeAttr.max})]";
                    return true;
                case ColorUsageAttribute colorUsageAttr:
                    attributeFullSignature =
                        $"[{attributeFullName}({colorUsageAttr.showAlpha.ToString().ToLower()}, " +
                        $"{colorUsageAttr.hdr.ToString().ToLower()})]";
                    return true;
                case ReferenceLinkURLAttribute referenceLinkAttr:
                    attributeFullSignature = $"[{attributeName}(\"{referenceLinkAttr.WebUrl}\")]";
                    return true;
            }

            attributeFullSignature = string.Empty;
            return false;
        }

        /// <summary>
        /// 提供的值被视为类型的默认值，返回 true 表示被视为类型的默认值，返回 false 表示不是。
        /// </summary>
        public static bool TreatedAsTypeDefaultValue(object value, Type type)
        {
            if (value is string str)
            {
                return string.IsNullOrEmpty(str);
            }

            if (type.IsReferenceTypeExcludeString() || type.IsAbstractOrInterface())
            {
                return true;
            }

            var defaultValue = Activator.CreateInstance(type);
            return defaultValue == null || value.Equals(defaultValue);
        }

        /// <summary>
        /// 判断类型是否为编译器或 Unity 源生成的内部类型（如 &lt;PrivateImplementationDetails&gt;、
        /// UnitySourceGenerated* 等），这类类型不属于用户 API，不应为其生成文档。
        /// </summary>
        public static bool IsGeneratedInternalType(Type type) =>
            type != null && (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null ||
                             IsGeneratedInternalTypeName(type.FullName));

        /// <summary>
        /// 判断类型完整名称是否属于编译器或 Unity 源生成的内部类型命名模式。
        /// 合法的 C# 源代码无法在类型名中产生尖括号，因此名称含尖括号的一定是编译器合成类型（如匿名类型、闭包类）。
        /// </summary>
        public static bool IsGeneratedInternalTypeName(string typeFullName) =>
            !string.IsNullOrEmpty(typeFullName) && (typeFullName.IndexOf('<') >= 0 ||
                                                    typeFullName.IndexOf('>') >= 0 ||
                                                    typeFullName.StartsWith("UnitySourceGenerated"));

        /// <summary>
        /// 将类型名称转换为文档文件名（不含扩展名）。
        /// 泛型尖括号转换为花括号（C# XML 文档注释 cref 规范）：尖括号在 Windows 文件名中非法，
        /// 而方括号在 Markdown 中是链接语法会导致引用失效，花括号则两者皆安全。
        /// </summary>
        public static string ConvertToDocumentationFileName(string typeName) =>
            typeName?.Replace('<', '{').Replace('>', '}');
    }
}
