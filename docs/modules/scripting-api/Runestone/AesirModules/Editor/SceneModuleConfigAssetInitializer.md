---
title: SceneModuleConfigAssetInitializer
description: "Runestone.AesirModules.Editor.SceneModuleConfigAssetInitializer 的 API 文档"
---

# `SceneModuleConfigAssetInitializer`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `SceneModuleConfigAssetInitializer`

## 声明

``` csharp
internal static class SceneModuleConfigAssetInitializer
```

场景模块配置资产的显式创建入口：在 Resources 兜底路径缺失且项目中无同类型资产时， 创建 SceneModuleConfigSO 至 Assets/Resources/SceneModuleConfig/SceneModuleConfig.asset。

**备注**

包导入时不自动创建——第三方包不应在导入瞬间写入用户工程。由菜单 Tools/Aesir/Modules/Create Module Config Assets 显式调用；未创建时运行时以内存默认配置兜底。 创建逻辑与 UI 模块共用 AesirSingletonAssetInitializer（单一真源）。

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
