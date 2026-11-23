---
title: SourceFileEntry
description: "Runestone.AesirModules.ScriptDocGenerator.SourceFileEntry 的 API 文档"
---

# `SourceFileEntry`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `SourceFileEntry`

## 声明

``` csharp
[Serializable]
public class SourceFileEntry
```

源代码文件路径与内容的绑定容器。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SourceFileEntry(string, string[])`](#constructor-sourcefileentry-string-string) | — |

</div>

### SourceFileEntry(string, string[]) {#constructor-sourcefileentry-string-string}

``` csharp
public SourceFileEntry(string filePath, string[] sourceLines)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `filePath` | `string` | — |
| `sourceLines` | `string[]` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`filePath`](#field-filepath) | 相对路径（Assets/ 开头）。 |
| [`sourceLines`](#field-sourcelines) | 按行分割的源代码内容。 |

</div>

### filePath {#field-filepath}

相对路径（Assets/ 开头）。

``` csharp
public string filePath;
```

### sourceLines {#field-sourcelines}

按行分割的源代码内容。

``` csharp
public string[] sourceLines;
```

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
