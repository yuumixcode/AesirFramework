---
title: ModuleConfigAssetsMenuItem
description: "Runestone.AesirModules.Editor.ModuleConfigAssetsMenuItem 的 API 文档"
---

# `ModuleConfigAssetsMenuItem`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `ModuleConfigAssetsMenuItem`

## 声明

``` csharp
internal static class ModuleConfigAssetsMenuItem
```

模块配置资产的一键初始化入口：显式在 Assets/Resources/ 下创建 UI / 场景两份配置 SO。

**备注**

包导入时不执行本菜单——第三方包不应在导入瞬间写入用户工程；这是开发者主动选择的开关。 两份资产均幂等创建（已存在或项目中已有同类型资产时跳过）。不点菜单也能运行：两个配置 SO 的 Instance 在 Resources 未命中时会以代码默认值在内存中各建一份实例，不落盘。

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
