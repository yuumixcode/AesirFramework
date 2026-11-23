---
title: AesirTimerDebugWindow
description: "Runestone.AesirArchitecture.Editor.AesirTimerDebugWindow 的 API 文档"
---

# `AesirTimerDebugWindow`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.EditorWindow` → `AesirTimerDebugWindow`

## 声明

``` csharp
public class AesirTimerDebugWindow : UnityEditor.EditorWindow
```

计时器调试窗口（UI Toolkit 版）— 按可调的拉取节奏列出当前注册表中的计时器， 逐行显示状态、形态、时间源、剩余秒数、进度、执行次数与回调归属。
安装 Odin Inspector 且样式开关 UseOdin 为 true 时， 菜单经 OdinWindowOpener 路由到 Odin 版窗口；否则打开本 UI Toolkit 窗口。 两版共用 AesirTimerDebugSource 抓取的数据与 AesirTimerText 的措辞。

性能取向：拉取（枚举注册表 + 逐行快照）与重绘是两条节奏。重绘由编辑器驱动， 只读缓存快照；拉取仅在 AesirTimerDebuggerSettings 设定的间隔到达时发生， 因此"打开调试面板就变卡"这件事由刷新频率配置兜住，而不是靠用户少开窗口。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirTimerDebugWindow()`](#constructor-aesirtimerdebugwindow) | — |

</div>

### AesirTimerDebugWindow() {#constructor-aesirtimerdebugwindow}

``` csharp
public AesirTimerDebugWindow()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OdinWindowOpener`](#property-odinwindowopener) | Odin 版窗口的打开委托（Odin 程序集注册；关闭 Odin 样式时为 null）。 |

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

Odin 版窗口的打开委托（Odin 程序集注册；关闭 Odin 样式时为 null）。

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
