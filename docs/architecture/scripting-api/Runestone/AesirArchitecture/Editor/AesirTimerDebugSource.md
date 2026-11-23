---
title: AesirTimerDebugSource
description: "Runestone.AesirArchitecture.Editor.AesirTimerDebugSource 的 API 文档"
---

# `AesirTimerDebugSource`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirTimerDebugSource`

## 声明

``` csharp
internal sealed class AesirTimerDebugSource
```

计时器面板的数据源：按设定节奏把注册表抓成一批 AesirTimerSnapshot 并汇总统计。

**备注**

只管抓，不管怎么显示：本类不产出任何文案、颜色或控件，视图（IMGUI 窗口 / Odin 窗口 / 字段绘制器 / 示例 Inspector）从 Rows 与 Statistics 自行取数。 因此"换一套显示样式"只需写新的视图，本类原样复用。

编辑模式下没有数据源：计时器只在 Play 模式下被 PlayerLoop 推进，退出 Play 后 累计计数可能残留在静态字段里（取决于是否发生域重载）。因此 Refresh 在 非 Play 模式直接返回全零、不触碰注册表；Tick 在 Play 状态切换的那一刻 立即重抓一次，让面板在进入 / 退出 Play 的同一帧就换成新口径，而不是等下一个拉取间隔。

限流的边界在"拉取"这一侧：Tick 只在到达 AesirTimerDebuggerSettings 设定的时间 / 帧间隔时才 Refresh， 其余时刻视图只重绘缓存行。因此打开面板的开销与计时器数量、面板重绘频率都无关， 只与拉取频率相关（默认 0.5 秒一次）。

排序与截断：已暂停排后面，其余按剩余时间升序（NextFrame 剩余恒为 0， 因此"最紧迫"的排在最上面）。超过 MaxRows 时只保留前 MaxRows 行并在统计中标出总数——上千个计时器时窗口不至于卡死， 也不会掩盖真实规模。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirTimerDebugSource()`](#constructor-aesirtimerdebugsource) | — |

</div>

### AesirTimerDebugSource() {#constructor-aesirtimerdebugsource}

``` csharp
public AesirTimerDebugSource()
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
