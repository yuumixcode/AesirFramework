---
title: AesirInspectorStyleSettingsProvider
description: "Runestone.AesirArchitecture.Editor.AesirInspectorStyleSettingsProvider 的 API 文档"
---

# `AesirInspectorStyleSettingsProvider`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEditor.SettingsProvider` → `AesirInspectorStyleSettingsProvider`

## 声明

``` csharp
internal sealed class AesirInspectorStyleSettingsProvider : UnityEditor.SettingsProvider
```

Aesir 偏好设置页（Preferences → Aesir）—— Odin 样式切换从此页进入，不再占用 Tools 菜单。

**备注**

偏好值由 AesirInspectorStyleSettingsSO（Editor 专属 ScriptableObject）持有； 本页只负责呈现与写入，样式切换的对外门面仍是 UseOdin。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `deactivateHandler` | — | `SettingsProvider` |
| `footerBarGuiHandler` | — | `SettingsProvider` |
| `inspectorUpdateHandler` | — | `SettingsProvider` |
| `titleBarGuiHandler` | — | `SettingsProvider` |
| `activateHandler` | — | `SettingsProvider` |
| `guiHandler` | — | `SettingsProvider` |
| `hasSearchInterestHandler` | — | `SettingsProvider` |
| `keywords` | — | `SettingsProvider` |
| `scope` | — | `SettingsProvider` |
| `label` | — | `SettingsProvider` |
| `settingsPath` | — | `SettingsProvider` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `OnActivate(string, VisualElement)` | — | `AesirInspectorStyleSettingsProvider` |
| `OnDeactivate()` | — | `AesirInspectorStyleSettingsProvider` |
| `Equals(object)` | — | `object` |
| `HasSearchInterest(string)` | — | `SettingsProvider` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `OnFooterBarGUI()` | — | `SettingsProvider` |
| `OnGUI(string)` | — | `SettingsProvider` |
| `OnInspectorUpdate()` | — | `SettingsProvider` |
| `OnTitleBarGUI()` | — | `SettingsProvider` |
| `PopulateSearchKeywordsFromGUIContentProperties()` | — | `SettingsProvider` |
| `Repaint()` | — | `SettingsProvider` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
