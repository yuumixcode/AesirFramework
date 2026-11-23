---
title: AesirTimerSummaryView
description: "Runestone.AesirArchitecture.Editor.AesirTimerSummaryView 的 API 文档"
---

# `AesirTimerSummaryView`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirTimerSummaryView`

## 声明

``` csharp
internal static class AesirTimerSummaryView
```

单行摘要的 IMGUI 样式：进度条 + 一行状态描述，供两个字段绘制器（原生 / Odin）共用。

**备注**

绘制与文案分开放：本类只决定"怎么摆"（进度条宽度占比、行内间距），措辞与颜色全部来自 AesirTimerText。要换一种字段外观（例如只画进度条、或改成两行）时替换本类， 数据（AesirTimerSnapshot）与文案都不用动。

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
