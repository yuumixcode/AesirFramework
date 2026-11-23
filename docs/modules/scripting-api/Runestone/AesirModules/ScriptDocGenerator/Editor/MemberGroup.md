---
title: MemberGroup
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.MemberGroup 的 API 文档"
---

# `MemberGroup`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `MemberGroup`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
internal enum MemberGroup : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

成员分组（文档生成共享）：常量 → 声明 → 继承 → 运算符。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Constant`](#field-constant) | — |
| [`Declared`](#field-declared) | — |
| [`Inherited`](#field-inherited) | — |
| [`None`](#field-none) | — |
| [`Operator`](#field-operator) | — |

</div>

### Constant {#field-constant}

``` csharp
public const MemberGroup Constant;
```

### Declared {#field-declared}

``` csharp
public const MemberGroup Declared;
```

### Inherited {#field-inherited}

``` csharp
public const MemberGroup Inherited;
```

### None {#field-none}

``` csharp
public const MemberGroup None;
```

### Operator {#field-operator}

``` csharp
public const MemberGroup Operator;
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
