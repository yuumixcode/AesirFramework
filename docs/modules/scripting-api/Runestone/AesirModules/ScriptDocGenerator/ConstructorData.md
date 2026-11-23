---
title: ConstructorData
description: "Runestone.AesirModules.ScriptDocGenerator.ConstructorData 的 API 文档"
---

# `ConstructorData`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `Runestone.AesirModules.ScriptDocGenerator.MemberData` → `ConstructorData`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`，`Runestone.AesirModules.ScriptDocGenerator.IMemberData`，`Runestone.AesirModules.ScriptDocGenerator.IConstructorData`

## 声明

``` csharp
[Serializable]
public class ConstructorData : Runestone.AesirModules.ScriptDocGenerator.MemberData, 
Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData, 
Runestone.AesirModules.ScriptDocGenerator.IMemberData, 
Runestone.AesirModules.ScriptDocGenerator.IConstructorData
```

构造方法解析数据

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ConstructorData(ConstructorInfo, IAttributeFilter)`](#constructor-constructordata-constructorinfo-iattributefilter) | — |

</div>

### ConstructorData(ConstructorInfo, IAttributeFilter) {#constructor-constructordata-constructorinfo-iattributefilter}

``` csharp
public ConstructorData(ConstructorInfo constructorInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `constructorInfo` | `ConstructorInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | 构造方法的访问修饰符类型 |
| [`Parameters`](#property-parameters) | 构造方法的参数解析数据数组，分析期从反射直接结构化产出 |
| [`ParamSummaries`](#property-paramsummaries) | 参数级注释字典（XML <param> 标签），键为参数名。无参数注释时为 null |
| [`MemberType`](#property-membertype) | 构造方法的成员类型 |
| [`IsStatic`](#property-isstatic) | 是否为静态构造方法 |
| [`AccessModifierName`](#property-accessmodifiername) | 构造方法的访问修饰符名称字符串 |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | 包含特性和签名的完整构造方法声明 |
| [`MemberTypeName`](#property-membertypename) | 构造方法的成员类型名称字符串 |
| [`ParametersDeclaration`](#property-parametersdeclaration) | 构造方法的参数声明字符串，包含参数名称和类型 |
| [`Signature`](#property-signature) | 构造方法的完整签名 |
| [`SignatureWithoutParameters`](#property-signaturewithoutparameters) | 不包含参数的简单构造方法签名 |

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

构造方法的访问修饰符类型

``` csharp
public AccessModifierType AccessModifier { get; }
```

### Parameters {#property-parameters}

构造方法的参数解析数据数组，分析期从反射直接结构化产出

``` csharp
public IParameterData[] Parameters { get; }
```

### ParamSummaries {#property-paramsummaries}

参数级注释字典（XML <param> 标签），键为参数名。无参数注释时为 null

``` csharp
public IReadOnlyDictionary<string, string> ParamSummaries { get; }
```

### MemberType {#property-membertype}

构造方法的成员类型

``` csharp
public MemberTypes MemberType { get; }
```

### IsStatic {#property-isstatic}

是否为静态构造方法

``` csharp
public bool IsStatic { get; }
```

### AccessModifierName {#property-accessmodifiername}

构造方法的访问修饰符名称字符串

``` csharp
public string AccessModifierName { get; }
```

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

包含特性和签名的完整构造方法声明

``` csharp
public string FullDeclarationWithAttributes { get; }
```

### MemberTypeName {#property-membertypename}

构造方法的成员类型名称字符串

``` csharp
public string MemberTypeName { get; }
```

### ParametersDeclaration {#property-parametersdeclaration}

构造方法的参数声明字符串，包含参数名称和类型

``` csharp
public string ParametersDeclaration { get; }
```

### Signature {#property-signature}

构造方法的完整签名

``` csharp
public string Signature { get; }
```

### SignatureWithoutParameters {#property-signaturewithoutparameters}

不包含参数的简单构造方法签名

``` csharp
public string SignatureWithoutParameters { get; }
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
