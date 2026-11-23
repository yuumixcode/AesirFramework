---
title: AesirTimerOdinDrawer
description: "Runestone.AesirArchitecture.Editor.AesirTimerOdinDrawer 的 API 文档"
---

# `AesirTimerOdinDrawer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinDrawer` → `Sirenix.OdinInspector.Editor.OdinValueDrawer<AesirTimer>` → `AesirTimerOdinDrawer`

## 声明

``` csharp
[DrawerPriority]
internal sealed class AesirTimerOdinDrawer : Sirenix.OdinInspector.Editor.OdinValueDrawer<AesirTimer>
```

AesirTimer 字段的 Odin 检视摘要：在默认绘制之前插入一行 "状态 · 形态 · 剩余 · 次数 · 回调"，并用进度条呈现当前间隔的进度。

**备注**

Odin 按反射绘制成员，因此只要该字段处于 Odin 的属性树中就会被绘制—— 例如宿主是 SerializedMonoBehaviour / SerializedScriptableObject， 或字段标了 [ShowInInspector]。普通 MonoBehaviour 的非序列化字段 两个 Inspector 都不会显示（Unity 侧的限制），请用 Tools/Aesir/Architecture/Timer Debugger 窗口查看。

绘制后经 CallNextDrawer 让默认绘制继续（空引用仍保持 Odin 的拖拽/引用语义）。 抓取走 AesirTimerSnapshot，外观走 AesirTimerSummaryView， 措辞由 AesirTimerText 统一提供。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirTimerOdinDrawer()`](#constructor-aesirtimerodindrawer) | — |

</div>

### AesirTimerOdinDrawer() {#constructor-aesirtimerodindrawer}

``` csharp
public AesirTimerOdinDrawer()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `ValueEntry` | — | `OdinValueDrawer<AesirTimer>` |
| `Property` | — | `OdinDrawer` |
| `Initialized` | — | `OdinDrawer` |
| `SkipWhenDrawing` | — | `OdinDrawer` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `CanDrawProperty(InspectorProperty)` | — | `OdinValueDrawer<AesirTimer>` |
| `CanDrawTypeFilter(Type)` | — | `OdinDrawer` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `DrawProperty(GUIContent)` | — | `OdinDrawer` |
| `Initialize(InspectorProperty)` | — | `OdinDrawer` |
| `CallNextDrawer(GUIContent)` | — | `OdinDrawer` |
| `MemberwiseClone()` | — | `object` |
| `DrawPropertyLayout(GUIContent)` | — | `AesirTimerOdinDrawer` |
| `CanDrawValueProperty(InspectorProperty)` | — | `OdinValueDrawer<AesirTimer>` |
| `Finalize()` | — | `object` |
| `Initialize()` | — | `OdinDrawer` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
