---
title: ObservableCollectionView
description: "Runestone.AesirArchitecture.Editor.ObservableCollectionView 的 API 文档"
---

# `ObservableCollectionView`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `ObservableCollectionView`

## 声明

``` csharp
internal static class ObservableCollectionView
```

可观察集合的 UI Toolkit 内联摘要（无 Odin 依赖）：复刻 Odin 版 ObservableCollectionDrawer 的 "元素数 | 变更监听 + 元素预览" 调试信息，其下保留默认子字段绘制。

**备注**

调试信息经反射读取（ObservableCollectionInspectorUtility，已迁到核心程序集，零 Odin 依赖）。 覆盖 ObservableList{T}、ObservableDictionary{TKey,TValue}、 ObservableHashSet{T} 三种集合，各用一个开放泛型 CustomPropertyDrawer。

生效前提：字段须是 Unity 可序列化属性（[SerializeReference] 或 Odin 序列化后端）。 普通泛型字段 Unity 原生不绘制，两个 Inspector 都不会显示。

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
