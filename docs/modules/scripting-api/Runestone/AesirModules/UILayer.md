---
title: UILayer
description: "Runestone.AesirModules.UILayer 的 API 文档"
---

# `UILayer`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `UILayer`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum UILayer : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

UI 层级。Background < Normal < Popup < Top。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Background`](#field-background) | 背景层，SortingOrder 基准 100。 |
| [`Normal`](#field-normal) | 常规层，SortingOrder 基准 200。 |
| [`Popup`](#field-popup) | 弹窗层，SortingOrder 基准 300。 |
| [`Top`](#field-top) | 顶层（Toast/系统提示），SortingOrder 基准 400。 |

</div>

### Background {#field-background}

背景层，SortingOrder 基准 100。

``` csharp
[InspectorName]
public const UILayer Background;
```

### Normal {#field-normal}

常规层，SortingOrder 基准 200。

``` csharp
[InspectorName]
public const UILayer Normal;
```

### Popup {#field-popup}

弹窗层，SortingOrder 基准 300。

``` csharp
[InspectorName]
public const UILayer Popup;
```

### Top {#field-top}

顶层（Toast/系统提示），SortingOrder 基准 400。

``` csharp
[InspectorName]
public const UILayer Top;
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
