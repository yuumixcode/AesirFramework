---
title: AesirInspectorStyleSettingsSO
description: "Runestone.AesirArchitecture.Editor.AesirInspectorStyleSettingsSO 的 API 文档"
---

# `AesirInspectorStyleSettingsSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.ScriptableSingleton<AesirInspectorStyleSettingsSO>` → `AesirInspectorStyleSettingsSO`

## 声明

``` csharp
[FilePath]
internal sealed class AesirInspectorStyleSettingsSO : UnityEditor.ScriptableSingleton<AesirInspectorStyleSettingsSO>
```

Aesir 编辑器偏好设置的 Editor 专属 ScriptableObject —— 只在 Editor 阶段读取，不进玩家构建。

**备注**

与场景模块的 SceneModuleConfigSO（Resources 下、随构建分发的运行时配置）成对： 本资产保存"编辑器怎么显示"的偏好，运行时配置保存"游戏怎么加载场景"的数据，两者职责不交叉。

用 ScriptableSingleton{T} 而非 EditorPrefs：偏好随工程走（shell 的 ScriptableSingleton/ 目录已 gitignore），换机 / 多人协作时不会各持一套显示样式。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirInspectorStyleSettingsSO()`](#constructor-aesirinspectorstylesettingsso) | — |

</div>

### AesirInspectorStyleSettingsSO() {#constructor-aesirinspectorstylesettingsso}

``` csharp
public AesirInspectorStyleSettingsSO()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

## 方法

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
| `Save(bool)` | — | `ScriptableSingleton<AesirInspectorStyleSettingsSO>` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
