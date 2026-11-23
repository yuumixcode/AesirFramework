---
title: SceneEditorSettings
description: "Runestone.AesirModules.Editor.SceneEditorSettings 的 API 文档"
---

# `SceneEditorSettings`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.ScriptableSingleton<SceneEditorSettings>` → `SceneEditorSettings`

## 声明

``` csharp
[FilePath]
public class SceneEditorSettings : UnityEditor.ScriptableSingleton<SceneEditorSettings>
```

Scene 模块编辑器设置（ScriptableSingleton）：Bootstrapper 场景搜集注册与启动流转的编辑器侧开关。

**备注**

本类是纯数据层，不依赖 Odin：展示与交互由窗口层承担——安装 Odin 时为 SceneModuleSettingsWindowOdin（InlineEditor 展示本单例），未安装时为原生 IMGUI 兜底 SceneModuleSettingsWindow，两窗口经同一菜单入口按 Odin 可用性路由（更新器双窗口同款模式）。 Odin 展示特性（LabelText / LabelWidth / ShowInInspector 等）经 #if ODIN_INSPECTOR 包裹， 未安装 Odin 的环境整体编译剔除，不影响数据读写。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneEditorSettings()`](#constructor-sceneeditorsettings) | — |

</div>

### SceneEditorSettings() {#constructor-sceneeditorsettings}

``` csharp
public SceneEditorSettings()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`FirstLoadBootstrapScene`](#property-firstloadbootstrapscene) | — |
| [`SetupBootstrapper`](#property-setupbootstrapper) | — |
| [`BootstrapperScenePath`](#property-bootstrapperscenepath) | — |
| [`PreviousScenePath`](#property-previousscenepath) | — |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

### FirstLoadBootstrapScene {#property-firstloadbootstrapscene}

``` csharp
[LabelWidth]
[LabelText]
[ShowInInspector]
public bool FirstLoadBootstrapScene { get; set; }
```

### SetupBootstrapper {#property-setupbootstrapper}

``` csharp
[LabelWidth]
[LabelText]
[ShowInInspector]
public bool SetupBootstrapper { get; set; }
```

### BootstrapperScenePath {#property-bootstrapperscenepath}

``` csharp
[PropertyOrder]
[ReadOnly]
[ShowInInspector]
public string BootstrapperScenePath { get; set; }
```

### PreviousScenePath {#property-previousscenepath}

``` csharp
[PropertyOrder]
[ReadOnly]
[ShowInInspector]
public string PreviousScenePath { get; set; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ManualSetupBootstrapper()`](#method-manualsetupbootstrapper) | — |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `Save(bool)` | — | `ScriptableSingleton<SceneEditorSettings>` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

### ManualSetupBootstrapper() {#method-manualsetupbootstrapper}

``` csharp
[Button]
public void ManualSetupBootstrapper()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
