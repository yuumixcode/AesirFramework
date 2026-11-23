---
title: QuickCreateSOMenuItem
description: "Runestone.AesirArchitecture.Editor.QuickCreateSOMenuItem 的 API 文档"
---

# `QuickCreateSOMenuItem`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `QuickCreateSOMenuItem`

## 声明

``` csharp
public static class QuickCreateSOMenuItem
```

右键快捷生成 ScriptableObject 资源文件。
项目同时安装 Aesir Inspector（独立包，写入 AESIR_INSPECTOR 宏）时本类整体不参与编译， 避免两包注册同名菜单；注意该宏在本仓库已无生产者（2026-09-05 随 Aesir Inspector 迁出清除）， 本守卫是否生效取决于外部独立仓库是否仍写该宏，本仓库无法自证， 由其提供同名菜单，避免重复菜单项。

**备注**

菜单优先级 80：实测 Assets/Create/C# Script 与 Assets/Create/2D 同为 81， 同段按注册顺序紧邻，无法用整数优先级插在两者之间；80 使该项位于 Folder（18）与 C# Script（81）之间。

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
