---
title: AesirTimerDebugWindowOdin.TimerRow
description: "Runestone.AesirArchitecture.Editor.AesirTimerDebugWindowOdin.TimerRow 的 API 文档"
---

# `AesirTimerDebugWindowOdin.TimerRow`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `AesirTimerDebugWindowOdin.TimerRow`

## 声明

``` csharp
[Serializable]
public sealed class AesirTimerDebugWindowOdin.TimerRow
```

单个计时器的行视图模型：把 AesirTimerSnapshot 按本窗口的列布局摊平。

**备注**

摊平（而不是直接序列化快照）是 Odin 列表的绘制要求：一列一个成员才有列宽。 它属于本窗口的样式——换一套列布局只需换这个视图模型，快照与措辞不动。

每个显示字段都要标 [EnableGUI]：列表本身是只读的，Odin 会把它的子项也当作 不可编辑逐个推入 GUI.enabled = false 绘制（父级 [EnableGUI] 管不到子属性 各自的绘制作用域），于是行文本被渲染成"禁用灰"。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirTimerDebugWindowOdin.TimerRow()`](#constructor-aesirtimerdebugwindowodin-timerrow) | 供 Odin 反序列化使用的空构造。 |

</div>

### AesirTimerDebugWindowOdin.TimerRow() {#constructor-aesirtimerdebugwindowodin-timerrow}

供 Odin 反序列化使用的空构造。

``` csharp
public AesirTimerDebugWindowOdin.TimerRow()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ProgressPercent`](#field-progresspercent) | 当前间隔的进度（0–100，进度条）。 |
| [`Callback`](#field-callback) | 回调归属（类型.方法）。 |
| [`Shape`](#field-shape) | 形态与时间源，如 "重复 ×3 · unscaled"。 |
| [`State`](#field-state) | 状态词（运行中 / 已暂停）。 |
| [`Timing`](#field-timing) | 剩余 / 已执行 / 还剩 / 存活 的合并文本。 |

</div>

### ProgressPercent {#field-progresspercent}

当前间隔的进度（0–100，进度条）。

``` csharp
[HorizontalGroup]
[EnableGUI]
[ProgressBar]
[HideLabel]
public float ProgressPercent;
```

### Callback {#field-callback}

回调归属（类型.方法）。

``` csharp
[EnableGUI]
[DisplayAsString]
[HideLabel]
public string Callback;
```

### Shape {#field-shape}

形态与时间源，如 "重复 ×3 · unscaled"。

``` csharp
[HorizontalGroup]
[EnableGUI]
[DisplayAsString]
[HideLabel]
public string Shape;
```

### State {#field-state}

状态词（运行中 / 已暂停）。

``` csharp
[HorizontalGroup]
[EnableGUI]
[DisplayAsString]
[HideLabel]
public string State;
```

### Timing {#field-timing}

剩余 / 已执行 / 还剩 / 存活 的合并文本。

``` csharp
[HorizontalGroup]
[EnableGUI]
[DisplayAsString]
[HideLabel]
public string Timing;
```

## 方法

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

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
