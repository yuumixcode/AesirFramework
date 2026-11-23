---
title: MonoLifecycleProxy.PendingChange
description: "Runestone.AesirArchitecture.MonoLifecycleProxy.PendingChange 的 API 文档"
---

# `MonoLifecycleProxy.PendingChange`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `MonoLifecycleProxy.PendingChange`

## 声明

``` csharp
private struct MonoLifecycleProxy.PendingChange : System.ValueType
```

挂起变更条目，记录调用期间累积的一次监听增删操作

**备注**

快照语义的实现载体：变更按发生顺序暂存于挂起队列，本趟遍历结束后统一应用。 先添加后移除同一回调可在按序应用中正确抵消。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Callback`](#field-callback) | — |
| [`Event`](#field-event) | — |
| [`Entry`](#field-entry) | — |
| [`IsAdd`](#field-isadd) | — |

</div>

### Callback {#field-callback}

``` csharp
public Action Callback;
```

### Event {#field-event}

``` csharp
public MonoLifecycleEvent Event;
```

### Entry {#field-entry}

``` csharp
public MonoLifecycleProxy.ListenerEntry Entry;
```

### IsAdd {#field-isadd}

``` csharp
public bool IsAdd;
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
