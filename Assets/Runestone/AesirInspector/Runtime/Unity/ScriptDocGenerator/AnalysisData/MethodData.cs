using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 方法数据接口，继承自 IDerivedMemberData
    /// </summary>
    [Summary("方法数据接口，继承自 IDerivedMemberData")]
    public interface IMethodData : IDerivedMemberData
    {
        /// <summary>
        /// 方法的返回类型
        /// </summary>
        [Summary("方法的返回类型")]
        Type ReturnType { get; }

        /// <summary>
        /// 方法的返回类型名称
        /// </summary>
        [Summary("方法的返回类型名称")]
        string ReturnTypeName { get; }

        /// <summary>
        /// 方法的返回类型的完整名称
        /// </summary>
        [Summary("方法的返回类型的完整名称")]
        string ReturnTypeFullName { get; }

        /// <summary>
        /// 是否带有抽象属性，不一定声明时带有 abstract 关键字
        /// </summary>
        [Summary("是否带有抽象属性，不一定声明时带有 abstract 关键字")]
        bool IsAbstract { get; }

        /// <summary>
        /// 是否带有虚方法属性，不一定声明时带有 virtual 关键字
        /// </summary>
        [Summary("是否带有虚方法属性，不一定声明时带有 virtual 关键字")]
        bool IsVirtual { get; }

        /// <summary>
        /// 是否为运算符方法
        /// </summary>
        [Summary("是否为运算符方法")]
        bool IsOperator { get; }

        /// <summary>
        /// 是否属于重写的方法，不一定带有 override 关键字
        /// </summary>
        [Summary("是否属于重写的方法，不一定带有 override 关键字")]
        bool IsOverride { get; }

        /// <summary>
        /// 是否为异步方法
        /// </summary>
        [Summary("是否为异步方法")]
        bool IsAsync { get; }

        /// <summary>
        /// 此方法继承自祖先（不是直接的基类，而是基类的上层）
        /// </summary>
        [Summary("此方法继承自祖先（不是直接的基类，而是基类的上层）")]
        bool IsFromAncestor { get; }

        /// <summary>
        /// 此方法继承自接口，在该类中实现
        /// </summary>
        [Summary("此方法继承自接口，在该类中实现")]
        bool IsFromInterfaceImplement { get; }

        /// <summary>
        /// 此方法在当前类中存在重载方法，需要在 Type 解析时进行处理
        /// </summary>
        [Summary("此方法在当前类中存在重载方法，需要在 Type 解析时进行处理")]
        bool IsOverloadMethodInDeclaringType { get; set; }

        /// <summary>
        /// 方法的参数声明字符串，包含参数名称和类型
        /// </summary>
        [Summary("方法的参数声明字符串，包含参数名称和类型")]
        string ParametersDeclaration { get; }

        /// <summary>
        /// 不包含参数的简单方法签名
        /// </summary>
        [Summary("不包含参数的简单方法签名")]
        string SignatureWithoutParameters { get; }

        /// <summary>
        /// 添加 [Overload] 前缀
        /// </summary>
        [Summary("添加 [Overload] 前缀")]
        void AddOverloadPrefix();
    }

    /// <summary>
    /// 方法解析数据类，用于存储 MethodInfo 的解析结果
    /// </summary>
    [Summary("方法解析数据类，用于存储 MethodInfo 的解析结果")]
    [Serializable]
    public class MethodData : MemberData, IMethodData
    {
        /// <summary>
        /// 创建方法解析数据实例
        /// </summary>
        public MethodData(MethodInfo memberInfo, IAttributeFilter filter = null) : base(memberInfo, filter)
        {
            IsStatic = memberInfo.IsStatic;
            MemberType = memberInfo.MemberType;
            MemberTypeName = MemberType.ToString();
            AccessModifier = memberInfo.GetMethodAccessModifierType();
            AccessModifierName = AccessModifier.ConvertToString();
            ReturnType = memberInfo.ReturnType;
            ReturnTypeName = ReturnType.GetReadableTypeName();
            ReturnTypeFullName = ReturnType.GetReadableTypeName(true);
            IsAbstract = memberInfo.IsAbstract;
            IsVirtual = memberInfo.IsVirtual;
            IsOperator = memberInfo.IsOperatorMethod();
            IsOverride = memberInfo.IsOverrideMethod();
            IsAsync = memberInfo.IsAsyncMethod();
            ParametersDeclaration = memberInfo.GetParametersNameWithDefaultValue();
            IsFromInterfaceImplement = memberInfo.IsFromInterfaceImplementMethod();
            IsFromAncestor = memberInfo.IsInheritedOverrideFromAncestor(memberInfo.DeclaringType);
            Signature = GetMethodFullSignature(AccessModifierName, memberInfo);
            SignatureWithoutParameters = Signature.Split('(')[0];
            FullDeclarationWithAttributes = AttributesDeclaration + Signature;
        }

        string GetMethodFullSignature(string accessModifierName, MethodInfo methodInfo)
        {
            var signature = string.Empty;
            if (ReflectionUtility.IsExtensionMethod(methodInfo))
            {
                signature = "[Ext] ";
            }

            signature += accessModifierName + " ";
            signature += GetMethodKeywordSnippet(methodInfo);
            if (!methodInfo.Name.Contains("op_Implicit") && !methodInfo.Name.Contains("op_Explicit"))
            {
                signature += methodInfo.ReturnType.GetReadableTypeName() + " ";
            }

            signature += methodInfo.GetMethodNameAndParameters();
            return signature;
        }

        /// <summary>
        /// 获取方法的关键字片段字符串
        /// </summary>
        [Summary("获取方法的关键字片段字符串")]
        public static string GetMethodKeywordSnippet(MethodInfo methodInfo)
        {
            var keyword = "";
            if (methodInfo.IsStatic)
            {
                keyword = "static ";
            }
            else if (methodInfo.IsAbstract)
            {
                keyword = "abstract ";
            }
            else if (methodInfo.IsVirtual && methodInfo.DeclaringType != methodInfo.GetBaseDefinition()
                         .DeclaringType)
            {
                keyword = "override ";
            }
            else if (methodInfo.DeclaringType == methodInfo.GetBaseDefinition().DeclaringType &&
                     methodInfo.IsVirtual && methodInfo.IsFromInterfaceImplementMethod())
            {
                keyword = "";
            }
            else if (methodInfo.IsVirtual)
            {
                keyword = "virtual ";
            }

            if (methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>() != null)
            {
                keyword += "async ";
            }

            return keyword;
        }

        #region IMethodData

        /// <summary>
        /// 方法的返回类型
        /// </summary>
        [Summary("方法的返回类型")]
        public Type ReturnType { get; }

        /// <summary>
        /// 方法的返回类型名称字符串
        /// </summary>
        [Summary("方法的返回类型名称字符串")]
        public string ReturnTypeName { get; }

        /// <summary>
        /// 方法的返回值完整类型名称字符串
        /// </summary>
        [Summary("方法的返回值完整类型名称字符串")]
        public string ReturnTypeFullName { get; }

        /// <summary>
        /// 方法的参数声明字符串，包含参数名称和类型
        /// </summary>
        [Summary("方法的参数声明字符串，包含参数名称和类型")]
        public string ParametersDeclaration { get; }

        /// <summary>
        /// 不包含参数的简单方法签名
        /// </summary>
        [Summary("不包含参数的简单方法签名")]
        public string SignatureWithoutParameters { get; }

        /// <summary>
        /// 是否为抽象方法（abstract）
        /// </summary>
        [Summary("是否为抽象方法（abstract）")]
        public bool IsAbstract { get; }

        /// <summary>
        /// 是否有虚拟方法（virtual）的特性，方法签名中不一定带有 virtual 关键字
        /// </summary>
        [Summary("是否有虚拟方法（virtual）的特性，方法签名中不一定带有 virtual 关键字")]
        public bool IsVirtual { get; }

        /// <summary>
        /// 是否为运算符方法（operator）
        /// </summary>
        [Summary("是否为运算符方法（operator）")]
        public bool IsOperator { get; }

        /// <summary>
        /// 是否有重写方法（override）的特性，方法签名中不一定带有 override 关键字
        /// </summary>
        [Summary("是否有重写方法（override）的特性，方法签名中不一定带有 override 关键字")]
        public bool IsOverride { get; }

        /// <summary>
        /// 是否为异步方法（async）
        /// </summary>
        [Summary("是否为异步方法（async）")]
        public bool IsAsync { get; }

        /// <summary>
        /// 是否从祖先类继承的重写方法，在该子类的方法签名中不一定带有 override 关键字
        /// </summary>
        [Summary("是否从祖先类继承的重写方法，在该子类的方法签名中不一定带有 override 关键字")]
        public bool IsFromAncestor { get; }

        /// <summary>
        /// 是否从接口实现的方法
        /// </summary>
        [Summary("是否从接口实现的方法")]
        public bool IsFromInterfaceImplement { get; }

        /// <summary>
        /// 是否为声明类型中的重载方法
        /// </summary>
        [Summary("是否为声明类型中的重载方法")]
        public bool IsOverloadMethodInDeclaringType { get; set; }

        /// <summary>
        /// 为方法添加重载前缀（[Overload]）
        /// </summary>
        [Summary("为方法添加重载前缀（[Overload]）")]
        public void AddOverloadPrefix() => Signature = "[Overload] " + Signature;

        #endregion

        #region IDerivedMemberData 接口实现

        /// <summary>
        /// 是否为静态方法（static）
        /// </summary>
        [Summary("是否为静态方法（static）")]
        public bool IsStatic { get; }

        /// <summary>
        /// 方法的成员类型
        /// </summary>
        [Summary("方法的成员类型")]
        public MemberTypes MemberType { get; }

        /// <summary>
        /// 方法的成员类型名称字符串
        /// </summary>
        [Summary("方法的成员类型名称字符串")]
        public string MemberTypeName { get; }

        /// <summary>
        /// 方法的访问修饰符类型
        /// </summary>
        [Summary("方法的访问修饰符类型")]
        public AccessModifierType AccessModifier { get; }

        /// <summary>
        /// 方法的访问修饰符名称字符串
        /// </summary>
        [Summary("方法的访问修饰符名称字符串")]
        public string AccessModifierName { get; }

        /// <summary>
        /// 方法的签名字符串
        /// </summary>
        [Summary("方法的签名字符串")]
        public string Signature { get; private set; }

        /// <summary>
        /// 包含特性的完整方法声明字符串，包含特性声明和方法签名
        /// </summary>
        [Summary("包含特性的完整方法声明字符串，包含特性声明和方法签名")]
        public string FullDeclarationWithAttributes { get; }

        #endregion
    }
}
