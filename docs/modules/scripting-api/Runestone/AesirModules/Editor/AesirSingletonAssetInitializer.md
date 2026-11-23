---
title: AesirSingletonAssetInitializer
description: "Runestone.AesirModules.Editor.AesirSingletonAssetInitializer 的 API 文档"
---

# `AesirSingletonAssetInitializer`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `AesirSingletonAssetInitializer`

## 声明

``` csharp
internal static class AesirSingletonAssetInitializer
```

模块配置资产（单例 ScriptableObject）的显式创建共用实现。

**备注**

由菜单 Tools/Aesir/Modules/Create Module Config Assets 显式调用，经各模块的 XxxModuleConfigAssetInitializer 转入本类——包导入时不写用户工程。 UI / Scene 两个配置资产此前各持一份等价实现，此处收敛为单一真源。

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
