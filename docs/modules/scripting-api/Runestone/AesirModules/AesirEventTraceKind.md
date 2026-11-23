---
title: AesirEventTraceKind
description: "Runestone.AesirModules.AesirEventTraceKind 的 API 文档"
---

# `AesirEventTraceKind`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `AesirEventTraceKind`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum AesirEventTraceKind : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

事件链路快照的种类。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Raised`](#field-raised) | 事件被触发并完成分发。 |
| [`SubscriberBound`](#field-subscriberbound) | 订阅者被绑定。 |
| [`SubscriberUnbound`](#field-subscriberunbound) | — |

</div>

### Raised {#field-raised}

事件被触发并完成分发。

``` csharp
public const AesirEventTraceKind Raised;
```

### SubscriberBound {#field-subscriberbound}

订阅者被绑定。

``` csharp
public const AesirEventTraceKind SubscriberBound;
```

### SubscriberUnbound {#field-subscriberunbound}

``` csharp
public const AesirEventTraceKind SubscriberUnbound;
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
