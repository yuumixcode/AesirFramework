---
title: WithPriority
description: "Runestone.AesirModules.WithPriority 的 API 文档"
---

# `WithPriority`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `WithPriority`

**实现接口:** `Runestone.AesirModules.ISubscriberFilter`

## 声明

``` csharp
public sealed class WithPriority : Runestone.AesirModules.ISubscriberFilter
```

按优先级档位过滤。仅绑定在指定档位的订阅者收到事件， 用于"同一事件类型只通知某一档"的定向分发。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`WithPriority(SubscriberPriority)`](#constructor-withpriority-subscriberpriority) | — |

</div>

### WithPriority(SubscriberPriority) {#constructor-withpriority-subscriberpriority}

``` csharp
public WithPriority(SubscriberPriority priority)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `priority` | `SubscriberPriority` | — |

</div>

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ShouldReceive(AesirEventArgs, object, SubscriberPriority)`](#method-shouldreceive-aesireventargs-object-subscriberpriority) | — |

</div>

**继承的方法**

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

### ShouldReceive(AesirEventArgs, object, SubscriberPriority) {#method-shouldreceive-aesireventargs-object-subscriberpriority}

``` csharp
public bool ShouldReceive(AesirEventArgs eventArgs, object subscriber, SubscriberPriority priority)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventArgs` | `AesirEventArgs` | — |
| `subscriber` | `object` | — |
| `priority` | `SubscriberPriority` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
