---
title: IntervalTimer
description: "Runestone.AesirArchitecture.IntervalTimer 的 API 文档"
---

# `IntervalTimer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `Runestone.AesirArchitecture.AbstractTimer` → `Runestone.AesirArchitecture.AesirTimer` → `IntervalTimer`

**实现接口:** `System.IDisposable`

## 声明

``` csharp
public sealed class IntervalTimer : Runestone.AesirArchitecture.AesirTimer, 
System.IDisposable
```

区间计时器：在一段总时长内每隔固定间隔触发一次事件，总时长走完即结束。

**备注**

与 RepeatingTimer 的区别：RepeatingTimer 以"次数"为终点（重复 N 次或无限）， 本类以"总时长"为终点（如 10 秒内每 1 秒触发一次）。两者都遵循"每结算最多一次回调、卡顿不补发"。

首个事件在第 interval 秒触发，而非创建当帧；若 interval 不能整除总时长， 最后一个完整间隔之后的余量不再触发事件，到点直接结束。

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`RemainingExecutions`](#property-remainingexecutions) | 还会触发几次间隔事件（0 表示间隔非法或已结束，未结束时为正数）。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `IsCancelled` | 是否被 Cancel 或 Dispose 取消且尚未到期。 | `AbstractTimer` |
| `IsFinished` | 是否已正常到期（有限次数用尽，或区间形态总时长结束）。被取消的实例恒为 false。 | `AbstractTimer` |
| `IsPaused` | 是否处于实例级暂停。 | `AbstractTimer` |
| `Duration` | 构造时给定的时长（秒）。"下一帧"与秒表恒为 0；重复形态为间隔，区间形态为总时长。 | `AbstractTimer` |
| `Progress` | 已走完的比例 0 → 1，可直接用于进度条。重复形态为"当前间隔"的进度，区间形态为总时长进度。 | `AbstractTimer` |
| `Remaining` | 剩余秒数。重复形态为距下一次回调的剩余，区间形态为距总时长结束的剩余；到期或取消后保持当时值。 秒表覆写为已走过的秒数。 | `AbstractTimer` |
| `UsesUnscaled` | 供子类读取的时间源标志。 | `AbstractTimer` |
| `DurationValue` | 供子类读取的构造时长（重复形态为间隔，区间形态为总时长）。 | `AbstractTimer` |
| `RemainingValue` | 供子类读写的倒计时剩余值。 | `AbstractTimer` |
| `CreatedPassId` | 供子类读写的创建周期号基准（"下一帧"形态判定落在哪一次结算）。 | `AbstractTimer` |

</div>

### RemainingExecutions {#property-remainingexecutions}

还会触发几次间隔事件（0 表示间隔非法或已结束，未结束时为正数）。

``` csharp
public int RemainingExecutions { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Restart()` | 重开同时复位下一个间隔触发点。 | `IntervalTimer` |
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
| `Advance(float, float)` | — | `IntervalTimer` |
| `Rearm()` | 总时长耗尽则结束（最后一次间隔回调恰与到点同帧时在此收尾），否则继续驻留。 | `IntervalTimer` |
| `Finalize()` | — | `object` |
| `Finish()` | 直接标记完成（不触发回调），用于"总时长结束但不等同一次 interval 触发"等场景。 | `AbstractTimer` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
