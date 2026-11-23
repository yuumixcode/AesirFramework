---
title: EmptySceneAssetWrapperException
description: "Runestone.AesirModules.EmptySceneAssetWrapperException 的 API 文档"
---

# `EmptySceneAssetWrapperException`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.Exception` → `System.SystemException` → `System.InvalidOperationException` → `Runestone.AesirModules.SceneAssetWrapperException` → `EmptySceneAssetWrapperException`

**实现接口:** `System.Runtime.InteropServices._Exception`，`System.Runtime.Serialization.ISerializable`

## 声明

``` csharp
public class EmptySceneAssetWrapperException : Runestone.AesirModules.SceneAssetWrapperException, 
System.Runtime.InteropServices._Exception, 
System.Runtime.Serialization.ISerializable
```

访问了未分配任何场景的 SceneAssetWrapper。

**备注**

修复：在 Inspector 中给字段拖入 SceneAsset，或使用 FromScenePath 构造。 规避：先检查 State 是否安全，或改用对应的 TryGet 方法。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`EmptySceneAssetWrapperException()`](#constructor-emptysceneassetwrapperexception) | — |

</div>

### EmptySceneAssetWrapperException() {#constructor-emptysceneassetwrapperexception}

``` csharp
public EmptySceneAssetWrapperException()
```

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
