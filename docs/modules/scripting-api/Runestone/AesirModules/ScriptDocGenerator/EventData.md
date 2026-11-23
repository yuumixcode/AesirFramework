---
title: EventData
description: "Runestone.AesirModules.ScriptDocGenerator.EventData 的 API 文档"
---

# `EventData`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `Runestone.AesirModules.ScriptDocGenerator.MemberData` → `EventData`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IEventData`，`Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`，`Runestone.AesirModules.ScriptDocGenerator.IMemberData`

## 声明

``` csharp
[Serializable]
public class EventData : Runestone.AesirModules.ScriptDocGenerator.MemberData, 
Runestone.AesirModules.ScriptDocGenerator.IEventData, 
Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData, 
Runestone.AesirModules.ScriptDocGenerator.IMemberData
```

事件解析数据类，用于存储事件的解析数据

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`EventData(EventInfo, IAttributeFilter)`](#constructor-eventdata-eventinfo-iattributefilter) | — |

</div>

### EventData(EventInfo, IAttributeFilter) {#constructor-eventdata-eventinfo-iattributefilter}

``` csharp
public EventData(EventInfo eventInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventInfo` | `EventInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | 访问修饰符类型 |
| [`MemberType`](#property-membertype) | 成员类型 |
| [`EventType`](#property-eventtype) | 事件类型 |
| [`IsStatic`](#property-isstatic) | 是否为静态事件 |
| [`AccessModifierName`](#property-accessmodifiername) | 访问修饰符名称 |
| [`EventTypeFullName`](#property-eventtypefullname) | 事件类型的完整名称，包括命名空间 |
| [`EventTypeName`](#property-eventtypename) | 事件类型名称 |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | 包含特性和签名的完整事件声明 |
| [`MemberTypeName`](#property-membertypename) | 成员类型名称 |
| [`Signature`](#property-signature) | 事件的完整签名 |

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

成员类型

``` csharp
public MemberTypes MemberType { get; }
```

### EventType {#property-eventtype}

事件类型

``` csharp
public Type EventType { get; }
```

### IsStatic {#property-isstatic}

是否为静态事件

``` csharp
public bool IsStatic { get; }
```

### AccessModifierName {#property-accessmodifiername}

访问修饰符名称

``` csharp
public string AccessModifierName { get; }
```

### EventTypeFullName {#property-eventtypefullname}

事件类型的完整名称，包括命名空间

``` csharp
public string EventTypeFullName { get; }
```

### EventTypeName {#property-eventtypename}

事件类型名称

``` csharp
public string EventTypeName { get; }
```

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

包含特性和签名的完整事件声明

``` csharp
public string FullDeclarationWithAttributes { get; }
```

### MemberTypeName {#property-membertypename}

成员类型名称

``` csharp
public string MemberTypeName { get; }
```

### Signature {#property-signature}

事件的完整签名

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
