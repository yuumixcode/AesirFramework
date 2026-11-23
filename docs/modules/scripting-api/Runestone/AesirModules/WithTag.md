---
title: WithTag
description: "Runestone.AesirModules.WithTag 的 API 文档"
---

# `WithTag`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `WithTag`

**实现接口:** `Runestone.AesirModules.ISubscriberFilter`

## 声明

``` csharp
public sealed class WithTag : Runestone.AesirModules.ISubscriberFilter
```

按 Unity Tag 过滤订阅者。仅 Tag 匹配的订阅者收到事件。
用法：new OnExplosion().WithFilter(new WithTag("Enemy")).Invoke(this)

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`WithTag(string)`](#constructor-withtag-string) | 创建 Tag 过滤器。 |

</div>

### WithTag(string) {#constructor-withtag-string}

创建 Tag 过滤器。

``` csharp
public WithTag(string tag)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `tag` | `string` | 目标 Tag，须已在 TagManager 中定义，否则 CompareTag 会抛异常并被分发隔离捕获。 |

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
