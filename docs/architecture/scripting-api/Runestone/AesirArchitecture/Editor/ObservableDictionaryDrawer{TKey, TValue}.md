---
title: ObservableDictionaryDrawer<TKey, TValue>
description: "Runestone.AesirArchitecture.Editor.ObservableDictionaryDrawer<TKey, TValue> 的 API 文档"
---

# `ObservableDictionaryDrawer<TKey, TValue>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinDrawer` → `Sirenix.OdinInspector.Editor.OdinValueDrawer<ObservableDictionary<TKey, TValue>>` → `ObservableDictionaryDrawer<TKey, TValue>`

## 声明

``` csharp
[DrawerPriority]
internal sealed class ObservableDictionaryDrawer<TKey, TValue> : Sirenix.OdinInspector.Editor.OdinValueDrawer<ObservableDictionary<TKey, TValue>>  
```

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ObservableDictionaryDrawer()`](#constructor-observabledictionarydrawer) | — |

</div>

### ObservableDictionaryDrawer() {#constructor-observabledictionarydrawer}

``` csharp
public ObservableDictionaryDrawer<TKey, TValue>()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `ValueEntry` | — | `OdinValueDrawer<ObservableDictionary<TKey, TValue>>` |
| `Property` | — | `OdinDrawer` |
| `Initialized` | — | `OdinDrawer` |
| `SkipWhenDrawing` | — | `OdinDrawer` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `CanDrawProperty(InspectorProperty)` | — | `OdinValueDrawer<ObservableDictionary<TKey, TValue>>` |
| `CanDrawTypeFilter(Type)` | — | `OdinDrawer` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `DrawProperty(GUIContent)` | — | `OdinDrawer` |
| `Initialize(InspectorProperty)` | — | `OdinDrawer` |
| `CallNextDrawer(GUIContent)` | — | `OdinDrawer` |
| `MemberwiseClone()` | — | `object` |
| `DrawPropertyLayout(GUIContent)` | — | `ObservableDictionaryDrawer<TKey, TValue>` |
| `CanDrawValueProperty(InspectorProperty)` | — | `OdinValueDrawer<ObservableDictionary<TKey, TValue>>` |
| `Finalize()` | — | `object` |
| `Initialize()` | — | `OdinDrawer` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
