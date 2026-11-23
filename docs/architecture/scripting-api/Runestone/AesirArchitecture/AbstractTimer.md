---
title: AbstractTimer
description: "Runestone.AesirArchitecture.AbstractTimer 的 API 文档"
---

# `AbstractTimer`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `AbstractTimer`

**实现接口:** `System.IDisposable`

## 声明

``` csharp
public abstract class AbstractTimer : System.IDisposable
```

计时器抽象基类：承载框架级调度引擎（注册表、PlayerLoop 注入、两阶段结算与累计统计） 与所有计时器共有的状态、查询、生命周期控制；具体计时形态由子类经 Advance 与 Rearm 两个模板钩子定义。

**备注**

为什么拆出本类：早期单一 AesirTimer 用一个类同时表达倒计时、重复、下一帧三种形态， 形态判断（_nextFrameMode / _totalTicks）与调度引擎、状态查询挤在一起，新增形态只能继续 往同一个类里加分支。本类用模板方法 把"何时推进、何时到期"的引擎固定下来，把"怎么推进"下沉给子类（OCP 对扩展开放）。 设计思路参考了开源项目 adammyhre/Unity-Improved-Timers 的抽象基类 + 具体 Timer 分层，但结算引擎与语义仍是本框架自有实现。

引用类型：框架需要"替你推进"并且"你随时能查"，可变结构体在这里会失效——注册表推进的是副本， 调用方读到的仍是旧值；引用类型让注册表与调用方操作同一个实例。

暂停的两层语义：全局暂停由 Time.timeScale = 0 表达（unscaled 实例不受影响）； Pause / Resume 是实例级暂停。两者可叠加，暂停期间 Remaining 与 Progress 一并冻结。

注入方式：经 AesirPlayerLoop 的 BeforeUpdate 钩子推进，order 取 MinValue——计时结算先于任何用户钩子，也复用其 EnsureInjected 自愈能力。

症状与代价：帧粒度结算，误差在 ±1 帧内。每个计时器一次堆分配（Gen0，生命周期短）， 框架不做对象池。单次到期形态卡顿帧只回调一次（不补发），避免"越慢越忙"的雪崩。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IsCancelled`](#property-iscancelled) | 是否被 Cancel 或 Dispose 取消且尚未到期。 |
| [`IsFinished`](#property-isfinished) | 是否已正常到期（有限次数用尽，或区间形态总时长结束）。被取消的实例恒为 false。 |
| [`IsPaused`](#property-ispaused) | 是否处于实例级暂停。 |
| [`Duration`](#property-duration) | 构造时给定的时长（秒）。"下一帧"与秒表恒为 0；重复形态为间隔，区间形态为总时长。 |
| [`Progress`](#property-progress) | 已走完的比例 0 → 1，可直接用于进度条。重复形态为"当前间隔"的进度，区间形态为总时长进度。 |
| [`Remaining`](#property-remaining) | 剩余秒数。重复形态为距下一次回调的剩余，区间形态为距总时长结束的剩余；到期或取消后保持当时值。 秒表覆写为已走过的秒数。 |
| [`RemainingExecutions`](#property-remainingexecutions) | 还会执行几次回调：无限返回 -1，已到期或已取消返回 0。 |
| [`UsesUnscaled`](#property-usesunscaled) | 供子类读取的时间源标志。 |
| [`DurationValue`](#property-durationvalue) | 供子类读取的构造时长（重复形态为间隔，区间形态为总时长）。 |
| [`RemainingValue`](#property-remainingvalue) | 供子类读写的倒计时剩余值。 |
| [`CreatedPassId`](#property-createdpassid) | 供子类读写的创建周期号基准（"下一帧"形态判定落在哪一次结算）。 |
| [`CurrentPassId`](#property-currentpassid) | 供子类读取的当前结算周期号（"下一帧"形态判定用）。 |

</div>

### IsCancelled {#property-iscancelled}

是否被 Cancel 或 Dispose 取消且尚未到期。

**备注**

已到期的计时器调用 Cancel 不改变任何状态。

``` csharp
public bool IsCancelled { get; }
```

### IsFinished {#property-isfinished}

是否已正常到期（有限次数用尽，或区间形态总时长结束）。被取消的实例恒为 false。

``` csharp
public bool IsFinished { get; }
```

### IsPaused {#property-ispaused}

是否处于实例级暂停。

``` csharp
public bool IsPaused { get; }
```

### Duration {#property-duration}

构造时给定的时长（秒）。"下一帧"与秒表恒为 0；重复形态为间隔，区间形态为总时长。

``` csharp
public float Duration { get; }
```

### Progress {#property-progress}

已走完的比例 0 → 1，可直接用于进度条。重复形态为"当前间隔"的进度，区间形态为总时长进度。

**备注**

正常到期返回 1；Duration 为 0 的实例在到期前返回 0（避免 0/0 产生 NaN）； 被暂停 / 取消时冻结在当时的值。秒表覆写为 0（无总时长概念）。

``` csharp
public float Progress { get; }
```

### Remaining {#property-remaining}

剩余秒数。重复形态为距下一次回调的剩余，区间形态为距总时长结束的剩余；到期或取消后保持当时值。 秒表覆写为已走过的秒数。

``` csharp
public float Remaining { get; }
```

### RemainingExecutions {#property-remainingexecutions}

还会执行几次回调：无限返回 -1，已到期或已取消返回 0。

**备注**

该值表示"从此刻起还会回调几次"，不含已经执行的次数。秒表与区间形态按需覆写。

``` csharp
public int RemainingExecutions { get; }
```

### UsesUnscaled {#property-usesunscaled}

供子类读取的时间源标志。

``` csharp
protected bool UsesUnscaled { protected get; }
```

### DurationValue {#property-durationvalue}

供子类读取的构造时长（重复形态为间隔，区间形态为总时长）。

``` csharp
protected float DurationValue { protected get; }
```

### RemainingValue {#property-remainingvalue}

供子类读写的倒计时剩余值。

``` csharp
protected float RemainingValue { protected get; protected set; }
```

### CreatedPassId {#property-createdpassid}

供子类读写的创建周期号基准（"下一帧"形态判定落在哪一次结算）。

``` csharp
protected int CreatedPassId { protected get; protected set; }
```

### CurrentPassId {#property-currentpassid}

供子类读取的当前结算周期号（"下一帧"形态判定用）。

``` csharp
protected static int CurrentPassId { protected get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Restart()`](#method-restart) | 重新开始计时并重新入队：剩余时间复位为 Duration，执行计数与暂停状态一并复位。 |
| [`Cancel()`](#method-cancel) | 取消计时：移出结算列表、不再触发回调，状态转为 IsCancelled。 |
| [`Dispose()`](#method-dispose) | 等同于 Cancel，供 using 与句柄风格调用。 |
| [`Pause()`](#method-pause) | 挂起计时：保留在注册表中、保留剩余时间与执行计数，但不再推进、不再回调。 |
| [`Resume()`](#method-resume) | 从 Pause 恢复：剩余时间从暂停时刻继续。 |
| [`Advance(float, float)`](#method-advance-float-float) | 模板钩子：推进一个结算周期，返回本次是否到期（需要触发一次回调）。 |
| [`Rearm()`](#method-rearm) | 模板钩子：一次到期回调记账后调用，返回 true 表示重新装填、继续留在注册表，false 表示完成、出队。 |
| [`RescheduleOrFinish()`](#method-rescheduleorfinish) | 有限次数用尽则标记完成并返回 false，否则返回 true（留在注册表）。 |
| [`SelectDelta(float, float)`](#method-selectdelta-float-float) | 按本实例的时间源从两个帧增量中挑选正确的一个。 |
| [`Finish()`](#method-finish) | 直接标记完成（不触发回调），用于"总时长结束但不等同一次 interval 触发"等场景。 |

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

### Restart() {#method-restart}

重新开始计时并重新入队：剩余时间复位为 Duration，执行计数与暂停状态一并复位。

**备注**

子类覆写时应先调用 base.Restart() 再复位各自的额外状态（如区间的下一触发点、秒表的累计值）。 已入队时不会重复注册；暂停中的实例经本方法恢复为运行态；totalTicks 为 0 的实例保持结束、不入队。

``` csharp
public virtual void Restart()
```

### Cancel() {#method-cancel}

取消计时：移出结算列表、不再触发回调，状态转为 IsCancelled。

``` csharp
public void Cancel()
```

### Dispose() {#method-dispose}

等同于 Cancel，供 using 与句柄风格调用。

``` csharp
public void Dispose()
```

### Pause() {#method-pause}

挂起计时：保留在注册表中、保留剩余时间与执行计数，但不再推进、不再回调。

``` csharp
public void Pause()
```

### Resume() {#method-resume}

从 Pause 恢复：剩余时间从暂停时刻继续。

**备注**

同时把周期号基准对齐到当前值——"下一帧"形态恢复后要等恢复后的第一次结算才执行， 不会因暂停期间周期号前进而"恢复即到期"；其余形态不读该字段，对齐无副作用。

``` csharp
public void Resume()
```

### Advance(float, float) {#method-advance-float-float}

模板钩子：推进一个结算周期，返回本次是否到期（需要触发一次回调）。

**备注**

子类在此改写 RemainingValue 等自有状态；需要"直接结束但不触发回调"时调用 Finish。返回值只决定是否触发回调，是否留在注册表由 Rearm 决定。

``` csharp
protected abstract bool Advance(float deltaTime, float unscaledDeltaTime)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `deltaTime` | `float` | 受 Time.timeScale 影响的帧增量。 |
| `unscaledDeltaTime` | `float` | 不受 Time.timeScale 影响的帧增量。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### Rearm() {#method-rearm}

模板钩子：一次到期回调记账后调用，返回 true 表示重新装填、继续留在注册表，false 表示完成、出队。

**备注**

有限次数形态可直接返回 RescheduleOrFinish；无限形态恒返回 true； 需要自定义装填（如区间形态重置下一触发点）的子类在此处理后再决定是否继续。

``` csharp
protected abstract bool Rearm()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### RescheduleOrFinish() {#method-rescheduleorfinish}

有限次数用尽则标记完成并返回 false，否则返回 true（留在注册表）。

``` csharp
protected bool RescheduleOrFinish()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### SelectDelta(float, float) {#method-selectdelta-float-float}

按本实例的时间源从两个帧增量中挑选正确的一个。

``` csharp
protected float SelectDelta(float deltaTime, float unscaledDeltaTime)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `deltaTime` | `float` | — |
| `unscaledDeltaTime` | `float` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `float` | — |

</div>

### Finish() {#method-finish}

直接标记完成（不触发回调），用于"总时长结束但不等同一次 interval 触发"等场景。

``` csharp
protected void Finish()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
