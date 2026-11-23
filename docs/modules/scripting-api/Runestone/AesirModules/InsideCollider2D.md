---
title: InsideCollider2D
description: "Runestone.AesirModules.InsideCollider2D 的 API 文档"
---

# `InsideCollider2D`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `InsideCollider2D`

**实现接口:** `Runestone.AesirModules.ISubscriberFilter`

## 声明

``` csharp
public sealed class InsideCollider2D : Runestone.AesirModules.ISubscriberFilter
```

仅投递给位于发布者 Collider2D 范围内的订阅者。用于空间局域广播（如爆炸半径）， 订阅者位置取其 Transform.position。发布者挂多个 Collider2D 时取第一个。

**备注**

发布者的 Collider2D 由参数实例按趟缓存（SenderCollider，一趟分发解析一次）， N 个订阅者共享同一次 GetComponent。分发中途增删/替换 Collider 按约定杜绝 （低概率场景不做趟内失效兜底）；每趟分发开始重置缓存，不会跨趟拿到失效引用。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`InsideCollider2D()`](#constructor-insidecollider2d) | — |

</div>

### InsideCollider2D() {#constructor-insidecollider2d}

``` csharp
public InsideCollider2D()
```

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
