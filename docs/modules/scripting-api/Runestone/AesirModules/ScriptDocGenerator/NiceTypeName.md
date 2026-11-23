---
title: NiceTypeName
description: "Runestone.AesirModules.ScriptDocGenerator.NiceTypeName 的 API 文档"
---

# `NiceTypeName`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `NiceTypeName`

## 声明

``` csharp
internal static class NiceTypeName
```

类型名称格式化工具（生成文档使用的"好看名"）。

**备注**

语义与 Sirenix.Utilities.TypeExtensions.GetNiceName / GetNiceFullName 对齐 （别名表、数组/可空/引用/泛型/嵌套类型的拼接规则逐条对应）。
为什么要自带一份：本程序集（Runestone.AesirModules.OdinInspector）是全平台程序集， Player 构建不应依赖 Odin 运行时程序集；而原名格式化是 Odin Sirenix.Utilities 的扩展方法， 导致 ScriptDocGenerator 的运行时代码把 Odin 拖进玩家构建。移植的是纯格式化逻辑， 不涉及 Odin 专属能力。

仅主线程使用（框架约定），故缓存不加锁；缓存避免泛型递归拼接的重复分配。

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
