using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 类型解析数据接口，继承自 IDerivedMemberData 接口
    /// </summary>
    [Summary("类型解析数据接口，继承自 IDerivedMemberData 接口")]
    public interface ITypeData : IDerivedMemberData
    {
        /// <summary>
        /// Type 种类
        /// </summary>
        [Summary("Type 种类")]
        TypeCategory TypeCategory { get; }

        /// <summary>
        /// 类型所在的程序集
        /// </summary>
        [Summary("类型所在的程序集")]
        Assembly Assembly { get; }

        /// <summary>
        /// 类型所在的程序集名称
        /// </summary>
        [Summary("类型所在的程序集名称")]
        string AssemblyName { get; }

        /// <summary>
        /// 类型所在的命名空间
        /// </summary>
        [Summary("类型所在的命名空间")]
        string NamespaceName { get; }

        /// <summary>
        /// 是否为泛型类型
        /// </summary>
        [Summary("是否为泛型类型")]
        bool IsGenericType { get; }

        /// <summary>
        /// 是否为密封类型
        /// </summary>
        [Summary("是否为密封类型")]
        bool IsSealed { get; }

        /// <summary>
        /// 是否为抽象类型
        /// </summary>
        [Summary("是否为抽象类型")]
        bool IsAbstract { get; }

        /// <summary>
        /// 类型的引用链接数组
        /// </summary>
        [Summary("类型的引用链接数组")]
        string[] ReferenceWebLinkArray { get; }

        /// <summary>
        /// 类型的继承链数组
        /// </summary>
        [Summary("类型的继承链数组")]
        string[] InheritanceChain { get; }

        /// <summary>
        /// 类型的接口数组
        /// </summary>
        [Summary("类型的接口数组")]
        string[] InterfaceArray { get; }

        /// <summary>
        /// 分析数据工厂实例对象
        /// </summary>
        [Summary("分析数据工厂实例对象")]
        IAnalysisDataFactory DataFactory { get; }

        /// <summary>
        /// 类型的构造函数解析数据数组，只包含公共构造函数，GetConstructors() 方法
        /// </summary>
        [Summary("类型的构造函数解析数据数组，只包含公共构造函数，GetConstructors() 方法")]
        IConstructorData[] RuntimeReflectedConstructorsData { get; }

        /// <summary>
        /// 类型的方法解析数据数组，GetRuntimeMethods() 方法
        /// </summary>
        [Summary("类型的方法解析数据数组，GetRuntimeMethods() 方法")]
        IMethodData[] RuntimeReflectedMethodsData { get; }

        /// <summary>
        /// 类型的事件解析数据数组，GetRuntimeEvents() 方法
        /// </summary>
        [Summary("类型的事件解析数据数组，GetRuntimeEvents() 方法")]
        IEventData[] RuntimeReflectedEventsData { get; }

        /// <summary>
        /// 类型的属性解析数据数组，GetRuntimeProperties() 方法
        /// </summary>
        [Summary("类型的属性解析数据数组，GetRuntimeProperties() 方法")]
        IPropertyData[] RuntimeReflectedPropertiesData { get; }

        /// <summary>
        /// 类型的字段解析数据数组，GetUserDefinedFields() 方法
        /// </summary>
        [Summary("类型的字段解析数据数组，GetUserDefinedFields() 方法")]
        IFieldData[] RuntimeReflectedFieldsData { get; }
    }

    /// <summary>
    /// 类型解析数据类，存储类型的各种成员的解析数据
    /// </summary>
    [Summary("类型解析数据类，存储类型的各种成员的解析数据")]
    [Serializable]
    public class TypeData : MemberData, ITypeData
    {
        /// <summary>
        /// 创建类型解析数据实例
        /// </summary>
        public TypeData(Type type, IAttributeFilter filter = null, IAnalysisDataFactory factory = null) :
            base(type, filter)
        {
            IsStatic = ReflectionUtility.IsStatic(type);
            MemberType = type.MemberType;
            MemberTypeName = MemberType.ToString();
            AccessModifier = type.GetTypeAccessModifier();
            AccessModifierName = AccessModifier.ConvertToString();
            DataFactory = factory ?? new DefaultAnalysisDataFactory();
            TypeInfo = type.GetTypeInfo();
            TypeCategory = type.GetTypeCategory();
            Assembly = type.Assembly;
            AssemblyName = Assembly.GetName().Name;
            NamespaceName = type.Namespace;
            IsGenericType = type.IsGenericType;
            IsSealed = type.IsSealed;
            IsAbstract = type.IsAbstract;
            ReferenceWebLinkArray = type.GetReferenceLinks();
            InheritanceChain = type.GetInheritanceChain();
            InterfaceArray = type.GetInterfaceArray();
            Signature = GetTypeFullSignature(type, AccessModifierName, TypeCategory);
            FullDeclarationWithAttributes = AttributesDeclaration + Signature;
            RuntimeReflectedConstructorsData = type.GetConstructors()
                .Select(c => DataFactory.CreateConstructorData(c))
                .OrderBy(data => data, new DerivedMemberDataComparer()).ToArray();
            RuntimeReflectedMethodsData = type.GetRuntimeMethods()
                .Where(x =>
                    x != null && !x.Name.Contains("add_") && !x.Name.Contains("remove_") &&
                    !x.Name.Contains("get_") && !x.Name.Contains("set_"))
                .Select(m => DataFactory.CreateMethodData(m))
                .OrderBy(data => data, new DerivedMemberDataComparer()).ToArray();
            RuntimeReflectedEventsData = type.GetRuntimeEvents().Select(e => DataFactory.CreateEventData(e))
                .OrderBy(data => data, new DerivedMemberDataComparer()).ToArray();
            RuntimeReflectedPropertiesData = type.GetRuntimeProperties()
                .Select(p => DataFactory.CreatePropertyData(p))
                .OrderBy(data => data, new DerivedMemberDataComparer()).ToArray();
            RuntimeReflectedFieldsData = type.GetUserDefinedFields()
                .Where(f =>
                    f != null && !f.IsSpecialName && !f.Name.Contains("k__BackingField") &&
                    !f.Name.Contains("__BackingField")).Select(f => DataFactory.CreateFieldData(f))
                .OrderBy(data => data, new DerivedMemberDataComparer()).ToArray();
            RuntimeReflectedMethodsData = MarkOverloadMethod(RuntimeReflectedMethodsData);
        }

        TypeInfo TypeInfo { get; }

        static string GetTypeFullSignature(Type type, string accessModifierName, TypeCategory category)
        {
            var sb = new StringBuilder();
            sb.Append(accessModifierName).Append(" ");
            if (ReflectionUtility.IsStatic(type))
            {
                sb.Append("static ");
            }
            else if (type.IsAbstract && !type.IsInterface)
            {
                sb.Append("abstract ");
            }
            else if (type.IsSealed && !type.IsEnum && !type.IsDelegate() && !type.IsInterface &&
                     !type.IsRecordStruct() && category != TypeCategory.Struct)
            {
                sb.Append("sealed ");
            }

            sb.Append(category.ToString().ToLower()).Append(" ");
            if (category == TypeCategory.Delegate)
            {
                var invokeMethod = type.GetMethod("Invoke");
                if (invokeMethod == null)
                {
                    Debug.LogError("无法获取委托的 Invoke 方法");
                }

                sb.Append(ReflectionUtility.GetReturnType(invokeMethod).GetReadableTypeName());
                sb.Append(" ");
                sb.Append(type.GetReadableTypeName());
                sb.Append("(");
                sb.Append(invokeMethod.GetParametersNameWithDefaultValue());
                sb.Append(")");
            }
            else
            {
                if (type.IsRecordStruct())
                {
                    sb.Append("struct ");
                }

                sb.Append(type.GetReadableTypeName());

                var inheritTypes = new List<string>();
                if (type.BaseType != null && type.BaseType != typeof(object))
                {
                    inheritTypes.Add(type.BaseType.GetReadableTypeName(true));
                }

                var interfaces = type.GetInterfaces()
                    .Where(i => !i.IsDefined(typeof(CompilerGeneratedAttribute), false));
                inheritTypes.AddRange(interfaces.Select(x => x.GetReadableTypeName(true)));
                if (inheritTypes.Count > 0)
                {
                    sb.Append(" : ");
                    for (var i = 0; i < inheritTypes.Count; i++)
                    {
                        sb.Append(inheritTypes[i]);
                        if (i < inheritTypes.Count - 1)
                        {
                            sb.AppendLine(", ");
                        }
                    }
                }

                if (type.IsGenericType)
                {
                    sb.Append(" " + OdinBridgeLocator.Bridge.GetGenericConstraintsString(type, true));
                }
            }

            return sb.ToString();
        }

        static IMethodData[] MarkOverloadMethod(IMethodData[] methodAnalysisDataArray)
        {
            for (var i = 0; i < methodAnalysisDataArray.Length; i++)
            {
                for (var j = 0; j < methodAnalysisDataArray.Length; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    if (methodAnalysisDataArray[i].SignatureWithoutParameters ==
                        methodAnalysisDataArray[j].SignatureWithoutParameters)
                    {
                        methodAnalysisDataArray[i].IsOverloadMethodInDeclaringType = true;
                        methodAnalysisDataArray[j].IsOverloadMethodInDeclaringType = true;
                        methodAnalysisDataArray[i].AddOverloadPrefix();
                        methodAnalysisDataArray[j].AddOverloadPrefix();
                    }
                }
            }

            return methodAnalysisDataArray;
        }

        #region ITypeData

        /// <summary>
        /// Type 种类
        /// </summary>
        [Summary("Type 种类")]
        public TypeCategory TypeCategory { get; }

        /// <summary>
        /// 类型所在的程序集
        /// </summary>
        [Summary("类型所在的程序集")]
        public Assembly Assembly { get; }

        /// <summary>
        /// 程序集名称
        /// </summary>
        [Summary("程序集名称")]
        public string AssemblyName { get; }

        /// <summary>
        /// 命名空间名称
        /// </summary>
        [Summary("命名空间名称")]
        public string NamespaceName { get; }

        /// <summary>
        /// 是否为泛型类型
        /// </summary>
        [Summary("是否为泛型类型")]
        public bool IsGenericType { get; }

        /// <summary>
        /// 是否为密封类
        /// </summary>
        [Summary("是否为密封类")]
        public bool IsSealed { get; }

        /// <summary>
        /// 是否为抽象类
        /// </summary>
        [Summary("是否为抽象类")]
        public bool IsAbstract { get; }

        /// <summary>
        /// 引用链接数组
        /// </summary>
        [Summary("引用链接数组")]
        public string[] ReferenceWebLinkArray { get; }

        /// <summary>
        /// 继承链数组
        /// </summary>
        [Summary("继承链数组")]
        public string[] InheritanceChain { get; }

        /// <summary>
        /// 接口列表数组
        /// </summary>
        [Summary("接口列表数组")]
        public string[] InterfaceArray { get; }

        /// <summary>
        /// 分析数据工厂实例对象
        /// </summary>
        [Summary("分析数据工厂实例对象")]
        public IAnalysisDataFactory DataFactory { get; }

        /// <summary>
        /// 声明的构造方法解析数据数组，只包含公共构造函数，GetConstructors() 方法
        /// </summary>
        [Summary("声明的构造方法解析数据数组，只包含公共构造函数，GetConstructors() 方法")]
        public IConstructorData[] RuntimeReflectedConstructorsData { get; }

        /// <summary>
        /// 声明的方法解析数据数组，GetRuntimeMethods() 方法
        /// </summary>
        [Summary("声明的方法解析数据数组，GetRuntimeMethods() 方法")]
        public IMethodData[] RuntimeReflectedMethodsData { get; }

        /// <summary>
        /// 声明的事件解析数据数组，GetRuntimeEvents() 方法
        /// </summary>
        [Summary("声明的事件解析数据数组，GetRuntimeEvents() 方法")]
        public IEventData[] RuntimeReflectedEventsData { get; }

        /// <summary>
        /// 声明的属性解析数据数组，GetRuntimeProperties() 方法
        /// </summary>
        [Summary("声明的属性解析数据数组，GetRuntimeProperties() 方法")]
        public IPropertyData[] RuntimeReflectedPropertiesData { get; }

        /// <summary>
        /// 类型的字段解析数据数组，GetUserDefinedFields() 方法
        /// </summary>
        [Summary("类型的字段解析数据数组，GetUserDefinedFields() 方法")]
        public IFieldData[] RuntimeReflectedFieldsData { get; }

        #endregion

        #region IDerivedMemberData

        /// <summary>
        /// 是否为静态类型
        /// </summary>
        [Summary("是否为静态类型")]
        public bool IsStatic { get; }

        /// <summary>
        /// 成员类型
        /// </summary>
        [Summary("成员类型")]
        public MemberTypes MemberType { get; }

        /// <summary>
        /// 成员类型名称
        /// </summary>
        [Summary("成员类型名称")]
        public string MemberTypeName { get; }

        /// <summary>
        /// 访问修饰符
        /// </summary>
        [Summary("访问修饰符")]
        public AccessModifierType AccessModifier { get; }

        /// <summary>
        /// 访问修饰符名称
        /// </summary>
        [Summary("访问修饰符名称")]
        public string AccessModifierName { get; }

        /// <summary>
        /// 类型签名，不包含特性声明
        /// </summary>
        [Summary("类型签名，不包含特性声明")]
        public string Signature { get; }

        /// <summary>
        /// 完整类型声明 - 包含特性和签名 - 默认剔除 [Summary] 特性
        /// </summary>
        [Summary("完整类型声明 - 包含特性和签名 - 默认剔除 [Summary] 特性")]
        public string FullDeclarationWithAttributes { get; }

        #endregion
    }
}
