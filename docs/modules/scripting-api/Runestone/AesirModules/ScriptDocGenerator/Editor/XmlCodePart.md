---
title: XmlCodePart
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.XmlCodePart 的 API 文档"
---

# `XmlCodePart`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `XmlCodePart`

## 声明

``` csharp
[Serializable]
public class XmlCodePart
```

XML 注释部分和代码块的组合。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`XmlCodePart(string, string)`](#constructor-xmlcodepart-string-string) | — |

</div>

### XmlCodePart(string, string) {#constructor-xmlcodepart-string-string}

``` csharp
public XmlCodePart(string xml, string code)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `xml` | `string` | — |
| `code` | `string` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`code`](#field-code) | 不以注释开头的代码块，除了注释对应的成员外，可能包含多个成员。 |
| [`xml`](#field-xml) | 注释部分的源代码，以 /// 开头。 |

</div>

### code {#field-code}

不以注释开头的代码块，除了注释对应的成员外，可能包含多个成员。

``` csharp
public string code;
```

### xml {#field-xml}

注释部分的源代码，以 /// 开头。

``` csharp
public string xml;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`HasNonFirstMemberSummaryAttribute`](#property-hasnonfirstmembersummaryattribute) | 块内首成员声明之后是否存在任何 [Summary] 特性（属于块内其他成员）。 此时工具不能安全判断归属——Sync/Replace/Remove 三模式均跳过本块并告警 （fail-closed，避免误删/误注非首成员的特性）。 |
| [`XmlHasSummaryTag`](#property-xmlhassummarytag) | — |
| [`CodeAfterLeadingPreprocessor`](#property-codeafterleadingpreprocessor) | code 去掉开头预处理指令行后的内容。 |
| [`LeadingPreprocessorLines`](#property-leadingpreprocessorlines) | code 开头的连续预处理指令行（如 #if、#elif、#else），确保添加 [Summary] 时位于条件编译块内部。 |
| [`PreferredSummaryContent`](#property-preferredsummarycontent) | 首选 Summary 内容：[Summary] 特性优先（特性为权威源），无特性或特性无法解析时回退 XML summary。 |
| [`RemoveAllSummaryAttributeCode`](#property-removeallsummaryattributecode) | 删除了所有 [Summary()] 部分的代码块（不含开头预处理指令行）。 |
| [`RemovedFirstSummaryAttributeCode`](#property-removedfirstsummaryattributecode) | 删除了第一个 [Summary()] 部分的代码块（不含开头预处理指令行）。 |
| [`RemovedSummaryXml`](#property-removedsummaryxml) | 删除了 summary 标签部分的 xml。 |
| [`SummaryAttributeContent`](#property-summaryattributecontent) | 代码块中属于"XML 注释归属成员（块内首成员）"的 [Summary] 特性文本内容（已反转义）。 不存在、无法解析或首个 [Summary] 属于块内非首成员时为 null。 |
| [`SummaryValue`](#property-summaryvalue) | 从 xml 中提取 Summary 的内容（压缩空白、剥离子标签、解码 XML 实体）。 |

</div>

### HasNonFirstMemberSummaryAttribute {#property-hasnonfirstmembersummaryattribute}

块内首成员声明之后是否存在任何 [Summary] 特性（属于块内其他成员）。 此时工具不能安全判断归属——Sync/Replace/Remove 三模式均跳过本块并告警 （fail-closed，避免误删/误注非首成员的特性）。

``` csharp
public bool HasNonFirstMemberSummaryAttribute { get; }
```

### XmlHasSummaryTag {#property-xmlhassummarytag}

``` csharp
public bool XmlHasSummaryTag { get; }
```

### CodeAfterLeadingPreprocessor {#property-codeafterleadingpreprocessor}

code 去掉开头预处理指令行后的内容。

``` csharp
public string CodeAfterLeadingPreprocessor { get; }
```

### LeadingPreprocessorLines {#property-leadingpreprocessorlines}

code 开头的连续预处理指令行（如 #if、#elif、#else），确保添加 [Summary] 时位于条件编译块内部。

``` csharp
public string LeadingPreprocessorLines { get; }
```

### PreferredSummaryContent {#property-preferredsummarycontent}

首选 Summary 内容：[Summary] 特性优先（特性为权威源），无特性或特性无法解析时回退 XML summary。

``` csharp
public string PreferredSummaryContent { get; }
```

### RemoveAllSummaryAttributeCode {#property-removeallsummaryattributecode}

删除了所有 [Summary()] 部分的代码块（不含开头预处理指令行）。

``` csharp
public string RemoveAllSummaryAttributeCode { get; }
```

### RemovedFirstSummaryAttributeCode {#property-removedfirstsummaryattributecode}

删除了第一个 [Summary()] 部分的代码块（不含开头预处理指令行）。

``` csharp
public string RemovedFirstSummaryAttributeCode { get; }
```

### RemovedSummaryXml {#property-removedsummaryxml}

删除了 summary 标签部分的 xml。

``` csharp
public string RemovedSummaryXml { get; }
```

### SummaryAttributeContent {#property-summaryattributecontent}

代码块中属于"XML 注释归属成员（块内首成员）"的 [Summary] 特性文本内容（已反转义）。 不存在、无法解析或首个 [Summary] 属于块内非首成员时为 null。

``` csharp
public string SummaryAttributeContent { get; }
```

### SummaryValue {#property-summaryvalue}

从 xml 中提取 Summary 的内容（压缩空白、剥离子标签、解码 XML 实体）。

``` csharp
public string SummaryValue { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetReplaceAllOutput()`](#method-getreplacealloutput) | 获取删除了 SummaryAttribute 的代码。块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。 |
| [`GetReplaceOutput()`](#method-getreplaceoutput) | 获取替换了 summary 标签的代码：内容首选 [Summary] 特性（特性为权威源），无特性时取 XML； 移除 XML summary 标签，并在前导预处理指令之后写入单行 [Summary] 特性。 块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。 |
| [`GetSummaryAlignedXml(string)`](#method-getsummaryalignedxml-string) | 以指定文本回写 XML 的 summary 标签内容（单行形式，保留原缩进），用于特性优先的双向对齐。 |
| [`GetSummaryAttributeText(string)`](#method-getsummaryattributetext-string) | 获取以指定内容生成的 [Summary] 特性行（内容经 C# 转义，缩进与 XML 注释块一致）。 |
| [`GetSyncOutput()`](#method-getsyncoutput) | 获取同步 Summary 后的代码（双向对齐，[Summary] 特性优先）： 已有可解析的特性时特性为权威内容——与 XML 不一致则以特性文本回写 XML summary，代码保持原样； 无特性时维持原有行为——以 XML 内容生成特性，插在前导预处理指令之后。 块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。 |

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

### GetReplaceAllOutput() {#method-getreplacealloutput}

获取删除了 SummaryAttribute 的代码。块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。

``` csharp
public string GetReplaceAllOutput()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetReplaceOutput() {#method-getreplaceoutput}

获取替换了 summary 标签的代码：内容首选 [Summary] 特性（特性为权威源），无特性时取 XML； 移除 XML summary 标签，并在前导预处理指令之后写入单行 [Summary] 特性。 块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。

``` csharp
public string GetReplaceOutput()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetSummaryAlignedXml(string) {#method-getsummaryalignedxml-string}

以指定文本回写 XML 的 summary 标签内容（单行形式，保留原缩进），用于特性优先的双向对齐。

``` csharp
public string GetSummaryAlignedXml(string content)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `content` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetSummaryAttributeText(string) {#method-getsummaryattributetext-string}

获取以指定内容生成的 [Summary] 特性行（内容经 C# 转义，缩进与 XML 注释块一致）。

``` csharp
public string GetSummaryAttributeText(string content)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `content` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetSyncOutput() {#method-getsyncoutput}

获取同步 Summary 后的代码（双向对齐，[Summary] 特性优先）： 已有可解析的特性时特性为权威内容——与 XML 不一致则以特性文本回写 XML summary，代码保持原样； 无特性时维持原有行为——以 XML 内容生成特性，插在前导预处理指令之后。 块内非首成员持有 [Summary] 时跳过并告警（fail-closed）。

``` csharp
public string GetSyncOutput()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
