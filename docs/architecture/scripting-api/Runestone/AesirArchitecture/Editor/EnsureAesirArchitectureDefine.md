---
title: EnsureAesirArchitectureDefine
description: "Runestone.AesirArchitecture.Editor.EnsureAesirArchitectureDefine 的 API 文档"
---

# `EnsureAesirArchitectureDefine`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `EnsureAesirArchitectureDefine`

## 声明

``` csharp
[InitializeOnLoad]
internal static class EnsureAesirArchitectureDefine
```

自动确保 AESIR_ARCHITECTURE 脚本宏定义符号存在。
通过 InitializeOnLoadAttribute 在编辑器加载时自动执行， 供 Aesir 系列其他插件通过 #if AESIR_ARCHITECTURE 检测本架构是否存在。

**备注**

[InitializeOnLoad] 特性使 Unity 在编辑器加载时自动调用此类的静态构造函数， 经 EnsureScriptingDefineSymbol 方法 确保所有构建目标中都存在 AESIR_ARCHITECTURE 宏定义， 从而使依赖本架构的其他包可以通过条件编译指令在编译期检测架构是否可用。
写入宏会触发脚本重编译，而静态构造函数运行在程序集注册 / 域重载期间—— 此时直接发起重编译属于重入，可能使 Unity 走到程序集注册的致命分支。 故实际写入推迟到 delayCall（编辑器空闲首帧）执行； EnsureScriptingDefineSymbol 本身按构建目标逐一比对、 仅在符号确实缺失时才写入（已存在则零写入、不触发重编译），此行为不因推迟而改变。

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
