---
title: SceneAssetWrapperException
description: "Runestone.AesirModules.SceneAssetWrapperException 的 API 文档"
---

# `SceneAssetWrapperException`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.Exception` → `System.SystemException` → `System.InvalidOperationException` → `SceneAssetWrapperException`

**实现接口:** `System.Runtime.InteropServices._Exception`，`System.Runtime.Serialization.ISerializable`

## 声明

``` csharp
public class SceneAssetWrapperException : System.InvalidOperationException, 
System.Runtime.InteropServices._Exception, 
System.Runtime.Serialization.ISerializable
```

所有 SceneAssetWrapper 相关异常的基类，便于调用方统一捕获。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneAssetWrapperException()`](#constructor-sceneassetwrapperexception) | 使用默认错误的描述初始化。 |
| [`SceneAssetWrapperException(string)`](#constructor-sceneassetwrapperexception-string) | 使用默认错误的描述初始化。 |
| [`SceneAssetWrapperException(string, Exception)`](#constructor-sceneassetwrapperexception-string-exception) | 使用默认错误的描述初始化。 |

</div>

### SceneAssetWrapperException() {#constructor-sceneassetwrapperexception}

使用默认错误的描述初始化。

``` csharp
public SceneAssetWrapperException()
```

### SceneAssetWrapperException(string) {#constructor-sceneassetwrapperexception-string}

使用默认错误的描述初始化。

``` csharp
public SceneAssetWrapperException(string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `message` | `string` | — |

</div>

### SceneAssetWrapperException(string, Exception) {#constructor-sceneassetwrapperexception-string-exception}

使用默认错误的描述初始化。

``` csharp
public SceneAssetWrapperException(string message, Exception innerException)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `message` | `string` | — |
| `innerException` | `Exception` | — |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `InnerException` | — | `Exception` |
| `Data` | — | `Exception` |
| `TargetSite` | — | `Exception` |
| `HResult` | — | `Exception` |
| `HelpLink` | — | `Exception` |
| `Message` | — | `Exception` |
| `Source` | — | `Exception` |
| `StackTrace` | — | `Exception` |

</div>

## 事件

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `SerializeObjectState` | — | `Exception` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `ToString()` | — | `Exception` |
| `GetBaseException()` | — | `Exception` |
| `GetType()` | — | `Exception` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `GetObjectData(SerializationInfo, StreamingContext)` | — | `Exception` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
