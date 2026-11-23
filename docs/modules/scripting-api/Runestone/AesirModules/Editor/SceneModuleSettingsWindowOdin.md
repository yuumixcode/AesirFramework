---
title: SceneModuleSettingsWindowOdin
description: "Runestone.AesirModules.Editor.SceneModuleSettingsWindowOdin 的 API 文档"
---

# `SceneModuleSettingsWindowOdin`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.EditorWindow` → `Sirenix.OdinInspector.Editor.OdinEditorWindow` → `SceneModuleSettingsWindowOdin`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`，`UnityEditor.IHasCustomMenu`

## 声明

``` csharp
public class SceneModuleSettingsWindowOdin : Sirenix.OdinInspector.Editor.OdinEditorWindow, 
UnityEngine.ISerializationCallbackReceiver, 
UnityEditor.IHasCustomMenu
```

Scene 模块设置窗口（Odin 版）：InlineEditor 展示 SceneEditorSettings 单例。

**备注**

本窗口经 OdinWindowOpener 静态委托路由打开（与包内更新器同款模式）： Odin 程序集在域加载期经 [InitializeOnLoadMethod] 注册打开方式，未安装 Odin 时菜单落回原生 IMGUI 兜底窗口 SceneModuleSettingsWindow。菜单项由兜底窗口持有，本类不注册菜单。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneModuleSettingsWindowOdin()`](#constructor-scenemodulesettingswindowodin) | — |

</div>

### SceneModuleSettingsWindowOdin() {#constructor-scenemodulesettingswindowodin}

``` csharp
public SceneModuleSettingsWindowOdin()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `ToastPopupArea` | — | `OdinEditorWindow` |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `titleContent` | — | `EditorWindow` |
| `hideFlags` | — | `Object` |
| `dataModeController` | — | `EditorWindow` |
| `overlayCanvas` | — | `EditorWindow` |
| `position` | — | `EditorWindow` |
| `maxSize` | — | `EditorWindow` |
| `minSize` | — | `EditorWindow` |
| `WindowPadding` | — | `OdinEditorWindow` |
| `rootVisualElement` | — | `EditorWindow` |
| `DrawUnityEditorPreview` | — | `OdinEditorWindow` |
| `UseScrollView` | — | `OdinEditorWindow` |
| `autoRepaintOnSceneChange` | — | `EditorWindow` |
| `docked` | — | `EditorWindow` |
| `hasFocus` | — | `EditorWindow` |
| `hasUnsavedChanges` | — | `EditorWindow` |
| `maximized` | — | `EditorWindow` |
| `wantsLessLayoutEvents` | — | `EditorWindow` |
| `wantsMouseEnterLeaveWindow` | — | `EditorWindow` |
| `wantsMouseMove` | — | `EditorWindow` |
| `DefaultEditorPreviewHeight` | — | `OdinEditorWindow` |
| `DefaultLabelWidth` | — | `OdinEditorWindow` |
| `depthBufferBits` | — | `EditorWindow` |
| `name` | — | `Object` |
| `saveChangesMessage` | — | `EditorWindow` |
| `CurrentDrawingTargets` | — | `OdinEditorWindow` |
| `PropertyTree` | — | `OdinEditorWindow` |
| `antiAlias` | — | `EditorWindow` |
| `title` | — | `EditorWindow` |

</div>

## 事件

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `OnBeginGUI` | — | `OdinEditorWindow` |
| `OnClose` | — | `OdinEditorWindow` |
| `OnEndGUI` | — | `OdinEditorWindow` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `SendEvent(Event)` | — | `EditorWindow` |
| `TryGetOverlay(string, ref Overlay)` | — | `EditorWindow` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `GetExtraPaneTypes()` | — | `EditorWindow` |
| `AddItemsToMenu(GenericMenu)` | — | `OdinEditorWindow` |
| `DiscardChanges()` | — | `EditorWindow` |
| `Repaint()` | — | `EditorWindow` |
| `SaveChanges()` | — | `EditorWindow` |
| `BeginWindows()` | — | `EditorWindow` |
| `Close()` | — | `EditorWindow` |
| `EndWindows()` | — | `EditorWindow` |
| `Focus()` | — | `EditorWindow` |
| `RemoveNotification()` | — | `EditorWindow` |
| `Show()` | — | `EditorWindow` |
| `Show(bool)` | — | `EditorWindow` |
| `ShowAsDropDown(Rect, Vector2)` | — | `EditorWindow` |
| `ShowAuxWindow()` | — | `EditorWindow` |
| `ShowModal()` | — | `EditorWindow` |
| `ShowModalUtility()` | — | `EditorWindow` |
| `ShowNotification(GUIContent)` | — | `EditorWindow` |
| `ShowNotification(GUIContent, double)` | — | `EditorWindow` |
| `ShowPopup()` | — | `EditorWindow` |
| `ShowTab()` | — | `EditorWindow` |
| `ShowToast(ToastPosition, SdfIconType, string, string, Color, float)` | — | `OdinEditorWindow` |
| `ShowToast(ToastPosition, SdfIconType, string, string, Color, float, string, Action)` | — | `OdinEditorWindow` |
| `ShowToast(ToastPosition, SdfIconType, string, Color, float)` | — | `OdinEditorWindow` |
| `ShowToast(ToastPosition, SdfIconType, string, Color, float, string, Action)` | — | `OdinEditorWindow` |
| `ShowUtility()` | — | `EditorWindow` |
| `MemberwiseClone()` | — | `object` |
| `OnEnable()` | — | `SceneModuleSettingsWindowOdin` |
| `GetTargets()` | — | `OdinEditorWindow` |
| `GetTarget()` | — | `OdinEditorWindow` |
| `DrawEditor(int)` | — | `OdinEditorWindow` |
| `DrawEditorPreview(int, float)` | — | `OdinEditorWindow` |
| `DrawEditors()` | — | `OdinEditorWindow` |
| `Finalize()` | — | `object` |
| `Initialize()` | — | `OdinEditorWindow` |
| `OnAfterDeserialize()` | — | `OdinEditorWindow` |
| `OnBackingScaleFactorChanged()` | — | `EditorWindow` |
| `OnBeforeSerialize()` | — | `OdinEditorWindow` |
| `OnBeginDrawEditors()` | — | `OdinEditorWindow` |
| `OnDestroy()` | — | `OdinEditorWindow` |
| `OnDisable()` | — | `OdinEditorWindow` |
| `OnEndDrawEditors()` | — | `OdinEditorWindow` |
| `OnImGUI()` | — | `OdinEditorWindow` |
| `EnableAutomaticHeightAdjustment(int, bool)` | — | `OdinEditorWindow` |
| `EnsureEditorsAreReady()` | — | `OdinEditorWindow` |
| `UpdateEditors()` | — | `OdinEditorWindow` |
| `SetDirty()` | — | `ScriptableObject` |
| `OnGUI()` | — | `OdinEditorWindow` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
