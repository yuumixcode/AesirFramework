---
title: AesirUpdateWindow
description: "Runestone.AesirArchitecture.Editor.AesirUpdateWindow 的 API 文档"
---

# `AesirUpdateWindow`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.EditorWindow` → `AesirUpdateWindow`

## 声明

``` csharp
public class AesirUpdateWindow : UnityEditor.EditorWindow
```

Aesir 包更新窗口（UI Toolkit 版）— 面向"代码导入 Assets/Runestone（非 UPM）"的用户， 检查远程最新版本并更新本地安装的 Aesir 包。两种入口： 包列表行内「更新」按钮仅更新单个包；工具栏「全部更新」让整个框架到达远程版本。
安装 Odin Inspector 且样式开关 UseOdin 为 true 时， 菜单经 OdinWindowOpener 路由到 Odin 版窗口；否则打开本 UI Toolkit 窗口。 全部编排逻辑（检测 / 更新日志 / 更新执行 / 忙碌门禁）在共享控制器 AesirUpdateController 中两版共用，本类只做状态持有与 UI Toolkit 展示。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateWindow()`](#constructor-aesirupdatewindow) | — |

</div>

### AesirUpdateWindow() {#constructor-aesirupdatewindow}

``` csharp
public AesirUpdateWindow()
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
