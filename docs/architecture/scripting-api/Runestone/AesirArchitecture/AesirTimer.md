---
title: AesirTimer
description: "Runestone.AesirArchitecture.AesirTimer 的 API 文档"
---

# `AesirTimer`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `Runestone.AesirArchitecture.AbstractTimer` → `AesirTimer`

**实现接口:** `System.IDisposable`

## 声明

``` csharp
public abstract class AesirTimer : Runestone.AesirArchitecture.AbstractTimer, 
System.IDisposable
```

计时器门面：以静态工厂方法创建各类计时器，并把调度引擎的测试 / 调试入口委托给 AbstractTimer。具体形态由 AbstractTimer 的子类实现，本类只负责"选哪一种"。

**备注**

为什么保留本类：0.33.0 起 AesirTimer.Countdown(...) / Every(...) / NextFrame(...) 已是公开入口，调用方常以 AesirTimer 声明字段。把它收敛为抽象门面（工厂 + 委托）后， 所有既有声明与调用零改动，同时把"一个巨类塞满所有形态"拆成 AbstractTimer + 六个职责单一的具体类（OCP / SRP）。

创建即开始；返回的实例持有引用可查询进度 / 取消，也可丢弃引用（到期自动出队，不泄漏）。 各形态的完整语义见对应具体类；时间源、暂停、重开等共有行为见 AbstractTimer。

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

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Countdown(float, Action, bool)`](#method-countdown-float-action-bool) | 创建一个倒计时并在同一次调用内开始计时。 |
| [`Frequency(int, Action, bool)`](#method-frequency-int-action-bool) | 创建一个频率计时器：每秒触发固定次数。 |
| [`Interval(float, float, Action, bool)`](#method-interval-float-float-action-bool) | 创建一个区间计时器：总时长内每隔固定间隔触发一次。 |
| [`NextFrame(Action)`](#method-nextframe-action) | 创建一个"下一帧执行一次"的计时器。 |
| [`Every(float, Action, int, bool)`](#method-every-float-action-int-bool) | 创建一个按固定间隔重复的计时器并在同一次调用内开始计时。 |
| [`Stopwatch(bool)`](#method-stopwatch-bool) | 创建一个从零开始正计时的秒表；不会自动结束，需持有引用读 Elapsed。 |

</div>

**继承的方法**

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
| `Advance(float, float)` | 模板钩子：推进一个结算周期，返回本次是否到期（需要触发一次回调）。 | `AbstractTimer` |
| `Rearm()` | 模板钩子：一次到期回调记账后调用，返回 true 表示重新装填、继续留在注册表，false 表示完成、出队。 | `AbstractTimer` |
| `RescheduleOrFinish()` | 有限次数用尽则标记完成并返回 false，否则返回 true（留在注册表）。 | `AbstractTimer` |
| `SelectDelta(float, float)` | 按本实例的时间源从两个帧增量中挑选正确的一个。 | `AbstractTimer` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `Finish()` | 直接标记完成（不触发回调），用于"总时长结束但不等同一次 interval 触发"等场景。 | `AbstractTimer` |

</div>

### Countdown(float, Action, bool) {#method-countdown-float-action-bool}

创建一个倒计时并在同一次调用内开始计时。

``` csharp
public static CountdownTimer Countdown(float seconds, Action onFinished = null, bool unscaled = false)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `seconds` | `float` | 倒计时时长（秒）。零或负值在下一次结算到期。 |
| `onFinished` | `Action` | 到期回调，可为 null。 |
| `unscaled` | `bool` | true 时不受 Time.timeScale 影响。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `CountdownTimer` | — |

</div>

### Frequency(int, Action, bool) {#method-frequency-int-action-bool}

创建一个频率计时器：每秒触发固定次数。

``` csharp
public static FrequencyTimer Frequency(int ticksPerSecond, Action onTick, bool unscaled = false)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `ticksPerSecond` | `int` | 每秒触发次数，必须大于 0。 |
| `onTick` | `Action` | 每次触发执行的回调。 |
| `unscaled` | `bool` | true 时不受 Time.timeScale 影响。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `FrequencyTimer` | — |

</div>

### Interval(float, float, Action, bool) {#method-interval-float-float-action-bool}

创建一个区间计时器：总时长内每隔固定间隔触发一次。

``` csharp
public static IntervalTimer Interval(float totalSeconds, float intervalSeconds, Action onInterval, bool unscaled = false)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `totalSeconds` | `float` | 总时长（秒），走完即结束。 |
| `intervalSeconds` | `float` | 触发间隔（秒），必须大于 0。 |
| `onInterval` | `Action` | 每次间隔到点时执行的回调。 |
| `unscaled` | `bool` | true 时不受 Time.timeScale 影响。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IntervalTimer` | — |

</div>

### NextFrame(Action) {#method-nextframe-action}

创建一个"下一帧执行一次"的计时器。

``` csharp
public static NextFrameTimer NextFrame(Action onFinished)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `onFinished` | `Action` | 下一帧结算时执行的回调。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `NextFrameTimer` | — |

</div>

### Every(float, Action, int, bool) {#method-every-float-action-int-bool}

创建一个按固定间隔重复的计时器并在同一次调用内开始计时。

``` csharp
public static RepeatingTimer Every(float interval, Action onTick, int repeatCount = -1, bool unscaled = false)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `interval` | `float` | 两次回调之间的间隔（秒）。 |
| `onTick` | `Action` | 每次到期执行的回调。 |
| `repeatCount` | `int` | 最多执行次数，省略或负数表示无限，0 表示一次也不执行。 |
| `unscaled` | `bool` | true 时不受 Time.timeScale 影响。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `RepeatingTimer` | — |

</div>

### Stopwatch(bool) {#method-stopwatch-bool}

创建一个从零开始正计时的秒表；不会自动结束，需持有引用读 Elapsed。

``` csharp
public static StopwatchTimer Stopwatch(bool unscaled = false)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `unscaled` | `bool` | true 时不受 Time.timeScale 影响。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `StopwatchTimer` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
