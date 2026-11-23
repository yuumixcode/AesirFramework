---
title: AesirTimerStatistics
description: "Runestone.AesirArchitecture.Editor.AesirTimerStatistics 的 API 文档"
---

# `AesirTimerStatistics`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `System.ValueType` → `AesirTimerStatistics`

## 声明

``` csharp
[IsReadOnly]
internal struct AesirTimerStatistics : System.ValueType
```

一次抓取得到的注册表汇总（存活数量 + 累计计数），编辑模式下恒为全零。

**备注**

与逐行快照分开是因为两者的消费者不同：工具条 / 统计行只读本结构， 表格只读行快照；换样式时统计行可以整体重排而无需碰行数据。

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
