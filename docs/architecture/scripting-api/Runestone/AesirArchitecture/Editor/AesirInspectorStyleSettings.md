---
title: AesirInspectorStyleSettings
description: "Runestone.AesirArchitecture.Editor.AesirInspectorStyleSettings 的 API 文档"
---

# `AesirInspectorStyleSettings`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirInspectorStyleSettings`

## 声明

``` csharp
internal static class AesirInspectorStyleSettings
```

Aesir 编辑器界面样式开关：在已安装 Odin Inspector 时，仍可选择改用 UI Toolkit 复刻样式。

**备注**

本开关只控制 Aesir 自己的 Odin 集成是否生效，与 Odin 自带的 "Disable Odin Inspector" 全局开关相互独立：Odin 全局开关会关掉所有 Odin 绘制， 而本开关让用户保留 Odin 用于其它插件、唯独让 Aesir 的面板/属性走 UI Toolkit 复刻版。

判据为 UseOdin：为 true 时 Odin 程序集的 AttributeProcessor 注入特性、 窗口 Opener 注册 Odin 版；为 false 时这些注册点整体让路，菜单落回 UI Toolkit / 原生窗口。 值由 AesirInspectorStyleSettingsSO 持有（Editor 专属 ScriptableObject）， 在 Preferences → Aesir 页切换。

本类零 Odin 依赖，放在核心编辑器程序集，RAA / RAM 的 Odin 程序集均可引用。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
