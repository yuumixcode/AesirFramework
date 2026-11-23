---
title: TypeAnalyzerUtility
description: "Runestone.AesirModules.ScriptDocGenerator.TypeAnalyzerUtility 的 API 文档"
---

# `TypeAnalyzerUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `TypeAnalyzerUtility`

## 声明

``` csharp
public static class TypeAnalyzerUtility
```

类型分析器工具类

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`TypeAliasMap`](#field-typealiasmap) | 将系统类型名称映射到其 C# 别名的字典 |

</div>

### TypeAliasMap {#field-typealiasmap}

将系统类型名称映射到其 C# 别名的字典

``` csharp
public static readonly IReadOnlyDictionary<Type, string> TypeAliasMap;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IsGeneratedInternalType(Type)`](#method-isgeneratedinternaltype-type) | 判断类型是否为编译器或 Unity 源生成的内部类型（如 <PrivateImplementationDetails>、 UnitySourceGenerated* 等），这类类型不属于用户 API，不应为其生成文档。 |
| [`IsGeneratedInternalTypeName(string)`](#method-isgeneratedinternaltypename-string) | 判断类型完整名称是否属于编译器或 Unity 源生成的内部类型命名模式。 合法的 C# 源代码无法在类型名中产生尖括号，因此名称含尖括号的一定是编译器合成类型（如匿名类型、闭包类）。 |
| [`TreatedAsTypeDefaultValue(object, Type)`](#method-treatedastypedefaultvalue-object-type) | 提供的值被视为类型的默认值，返回 true 表示被视为类型的默认值，返回 false 表示不是。 |
| [`TryGetFormatedAttributeWithFullParameter(object, ref string)`](#method-trygetformatedattributewithfullparameter-object-ref-string) | 获取格式化的完整特性签名字符串，返回 true 表示该特性支持格式化为完整特性签名字符串，返回 false 表示不支持。 |
| [`ConvertToDocumentationFileName(string)`](#method-converttodocumentationfilename-string) | 将类型名称转换为文档文件名（不含扩展名）。 泛型尖括号转换为花括号（C# XML 文档注释 cref 规范）：尖括号在 Windows 文件名中非法， 而方括号在 Markdown 中是链接语法会导致引用失效，花括号则两者皆安全。 |
| [`GetAttributeNameWithoutSuffix(string)`](#method-getattributenamewithoutsuffix-string) | 获取没有后缀的 Attribute 名称 |
| [`GetFieldKeywordSnippet(bool, bool, bool)`](#method-getfieldkeywordsnippet-bool-bool-bool) | 获取字段的关键字片段字符串 |
| [`GetFormattedDefaultValue(Type, object)`](#method-getformatteddefaultvalue-type-object) | 获取格式化的默认值字符串，用于生成签名 |

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

### IsGeneratedInternalType(Type) {#method-isgeneratedinternaltype-type}

判断类型是否为编译器或 Unity 源生成的内部类型（如 <PrivateImplementationDetails>、 UnitySourceGenerated* 等），这类类型不属于用户 API，不应为其生成文档。

``` csharp
public static bool IsGeneratedInternalType(Type type)
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
| `bool` | — |

</div>

### IsGeneratedInternalTypeName(string) {#method-isgeneratedinternaltypename-string}

判断类型完整名称是否属于编译器或 Unity 源生成的内部类型命名模式。 合法的 C# 源代码无法在类型名中产生尖括号，因此名称含尖括号的一定是编译器合成类型（如匿名类型、闭包类）。

``` csharp
public static bool IsGeneratedInternalTypeName(string typeFullName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `typeFullName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TreatedAsTypeDefaultValue(object, Type) {#method-treatedastypedefaultvalue-object-type}

提供的值被视为类型的默认值，返回 true 表示被视为类型的默认值，返回 false 表示不是。

``` csharp
public static bool TreatedAsTypeDefaultValue(object value, Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `value` | `object` | — |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetFormatedAttributeWithFullParameter(object, ref string) {#method-trygetformatedattributewithfullparameter-object-ref-string}

获取格式化的完整特性签名字符串，返回 true 表示该特性支持格式化为完整特性签名字符串，返回 false 表示不支持。

``` csharp
public static bool TryGetFormatedAttributeWithFullParameter(object attrInstance, out ref string attributeFullSignature)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `attrInstance` | `object` | — |
| `attributeFullSignature` | `ref string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### ConvertToDocumentationFileName(string) {#method-converttodocumentationfilename-string}

将类型名称转换为文档文件名（不含扩展名）。 泛型尖括号转换为花括号（C# XML 文档注释 cref 规范）：尖括号在 Windows 文件名中非法， 而方括号在 Markdown 中是链接语法会导致引用失效，花括号则两者皆安全。

``` csharp
public static string ConvertToDocumentationFileName(string typeName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `typeName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetAttributeNameWithoutSuffix(string) {#method-getattributenamewithoutsuffix-string}

获取没有后缀的 Attribute 名称

``` csharp
public static string GetAttributeNameWithoutSuffix(string attributeName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `attributeName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetFieldKeywordSnippet(bool, bool, bool) {#method-getfieldkeywordsnippet-bool-bool-bool}

获取字段的关键字片段字符串

``` csharp
public static string GetFieldKeywordSnippet(bool isConst, bool isStatic, bool isReadOnly)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `isConst` | `bool` | — |
| `isStatic` | `bool` | — |
| `isReadOnly` | `bool` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetFormattedDefaultValue(Type, object) {#method-getformatteddefaultvalue-type-object}

获取格式化的默认值字符串，用于生成签名

``` csharp
public static string GetFormattedDefaultValue(Type memberType, object value)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `memberType` | `Type` | — |
| `value` | `object` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
