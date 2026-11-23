---
title: FieldData
description: "Runestone.AesirModules.ScriptDocGenerator.FieldData 的 API 文档"
---

# `FieldData`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `Runestone.AesirModules.ScriptDocGenerator.MemberData` → `FieldData`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IFieldData`，`Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`，`Runestone.AesirModules.ScriptDocGenerator.IMemberData`

## 声明

``` csharp
[Serializable]
public class FieldData : Runestone.AesirModules.ScriptDocGenerator.MemberData, 
Runestone.AesirModules.ScriptDocGenerator.IFieldData, 
Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData, 
Runestone.AesirModules.ScriptDocGenerator.IMemberData
```

字段解析数据类，用于存储字段的解析数据

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`FieldData(FieldInfo, IAttributeFilter)`](#constructor-fielddata-fieldinfo-iattributefilter) | — |

</div>

### FieldData(FieldInfo, IAttributeFilter) {#constructor-fielddata-fieldinfo-iattributefilter}

``` csharp
public FieldData(FieldInfo fieldInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `fieldInfo` | `FieldInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | 访问修饰符类型 |
| [`MemberType`](#property-membertype) | 成员类型，指示该成员的类型 |
| [`FieldType`](#property-fieldtype) | 字段的类型 |
| [`IsConstant`](#property-isconstant) | 是否为常量字段 |
| [`IsDynamic`](#property-isdynamic) | 是否为动态类型字段 |
| [`IsReadOnly`](#property-isreadonly) | 是否为只读字段 |
| [`IsStatic`](#property-isstatic) | 指示该字段是否为静态字段 |
| [`DefaultValue`](#property-defaultvalue) | 字段的默认值，没有默认值返回 null |
| [`AccessModifierName`](#property-accessmodifiername) | 访问修饰符名称 |
| [`FieldTypeFullName`](#property-fieldtypefullname) | 字段类型的完整名称 |
| [`FieldTypeName`](#property-fieldtypename) | 字段类型的名称 |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | 完整字段声明，包含特性和签名，默认剔除 Summary 特性 |
| [`MemberTypeName`](#property-membertypename) | 成员类型名称 |
| [`Signature`](#property-signature) | 字段签名 |

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

访问修饰符类型

``` csharp
public AccessModifierType AccessModifier { get; }
```

### MemberType {#property-membertype}

成员类型，指示该成员的类型

``` csharp
public MemberTypes MemberType { get; }
```

### FieldType {#property-fieldtype}

字段的类型

``` csharp
public Type FieldType { get; }
```

### IsConstant {#property-isconstant}

是否为常量字段

``` csharp
public bool IsConstant { get; }
```

### IsDynamic {#property-isdynamic}

是否为动态类型字段

``` csharp
public bool IsDynamic { get; }
```

### IsReadOnly {#property-isreadonly}

是否为只读字段

``` csharp
public bool IsReadOnly { get; }
```

### IsStatic {#property-isstatic}

指示该字段是否为静态字段

``` csharp
public bool IsStatic { get; }
```

### DefaultValue {#property-defaultvalue}

字段的默认值，没有默认值返回 null

``` csharp
public object DefaultValue { get; }
```

### AccessModifierName {#property-accessmodifiername}

访问修饰符名称

``` csharp
public string AccessModifierName { get; }
```

### FieldTypeFullName {#property-fieldtypefullname}

字段类型的完整名称

``` csharp
public string FieldTypeFullName { get; }
```

### FieldTypeName {#property-fieldtypename}

字段类型的名称

``` csharp
public string FieldTypeName { get; }
```

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

完整字段声明，包含特性和签名，默认剔除 Summary 特性

``` csharp
public string FullDeclarationWithAttributes { get; }
```

### MemberTypeName {#property-membertypename}

成员类型名称

``` csharp
public string MemberTypeName { get; }
```

### Signature {#property-signature}

字段签名

``` csharp
public string Signature { get; private set; }
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
