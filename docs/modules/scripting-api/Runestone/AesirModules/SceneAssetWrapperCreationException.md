---
title: SceneAssetWrapperCreationException
description: "Runestone.AesirModules.SceneAssetWrapperCreationException 的 API 文档"
---

# `SceneAssetWrapperCreationException`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.Exception` → `System.SystemException` → `System.InvalidOperationException` → `Runestone.AesirModules.SceneAssetWrapperException` → `SceneAssetWrapperCreationException`

**实现接口:** `System.Runtime.InteropServices._Exception`，`System.Runtime.Serialization.ISerializable`

## 声明

``` csharp
public class SceneAssetWrapperCreationException : Runestone.AesirModules.SceneAssetWrapperException, 
System.Runtime.InteropServices._Exception, 
System.Runtime.Serialization.ISerializable
```

通过工厂或构造方法创建 SceneAssetWrapper 时入参无效。

**备注**

错误描述中包含具体的无效原因与修复建议。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneAssetWrapperCreationException(string)`](#constructor-sceneassetwrappercreationexception-string) | — |
| [`SceneAssetWrapperCreationException(string, Exception)`](#constructor-sceneassetwrappercreationexception-string-exception) | — |

</div>

### SceneAssetWrapperCreationException(string) {#constructor-sceneassetwrappercreationexception-string}

``` csharp
public SceneAssetWrapperCreationException(string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `message` | `string` | — |

</div>

### SceneAssetWrapperCreationException(string, Exception) {#constructor-sceneassetwrappercreationexception-string-exception}

``` csharp
public SceneAssetWrapperCreationException(string message, Exception innerException)
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
