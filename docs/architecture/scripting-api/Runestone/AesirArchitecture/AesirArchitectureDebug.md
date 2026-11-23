---
title: AesirArchitectureDebug
description: "Runestone.AesirArchitecture.AesirArchitectureDebug 的 API 文档"
---

# `AesirArchitectureDebug`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `AesirArchitectureDebug`

## 声明

``` csharp
public static class AesirArchitectureDebug
```

AesirArchitecture 内部日志工具。
所有架构模块的日志输出应走此工具，以醒目的颜色和 [AesirArchitecture] 标识区分来源。 Log/Warning 通过 [Conditional] 在打包时自动剔除；Error 始终保留。

**备注**

编译行为差异： Log 与 LogWarning 系列方法标注了 [Conditional("UNITY_EDITOR")]， 在非编辑器构建时编译器会自动移除所有调用点，不会产生任何运行时开销。 LogError 系列方法未使用 [Conditional] 特性，在所有构建中均保留， 因为错误日志在生产环境中同样需要可见，以便定位问题。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ErrorTag`](#field-errortag) | Error 级别的富文本标签，供异常消息复用以保持控制台输出风格一致。 |

</div>

### ErrorTag {#field-errortag}

Error 级别的富文本标签，供异常消息复用以保持控制台输出风格一致。

**备注**

此常量供 CapabilityExtensions 的异常消息复用， 确保抛出的异常在控制台中与直接调用 LogError(string) 保持一致的 [AesirArchitecture] 标识风格， 便于开发者快速识别错误来源。

``` csharp
public const string ErrorTag = "<color=#FF4444><b>[AesirArchitecture]</b></color>";
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Log(object, string)`](#method-log-object-string) | 输出 Log 级别消息，附带来源标识 |
| [`Log(string)`](#method-log-string) | 输出 Log 级别消息 |
| [`LogError(object, string)`](#method-logerror-object-string) | 输出 Error 级别消息，附带来源标识 |
| [`LogError(string)`](#method-logerror-string) | 输出 Error 级别消息 |
| [`LogTestInfo(object, string)`](#method-logtestinfo-object-string) | 输出单元测试日志消息，附带来源标识 |
| [`LogTestInfo(string)`](#method-logtestinfo-string) | 输出单元测试日志消息。 仅在定义了 UNITY_INCLUDE_TESTS 的程序集中生效，非测试构建自动剔除调用。 |
| [`LogWarning(object, string)`](#method-logwarning-object-string) | 输出 Warning 级别消息，附带来源标识 |
| [`LogWarning(string)`](#method-logwarning-string) | 输出 Warning 级别消息 |

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

### Log(object, string) {#method-log-object-string}

输出 Log 级别消息，附带来源标识

``` csharp
[Conditional]
public static void Log(object source, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `object` | 日志来源标识，通常传入 nameof(TypeName) 以便快速定位日志产生的模块 |
| `message` | `string` | 日志内容 |

</div>

### Log(string) {#method-log-string}

输出 Log 级别消息

``` csharp
[Conditional]
public static void Log(string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `message` | `string` | — |

</div>

### LogError(object, string) {#method-logerror-object-string}

输出 Error 级别消息，附带来源标识

``` csharp
public static void LogError(object source, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `object` | 日志来源标识，通常传入 nameof(TypeName) 以便快速定位日志产生的模块 |
| `message` | `string` | 错误内容 |

</div>

### LogError(string) {#method-logerror-string}

输出 Error 级别消息

``` csharp
public static void LogError(string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `message` | `string` | — |

</div>

### LogTestInfo(object, string) {#method-logtestinfo-object-string}

输出单元测试日志消息，附带来源标识

**备注**

该方法使用 [Conditional("UNITY_INCLUDE_TESTS")] 特性， 仅在包含测试代码的程序集编译时保留调用，正式发布构建中自动移除。

``` csharp
[Conditional]
public static void LogTestInfo(object source, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `object` | 日志来源标识，通常传入 nameof(TypeName) 以便快速定位日志产生的模块 |
| `message` | `string` | 测试日志内容 |

</div>

### LogTestInfo(string) {#method-logtestinfo-string}

输出单元测试日志消息。
仅在定义了 UNITY_INCLUDE_TESTS 的程序集中生效，非测试构建自动剔除调用。

**备注**

该方法使用 [Conditional("UNITY_INCLUDE_TESTS")] 特性， 仅在包含测试代码的程序集编译时保留调用。在正式发布构建中 Unity 不会定义 UNITY_INCLUDE_TESTS， 所有调用点都会被编译器自动移除，确保测试日志不会泄漏到生产环境。

``` csharp
[Conditional]
public static void LogTestInfo(string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `message` | `string` | 测试日志内容 |

</div>

### LogWarning(object, string) {#method-logwarning-object-string}

输出 Warning 级别消息，附带来源标识

``` csharp
[Conditional]
public static void LogWarning(object source, string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `object` | 日志来源标识，通常传入 nameof(TypeName) 以便快速定位日志产生的模块 |
| `message` | `string` | 警告内容 |

</div>

### LogWarning(string) {#method-logwarning-string}

输出 Warning 级别消息

``` csharp
[Conditional]
public static void LogWarning(string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `message` | `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
