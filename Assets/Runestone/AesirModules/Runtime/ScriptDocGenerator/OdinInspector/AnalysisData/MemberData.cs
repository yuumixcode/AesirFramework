using System;
using System.Collections.Generic;
using System.Reflection;

namespace Runestone.AesirModules.ScriptDocGenerator
{
    /// <summary>
    /// 成员数据接口
    /// </summary>
    public interface IMemberData
    {
        /// <summary>
        /// 成员名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 是否已过时
        /// </summary>
        bool IsObsolete { get; }

        /// <summary>
        /// 声明此成员的类型
        /// </summary>
        Type DeclaringType { get; }

        /// <summary>
        /// 声明类型的名称
        /// </summary>
        string DeclaringTypeName { get; }

        /// <summary>
        /// 声明类型的完整名称，包括命名空间
        /// </summary>
        string DeclaringTypeFullName { get; }

        /// <summary>
        /// 通过反射获取该成员的类型
        /// </summary>
        Type ReflectedType { get; }

        /// <summary>
        /// 通过反射获取该成员的类型名称
        /// </summary>
        string ReflectedTypeName { get; }

        /// <summary>
        /// 通过反射获取该成员的类型的完整名称，包括命名空间
        /// </summary>
        string ReflectedTypeFullName { get; }

        /// <summary>
        /// 特性声明字符串
        /// </summary>
        string AttributesDeclaration { get; }

        /// <summary>
        /// 注释
        /// </summary>
        string SummaryAttributeValue { get; }

        /// <summary>
        /// 备注注释（XML <c>&lt;remarks&gt;</c> 标签）。无注释时为 null
        /// </summary>
        string RemarksSummary { get; }

        /// <summary>
        /// 成员是否从继承中获取，这里的成员不包括 Type 类型
        /// </summary>
        bool IsFromInheritance { get; }
    }

    /// <summary>
    /// 解析成员数据的基类
    /// </summary>
    [Serializable]
    public abstract class MemberData : IMemberData
    {
        /// <summary>
        /// 默认特性过滤器，被过滤的特性不会包含在 AttributesDeclaration 中
        /// </summary>
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
            SummaryAttributeValue = SummaryResolver(memberInfo);
            RemarksSummary = RemarksResolver(memberInfo);
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

        /// <summary>
        /// Summary 解析委托。Editor 程序集在加载时注入源文件解析实现（基于 SourceScanner），
        /// 从源代码的 XML <c>/// &lt;summary&gt;</c> 注释中读取成员摘要。
        /// 默认回退到 [Summary] 特性，保持向后兼容。
        /// </summary>
        public static Func<MemberInfo, string> SummaryResolver { get; set; } = ResolveSummaryFromAttribute;

        /// <summary>
        /// 参数级注释解析委托（XML <c>&lt;param&gt;</c> 标签），键为参数名，适用于方法与构造函数。
        /// Editor 程序集在加载时注入源文件解析实现；默认无参数级注释（返回 null）。
        /// </summary>
        public static Func<MethodBase, IReadOnlyDictionary<string, string>> ParamSummariesResolver
        {
            get;
            set;
        } = _ => null;

        /// <summary>
        /// 返回值注释解析委托（XML <c>&lt;returns&gt;</c> 标签），适用于方法。
        /// Editor 程序集在加载时注入源文件解析实现；默认无返回值注释（返回 null）。
        /// </summary>
        public static Func<MethodInfo, string> ReturnsSummaryResolver { get; set; } = _ => null;

        /// <summary>
        /// 备注注释解析委托（XML <c>&lt;remarks&gt;</c> 标签）。
        /// Editor 程序集在加载时注入源文件解析实现；默认无备注注释（返回 null）。
        /// </summary>
        public static Func<MemberInfo, string> RemarksResolver { get; set; } = _ => null;

        /// <summary>
        /// 属性值注释解析委托（XML <c>&lt;value&gt;</c> 标签），由属性数据类消费。
        /// Editor 程序集在加载时注入源文件解析实现；默认无注释（返回 null）。
        /// </summary>
        public static Func<MemberInfo, string> ValueResolver { get; set; } = _ => null;

        /// <summary>
        /// 泛型参数注释解析委托（XML <c>&lt;typeparam&gt;</c> 标签），键为参数名，由类型数据类消费。
        /// Editor 程序集在加载时注入源文件解析实现；默认无注释（返回 null）。
        /// </summary>
        public static Func<Type, IReadOnlyDictionary<string, string>> TypeParamsResolver { get; set; } = _ =>
            null;

        static string ResolveSummaryFromAttribute(MemberInfo memberInfo) =>
            memberInfo?.GetCustomAttribute<SummaryAttribute>()?.GetSummary();

        #region IMemberData Members

        /// <summary>
        /// 成员名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否已过时
        /// </summary>
        public bool IsObsolete { get; }

        /// <summary>
        /// 声明此成员的类型
        /// </summary>
        public Type DeclaringType { get; }

        /// <summary>
        /// 声明类型的名称
        /// </summary>
        public string DeclaringTypeName { get; }

        /// <summary>
        /// 声明类型的完整名称，包括命名空间
        /// </summary>
        public string DeclaringTypeFullName { get; }

        /// <summary>
        /// 通过反射获取该成员的类型
        /// </summary>
        public Type ReflectedType { get; }

        /// <summary>
        /// 通过反射获取该成员的类型名称
        /// </summary>
        public string ReflectedTypeName { get; }

        /// <summary>
        /// 通过反射获取该成员的类型的完整名称，包括命名空间
        /// </summary>
        public string ReflectedTypeFullName { get; }

        /// <summary>
        /// 特性声明字符串
        /// </summary>
        public string AttributesDeclaration { get; }

        /// <summary>
        /// 注释
        /// </summary>
        public string SummaryAttributeValue { get; }

        /// <summary>
        /// 备注注释（XML <c>&lt;remarks&gt;</c> 标签）。无注释时为 null
        /// </summary>
        public string RemarksSummary { get; }

        /// <summary>
        /// 成员是否从继承中获取，这里的成员不包括 Type 类型
        /// </summary>
        public bool IsFromInheritance { get; }

        #endregion
    }
}
