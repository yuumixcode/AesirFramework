---
title: AesirGetStartedWindow
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedWindow 的 API 文档"
---

# `AesirGetStartedWindow`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.EditorWindow` → `AesirGetStartedWindow`

## 声明

``` csharp
public class AesirGetStartedWindow : UnityEditor.EditorWindow
```

Aesir Getting Started 窗口（UI Toolkit 版）— 框架示例导航：列出本机安装的 Aesir 包及其示例， 按教学分组展示，一键打开示例场景或定位示例目录。
安装 Odin Inspector 且样式开关 UseOdin 为 true 时， 菜单经 OdinWindowOpener 路由到 Odin 版窗口（AesirGetStartedWindowOdin）； 否则打开本窗口。外观由 AesirGetStarted.uss 统一控制，复刻 Odin 深色卡片风格。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirGetStartedWindow()`](#constructor-aesirgetstartedwindow) | — |

</div>

### AesirGetStartedWindow() {#constructor-aesirgetstartedwindow}

``` csharp
public AesirGetStartedWindow()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OdinWindowOpener`](#property-odinwindowopener) | Odin 版窗口的打开委托（由 Odin 程序集经 [InitializeOnLoadMethod] 注册； 未安装 Odin Inspector 或样式开关关闭时为 null，菜单打开本 UI Toolkit 窗口）。 |

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
| `rootVisualElement` | — | `EditorWindow` |
| `autoRepaintOnSceneChange` | — | `EditorWindow` |
| `docked` | — | `EditorWindow` |
| `hasFocus` | — | `EditorWindow` |
| `hasUnsavedChanges` | — | `EditorWindow` |
| `maximized` | — | `EditorWindow` |
| `wantsLessLayoutEvents` | — | `EditorWindow` |
| `wantsMouseEnterLeaveWindow` | — | `EditorWindow` |
| `wantsMouseMove` | — | `EditorWindow` |
| `depthBufferBits` | — | `EditorWindow` |
| `name` | — | `Object` |
| `saveChangesMessage` | — | `EditorWindow` |
| `antiAlias` | — | `EditorWindow` |
| `title` | — | `EditorWindow` |

</div>

### OdinWindowOpener {#property-odinwindowopener}

Odin 版窗口的打开委托（由 Odin 程序集经 [InitializeOnLoadMethod] 注册； 未安装 Odin Inspector 或样式开关关闭时为 null，菜单打开本 UI Toolkit 窗口）。

``` csharp
public static Action OdinWindowOpener { get; private set; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`RegisterOdinWindowOpener(Action)`](#method-registerodinwindowopener-action) | 注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。 |

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
| `ShowUtility()` | — | `EditorWindow` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `OnBackingScaleFactorChanged()` | — | `EditorWindow` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

### RegisterOdinWindowOpener(Action) {#method-registerodinwindowopener-action}

注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。

``` csharp
public static void RegisterOdinWindowOpener(Action opener)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `opener` | `Action` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
