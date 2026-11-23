---
title: AesirPathLookupAssetEditor
description: "Runestone.AesirArchitecture.Editor.AesirPathLookupAssetEditor 的 API 文档"
---

# `AesirPathLookupAssetEditor`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.Editor` → `AesirPathLookupAssetEditor`

**实现接口:** `UnityEditor.IToolModeOwner`，`UnityEditor.IPreviewable`

## 声明

``` csharp
[CustomEditor]
internal class AesirPathLookupAssetEditor : UnityEditor.Editor, 
UnityEditor.IToolModeOwner, 
UnityEditor.IPreviewable
```

AesirPathLookup 锚点资产的 Inspector — 说明资产用途并比对期望 / 实际 GUID （参照 Odin 的 SirenixPathLookupScriptableObjectEditor）：锚点 GUID 被外部工具改写会导致 Runestone 移动后的定位失效，此处给出可视化核对面。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirPathLookupAssetEditor()`](#constructor-aesirpathlookupasseteditor) | — |

</div>

### AesirPathLookupAssetEditor() {#constructor-aesirpathlookupasseteditor}

``` csharp
public AesirPathLookupAssetEditor()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `target` | — | `Editor` |
| `targets` | — | `Editor` |
| `serializedObject` | — | `Editor` |
| `hasUnsavedChanges` | — | `Editor` |
| `name` | — | `Object` |
| `saveChangesMessage` | — | `Editor` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `DrawDefaultInspector()` | — | `Editor` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `OnInspectorGUI()` | — | `AesirPathLookupAssetEditor` |
| `GetPreviewTitle()` | — | `Editor` |
| `RenderStaticPreview(string, Object[], int, int)` | — | `Editor` |
| `CreateInspectorGUI()` | — | `Editor` |
| `HasPreviewGUI()` | — | `Editor` |
| `MoveNextTarget()` | — | `Editor` |
| `RequiresConstantRepaint()` | — | `Editor` |
| `UseDefaultMargins()` | — | `Editor` |
| `GetInfoString()` | — | `Editor` |
| `Cleanup()` | — | `Editor` |
| `DiscardChanges()` | — | `Editor` |
| `DrawPreview(Rect)` | — | `Editor` |
| `Initialize(Object[])` | — | `Editor` |
| `OnInteractivePreviewGUI(Rect, GUIStyle)` | — | `Editor` |
| `OnPreviewGUI(Rect, GUIStyle)` | — | `Editor` |
| `OnPreviewSettings()` | — | `Editor` |
| `ReloadPreviewInstances()` | — | `Editor` |
| `Repaint()` | — | `Editor` |
| `ResetTarget()` | — | `Editor` |
| `SaveChanges()` | — | `Editor` |
| `DrawHeader()` | — | `Editor` |
| `MemberwiseClone()` | — | `object` |
| `ShouldHideOpenButton()` | — | `Editor` |
| `Finalize()` | — | `object` |
| `OnHeaderGUI()` | — | `Editor` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
