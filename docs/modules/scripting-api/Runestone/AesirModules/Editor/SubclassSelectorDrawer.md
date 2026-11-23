---
title: SubclassSelectorDrawer
description: "Runestone.AesirModules.Editor.SubclassSelectorDrawer 的 API 文档"
---

# `SubclassSelectorDrawer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `UnityEditor.GUIDrawer` → `UnityEditor.PropertyDrawer` → `SubclassSelectorDrawer`

## 声明

``` csharp
[CustomPropertyDrawer]
public class SubclassSelectorDrawer : UnityEditor.PropertyDrawer
```

SubclassSelectorAttribute 的 UI Toolkit 属性绘制器。 为 [SerializeReference] 字段提供子类下拉选择： 点击按钮弹出字段声明类型的全部可选子类（按命名空间分组）， 选择后创建实例并渲染其序列化字段。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SubclassSelectorDrawer()`](#constructor-subclassselectordrawer) | — |

</div>

### SubclassSelectorDrawer() {#constructor-subclassselectordrawer}

``` csharp
public SubclassSelectorDrawer()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `fieldInfo` | — | `PropertyDrawer` |
| `attribute` | — | `PropertyDrawer` |
| `preferredLabel` | — | `PropertyDrawer` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `CreatePropertyGUI(SerializedProperty)` | — | `SubclassSelectorDrawer` |
| `CanCacheInspectorGUI(SerializedProperty)` | — | `PropertyDrawer` |
| `Equals(object)` | — | `object` |
| `GetPropertyHeight(SerializedProperty, GUIContent)` | — | `PropertyDrawer` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `OnGUI(Rect, SerializedProperty, GUIContent)` | — | `PropertyDrawer` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
