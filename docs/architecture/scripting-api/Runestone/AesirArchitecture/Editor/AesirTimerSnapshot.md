---
title: AesirTimerSnapshot
description: "Runestone.AesirArchitecture.Editor.AesirTimerSnapshot 的 API 文档"
---

# `AesirTimerSnapshot`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `System.ValueType` → `AesirTimerSnapshot`

## 声明

``` csharp
[IsReadOnly]
internal struct AesirTimerSnapshot : System.ValueType
```

单个计时器的一次只读抓取：只搬数据，不含任何显示文案、颜色或控件。

**备注**

它是"数据源"与"显示样式"之间的唯一契约。数据源（AesirTimerDebugSource） 只负责按节奏把注册表抓成一批本结构；视图（调试窗口、字段绘制器、示例 Inspector）各自决定 怎么展示。换一套面板样式时只写新的视图，抓取逻辑不动；新增一个展示字段时只在这里抓一次， 所有视图立刻可用。

回调归属为什么在抓取侧算：Lambda 折叠要读 MethodInfo 并向上找声明类型， 而抓取默认 0.5 秒一次、重绘每帧多次——放在这里重绘才能保持零反射。折叠只做 "把编译器生成的 <Outer>b__0_0 换回责任人"的归一化，不属于措辞，故不归样式层。

default 表示"字段没有持有实例"，与"实例存在但从未被真正构造" （Uninitialized）是两件事，用 HasTimer 区分。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `ValueType` |
| `GetHashCode()` | — | `ValueType` |
| `ToString()` | — | `ValueType` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
