---
title: AesirSubscriberSnapshot
description: "Runestone.AesirModules.AesirSubscriberSnapshot 的 API 文档"
---

# `AesirSubscriberSnapshot`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `AesirSubscriberSnapshot`

## 声明

``` csharp
[IsReadOnly]
public struct AesirSubscriberSnapshot : System.ValueType
```

单条订阅绑定快照（供监视面板展开订阅者列表）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirSubscriberSnapshot(object, string, string, SubscriberPriority, bool)`](#constructor-aesirsubscribersnapshot-object-string-string-subscriberpriority-bool) | — |

</div>

### AesirSubscriberSnapshot(object, string, string, SubscriberPriority, bool) {#constructor-aesirsubscribersnapshot-object-string-string-subscriberpriority-bool}

``` csharp
public AesirSubscriberSnapshot(object subscriber, string subscriberName, string memberName, SubscriberPriority priority, bool isAlive)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `subscriber` | `object` | — |
| `subscriberName` | `string` | — |
| `memberName` | `string` | — |
| `priority` | `SubscriberPriority` | — |
| `isAlive` | `bool` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Priority`](#field-priority) | — |
| [`IsAlive`](#field-isalive) | — |
| [`Subscriber`](#field-subscriber) | — |
| [`MemberName`](#field-membername) | — |
| [`SubscriberName`](#field-subscribername) | — |

</div>

### Priority {#field-priority}

``` csharp
public readonly SubscriberPriority Priority;
```

### IsAlive {#field-isalive}

``` csharp
public readonly bool IsAlive;
```

### Subscriber {#field-subscriber}

``` csharp
public readonly object Subscriber;
```

### MemberName {#field-membername}

``` csharp
public readonly string MemberName;
```

### SubscriberName {#field-subscribername}

``` csharp
public readonly string SubscriberName;
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
