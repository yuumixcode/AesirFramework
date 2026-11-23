---
title: SceneAssetWrapperUnsafeReason
description: "Runestone.AesirModules.SceneAssetWrapperUnsafeReason 的 API 文档"
---

# `SceneAssetWrapperUnsafeReason`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `SceneAssetWrapperUnsafeReason`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum SceneAssetWrapperUnsafeReason : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

描述 SceneAssetWrapper 不安全的具体原因。

**备注**

Empty 优先级最高。对位 Eflatun.SceneReference 的 SceneReferenceUnsafeReason： NotInMaps 一项在本实现中不存在——运行时直接使用序列化的路径/地址数据，没有映射表的概念。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Empty`](#field-empty) | 空引用，未分配任何场景。 |
| [`None`](#field-none) | 引用安全可用。 |
| [`NotInBuild`](#field-notinbuild) | — |

</div>

### Empty {#field-empty}

空引用，未分配任何场景。

``` csharp
public const SceneAssetWrapperUnsafeReason Empty;
```

### None {#field-none}

引用安全可用。

``` csharp
public const SceneAssetWrapperUnsafeReason None;
```

### NotInBuild {#field-notinbuild}

``` csharp
public const SceneAssetWrapperUnsafeReason NotInBuild;
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `HasFlag(Enum)` | — | `Enum` |
| `Equals(object)` | — | `Enum` |
| `GetHashCode()` | — | `Enum` |
| `ToString()` | — | `Enum` |
| `ToString(string)` | — | `Enum` |
| `GetTypeCode()` | — | `Enum` |
| `CompareTo(object)` | — | `Enum` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `ToString(IFormatProvider)` | — | `Enum` |
| `ToString(string, IFormatProvider)` | — | `Enum` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
