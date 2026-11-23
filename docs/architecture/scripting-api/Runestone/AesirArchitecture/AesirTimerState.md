---
title: AesirTimerState
description: "Runestone.AesirArchitecture.AesirTimerState 的 API 文档"
---

# `AesirTimerState`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `AesirTimerState`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
internal enum AesirTimerState : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

计时器的可观察状态，供编辑器调试窗口与检视面板分类展示。

**备注**

顺序即"关注优先级"：调试面板按此排序，异常态（Uninitialized）排在最后。 运行时逻辑不依赖本枚举，结算只读写 AesirTimer 内部的布尔标志。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Cancelled`](#field-cancelled) | 已被取消（与 Finished 互斥）。 |
| [`Finished`](#field-finished) | 已正常到期（有限次执行已用尽）。 |
| [`Paused`](#field-paused) | 已暂停：仍在注册表中，但结算跳过时间累减与执行。 |
| [`Running`](#field-running) | 运行中：已入队且未被暂停。 |
| [`Uninitialized`](#field-uninitialized) | — |

</div>

### Cancelled {#field-cancelled}

已被取消（与 Finished 互斥）。

``` csharp
public const AesirTimerState Cancelled;
```

### Finished {#field-finished}

已正常到期（有限次执行已用尽）。

``` csharp
public const AesirTimerState Finished;
```

### Paused {#field-paused}

已暂停：仍在注册表中，但结算跳过时间累减与执行。

``` csharp
public const AesirTimerState Paused;
```

### Running {#field-running}

运行中：已入队且未被暂停。

``` csharp
public const AesirTimerState Running;
```

### Uninitialized {#field-uninitialized}

``` csharp
public const AesirTimerState Uninitialized;
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
