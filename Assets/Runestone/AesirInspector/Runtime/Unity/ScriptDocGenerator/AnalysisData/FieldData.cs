using System;
using System.Reflection;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 字段数据接口，继承自 IDerivedMemberData
    /// </summary>
    public interface IFieldData : IDerivedMemberData
    {
        /// <summary>
        /// 是否为只读字段（readonly）
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 是否为常量字段（const）
        /// </summary>
        bool IsConstant { get; }

        /// <summary>
        /// 是否为动态字段（dynamic）
        /// </summary>
        bool IsDynamic { get; }

        /// <summary>
        /// 字段的默认值
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// 字段的类型
        /// </summary>
        Type FieldType { get; }

        /// <summary>
        /// 这个字段的类型的名称
        /// </summary>
        string FieldTypeName { get; }

        /// <summary>
        /// 这个字段的类型的完整名称
        /// </summary>
        string FieldTypeFullName { get; }
    }

    /// <summary>
    /// 字段解析数据类，用于存储字段的解析数据
    /// </summary>
    [Serializable]
    public class FieldData : MemberData, IFieldData
    {
        /// <summary>
        /// 创建字段解析数据实例
        /// </summary>
        public FieldData(FieldInfo fieldInfo, IAttributeFilter filter = null) : base(fieldInfo, filter)
        {
            IsStatic = fieldInfo.IsStatic;
            MemberType = fieldInfo.MemberType;
            MemberTypeName = MemberType.ToString();
            AccessModifier = fieldInfo.GetFieldAccessModifier();
            AccessModifierName = AccessModifier.ConvertToString();
            FieldType = fieldInfo.FieldType;
            IsDynamic = fieldInfo.IsDynamicField();
            FieldTypeName = IsDynamic ? "dynamic" : FieldType?.GetReadableTypeName();
            FieldTypeFullName = IsDynamic ? "dynamic" : FieldType?.GetReadableTypeName(true);
            IsReadOnly = fieldInfo.IsInitOnly;
            IsConstant = fieldInfo.IsLiteral && !fieldInfo.IsInitOnly;
            DefaultValue = fieldInfo.TryGetFieldCustomDefaultValue(out var value) ? value : null;
            Signature = GetFieldSignature(
                TypeAnalyzerUtility.GetFieldKeywordSnippet(IsConstant, IsStatic, IsReadOnly),
                TypeAnalyzerUtility.GetFormattedDefaultValue(FieldType, DefaultValue));
            FullDeclarationWithAttributes = AttributesDeclaration + Signature;
        }

        string GetFieldSignature(string keywordSnippet, string formattedDefaultValue)
        {
            if (DeclaringType.IsEnum)
            {
                return $"{AccessModifierName} {keywordSnippet}{FieldTypeName} {Name};";
            }

            return DefaultValue != null
                ? $"{AccessModifierName} {keywordSnippet}{FieldTypeName} {Name} = {formattedDefaultValue};"
                : $"{AccessModifierName} {keywordSnippet}{FieldTypeName} {Name};";
        }

        #region IDerivedMemberData

        /// <summary>
        /// 指示该字段是否为静态字段
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// 成员类型，指示该成员的类型
        /// </summary>
        public MemberTypes MemberType { get; }

        /// <summary>
        /// 成员类型名称
        /// </summary>
        public string MemberTypeName { get; }

        /// <summary>
        /// 访问修饰符类型
        /// </summary>
        public AccessModifierType AccessModifier { get; }

        /// <summary>
        /// 访问修饰符名称
        /// </summary>
        public string AccessModifierName { get; }

        /// <summary>
        /// 字段签名
        /// </summary>
        public string Signature { get; private set; }

        /// <summary>
        /// 完整字段声明，包含特性和签名，默认剔除 Summary 特性
        /// </summary>
        public string FullDeclarationWithAttributes { get; }

        #endregion

        #region IFieldData

        /// <summary>
        /// 是否为只读字段
        /// </summary>
        public bool IsReadOnly { get; }

        /// <summary>
        /// 是否为常量字段
        /// </summary>
        public bool IsConstant { get; }

        /// <summary>
        /// 是否为动态类型字段
        /// </summary>
        public bool IsDynamic { get; }

        /// <summary>
        /// 字段的默认值，没有默认值返回 null
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// 字段的类型
        /// </summary>
        public Type FieldType { get; }

        /// <summary>
        /// 字段类型的名称
        /// </summary>
        public string FieldTypeName { get; }

        /// <summary>
        /// 字段类型的完整名称
        /// </summary>
        public string FieldTypeFullName { get; }

        #endregion
    }
}
