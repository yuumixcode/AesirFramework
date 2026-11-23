---
title: TypeCategory
description: "Runestone.AesirModules.ScriptDocGenerator.TypeCategory 的 API 文档"
---

# `TypeCategory`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `TypeCategory`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum TypeCategory : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

类型种类枚举

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Class`](#field-class) | 类 |
| [`Delegate`](#field-delegate) | 委托 |
| [`Enum`](#field-enum) | 枚举 |
| [`Interface`](#field-interface) | 接口 |
| [`Record`](#field-record) | 记录类型 |
| [`Struct`](#field-struct) | 结构体 |
| [`Unknown`](#field-unknown) | — |

</div>

### Class {#field-class}

类

``` csharp
public const TypeCategory Class;
```

### Delegate {#field-delegate}

委托

``` csharp
public const TypeCategory Delegate;
```

### Enum {#field-enum}

枚举

``` csharp
public const TypeCategory Enum;
```

### Interface {#field-interface}

接口

``` csharp
public const TypeCategory Interface;
```

### Record {#field-record}

记录类型

``` csharp
public const TypeCategory Record;
```

### Struct {#field-struct}

结构体

``` csharp
public const TypeCategory Struct;
```

### Unknown {#field-unknown}

``` csharp
public const TypeCategory Unknown;
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
