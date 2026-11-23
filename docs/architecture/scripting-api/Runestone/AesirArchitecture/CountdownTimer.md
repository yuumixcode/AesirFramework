---
title: CountdownTimer
description: "Runestone.AesirArchitecture.CountdownTimer 的 API 文档"
---

# `CountdownTimer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `Runestone.AesirArchitecture.AbstractTimer` → `Runestone.AesirArchitecture.AesirTimer` → `CountdownTimer`

**实现接口:** `System.IDisposable`

## 声明

``` csharp
public sealed class CountdownTimer : Runestone.AesirArchitecture.AesirTimer, 
System.IDisposable
```

倒计时：从给定时长倒数到零，到期回调一次后结束。对应工厂 Countdown。

**备注**

零或负时长在下一次结算即到期（Progress 到期前为 0、到期后为 1，不产生 NaN）。 到期后可 Restart 重新使用同一个引用。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `IsCancelled` | 是否被 Cancel 或 Dispose 取消且尚未到期。 | `AbstractTimer` |
| `IsFinished` | 是否已正常到期（有限次数用尽，或区间形态总时长结束）。被取消的实例恒为 false。 | `AbstractTimer` |
| `IsPaused` | 是否处于实例级暂停。 | `AbstractTimer` |
| `Duration` | 构造时给定的时长（秒）。"下一帧"与秒表恒为 0；重复形态为间隔，区间形态为总时长。 | `AbstractTimer` |
| `Progress` | 已走完的比例 0 → 1，可直接用于进度条。重复形态为"当前间隔"的进度，区间形态为总时长进度。 | `AbstractTimer` |
| `Remaining` | 剩余秒数。重复形态为距下一次回调的剩余，区间形态为距总时长结束的剩余；到期或取消后保持当时值。 秒表覆写为已走过的秒数。 | `AbstractTimer` |
| `RemainingExecutions` | 还会执行几次回调：无限返回 -1，已到期或已取消返回 0。 | `AbstractTimer` |
| `UsesUnscaled` | 供子类读取的时间源标志。 | `AbstractTimer` |
| `DurationValue` | 供子类读取的构造时长（重复形态为间隔，区间形态为总时长）。 | `AbstractTimer` |
| `RemainingValue` | 供子类读写的倒计时剩余值。 | `AbstractTimer` |
| `CreatedPassId` | 供子类读写的创建周期号基准（"下一帧"形态判定落在哪一次结算）。 | `AbstractTimer` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `Dispose()` | 等同于 Cancel，供 using 与句柄风格调用。 | `AbstractTimer` |
| `Restart()` | 重新开始计时并重新入队：剩余时间复位为 Duration，执行计数与暂停状态一并复位。 | `AbstractTimer` |
| `Cancel()` | 取消计时：移出结算列表、不再触发回调，状态转为 IsCancelled。 | `AbstractTimer` |
| `Pause()` | 挂起计时：保留在注册表中、保留剩余时间与执行计数，但不再推进、不再回调。 | `AbstractTimer` |
| `Resume()` | 从 Pause 恢复：剩余时间从暂停时刻继续。 | `AbstractTimer` |
| `RescheduleOrFinish()` | 有限次数用尽则标记完成并返回 false，否则返回 true（留在注册表）。 | `AbstractTimer` |
| `SelectDelta(float, float)` | 按本实例的时间源从两个帧增量中挑选正确的一个。 | `AbstractTimer` |
| `MemberwiseClone()` | — | `object` |
| `Advance(float, float)` | — | `CountdownTimer` |
| `Rearm()` | — | `CountdownTimer` |
| `Finalize()` | — | `object` |
| `Finish()` | 直接标记完成（不触发回调），用于"总时长结束但不等同一次 interval 触发"等场景。 | `AbstractTimer` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
