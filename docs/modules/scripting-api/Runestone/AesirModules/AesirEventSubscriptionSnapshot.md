---
title: AesirEventSubscriptionSnapshot
description: "Runestone.AesirModules.AesirEventSubscriptionSnapshot 的 API 文档"
---

# `AesirEventSubscriptionSnapshot`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `AesirEventSubscriptionSnapshot`

## 声明

``` csharp
[IsReadOnly]
public struct AesirEventSubscriptionSnapshot : System.ValueType
```

单个事件类型的订阅汇总快照（供监视面板展示"某事件有多少订阅者"）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirEventSubscriptionSnapshot(Type, string, int)`](#constructor-aesireventsubscriptionsnapshot-type-string-int) | — |

</div>

### AesirEventSubscriptionSnapshot(Type, string, int) {#constructor-aesireventsubscriptionsnapshot-type-string-int}

``` csharp
public AesirEventSubscriptionSnapshot(Type eventType, string eventName, int subscriberCount)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventType` | `Type` | — |
| `eventName` | `string` | — |
| `subscriberCount` | `int` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`EventType`](#field-eventtype) | — |
| [`SubscriberCount`](#field-subscribercount) | — |
| [`EventName`](#field-eventname) | — |

</div>

### EventType {#field-eventtype}

``` csharp
public readonly Type EventType;
```

### SubscriberCount {#field-subscribercount}

``` csharp
public readonly int SubscriberCount;
```

### EventName {#field-eventname}

``` csharp
public readonly string EventName;
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `ValueType` |
| `GetHashCode()` | — | `ValueType` |
| `ToString()` | — | `ValueType` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
