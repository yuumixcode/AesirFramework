using System;
using System.Collections.Generic;
using System.Reflection;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 属性数据接口，继承自 IDerivedMemberData，包含属性特有的数据信息和方法，派生类的通用数据信息和方法
    /// </summary>
    public interface IPropertyData : IDerivedMemberData
    {
        /// <summary>
        /// 自定义默认值，如果没有自定义默认值，则为 null
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// 属性类型
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// 属性类型名称
        /// </summary>
        string PropertyTypeName { get; }

        /// <summary>
        /// 属性类型的完整名称，包括命名空间
        /// </summary>
        string PropertyTypeFullName { get; }
    }

    /// <summary>
    /// 属性解析数据类，用于存储属性的解析数据
    /// </summary>
    [Serializable]
    public class PropertyData : MemberData, IPropertyData
    {
        /// <summary>
        /// 创建属性解析数据实例
        /// </summary>
        public PropertyData(PropertyInfo propertyInfo, IAttributeFilter filter = null) : base(propertyInfo,
            filter)
        {
            IsStatic = propertyInfo.IsStaticProperty();
            MemberType = propertyInfo.MemberType;
            MemberTypeName = MemberType.ToString();
            AccessModifier = propertyInfo.GetPropertyAccessModifierType();
            AccessModifierName = AccessModifier.ConvertToString();
            PropertyType = propertyInfo.PropertyType;
            PropertyTypeName = PropertyType.GetReadableTypeName();
            PropertyTypeFullName = PropertyType.GetReadableTypeName(true);
            DefaultValue = propertyInfo.TryGetPropertyCustomDefaultValue(out var value) ? value : null;
            Signature = GetPropertySignature(propertyInfo,
                TypeAnalyzerUtility.GetFormattedDefaultValue(PropertyType, DefaultValue));
            FullDeclarationWithAttributes = AttributesDeclaration + Signature;
        }

        string GetPropertySignature(PropertyInfo propertyInfo, string formattedDefaultValue)
        {
            var getSetPart = "{ ";
            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod != null)
            {
                getSetPart += getMethod.GetMethodAccessModifierType() == AccessModifierType.Public
                    ? "get; "
                    : getMethod.GetMethodAccessModifierType().ConvertToString() + " get; ";
            }

            var setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod != null)
            {
                getSetPart += setMethod.GetMethodAccessModifierType() == AccessModifierType.Public
                    ? "set; "
                    : setMethod.GetMethodAccessModifierType().ConvertToString() + " set; ";
            }

            getSetPart += "}";
            var hasDefaultValue = DefaultValue != null;
            var signatureParts = new List<string>
            {
                IsStatic ? AccessModifierName + " static" : AccessModifierName,
                PropertyTypeName,
                Name,
                hasDefaultValue ? getSetPart + " = " + formattedDefaultValue + ";" : getSetPart
            };
            var signature = string.Join(" ", signatureParts);
            return signature;
        }

        #region IDerivedMemberData

        /// <summary>
        /// 是否为静态属性
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// 成员类型
        /// </summary>
        public MemberTypes MemberType { get; }

        /// <summary>
        /// 成员类型名称
        /// </summary>
        public string MemberTypeName { get; }

        /// <summary>
        /// 访问修饰符
        /// </summary>
        public AccessModifierType AccessModifier { get; }

        /// <summary>
        /// 访问修饰符名称
        /// </summary>
        public string AccessModifierName { get; }

        /// <summary>
        /// 属性签名
        /// </summary>
        public string Signature { get; private set; }

        /// <summary>
        /// 完整属性声明 - 包含特性和签名 - 默认剔除 [Summary] 特性
        /// </summary>
        public string FullDeclarationWithAttributes { get; }

        #endregion

        #region IPropertyData

        /// <summary>
        /// 自定义默认值，如果没有自定义默认值，则为 null
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// 属性类型名称
        /// </summary>
        public string PropertyTypeName { get; }

        /// <summary>
        /// 属性类型的完整名称
        /// </summary>
        public string PropertyTypeFullName { get; }

        #endregion
    }
}
