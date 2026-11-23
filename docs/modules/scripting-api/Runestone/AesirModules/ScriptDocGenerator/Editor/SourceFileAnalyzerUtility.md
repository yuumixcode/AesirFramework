---
title: SourceFileAnalyzerUtility
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.SourceFileAnalyzerUtility 的 API 文档"
---

# `SourceFileAnalyzerUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `SourceFileAnalyzerUtility`

## 声明

``` csharp
public static class SourceFileAnalyzerUtility
```

源文件查找与成员名提取工具。 查找链路：ScriptAssemblyFilter 程序集过滤 → AssetDatabase 按名搜索 + GetClass() 验证（单遍，partial 全收集）→ ProjectScriptIndex 内容索引兜底（文件名与类型名不一致的场景，全项目仅扫描一次）。 非 partial 场景返回唯一匹配；partial 类型返回全部声明文件。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetSourceFiles(Type)`](#method-getsourcefiles-type) | 获取类型对应的源文件条目数组（路径 + 代码内容）。 兼容包装：每次现读文件内容，不在静态缓存中驻留行数组（内容由解析结果缓存承接）。 |
| [`ExtractMemberName(string)`](#method-extractmembername-string) | 从声明行中提取成员名称。 |
| [`FindSourceFilePaths(Type)`](#method-findsourcefilepaths-type) | 查找类型对应的源文件相对路径（Assets/ 开头），结果按类型缓存。 引擎模块 / 预编译 DLL 类型直接返回空数组（不可能存在项目源码）。 |
| [`ClearCache()`](#method-clearcache) | 清空所有缓存。 |

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

### GetSourceFiles(Type) {#method-getsourcefiles-type}

获取类型对应的源文件条目数组（路径 + 代码内容）。 兼容包装：每次现读文件内容，不在静态缓存中驻留行数组（内容由解析结果缓存承接）。

``` csharp
public static SourceFileEntry[] GetSourceFiles(Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `SourceFileEntry[]` | — |

</div>

### ExtractMemberName(string) {#method-extractmembername-string}

从声明行中提取成员名称。

``` csharp
public static string ExtractMemberName(string declarationLine)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `declarationLine` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### FindSourceFilePaths(Type) {#method-findsourcefilepaths-type}

查找类型对应的源文件相对路径（Assets/ 开头），结果按类型缓存。 引擎模块 / 预编译 DLL 类型直接返回空数组（不可能存在项目源码）。

``` csharp
public static string[] FindSourceFilePaths(Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string[]` | — |

</div>

### ClearCache() {#method-clearcache}

清空所有缓存。

``` csharp
public static void ClearCache()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
