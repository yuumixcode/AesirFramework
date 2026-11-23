---
title: XmlSummaryTool
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.XmlSummaryTool 的 API 文档"
---

# `XmlSummaryTool`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `XmlSummaryTool`

## 声明

``` csharp
[Serializable]
public class XmlSummaryTool
```

C# 脚本的 XML 中的 Summary 注释的处理器。 内容对齐方向（Sync/Replace）：[Summary] 特性优先——已有可解析特性时以特性文本为准（必要时回写 XML）， 无特性时回退 XML summary 生成特性。仅 Remove 模式不做内容对齐。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`XmlSummaryTool(string)`](#constructor-xmlsummarytool-string) | — |

</div>

### XmlSummaryTool(string) {#constructor-xmlsummarytool-string}

``` csharp
public XmlSummaryTool(string sourceScript)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sourceScript` | `string` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`xmlCodeParts`](#field-xmlcodeparts) | — |
| [`headerLines`](#field-headerlines) | 第一个 XML 文档注释之前的所有代码行。 |
| [`sourceScriptLines`](#field-sourcescriptlines) | 源代码按行分割后的列表。 |
| [`firstXmlCommentLineIndex`](#field-firstxmlcommentlineindex) | 第一个 XML 文档注释的行号索引，从这一行开始处理 XML 文档注释。 |
| [`sourceScriptText`](#field-sourcescripttext) | 原始源代码内容。 |

</div>

### xmlCodeParts {#field-xmlcodeparts}

``` csharp
public List<XmlCodePart> xmlCodeParts;
```

### headerLines {#field-headerlines}

第一个 XML 文档注释之前的所有代码行。

``` csharp
public List<string> headerLines;
```

### sourceScriptLines {#field-sourcescriptlines}

源代码按行分割后的列表。

``` csharp
public List<string> sourceScriptLines;
```

### firstXmlCommentLineIndex {#field-firstxmlcommentlineindex}

第一个 XML 文档注释的行号索引，从这一行开始处理 XML 文档注释。

``` csharp
public int firstXmlCommentLineIndex;
```

### sourceScriptText {#field-sourcescripttext}

原始源代码内容。

``` csharp
public string sourceScriptText;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`HeaderScript`](#property-headerscript) | — |

</div>

### HeaderScript {#property-headerscript}

``` csharp
public string HeaderScript { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ParseSourceScript()`](#method-parsesourcescript) | 解析源脚本，将其分解为头部部分和 XML 文档注释与代码块的组合列表。 |
| [`GetProcessedSourceScript(XmlSummaryTool.ProcessMode)`](#method-getprocessedsourcescript-xmlsummarytool-processmode) | 获取处理后的完整脚本内容。Remove 模式下若无 [Summary] 特性则原样返回（不重写格式）。 |

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

### ParseSourceScript() {#method-parsesourcescript}

解析源脚本，将其分解为头部部分和 XML 文档注释与代码块的组合列表。

``` csharp
public XmlSummaryTool ParseSourceScript()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `XmlSummaryTool` | — |

</div>

### GetProcessedSourceScript(XmlSummaryTool.ProcessMode) {#method-getprocessedsourcescript-xmlsummarytool-processmode}

获取处理后的完整脚本内容。Remove 模式下若无 [Summary] 特性则原样返回（不重写格式）。

``` csharp
public string GetProcessedSourceScript(XmlSummaryTool.ProcessMode processMode)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `processMode` | `XmlSummaryTool.ProcessMode` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
