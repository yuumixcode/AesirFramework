---
title: AddressablesSupportDisabledException
description: "Runestone.AesirModules.AddressablesSupportDisabledException 的 API 文档"
---

# `AddressablesSupportDisabledException`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.Exception` → `System.SystemException` → `System.InvalidOperationException` → `Runestone.AesirModules.SceneAssetWrapperException` → `AddressablesSupportDisabledException`

**实现接口:** `System.Runtime.InteropServices._Exception`，`System.Runtime.Serialization.ISerializable`

## 声明

``` csharp
public class AddressablesSupportDisabledException : Runestone.AesirModules.SceneAssetWrapperException, 
System.Runtime.InteropServices._Exception, 
System.Runtime.Serialization.ISerializable
```

当前项目未安装 Addressables 包时访问了 Addressables 相关 API。

**备注**

最小惊讶原则：Addressables 相关 API 在未安装包时依旧可见、可编译（卸载包不会导致任何编译错误）， 但运行期访问会抛出本异常。安装 com.unity.addressables 包后无需任何代码改动即可直接生效。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddressablesSupportDisabledException()`](#constructor-addressablessupportdisabledexception) | — |

</div>

### AddressablesSupportDisabledException() {#constructor-addressablessupportdisabledexception}

``` csharp
public AddressablesSupportDisabledException()
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
