using System;
using System.Reflection;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 成员数据接口
    /// </summary>
    [Summary("成员数据接口")]
    public interface IMemberData
    {
        /// <summary>
        /// 成员名称
        /// </summary>
        [Summary("成员名称")]
        string Name { get; }

        /// <summary>
        /// 是否已过时
        /// </summary>
        [Summary("是否已过时")]
        bool IsObsolete { get; }

        /// <summary>
        /// 声明此成员的类型
        /// </summary>
        [Summary("声明此成员的类型")]
        Type DeclaringType { get; }

        /// <summary>
        /// 声明类型的名称
        /// </summary>
        [Summary("声明类型的名称")]
        string DeclaringTypeName { get; }

        /// <summary>
        /// 声明类型的完整名称，包括命名空间
        /// </summary>
        [Summary("声明类型的完整名称，包括命名空间")]
        string DeclaringTypeFullName { get; }

        /// <summary>
        /// 通过反射获取该成员的类型
        /// </summary>
        [Summary("通过反射获取该成员的类型")]
        Type ReflectedType { get; }

        /// <summary>
        /// 通过反射获取该成员的类型名称
        /// </summary>
        [Summary("通过反射获取该成员的类型名称")]
        string ReflectedTypeName { get; }

        /// <summary>
        /// 通过反射获取该成员的类型的完整名称，包括命名空间
        /// </summary>
        [Summary("通过反射获取该成员的类型的完整名称，包括命名空间")]
        string ReflectedTypeFullName { get; }

        /// <summary>
        /// 特性声明字符串
        /// </summary>
        [Summary("特性声明字符串")]
        string AttributesDeclaration { get; }

        /// <summary>
        /// 注释
        /// </summary>
        [Summary("注释")]
        string SummaryAttributeValue { get; }

        /// <summary>
        /// 成员是否从继承中获取，这里的成员不包括 Type 类型
        /// </summary>
        [Summary("成员是否从继承中获取，这里的成员不包括 Type 类型")]
        bool IsFromInheritance { get; }
    }

    /// <summary>
    /// 解析成员数据的基类
    /// </summary>
    [Summary("解析成员数据的基类")]
    [Serializable]
    public abstract class MemberData : IMemberData
    {
        /// <summary>
        /// 默认特性过滤器，被过滤的特性不会包含在 AttributesDeclaration 中
        /// </summary>
        [Summary("默认特性过滤器，被过滤的特性不会包含在 AttributesDeclaration 中")]
        public static readonly DefaultAttributeFilter DefaultAttributeFilter = new DefaultAttributeFilter(
            new[]
            {
                typeof(SummaryAttribute)
            });

        /// <summary>
        /// 创建成员数据基类实例
        /// </summary>
        protected MemberData(MemberInfo memberInfo, IAttributeFilter filter = null)
        {
            Name = memberInfo.Name;
            IsObsolete = memberInfo.IsDefined(typeof(ObsoleteAttribute), false);
            DeclaringType = memberInfo.DeclaringType;
            DeclaringTypeName = DeclaringType?.GetReadableTypeName();
            DeclaringTypeFullName = DeclaringType?.GetReadableTypeName(true);
            ReflectedType = memberInfo.ReflectedType;
            ReflectedTypeName = ReflectedType?.GetReadableTypeName();
            ReflectedTypeFullName = ReflectedType?.GetReadableTypeName(true);
            AttributesDeclaration =
                memberInfo.GetAttributesDeclarationWithMultiLine(filter ?? DefaultAttributeFilter);
            SummaryAttributeValue = memberInfo.GetCustomAttribute<SummaryAttribute>()?.GetSummary();
            IsFromInheritance = memberInfo.IsFromInheritance();
            if (memberInfo is Type type)
            {
                Name = type.GetReadableTypeName();
            }
            else if (memberInfo is ConstructorInfo)
            {
                Name = DeclaringTypeName?.Split('<')[0];
            }
        }

        #region IMemberData Members

        /// <summary>
        /// 成员名称
        /// </summary>
        [Summary("成员名称")]
        public string Name { get; }

        /// <summary>
        /// 是否已过时
        /// </summary>
        [Summary("是否已过时")]
        public bool IsObsolete { get; }

        /// <summary>
        /// 声明此成员的类型
        /// </summary>
        [Summary("声明此成员的类型")]
        public Type DeclaringType { get; }

        /// <summary>
        /// 声明类型的名称
        /// </summary>
        [Summary("声明类型的名称")]
        public string DeclaringTypeName { get; }

        /// <summary>
        /// 声明类型的完整名称，包括命名空间
        /// </summary>
        [Summary("声明类型的完整名称，包括命名空间")]
        public string DeclaringTypeFullName { get; }

        /// <summary>
        /// 通过反射获取该成员的类型
        /// </summary>
        [Summary("通过反射获取该成员的类型")]
        public Type ReflectedType { get; }

        /// <summary>
        /// 通过反射获取该成员的类型名称
        /// </summary>
        [Summary("通过反射获取该成员的类型名称")]
        public string ReflectedTypeName { get; }

        /// <summary>
        /// 通过反射获取该成员的类型的完整名称，包括命名空间
        /// </summary>
        [Summary("通过反射获取该成员的类型的完整名称，包括命名空间")]
        public string ReflectedTypeFullName { get; }

        /// <summary>
        /// 特性声明字符串
        /// </summary>
        [Summary("特性声明字符串")]
        public string AttributesDeclaration { get; }

        /// <summary>
        /// 注释
        /// </summary>
        [Summary("注释")]
        public string SummaryAttributeValue { get; }

        /// <summary>
        /// 成员是否从继承中获取，这里的成员不包括 Type 类型
        /// </summary>
        [Summary("成员是否从继承中获取，这里的成员不包括 Type 类型")]
        public bool IsFromInheritance { get; }

        #endregion
    }
}
