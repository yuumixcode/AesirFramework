---
title: SceneAssetWrapperDrawer
description: "Runestone.AesirModules.Editor.SceneAssetWrapperDrawer 的 API 文档"
---

# `SceneAssetWrapperDrawer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `UnityEditor.GUIDrawer` → `UnityEditor.PropertyDrawer` → `SceneAssetWrapperDrawer`

## 声明

``` csharp
[CustomPropertyDrawer]
public class SceneAssetWrapperDrawer : UnityEditor.PropertyDrawer
```

SceneAssetWrapper 的属性绘制器 —— UI Toolkit 优先，IMGUI 兜底，零 Odin 依赖。

**备注**

装了 Odin 时由 SceneAssetWrapperAttributeProcessor 依据样式开关决定让路（注入 DrawWithUnityAttribute）还是接管；未装 Odin 时本绘制器是唯一实现。两条绘制路径共用 BuildStateText 与 GetSceneAssetColor，信息量一致。

仅支持单值字段（wrapper 不作集合使用）；数组 / 列表元素显示为不支持。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneAssetWrapperDrawer()`](#constructor-sceneassetwrapperdrawer) | — |

</div>

### SceneAssetWrapperDrawer() {#constructor-sceneassetwrapperdrawer}

``` csharp
public SceneAssetWrapperDrawer()
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
| `CreatePropertyGUI(SerializedProperty)` | UI Toolkit 路径：Inspector / UIToolkit 编辑器走这里。 | `SceneAssetWrapperDrawer` |
| `GetPropertyHeight(SerializedProperty, GUIContent)` | IMGUI 路径：Odin 编辑器窗口内 DrawWithUnity 让路后的绘制。 | `SceneAssetWrapperDrawer` |
| `OnGUI(Rect, SerializedProperty, GUIContent)` | — | `SceneAssetWrapperDrawer` |
| `CanCacheInspectorGUI(SerializedProperty)` | — | `PropertyDrawer` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
