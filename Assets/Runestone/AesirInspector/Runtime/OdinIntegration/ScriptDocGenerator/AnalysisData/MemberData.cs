using System;
using System.Reflection;

namespace Runestone.AesirInspector
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
        /// Summary 解析委托。Editor 程序集在加载时注入源文件解析实现（基于 OdinSourceFileHelper），
        /// 从源代码的 XML <c>/// &lt;summary&gt;</c> 注释中读取成员摘要。
        /// 默认回退到 [Summary] 特性，保持向后兼容。
        /// </summary>
        public static Func<MemberInfo, string> SummaryResolver { get; set; } = ResolveSummaryFromAttribute;

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
        /// 成员是否从继承中获取，这里的成员不包括 Type 类型
        /// </summary>
        public bool IsFromInheritance { get; }

        #endregion
    }
}
