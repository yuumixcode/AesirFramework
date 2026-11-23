---
title: AesirModulesDebug
description: "Runestone.AesirModules.AesirModulesDebug 的 API 文档"
---

# `AesirModulesDebug`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `AesirModulesDebug`

## 声明

``` csharp
public static class AesirModulesDebug
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirModulesTag`](#field-aesirmodulestag) | — |
| [`AudioModuleTag`](#field-audiomoduletag) | — |
| [`EventModuleTag`](#field-eventmoduletag) | — |
| [`ObjectBinderTag`](#field-objectbindertag) | — |
| [`SceneModuleTag`](#field-scenemoduletag) | — |
| [`ScriptDocGeneratorTag`](#field-scriptdocgeneratortag) | ScriptDocGenerator 模块前缀。该模块的可读日志此前散落在 Debug.Log* 与多种裸前缀上 （含无前缀），统一收敛到门面后以其为模块标识。 |
| [`UIModuleTag`](#field-uimoduletag) | — |

</div>

### AesirModulesTag {#field-aesirmodulestag}

``` csharp
public const string AesirModulesTag = "[AesirModules]";
```

### AudioModuleTag {#field-audiomoduletag}

``` csharp
public const string AudioModuleTag = "[AudioModule]";
```

### EventModuleTag {#field-eventmoduletag}

``` csharp
public const string EventModuleTag = "[EventModule]";
```

### ObjectBinderTag {#field-objectbindertag}

``` csharp
public const string ObjectBinderTag = "[ObjectBinder]";
```

### SceneModuleTag {#field-scenemoduletag}

``` csharp
public const string SceneModuleTag = "[SceneModule]";
```

### ScriptDocGeneratorTag {#field-scriptdocgeneratortag}

ScriptDocGenerator 模块前缀。该模块的可读日志此前散落在 Debug.Log* 与多种裸前缀上 （含无前缀），统一收敛到门面后以其为模块标识。

``` csharp
public const string ScriptDocGeneratorTag = "[ScriptDocGenerator]";
```

### UIModuleTag {#field-uimoduletag}

``` csharp
public const string UIModuleTag = "[UIModule]";
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Log(object, string, string)`](#method-log-object-string-string) | 输出 Log 级别消息，附带来源标识 |
| [`Log(string, string)`](#method-log-string-string) | 输出 Log 级别消息 |
| [`LogError(object, string, string)`](#method-logerror-object-string-string) | 输出 Error 级别消息，附带来源标识 |
| [`LogError(string, string)`](#method-logerror-string-string) | 输出 Error 级别消息 |
| [`LogTestInfo(object, string, string)`](#method-logtestinfo-object-string-string) | 输出单元测试日志消息，附带来源标识 |
| [`LogTestInfo(string, string)`](#method-logtestinfo-string-string) | 输出单元测试日志消息。 仅在定义了 UNITY_INCLUDE_TESTS 的程序集中生效，非测试构建自动剔除调用。 |
| [`LogWarning(object, string, string)`](#method-logwarning-object-string-string) | 输出 Warning 级别消息，附带来源标识 |
| [`LogWarning(string, string)`](#method-logwarning-string-string) | 输出 Warning 级别消息 |

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

### Log(object, string, string) {#method-log-object-string-string}

输出 Log 级别消息，附带来源标识

``` csharp
[Conditional]
public static void Log(object source, string prefixTag, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `object` | — |
| `prefixTag` | `string` | — |
| `message` | `string` | — |

</div>

### Log(string, string) {#method-log-string-string}

输出 Log 级别消息

``` csharp
[Conditional]
public static void Log(string prefixTag, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `prefixTag` | `string` | — |
| `message` | `string` | — |

</div>

### LogError(object, string, string) {#method-logerror-object-string-string}

输出 Error 级别消息，附带来源标识

``` csharp
public static void LogError(object source, string prefixTag, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `object` | — |
| `prefixTag` | `string` | — |
| `message` | `string` | — |

</div>

### LogError(string, string) {#method-logerror-string-string}

输出 Error 级别消息

``` csharp
public static void LogError(string prefixTag, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `prefixTag` | `string` | — |
| `message` | `string` | — |

</div>

### LogTestInfo(object, string, string) {#method-logtestinfo-object-string-string}

输出单元测试日志消息，附带来源标识

``` csharp
[Conditional]
public static void LogTestInfo(object source, string prefixTag, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `object` | — |
| `prefixTag` | `string` | — |
| `message` | `string` | — |

</div>

### LogTestInfo(string, string) {#method-logtestinfo-string-string}

输出单元测试日志消息。
仅在定义了 UNITY_INCLUDE_TESTS 的程序集中生效，非测试构建自动剔除调用。

``` csharp
[Conditional]
public static void LogTestInfo(string prefixTag, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `prefixTag` | `string` | — |
| `message` | `string` | — |

</div>

### LogWarning(object, string, string) {#method-logwarning-object-string-string}

输出 Warning 级别消息，附带来源标识

``` csharp
[Conditional]
public static void LogWarning(object source, string prefixTag, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `object` | — |
| `prefixTag` | `string` | — |
| `message` | `string` | — |

</div>

### LogWarning(string, string) {#method-logwarning-string-string}

输出 Warning 级别消息

``` csharp
[Conditional]
public static void LogWarning(string prefixTag, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `prefixTag` | `string` | — |
| `message` | `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
