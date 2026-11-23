---
title: AesirPlayerLoop.HookEntry
description: "Runestone.AesirArchitecture.AesirPlayerLoop.HookEntry 的 API 文档"
---

# `AesirPlayerLoop.HookEntry`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `AesirPlayerLoop.HookEntry`

## 声明

``` csharp
private struct AesirPlayerLoop.HookEntry : System.ValueType
```

回调条目，记录单个生命周期回调及其排序信息

**备注**

InsertionIndex 是一个自增的序号，在每次 AddHook 时分配。 当多个条目的 Order 相同时，使用 InsertionIndex 作为次级排序键， 确保相同优先级的回调按注册先后顺序执行，实现稳定排序。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Callback`](#field-callback) | — |
| [`Order`](#field-order) | — |
| [`InsertionIndex`](#field-insertionindex) | — |

</div>

### Callback {#field-callback}

``` csharp
public Action Callback;
```

### Order {#field-order}

``` csharp
public int Order;
```

### InsertionIndex {#field-insertionindex}

``` csharp
public long InsertionIndex;
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
