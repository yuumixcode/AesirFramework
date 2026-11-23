---
title: ScriptDocGenerationResult
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGenerationResult 的 API 文档"
---

# `ScriptDocGenerationResult`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `ScriptDocGenerationResult`

## 声明

``` csharp
public sealed class ScriptDocGenerationResult
```

一次脚本文档生成调用的结果：写入的文档文件清单、使用的设置与输出根目录， 以及未能映射到编译产物的源码类型名（文件夹模式）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ScriptDocGenerationResult()`](#constructor-scriptdocgenerationresult) | — |

</div>

### ScriptDocGenerationResult() {#constructor-scriptdocgenerationresult}

``` csharp
public ScriptDocGenerationResult()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GeneratedFiles`](#property-generatedfiles) | — |
| [`UnresolvedTypeNames`](#property-unresolvedtypenames) | — |
| [`Success`](#property-success) | 是否至少写入了一份文档。 |
| [`GeneratedCount`](#property-generatedcount) | 成功写入的文档数量。 |
| [`OutputFolder`](#property-outputfolder) | 本次写入的输出根目录（绝对路径）。 |
| [`SettingsName`](#property-settingsname) | 使用的文档生成器设置显示名（资产名 + 类型名）。 |

</div>

### GeneratedFiles {#property-generatedfiles}

``` csharp
public List<string> GeneratedFiles { get; }
```

### UnresolvedTypeNames {#property-unresolvedtypenames}

``` csharp
public List<string> UnresolvedTypeNames { get; }
```

### Success {#property-success}

是否至少写入了一份文档。

``` csharp
public bool Success { get; }
```

### GeneratedCount {#property-generatedcount}

成功写入的文档数量。

``` csharp
public int GeneratedCount { get; }
```

### OutputFolder {#property-outputfolder}

本次写入的输出根目录（绝对路径）。

``` csharp
public string OutputFolder { get; internal set; }
```

### SettingsName {#property-settingsname}

使用的文档生成器设置显示名（资产名 + 类型名）。

``` csharp
public string SettingsName { get; internal set; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `ToString()` | 单行摘要（含未解析类型提示），便于日志与 AI 助手回显。 | `ScriptDocGenerationResult` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
