---
title: MethodData
description: "Runestone.AesirModules.ScriptDocGenerator.MethodData 的 API 文档"
---

# `MethodData`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `Runestone.AesirModules.ScriptDocGenerator.MemberData` → `MethodData`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`，`Runestone.AesirModules.ScriptDocGenerator.IMemberData`，`Runestone.AesirModules.ScriptDocGenerator.IMethodData`

## 声明

``` csharp
[Serializable]
public class MethodData : Runestone.AesirModules.ScriptDocGenerator.MemberData, 
Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData, 
Runestone.AesirModules.ScriptDocGenerator.IMemberData, 
Runestone.AesirModules.ScriptDocGenerator.IMethodData
```

方法解析数据类，用于存储 MethodInfo 的解析结果

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`MethodData(MethodInfo, IAttributeFilter)`](#constructor-methoddata-methodinfo-iattributefilter) | — |

</div>

### MethodData(MethodInfo, IAttributeFilter) {#constructor-methoddata-methodinfo-iattributefilter}

``` csharp
public MethodData(MethodInfo memberInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `memberInfo` | `MethodInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | 方法的访问修饰符类型 |
| [`Parameters`](#property-parameters) | 方法的参数解析数据数组，分析期从反射直接结构化产出 |
| [`ParamSummaries`](#property-paramsummaries) | 参数级注释字典（XML <param> 标签），键为参数名。无参数注释时为 null |
| [`MemberType`](#property-membertype) | 方法的成员类型 |
| [`ReturnType`](#property-returntype) | 方法的返回类型 |
| [`IsAbstract`](#property-isabstract) | 是否为抽象方法（abstract） |
| [`IsAsync`](#property-isasync) | 是否为异步方法（async） |
| [`IsFromAncestor`](#property-isfromancestor) | 是否从祖先类继承的重写方法，在该子类的方法签名中不一定带有 override 关键字 |
| [`IsFromInterfaceImplement`](#property-isfrominterfaceimplement) | 是否从接口实现的方法 |
| [`IsOperator`](#property-isoperator) | 是否为运算符方法（operator） |
| [`IsOverloadMethodInDeclaringType`](#property-isoverloadmethodindeclaringtype) | 是否为声明类型中的重载方法 |
| [`IsOverride`](#property-isoverride) | 是否有重写方法（override）的特性，方法签名中不一定带有 override 关键字 |
| [`IsStatic`](#property-isstatic) | 是否为静态方法（static） |
| [`IsVirtual`](#property-isvirtual) | 是否有虚拟方法（virtual）的特性，方法签名中不一定带有 virtual 关键字 |
| [`AccessModifierName`](#property-accessmodifiername) | 方法的访问修饰符名称字符串 |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | 包含特性的完整方法声明字符串，包含特性声明和方法签名 |
| [`MemberTypeName`](#property-membertypename) | 方法的成员类型名称字符串 |
| [`ParametersDeclaration`](#property-parametersdeclaration) | 方法的参数声明字符串，包含参数名称和类型 |
| [`ReturnTypeFullName`](#property-returntypefullname) | 方法的返回值完整类型名称字符串 |
| [`ReturnTypeName`](#property-returntypename) | 方法的返回类型名称字符串 |
| [`ReturnsSummary`](#property-returnssummary) | 返回值注释（XML <returns> 标签）。无注释时为 null |
| [`Signature`](#property-signature) | 方法的签名字符串 |
| [`SignatureWithoutParameters`](#property-signaturewithoutparameters) | 不包含参数的简单方法签名 |

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

方法的访问修饰符类型

``` csharp
public AccessModifierType AccessModifier { get; }
```

### Parameters {#property-parameters}

方法的参数解析数据数组，分析期从反射直接结构化产出

``` csharp
public IParameterData[] Parameters { get; }
```

### ParamSummaries {#property-paramsummaries}

参数级注释字典（XML <param> 标签），键为参数名。无参数注释时为 null

``` csharp
public IReadOnlyDictionary<string, string> ParamSummaries { get; }
```

### MemberType {#property-membertype}

方法的成员类型

``` csharp
public MemberTypes MemberType { get; }
```

### ReturnType {#property-returntype}

方法的返回类型

``` csharp
public Type ReturnType { get; }
```

### IsAbstract {#property-isabstract}

是否为抽象方法（abstract）

``` csharp
public bool IsAbstract { get; }
```

### IsAsync {#property-isasync}

是否为异步方法（async）

``` csharp
public bool IsAsync { get; }
```

### IsFromAncestor {#property-isfromancestor}

是否从祖先类继承的重写方法，在该子类的方法签名中不一定带有 override 关键字

``` csharp
public bool IsFromAncestor { get; }
```

### IsFromInterfaceImplement {#property-isfrominterfaceimplement}

是否从接口实现的方法

``` csharp
public bool IsFromInterfaceImplement { get; }
```

### IsOperator {#property-isoperator}

是否为运算符方法（operator）

``` csharp
public bool IsOperator { get; }
```

### IsOverloadMethodInDeclaringType {#property-isoverloadmethodindeclaringtype}

是否为声明类型中的重载方法

``` csharp
public bool IsOverloadMethodInDeclaringType { get; set; }
```

### IsOverride {#property-isoverride}

是否有重写方法（override）的特性，方法签名中不一定带有 override 关键字

``` csharp
public bool IsOverride { get; }
```

### IsStatic {#property-isstatic}

是否为静态方法（static）

``` csharp
public bool IsStatic { get; }
```

### IsVirtual {#property-isvirtual}

是否有虚拟方法（virtual）的特性，方法签名中不一定带有 virtual 关键字

``` csharp
public bool IsVirtual { get; }
```

### AccessModifierName {#property-accessmodifiername}

方法的访问修饰符名称字符串

``` csharp
public string AccessModifierName { get; }
```

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

包含特性的完整方法声明字符串，包含特性声明和方法签名

``` csharp
public string FullDeclarationWithAttributes { get; }
```

### MemberTypeName {#property-membertypename}

方法的成员类型名称字符串

``` csharp
public string MemberTypeName { get; }
```

### ParametersDeclaration {#property-parametersdeclaration}

方法的参数声明字符串，包含参数名称和类型

``` csharp
public string ParametersDeclaration { get; }
```

### ReturnTypeFullName {#property-returntypefullname}

方法的返回值完整类型名称字符串

``` csharp
public string ReturnTypeFullName { get; }
```

### ReturnTypeName {#property-returntypename}

方法的返回类型名称字符串

``` csharp
public string ReturnTypeName { get; }
```

### ReturnsSummary {#property-returnssummary}

返回值注释（XML <returns> 标签）。无注释时为 null

``` csharp
public string ReturnsSummary { get; }
```

### Signature {#property-signature}

方法的签名字符串

``` csharp
public string Signature { get; private set; }
```

### SignatureWithoutParameters {#property-signaturewithoutparameters}

不包含参数的简单方法签名

``` csharp
public string SignatureWithoutParameters { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddOverloadPrefix()`](#method-addoverloadprefix) | 为方法添加重载前缀（[Overload]） |
| [`GetMethodKeywordSnippet(MethodInfo)`](#method-getmethodkeywordsnippet-methodinfo) | 获取方法的关键字片段字符串 |

</div>

**继承的方法**

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

### AddOverloadPrefix() {#method-addoverloadprefix}

为方法添加重载前缀（[Overload]）

``` csharp
public void AddOverloadPrefix()
```

### GetMethodKeywordSnippet(MethodInfo) {#method-getmethodkeywordsnippet-methodinfo}

获取方法的关键字片段字符串

``` csharp
public static string GetMethodKeywordSnippet(MethodInfo methodInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `methodInfo` | `MethodInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
