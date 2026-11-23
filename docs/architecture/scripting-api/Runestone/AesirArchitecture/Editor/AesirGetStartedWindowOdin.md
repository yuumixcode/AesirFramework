---
title: AesirGetStartedWindowOdin
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedWindowOdin 的 API 文档"
---

# `AesirGetStartedWindowOdin`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.EditorWindow` → `Sirenix.OdinInspector.Editor.OdinEditorWindow` → `AesirGetStartedWindowOdin`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`，`UnityEditor.IHasCustomMenu`

## 声明

``` csharp
public class AesirGetStartedWindowOdin : Sirenix.OdinInspector.Editor.OdinEditorWindow, 
UnityEngine.ISerializationCallbackReceiver, 
UnityEditor.IHasCustomMenu
```

Aesir Getting Started 窗口（Odin Inspector 版）— 框架示例导航主入口：概览页以包卡片展示本机安装的 Aesir 包（未安装的已知包显示占位引导），包页按教学分组列出全部示例，点击卡片在 Project 窗口选中 示例文件夹、有场景的示例经「打开场景」按钮直达场景，动作结果经右下角 Toast 提示。
结构与动效参照 Odin Inspector 自带 Getting Started 窗口：页面栈导航（顶部面包屑 + 底部返回）、 概览区随页面进出垂直收起为常驻条带（点击条带卡片可在包之间水平滑动切换）、页面内容入场渐显； 绘制全部使用 SirenixEditorGUI / SirenixGUIStyles / SdfIcons 官方基础设施，样式静态懒加载， 包卡片与示例卡片的主要显示文本在扫描 / 进页时预计算；例外是示例卡的档位徽章与"无场景"提示 （DrawSampleCard 每帧现算，含一次 Substring），量级极小但严格意义上不在"零分配"之列。

数据层与 IMGUI 兜底窗口共用 AesirGetStartedService；菜单入口为 AesirGetStartedWindow，编辑器加载时经 [InitializeOnLoadMethod] 把打开方式注册进 其 OdinWindowOpener 委托——未安装 Odin Inspector 时本程序集整体不参与编译，菜单自动落回兜底窗口。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirGetStartedWindowOdin()`](#constructor-aesirgetstartedwindowodin) | — |

</div>

### AesirGetStartedWindowOdin() {#constructor-aesirgetstartedwindowodin}

``` csharp
public AesirGetStartedWindowOdin()
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
| [`HorizontalSlideT`](#property-horizontalslidet) | — |
| [`VerticalSlideT`](#property-verticalslidet) | — |

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

### HorizontalSlideT {#property-horizontalslidet}

``` csharp
public float HorizontalSlideT { get; private set; }
```

### VerticalSlideT {#property-verticalslidet}

``` csharp
public float VerticalSlideT { get; private set; }
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
| [`RefreshAfterImport()`](#method-refreshafterimport) | 示例导入成功后的刷新：重扫包与示例（新导入的示例目录对 AssetDatabase 已可见）， 并把当前打开的包页重绑到最新扫描实例——未导入卡片即时切换为已导入态，无需手动刷新或重进页面。 |
| [`OpenWindow()`](#method-openwindow) | 打开窗口（菜单路由到此）。 |
| [`Update()`](#method-update) | 编辑器 tick 驱动：滑动动画与 Toast 活跃期间主动请求窗口重绘。IMGUI 窗口默认按需重绘 ——鼠标静止时无事件、无 OnGUI，滑动动画与 Toast 的时长进度条（两者都只在 OnGUI 帧推进）会走走停停； Sirenix 的 GUIHelper.RequestRepaint 仅置静态标志、无实际重绘驱动，无法依赖。Repaint 请求会让 编辑器对窗口保持连续重绘，动画即平滑；动画结束（T 到端点、Toast 过期）后停止请求，不空转。 EditorWindow.Update 为消息方法（非 virtual），由编辑器对可见窗口持续调用。 |

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
| `OnEnable()` | — | `AesirGetStartedWindowOdin` |
| `OnImGUI()` | — | `AesirGetStartedWindowOdin` |
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
| `EnableAutomaticHeightAdjustment(int, bool)` | — | `OdinEditorWindow` |
| `EnsureEditorsAreReady()` | — | `OdinEditorWindow` |
| `UpdateEditors()` | — | `OdinEditorWindow` |
| `SetDirty()` | — | `ScriptableObject` |
| `OnGUI()` | — | `OdinEditorWindow` |

</div>

### RefreshAfterImport() {#method-refreshafterimport}

示例导入成功后的刷新：重扫包与示例（新导入的示例目录对 AssetDatabase 已可见）， 并把当前打开的包页重绑到最新扫描实例——未导入卡片即时切换为已导入态，无需手动刷新或重进页面。

``` csharp
public void RefreshAfterImport()
```

### OpenWindow() {#method-openwindow}

打开窗口（菜单路由到此）。

``` csharp
public static void OpenWindow()
```

### Update() {#method-update}

编辑器 tick 驱动：滑动动画与 Toast 活跃期间主动请求窗口重绘。IMGUI 窗口默认按需重绘 ——鼠标静止时无事件、无 OnGUI，滑动动画与 Toast 的时长进度条（两者都只在 OnGUI 帧推进）会走走停停； Sirenix 的 GUIHelper.RequestRepaint 仅置静态标志、无实际重绘驱动，无法依赖。Repaint 请求会让 编辑器对窗口保持连续重绘，动画即平滑；动画结束（T 到端点、Toast 过期）后停止请求，不空转。 EditorWindow.Update 为消息方法（非 virtual），由编辑器对可见窗口持续调用。

``` csharp
protected void Update()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
