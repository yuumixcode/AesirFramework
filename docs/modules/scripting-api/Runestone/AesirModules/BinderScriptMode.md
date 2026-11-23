---
title: BinderScriptMode
description: "Runestone.AesirModules.BinderScriptMode 的 API 文档"
---

# `BinderScriptMode`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `BinderScriptMode`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum BinderScriptMode : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

Binder 脚本生成模式。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`PartialClass`](#field-partialclass) | partial 分部类: 生成自动维护文件（后缀可选，默认 .designer.cs，整体覆盖）+ 手写 partial *.cs（仅首次生成）。 |
| [`SameScriptIncrement`](#field-samescriptincrement) | — |

</div>

### PartialClass {#field-partialclass}

partial 分部类: 生成自动维护文件（后缀可选，默认 .designer.cs，整体覆盖）+ 手写 partial *.cs（仅首次生成）。

``` csharp
[InspectorName]
public const BinderScriptMode PartialClass;
```

### SameScriptIncrement {#field-samescriptincrement}

``` csharp
[InspectorName]
public const BinderScriptMode SameScriptIncrement;
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
