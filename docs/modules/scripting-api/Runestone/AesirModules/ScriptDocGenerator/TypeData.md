---
title: TypeData
description: "Runestone.AesirModules.ScriptDocGenerator.TypeData 的 API 文档"
---

# `TypeData`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `Runestone.AesirModules.ScriptDocGenerator.MemberData` → `TypeData`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.ITypeData`，`Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`，`Runestone.AesirModules.ScriptDocGenerator.IMemberData`

## 声明

``` csharp
[Serializable]
public class TypeData : Runestone.AesirModules.ScriptDocGenerator.MemberData, 
Runestone.AesirModules.ScriptDocGenerator.ITypeData, 
Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData, 
Runestone.AesirModules.ScriptDocGenerator.IMemberData
```

类型解析数据类，存储类型的各种成员的解析数据

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`TypeData(Type, IAttributeFilter, IAnalysisDataFactory)`](#constructor-typedata-type-iattributefilter-ianalysisdatafactory) | — |

</div>

### TypeData(Type, IAttributeFilter, IAnalysisDataFactory) {#constructor-typedata-type-iattributefilter-ianalysisdatafactory}

``` csharp
public TypeData(Type type, IAttributeFilter filter = null, IAnalysisDataFactory factory = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |
| `filter` | `IAttributeFilter` | — |
| `factory` | `IAnalysisDataFactory` | — |

</div>

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | 访问修饰符 |
| [`Assembly`](#property-assembly) | 类型所在的程序集 |
| [`DataFactory`](#property-datafactory) | 分析数据工厂实例对象 |
| [`RuntimeReflectedConstructorsData`](#property-runtimereflectedconstructorsdata) | 声明的构造方法解析数据数组，只包含公共构造函数，GetConstructors() 方法 |
| [`RuntimeReflectedEventsData`](#property-runtimereflectedeventsdata) | 声明的事件解析数据数组，GetRuntimeEvents() 方法 |
| [`RuntimeReflectedFieldsData`](#property-runtimereflectedfieldsdata) | 类型的字段解析数据数组，GetUserDefinedFields() 方法 |
| [`RuntimeReflectedMethodsData`](#property-runtimereflectedmethodsdata) | 声明的方法解析数据数组，GetRuntimeMethods() 方法 |
| [`RuntimeReflectedPropertiesData`](#property-runtimereflectedpropertiesdata) | 声明的属性解析数据数组，GetRuntimeProperties() 方法 |
| [`TypeParamSummaries`](#property-typeparamsummaries) | 泛型参数注释字典（XML <typeparam> 标签），键为参数名。无注释时为 null |
| [`MemberType`](#property-membertype) | 成员类型 |
| [`TypeCategory`](#property-typecategory) | Type 种类 |
| [`IsAbstract`](#property-isabstract) | 是否为抽象类 |
| [`IsGenericType`](#property-isgenerictype) | 是否为泛型类型 |
| [`IsSealed`](#property-issealed) | 是否为密封类 |
| [`IsStatic`](#property-isstatic) | 是否为静态类型 |
| [`AccessModifierName`](#property-accessmodifiername) | 访问修饰符名称 |
| [`AssemblyName`](#property-assemblyname) | 程序集名称 |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | 完整类型声明 - 包含特性和签名 - 默认剔除 [Summary] 特性 |
| [`MemberTypeName`](#property-membertypename) | 成员类型名称 |
| [`NamespaceName`](#property-namespacename) | 命名空间名称 |
| [`Signature`](#property-signature) | 类型签名，不包含特性声明 |
| [`InheritanceChain`](#property-inheritancechain) | 继承链数组 |
| [`InterfaceArray`](#property-interfacearray) | 接口列表数组 |
| [`ReferenceWebLinkArray`](#property-referenceweblinkarray) | 引用链接数组 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `DeclaringType` | 声明此成员的类型 | `MemberData` |
| `ReflectedType` | 通过反射获取该成员的类型 | `MemberData` |
| `IsFromInheritance` | 成员是否从继承中获取，这里的成员不包括 Type 类型 | `MemberData` |
| `IsObsolete` | 是否已过时 | `MemberData` |
| `AttributesDeclaration` | 特性声明字符串 | `MemberData` |
| `DeclaringTypeFullName` | 声明类型的完整名称，包括命名空间 | `MemberData` |
| `DeclaringTypeName` | 声明类型的名称 | `MemberData` |
| `Name` | 成员名称 | `MemberData` |
| `ReflectedTypeFullName` | 通过反射获取该成员的类型的完整名称，包括命名空间 | `MemberData` |
| `ReflectedTypeName` | 通过反射获取该成员的类型名称 | `MemberData` |
| `RemarksSummary` | 备注注释（XML <remarks> 标签）。无注释时为 null | `MemberData` |
| `SummaryAttributeValue` | 注释 | `MemberData` |

</div>

### AccessModifier {#property-accessmodifier}

访问修饰符

``` csharp
public AccessModifierType AccessModifier { get; }
```

### Assembly {#property-assembly}

类型所在的程序集

``` csharp
public Assembly Assembly { get; }
```

### DataFactory {#property-datafactory}

分析数据工厂实例对象

``` csharp
public IAnalysisDataFactory DataFactory { get; }
```

### RuntimeReflectedConstructorsData {#property-runtimereflectedconstructorsdata}

声明的构造方法解析数据数组，只包含公共构造函数，GetConstructors() 方法

``` csharp
public IConstructorData[] RuntimeReflectedConstructorsData { get; }
```

### RuntimeReflectedEventsData {#property-runtimereflectedeventsdata}

声明的事件解析数据数组，GetRuntimeEvents() 方法

``` csharp
public IEventData[] RuntimeReflectedEventsData { get; }
```

### RuntimeReflectedFieldsData {#property-runtimereflectedfieldsdata}

类型的字段解析数据数组，GetUserDefinedFields() 方法

``` csharp
public IFieldData[] RuntimeReflectedFieldsData { get; }
```

### RuntimeReflectedMethodsData {#property-runtimereflectedmethodsdata}

声明的方法解析数据数组，GetRuntimeMethods() 方法

``` csharp
public IMethodData[] RuntimeReflectedMethodsData { get; }
```

### RuntimeReflectedPropertiesData {#property-runtimereflectedpropertiesdata}

声明的属性解析数据数组，GetRuntimeProperties() 方法

``` csharp
public IPropertyData[] RuntimeReflectedPropertiesData { get; }
```

### TypeParamSummaries {#property-typeparamsummaries}

泛型参数注释字典（XML <typeparam> 标签），键为参数名。无注释时为 null

``` csharp
public IReadOnlyDictionary<string, string> TypeParamSummaries { get; }
```

### MemberType {#property-membertype}

成员类型

``` csharp
public MemberTypes MemberType { get; }
```

### TypeCategory {#property-typecategory}

Type 种类

``` csharp
public TypeCategory TypeCategory { get; }
```

### IsAbstract {#property-isabstract}

是否为抽象类

``` csharp
public bool IsAbstract { get; }
```

### IsGenericType {#property-isgenerictype}

是否为泛型类型

``` csharp
public bool IsGenericType { get; }
```

### IsSealed {#property-issealed}

是否为密封类

``` csharp
public bool IsSealed { get; }
```

### IsStatic {#property-isstatic}

是否为静态类型

``` csharp
public bool IsStatic { get; }
```

### AccessModifierName {#property-accessmodifiername}

访问修饰符名称

``` csharp
public string AccessModifierName { get; }
```

### AssemblyName {#property-assemblyname}

程序集名称

``` csharp
public string AssemblyName { get; }
```

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

完整类型声明 - 包含特性和签名 - 默认剔除 [Summary] 特性

``` csharp
public string FullDeclarationWithAttributes { get; }
```

### MemberTypeName {#property-membertypename}

成员类型名称

``` csharp
public string MemberTypeName { get; }
```

### NamespaceName {#property-namespacename}

命名空间名称

``` csharp
public string NamespaceName { get; }
```

### Signature {#property-signature}

类型签名，不包含特性声明

``` csharp
public string Signature { get; }
```

### InheritanceChain {#property-inheritancechain}

继承链数组

``` csharp
public string[] InheritanceChain { get; }
```

### InterfaceArray {#property-interfacearray}

接口列表数组

``` csharp
public string[] InterfaceArray { get; }
```

### ReferenceWebLinkArray {#property-referenceweblinkarray}

引用链接数组

``` csharp
public string[] ReferenceWebLinkArray { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
