---
title: StopwatchTimer
description: "Runestone.AesirArchitecture.StopwatchTimer 的 API 文档"
---

# `StopwatchTimer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `Runestone.AesirArchitecture.AbstractTimer` → `Runestone.AesirArchitecture.AesirTimer` → `StopwatchTimer`

**实现接口:** `System.IDisposable`

## 声明

``` csharp
public sealed class StopwatchTimer : Runestone.AesirArchitecture.AesirTimer, 
System.IDisposable
```

秒表：从零开始正计时，用于测量一段时长；不会自动结束，需持有引用读 Elapsed 或自行取消。

**备注**

与倒计时的根本区别是"没有终点"——因此 IsFinished 在取消前恒为 false， Progress 无意义（恒 0），真正的读数是 Elapsed。

设计取舍：原单一 AesirTimer 有意不做秒表（"永不结束 = 泄漏源"）。独立成类型后语义被约束—— 调用方必须持有引用并在合适时机 Cancel / Dispose； 只是想读当前时间戳的场合仍应直接用 Time.time，不必创建实例。

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Elapsed`](#property-elapsed) | 自创建（或最近一次 Restart）起累计的秒数。 |
| [`Progress`](#property-progress) | 秒表没有"已走完比例"，恒为 0。 |
| [`Remaining`](#property-remaining) | 秒表没有终点，剩余时间展示为已走过的秒数。 |
| [`RemainingExecutions`](#property-remainingexecutions) | 秒表不触发到期回调。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `IsCancelled` | 是否被 Cancel 或 Dispose 取消且尚未到期。 | `AbstractTimer` |
| `IsFinished` | 是否已正常到期（有限次数用尽，或区间形态总时长结束）。被取消的实例恒为 false。 | `AbstractTimer` |
| `IsPaused` | 是否处于实例级暂停。 | `AbstractTimer` |
| `Duration` | 构造时给定的时长（秒）。"下一帧"与秒表恒为 0；重复形态为间隔，区间形态为总时长。 | `AbstractTimer` |
| `UsesUnscaled` | 供子类读取的时间源标志。 | `AbstractTimer` |
| `DurationValue` | 供子类读取的构造时长（重复形态为间隔，区间形态为总时长）。 | `AbstractTimer` |
| `RemainingValue` | 供子类读写的倒计时剩余值。 | `AbstractTimer` |
| `CreatedPassId` | 供子类读写的创建周期号基准（"下一帧"形态判定落在哪一次结算）。 | `AbstractTimer` |

</div>

### Elapsed {#property-elapsed}

自创建（或最近一次 Restart）起累计的秒数。

``` csharp
public float Elapsed { get; }
```

### Progress {#property-progress}

秒表没有"已走完比例"，恒为 0。

``` csharp
public float Progress { get; }
```

### Remaining {#property-remaining}

秒表没有终点，剩余时间展示为已走过的秒数。

``` csharp
public float Remaining { get; }
```

### RemainingExecutions {#property-remainingexecutions}

秒表不触发到期回调。

``` csharp
public int RemainingExecutions { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Restart()` | 重开同时清零累计值。 | `StopwatchTimer` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `Dispose()` | 等同于 Cancel，供 using 与句柄风格调用。 | `AbstractTimer` |
| `Cancel()` | 取消计时：移出结算列表、不再触发回调，状态转为 IsCancelled。 | `AbstractTimer` |
| `Pause()` | 挂起计时：保留在注册表中、保留剩余时间与执行计数，但不再推进、不再回调。 | `AbstractTimer` |
| `Resume()` | 从 Pause 恢复：剩余时间从暂停时刻继续。 | `AbstractTimer` |
| `RescheduleOrFinish()` | 有限次数用尽则标记完成并返回 false，否则返回 true（留在注册表）。 | `AbstractTimer` |
| `SelectDelta(float, float)` | 按本实例的时间源从两个帧增量中挑选正确的一个。 | `AbstractTimer` |
| `MemberwiseClone()` | — | `object` |
| `Advance(float, float)` | 只累计时间、永不到期；取消由调用方显式发起。 | `StopwatchTimer` |
| `Rearm()` | — | `StopwatchTimer` |
| `Finalize()` | — | `object` |
| `Finish()` | 直接标记完成（不触发回调），用于"总时长结束但不等同一次 interval 触发"等场景。 | `AbstractTimer` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
