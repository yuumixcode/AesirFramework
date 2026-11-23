---
title: AesirTimerRefreshMode
description: "Runestone.AesirArchitecture.Editor.AesirTimerRefreshMode 的 API 文档"
---

# `AesirTimerRefreshMode`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `AesirTimerRefreshMode`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
internal enum AesirTimerRefreshMode : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

计时器调试面板的拉取节奏（按时间 / 按帧两种模式）。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Frames`](#field-frames) | 按编辑器 update 计数拉取（≈ 帧；适合跟随帧节奏观察）。 |
| [`Interval`](#field-interval) | 按真实时间间隔拉取（与帧率无关，编辑模式与 Play 模式一致）。 |

</div>

### Frames {#field-frames}

按编辑器 update 计数拉取（≈ 帧；适合跟随帧节奏观察）。

``` csharp
public const AesirTimerRefreshMode Frames;
```

### Interval {#field-interval}

按真实时间间隔拉取（与帧率无关，编辑模式与 Play 模式一致）。

``` csharp
public const AesirTimerRefreshMode Interval;
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
