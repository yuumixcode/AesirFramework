---
title: ScriptAssemblyFilter
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptAssemblyFilter 的 API 文档"
---

# `ScriptAssemblyFilter`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `ScriptAssemblyFilter`

## 声明

``` csharp
internal static class ScriptAssemblyFilter
```

脚本程序集过滤器。通过 CompilationPipeline 缓存本项目的脚本程序集名集合， 用于在查找源文件前拦截引擎模块、预编译 DLL 等不可能存在项目源码的类型， 避免其触发昂贵的项目级内容扫描（历史上这是编辑器卡顿的主要来源）。 无法取得程序集清单时放行（fail-open），保持无过滤时的旧行为。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IsScriptAssembly(Assembly)`](#method-isscriptassembly-assembly) | 类型是否属于本项目编译产物的脚本程序集（含 Packages 源码程序集）。 引擎模块与预编译 DLL 类型返回 false——它们不存在项目源文件。 |

</div>

**继承的方法**

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

### IsScriptAssembly(Assembly) {#method-isscriptassembly-assembly}

类型是否属于本项目编译产物的脚本程序集（含 Packages 源码程序集）。 引擎模块与预编译 DLL 类型返回 false——它们不存在项目源文件。

``` csharp
public static bool IsScriptAssembly(Assembly assembly)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `assembly` | `Assembly` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
