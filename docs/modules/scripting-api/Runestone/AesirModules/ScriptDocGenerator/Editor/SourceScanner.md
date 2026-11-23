---
title: SourceScanner
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.SourceScanner 的 API 文档"
---

# `SourceScanner`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `SourceScanner`

## 声明

``` csharp
public static class SourceScanner
```

单遍字符级状态机源码扫描器： 第一遍净化——字符串（普通/逐字）、字符字面量、行注释、块注释内容置空，产出净化行， 使后续正则天然免疫字符串/注释里的假类型声明、假命名空间与假花括号； 第二遍扫描——命名空间栈（块式/文件级）+ 类型栈（花括号深度配对）单次前向扫描， 将 /// 文档注释块关联到其后的声明行，产出结构化 ParsedSourceDoc。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Scan(string[], bool)`](#method-scan-string-bool) | 单遍扫描源码行数组，返回结构化文档注释结果。 parseDocs 为 false 时跳过 /// 文档块的收集与解析（仅收集类型/命名空间声明，供项目级索引用）。 |
| [`ParseSummaryTextFromLines(List<string>)`](#method-parsesummarytextfromlines-list-string) | 从文档注释行列表（已移除 /// 前缀）中提取 summary 纯文本。 |
| [`StripXmlTags(string)`](#method-stripxmltags-string) | 清理 XML 标签：cref/paramref/typeparamref/langword 替换为名称，<c>/<code> 保留内容， <para> 分段为换行，其余标签删除，折叠多余空格，最后解码 XML 实体。 |

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

### Scan(string[], bool) {#method-scan-string-bool}

单遍扫描源码行数组，返回结构化文档注释结果。 parseDocs 为 false 时跳过 /// 文档块的收集与解析（仅收集类型/命名空间声明，供项目级索引用）。

``` csharp
public static ParsedSourceDoc Scan(string[] lines, bool parseDocs = true)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `lines` | `string[]` | — |
| `parseDocs` | `bool` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ParsedSourceDoc` | — |

</div>

### ParseSummaryTextFromLines(List<string>) {#method-parsesummarytextfromlines-list-string}

从文档注释行列表（已移除 /// 前缀）中提取 summary 纯文本。

``` csharp
public static string ParseSummaryTextFromLines(List<string> summaryLines)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `summaryLines` | `List<string>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### StripXmlTags(string) {#method-stripxmltags-string}

清理 XML 标签：cref/paramref/typeparamref/langword 替换为名称，<c>/<code> 保留内容， <para> 分段为换行，其余标签删除，折叠多余空格，最后解码 XML 实体。

``` csharp
public static string StripXmlTags(string text)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `text` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
