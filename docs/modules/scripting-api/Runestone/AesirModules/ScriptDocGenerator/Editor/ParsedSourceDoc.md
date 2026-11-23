---
title: ParsedSourceDoc
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ParsedSourceDoc 的 API 文档"
---

# `ParsedSourceDoc`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `ParsedSourceDoc`

## 声明

``` csharp
public sealed class ParsedSourceDoc
```

单个源文件（或合并后的类型级视图）的结构化 XML 文档注释解析结果。 键为全限定键（无程序集前缀，合并时按需添加）： 类型级 Namespace.TypeName；成员级 Namespace.TypeName.MemberName， 方法与构造函数附参数后缀——参数类型键 Member(int, string)、参数计数键 Member(2)， 参数列表提取失败时退化为无后缀键。构造函数额外发射 #ctor 键。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ParsedSourceDoc()`](#constructor-parsedsourcedoc) | — |

</div>

### ParsedSourceDoc() {#constructor-parsedsourcedoc}

``` csharp
public ParsedSourceDoc()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ParamSummaries`](#property-paramsummaries) | 参数级文档字典（<param> 标签），成员键 → 参数名 → 文本。 |
| [`TypeParamSummaries`](#property-typeparamsummaries) | 泛型参数文档字典（<typeparam> 标签），成员键 → 参数名 → 文本。 |
| [`RemarksSummaries`](#property-remarkssummaries) | — |
| [`ReturnsSummaries`](#property-returnssummaries) | — |
| [`Summaries`](#property-summaries) | — |
| [`ValueSummaries`](#property-valuesummaries) | — |
| [`DeclaredNamespaces`](#property-declarednamespaces) | — |
| [`DeclaredTypeNames`](#property-declaredtypenames) | — |

</div>

### ParamSummaries {#property-paramsummaries}

参数级文档字典（<param> 标签），成员键 → 参数名 → 文本。

``` csharp
public Dictionary<string, Dictionary<string, string>> ParamSummaries { get; }
```

### TypeParamSummaries {#property-typeparamsummaries}

泛型参数文档字典（<typeparam> 标签），成员键 → 参数名 → 文本。

``` csharp
public Dictionary<string, Dictionary<string, string>> TypeParamSummaries { get; }
```

### RemarksSummaries {#property-remarkssummaries}

``` csharp
public Dictionary<string, string> RemarksSummaries { get; }
```

### ReturnsSummaries {#property-returnssummaries}

``` csharp
public Dictionary<string, string> ReturnsSummaries { get; }
```

### Summaries {#property-summaries}

``` csharp
public Dictionary<string, string> Summaries { get; }
```

### ValueSummaries {#property-valuesummaries}

``` csharp
public Dictionary<string, string> ValueSummaries { get; }
```

### DeclaredNamespaces {#property-declarednamespaces}

``` csharp
public HashSet<string> DeclaredNamespaces { get; }
```

### DeclaredTypeNames {#property-declaredtypenames}

``` csharp
public HashSet<string> DeclaredTypeNames { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`MergeWithPrefix(ParsedSourceDoc, string)`](#method-mergewithprefix-parsedsourcedoc-string) | 将另一份解析结果按程序集键前缀合并进来（后写入覆盖同键，与历史行为一致）。 |

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

### MergeWithPrefix(ParsedSourceDoc, string) {#method-mergewithprefix-parsedsourcedoc-string}

将另一份解析结果按程序集键前缀合并进来（后写入覆盖同键，与历史行为一致）。

``` csharp
public void MergeWithPrefix(ParsedSourceDoc other, string prefix)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `other` | `ParsedSourceDoc` | — |
| `prefix` | `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
