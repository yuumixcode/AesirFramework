---
title: AesirTimerText
description: "Runestone.AesirArchitecture.Editor.AesirTimerText 的 API 文档"
---

# `AesirTimerText`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirTimerText`

## 声明

``` csharp
internal static class AesirTimerText
```

计时器面板的默认文案与颜色样式：把 AesirTimerSnapshot 翻译成中文显示文本。

**备注**

它是"样式"，不是"数据"：输入只有抓取结果，输出只有字符串与颜色，不读 AesirTimer、不碰注册表、不画控件。因此换一套文案风格（英文 / 表格口径 / 省略单位）＝另写一个同签证的样式类，数据源（AesirTimerDebugSource）与 抓取结构完全不动。

调试窗口（Odin 版与 IMGUI 版）、字段绘制器、示例 Inspector 共用这一份措辞， 避免同一状态在多个面板里说法不同。

是否刷新由调用方决定（窗口按 AesirTimerDebuggerSettings 的节奏， 绘制器随重绘），本类不做任何调度。

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
