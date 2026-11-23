---
title: XmlCodePart.SummaryAttribution
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.XmlCodePart.SummaryAttribution 的 API 文档"
---

# `XmlCodePart.SummaryAttribution`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `XmlCodePart.SummaryAttribution`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
private enum XmlCodePart.SummaryAttribution : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

[Summary] 特性的归属分析结果。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`FirstMember`](#field-firstmember) | 首个 [Summary] 位于首成员声明之前——属于 XML 注释的归属成员。 |
| [`NonFirstMember`](#field-nonfirstmember) | — |
| [`None`](#field-none) | 块内无 [Summary] 特性。 |

</div>

### FirstMember {#field-firstmember}

首个 [Summary] 位于首成员声明之前——属于 XML 注释的归属成员。

``` csharp
public const XmlCodePart.SummaryAttribution FirstMember;
```

### NonFirstMember {#field-nonfirstmember}

``` csharp
public const XmlCodePart.SummaryAttribution NonFirstMember;
```

### None {#field-none}

块内无 [Summary] 特性。

``` csharp
public const XmlCodePart.SummaryAttribution None;
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `HasFlag(Enum)` | — | `Enum` |
| `Equals(object)` | — | `Enum` |
| `GetHashCode()` | — | `Enum` |
| `ToString()` | — | `Enum` |
| `ToString(string)` | — | `Enum` |
| `GetTypeCode()` | — | `Enum` |
| `CompareTo(object)` | — | `Enum` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `ToString(IFormatProvider)` | — | `Enum` |
| `ToString(string, IFormatProvider)` | — | `Enum` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
