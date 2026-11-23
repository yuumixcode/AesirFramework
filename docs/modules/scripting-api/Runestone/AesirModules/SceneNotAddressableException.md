---
title: SceneNotAddressableException
description: "Runestone.AesirModules.SceneNotAddressableException 的 API 文档"
---

# `SceneNotAddressableException`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.Exception` → `System.SystemException` → `System.InvalidOperationException` → `Runestone.AesirModules.SceneAssetWrapperException` → `SceneNotAddressableException`

**实现接口:** `System.Runtime.InteropServices._Exception`，`System.Runtime.Serialization.ISerializable`

## 声明

``` csharp
public class SceneNotAddressableException : Runestone.AesirModules.SceneAssetWrapperException, 
System.Runtime.InteropServices._Exception, 
System.Runtime.Serialization.ISerializable
```

对非 Addressable 场景的 SceneAssetWrapper 访问了 Address。

**备注**

修复：把场景加入 Addressables 组（可在 Inspector 字段的工具按钮中一键加入），或改用 BuildSettings 加载途径。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneNotAddressableException()`](#constructor-scenenotaddressableexception) | — |

</div>

### SceneNotAddressableException() {#constructor-scenenotaddressableexception}

``` csharp
public SceneNotAddressableException()
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
