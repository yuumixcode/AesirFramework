---
title: SummaryAttribute
description: "Runestone.AesirModules.ScriptDocGenerator.SummaryAttribute 的 API 文档"
---

# `SummaryAttribute`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `System.Attribute` → `SummaryAttribute`

**实现接口:** `System.Runtime.InteropServices._Attribute`

## 声明

``` csharp
[AttributeUsage]
public class SummaryAttribute : System.Attribute, 
System.Runtime.InteropServices._Attribute
```

提供类似于 XML 文档 summary 部分的描述性元数据。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SummaryAttribute(string)`](#constructor-summaryattribute-string) | — |

</div>

### SummaryAttribute(string) {#constructor-summaryattribute-string}

``` csharp
public SummaryAttribute(string summaryText)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `summaryText` | `string` | — |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `TypeId` | — | `Attribute` |

</div>

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetSummary()`](#method-getsummary) | — |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `Attribute` |
| `GetHashCode()` | — | `Attribute` |
| `IsDefaultAttribute()` | — | `Attribute` |
| `Match(object)` | — | `Attribute` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

### GetSummary() {#method-getsummary}

``` csharp
public string GetSummary()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
