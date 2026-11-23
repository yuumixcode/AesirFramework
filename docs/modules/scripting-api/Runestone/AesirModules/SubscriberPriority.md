---
title: SubscriberPriority
description: "Runestone.AesirModules.SubscriberPriority 的 API 文档"
---

# `SubscriberPriority`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `SubscriberPriority`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum SubscriberPriority : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

事件订阅者优先级。4 档排序：First → High → Medium → Last。
Medium 为 Script 订阅（AddListener<T>）默认值；High 需显式指定。 First / Last 用于前置校验与收尾清理——这两档不受 Cancel 取消传播影响。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`First`](#field-first) | 前。比所有默认档位更早执行；不受取消传播影响，用于前置校验。 |
| [`High`](#field-high) | 高优先级。需显式指定。 |
| [`Last`](#field-last) | — |
| [`Medium`](#field-medium) | 中优先级。Script 订阅 AddListener<T> 默认值。 |

</div>

### First {#field-first}

前。比所有默认档位更早执行；不受取消传播影响，用于前置校验。

``` csharp
public const SubscriberPriority First;
```

### High {#field-high}

高优先级。需显式指定。

``` csharp
public const SubscriberPriority High;
```

### Last {#field-last}

``` csharp
public const SubscriberPriority Last;
```

### Medium {#field-medium}

中优先级。Script 订阅 AddListener<T> 默认值。

``` csharp
public const SubscriberPriority Medium;
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
