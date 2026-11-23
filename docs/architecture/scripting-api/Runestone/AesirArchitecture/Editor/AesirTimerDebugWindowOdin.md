---
title: AesirTimerDebugWindowOdin
description: "Runestone.AesirArchitecture.Editor.AesirTimerDebugWindowOdin 的 API 文档"
---

# `AesirTimerDebugWindowOdin`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.EditorWindow` → `Sirenix.OdinInspector.Editor.OdinEditorWindow` → `AesirTimerDebugWindowOdin`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`，`UnityEditor.IHasCustomMenu`

## 声明

``` csharp
public class AesirTimerDebugWindowOdin : Sirenix.OdinInspector.Editor.OdinEditorWindow, 
UnityEngine.ISerializationCallbackReceiver, 
UnityEditor.IHasCustomMenu
```

计时器调试窗口（Odin Inspector 版）— 按可调的拉取节奏列出注册表中的计时器。
与 IMGUI 兜底窗口 AesirTimerDebugWindow 共用 AesirTimerDebugSource 抓取的数据与 AesirTimerText 的措辞，本类只做 Odin 侧的行视图模型与工具栏绑定。 编辑器加载时经 RegisterOpener 把打开方式注册到菜单入口；未安装 Odin Inspector 时本程序集整体不参与编译，菜单自动回退到 IMGUI 窗口。

行视图模型（TimerRow）在每次抓取后用快照重建并算好显示文本， 重绘期间零 LINQ、零字符串拼接、零注册表枚举；拉取频率由 AesirTimerDebuggerSettings 决定（默认 0.5 秒一次）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirTimerDebugWindowOdin()`](#constructor-aesirtimerdebugwindowodin) | — |

</div>

### AesirTimerDebugWindowOdin() {#constructor-aesirtimerdebugwindowodin}

``` csharp
public AesirTimerDebugWindowOdin()
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

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OpenWindow()`](#method-openwindow) | 打开窗口（菜单路由到此）。 |

</div>

**继承的方法**

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
| `DrawEditor(int)` | — | `AesirTimerDebugWindowOdin` |
| `OnDisable()` | — | `AesirTimerDebugWindowOdin` |
| `OnEnable()` | — | `AesirTimerDebugWindowOdin` |
| `GetTargets()` | — | `OdinEditorWindow` |
| `GetTarget()` | — | `OdinEditorWindow` |
| `DrawEditorPreview(int, float)` | — | `OdinEditorWindow` |
| `DrawEditors()` | — | `OdinEditorWindow` |
| `Finalize()` | — | `object` |
| `Initialize()` | — | `OdinEditorWindow` |
| `OnAfterDeserialize()` | — | `OdinEditorWindow` |
| `OnBackingScaleFactorChanged()` | — | `EditorWindow` |
| `OnBeforeSerialize()` | — | `OdinEditorWindow` |
| `OnBeginDrawEditors()` | — | `OdinEditorWindow` |
| `OnDestroy()` | — | `OdinEditorWindow` |
| `OnEndDrawEditors()` | — | `OdinEditorWindow` |
| `OnImGUI()` | — | `OdinEditorWindow` |
| `EnableAutomaticHeightAdjustment(int, bool)` | — | `OdinEditorWindow` |
| `EnsureEditorsAreReady()` | — | `OdinEditorWindow` |
| `UpdateEditors()` | — | `OdinEditorWindow` |
| `SetDirty()` | — | `ScriptableObject` |
| `OnGUI()` | — | `OdinEditorWindow` |

</div>

### OpenWindow() {#method-openwindow}

打开窗口（菜单路由到此）。

``` csharp
public static void OpenWindow()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
