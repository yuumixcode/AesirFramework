---
title: ObservableListDrawer<T>
description: "Runestone.AesirArchitecture.Editor.ObservableListDrawer<T> 的 API 文档"
---

# `ObservableListDrawer<T>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinDrawer` → `Sirenix.OdinInspector.Editor.OdinValueDrawer<ObservableList<T>>` → `ObservableListDrawer<T>`

## 声明

``` csharp
[DrawerPriority]
internal sealed class ObservableListDrawer<T> : Sirenix.OdinInspector.Editor.OdinValueDrawer<ObservableList<T>> 
```

可观察列表的内联调试面板。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ObservableListDrawer()`](#constructor-observablelistdrawer) | — |

</div>

### ObservableListDrawer() {#constructor-observablelistdrawer}

``` csharp
public ObservableListDrawer<T>()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `ValueEntry` | — | `OdinValueDrawer<ObservableList<T>>` |
| `Property` | — | `OdinDrawer` |
| `Initialized` | — | `OdinDrawer` |
| `SkipWhenDrawing` | — | `OdinDrawer` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `CanDrawProperty(InspectorProperty)` | — | `OdinValueDrawer<ObservableList<T>>` |
| `CanDrawTypeFilter(Type)` | — | `OdinDrawer` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `DrawProperty(GUIContent)` | — | `OdinDrawer` |
| `Initialize(InspectorProperty)` | — | `OdinDrawer` |
| `CallNextDrawer(GUIContent)` | — | `OdinDrawer` |
| `MemberwiseClone()` | — | `object` |
| `DrawPropertyLayout(GUIContent)` | — | `ObservableListDrawer<T>` |
| `CanDrawValueProperty(InspectorProperty)` | — | `OdinValueDrawer<ObservableList<T>>` |
| `Finalize()` | — | `object` |
| `Initialize()` | — | `OdinDrawer` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
