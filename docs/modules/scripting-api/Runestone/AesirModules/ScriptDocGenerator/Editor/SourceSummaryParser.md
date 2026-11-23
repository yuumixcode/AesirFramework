---
title: SourceSummaryParser
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.SourceSummaryParser 的 API 文档"
---

# `SourceSummaryParser`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `SourceSummaryParser`

## 声明

``` csharp
public static class SourceSummaryParser
```

源码 XML 文档注释解析门面。实际解析由 SourceScanner 单遍状态机完成： 字符串/逐字字符串/注释感知净化 + 命名空间栈 + 类型栈，支持全限定键（含嵌套类型规范键 与扁平旧键）、方法参数类型键与参数计数键、构造函数 #ctor 键。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ParseSummaries(SourceFileEntry[], string)`](#method-parsesummaries-sourcefileentry-string) | 解析多个源文件条目中的 summary 注释，返回全限定键 → summary 字典。 键格式：Namespace.TypeName（类型级）或 Namespace.TypeName.MemberName（成员级）；方法与构造函数附参数后缀—— 参数类型键 Member(int, string)（历史格式）、参数计数键 Member(2)； 参数提取失败时为无后缀键；构造函数额外发射 #ctor 键。 assemblyName 非空时作为键前缀，避免不同程序集中同名命名空间+类型名的键冲突。 |
| [`ParseDocComments(SourceFileEntry[], string)`](#method-parsedoccomments-sourcefileentry-string) | 解析多个源文件条目中的全部 XML 文档标签（summary/param/returns/remarks/value/typeparam）， 返回结构化解析结果，键规则与 ParseSummaries 一致。 |
| [`ParseSummaryText(List<string>)`](#method-parsesummarytext-list-string) | 从文档注释行列表（已移除 /// 前缀）中提取 summary 纯文本，清理 XML 标签与实体。 |
| [`StripXmlTags(string)`](#method-stripxmltags-string) | 清理 XML 标签：将 <see cref="A.B"/> 替换为 B，paramref/typeparamref/langword 替换为名称，<c>/<code> 保留内容，<para> 分段，移除其他 XML 标签，折叠多余空格。 |

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

### ParseSummaries(SourceFileEntry[], string) {#method-parsesummaries-sourcefileentry-string}

解析多个源文件条目中的 summary 注释，返回全限定键 → summary 字典。 键格式：Namespace.TypeName（类型级）或 Namespace.TypeName.MemberName（成员级）；方法与构造函数附参数后缀—— 参数类型键 Member(int, string)（历史格式）、参数计数键 Member(2)； 参数提取失败时为无后缀键；构造函数额外发射 #ctor 键。 assemblyName 非空时作为键前缀，避免不同程序集中同名命名空间+类型名的键冲突。

``` csharp
public static Dictionary<string, string> ParseSummaries(SourceFileEntry[] entries, string assemblyName = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `entries` | `SourceFileEntry[]` | — |
| `assemblyName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Dictionary<string, string>` | — |

</div>

### ParseDocComments(SourceFileEntry[], string) {#method-parsedoccomments-sourcefileentry-string}

解析多个源文件条目中的全部 XML 文档标签（summary/param/returns/remarks/value/typeparam）， 返回结构化解析结果，键规则与 ParseSummaries 一致。

``` csharp
public static ParsedSourceDoc ParseDocComments(SourceFileEntry[] entries, string assemblyName = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `entries` | `SourceFileEntry[]` | — |
| `assemblyName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ParsedSourceDoc` | — |

</div>

### ParseSummaryText(List<string>) {#method-parsesummarytext-list-string}

从文档注释行列表（已移除 /// 前缀）中提取 summary 纯文本，清理 XML 标签与实体。

``` csharp
public static string ParseSummaryText(List<string> summaryLines)
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

清理 XML 标签：将 <see cref="A.B"/> 替换为 B，paramref/typeparamref/langword 替换为名称，<c>/<code> 保留内容，<para> 分段，移除其他 XML 标签，折叠多余空格。

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
