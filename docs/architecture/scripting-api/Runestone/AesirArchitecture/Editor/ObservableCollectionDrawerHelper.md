---
title: ObservableCollectionDrawerHelper
description: "Runestone.AesirArchitecture.Editor.ObservableCollectionDrawerHelper 的 API 文档"
---

# `ObservableCollectionDrawerHelper`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `ObservableCollectionDrawerHelper`

## 声明

``` csharp
internal static class ObservableCollectionDrawerHelper
```

可观察集合的 Odin 内联调试面板 —— 在 Inspector 中直接显示集合运行状态与元素预览， 其下仍保留默认绘制（元素可正常编辑）。

**备注**

调试信息经反射读取（ObservableCollectionInspectorUtility）。 未安装 Odin Inspector 时本文件整体不参与编译，纯代码 API 不受影响。

覆盖 ObservableList{T}、ObservableDictionary{TKey, TValue}、 ObservableHashSet{T} 三种集合。

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
