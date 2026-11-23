---
title: AesirUpdateWindowOdin
description: "Runestone.AesirArchitecture.Editor.AesirUpdateWindowOdin 的 API 文档"
---

# `AesirUpdateWindowOdin`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.EditorWindow` → `Sirenix.OdinInspector.Editor.OdinEditorWindow` → `AesirUpdateWindowOdin`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`，`UnityEditor.IHasCustomMenu`

## 声明

``` csharp
public class AesirUpdateWindowOdin : Sirenix.OdinInspector.Editor.OdinEditorWindow, 
UnityEngine.ISerializationCallbackReceiver, 
UnityEditor.IHasCustomMenu
```

Aesir 包更新窗口（Odin Inspector 版）— 检测远程最新版本、展示「本地 → 远程」更新日志、 确认后更新 InstallRootRelativePath 下的本地安装包。 两种入口：包列表行内「更新」按钮仅更新单个包（配套版本风险由确认框提示）； 「全部更新」让整个框架到达远程版本（旧的更新、缺失的已知包补装，确认框明示）。
全部编排逻辑（检测 / 更新日志 / 更新执行 / 忙碌门禁）在共享控制器 AesirUpdateController 中与 IMGUI 兜底窗口共用，本类只做状态序列化、标题区手绘、 行视图模型与 Odin 特性绘制。编辑器加载时经 RegisterOpener 把打开方式注册进 菜单入口 AesirUpdateWindow；未安装 Odin Inspector 时本程序集整体不参与编译， 菜单自动回退到 IMGUI 兜底窗口。

状态设计：远程版本 / 检测结果 / 更新日志均为序列化字段，更新导入触发域重载后窗口内容不丢失； 行视图模型（PackageRow）在状态变化时一次性重建并重算显示文本与颜色， 稳态 OnGUI 期间零 LINQ、零字符串拼接、零磁盘 IO（行内按钮的 Owner 引用为非序列化，域重载后随重建回填）； 例外是域重载后首帧的兜底 RebuildRows()（包数变化时的重建路径，含拼串与目标集重算）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateWindowOdin()`](#constructor-aesirupdatewindowodin) | — |

</div>

### AesirUpdateWindowOdin() {#constructor-aesirupdatewindowodin}

``` csharp
public AesirUpdateWindowOdin()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `ToastPopupArea` | — | `OdinEditorWindow` |

</div>

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Busy`](#property-busy) | 是否处于忙碌状态（据此禁用更新按钮）。 |

</div>

**继承的属性**

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

### Busy {#property-busy}

是否处于忙碌状态（据此禁用更新按钮）。

``` csharp
public bool Busy { get; }
```

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
| [`RequestUpdateSingle(AesirUpdateService.InstalledPackage)`](#method-requestupdatesingle-aesirupdateservice-installedpackage) | 行内单包更新按钮的转发（PackageRow 经 Owner 调用；忙碌门禁在控制器内兜底）。 |
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
| `DrawEditor(int)` | — | `AesirUpdateWindowOdin` |
| `OnEnable()` | — | `AesirUpdateWindowOdin` |
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
| `OnDisable()` | — | `OdinEditorWindow` |
| `OnEndDrawEditors()` | — | `OdinEditorWindow` |
| `OnImGUI()` | — | `OdinEditorWindow` |
| `EnableAutomaticHeightAdjustment(int, bool)` | — | `OdinEditorWindow` |
| `EnsureEditorsAreReady()` | — | `OdinEditorWindow` |
| `UpdateEditors()` | — | `OdinEditorWindow` |
| `SetDirty()` | — | `ScriptableObject` |
| `OnGUI()` | — | `OdinEditorWindow` |

</div>

### RequestUpdateSingle(AesirUpdateService.InstalledPackage) {#method-requestupdatesingle-aesirupdateservice-installedpackage}

行内单包更新按钮的转发（PackageRow 经 Owner 调用；忙碌门禁在控制器内兜底）。

``` csharp
public void RequestUpdateSingle(AesirUpdateService.InstalledPackage package)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `package` | `AesirUpdateService.InstalledPackage` | — |

</div>

### OpenWindow() {#method-openwindow}

打开窗口（菜单路由到此）。

``` csharp
public static void OpenWindow()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
