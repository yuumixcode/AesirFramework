---
title: AesirEventArgsSOEditor
description: "Runestone.AesirModules.Editor.AesirEventArgsSOEditor 的 API 文档"
---

# `AesirEventArgsSOEditor`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.Editor` → `AesirEventArgsSOEditor`

**实现接口:** `UnityEditor.IToolModeOwner`，`UnityEditor.IPreviewable`

## 声明

``` csharp
[CustomEditor]
public class AesirEventArgsSOEditor : UnityEditor.Editor, 
UnityEditor.IToolModeOwner, 
UnityEditor.IPreviewable
```

AesirEventArgsSO 自定义 Inspector： 运行模式（Play Mode）限定的事件触发按钮 + 事件参数配置字段。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirEventArgsSOEditor()`](#constructor-aesireventargssoeditor) | — |

</div>

### AesirEventArgsSOEditor() {#constructor-aesireventargssoeditor}

``` csharp
public AesirEventArgsSOEditor()
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
| `CreateInspectorGUI()` | — | `AesirEventArgsSOEditor` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `GetPreviewTitle()` | — | `Editor` |
| `RenderStaticPreview(string, Object[], int, int)` | — | `Editor` |
| `HasPreviewGUI()` | — | `Editor` |
| `MoveNextTarget()` | — | `Editor` |
| `RequiresConstantRepaint()` | — | `Editor` |
| `UseDefaultMargins()` | — | `Editor` |
| `GetInfoString()` | — | `Editor` |
| `Cleanup()` | — | `Editor` |
| `DiscardChanges()` | — | `Editor` |
| `DrawPreview(Rect)` | — | `Editor` |
| `Initialize(Object[])` | — | `Editor` |
| `OnInspectorGUI()` | — | `Editor` |
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
