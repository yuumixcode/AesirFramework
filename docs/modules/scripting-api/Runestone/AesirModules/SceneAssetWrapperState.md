---
title: SceneAssetWrapperState
description: "Runestone.AesirModules.SceneAssetWrapperState 的 API 文档"
---

# `SceneAssetWrapperState`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `SceneAssetWrapperState`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum SceneAssetWrapperState : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

SceneAssetWrapper 的可用状态。 Unsafe：引用不安全（空引用，或场景既不在 BuildSettings 也不可 Addressable） Regular：引用安全，指向 BuildSettings 中的常规场景 Addressable：引用安全，指向 Addressable 场景

**备注**

对位 Eflatun.SceneReference 的 SceneReferenceState：空引用没有独立状态值， 表现为 Unsafe，具体原因用 SceneAssetWrapperUnsafeReason 区分。 场景同时存在于 BuildSettings 与 Addressables 时以 Regular 优先 （BuildSettings 加载途径不依赖 Addressables 包，兼容性最好）。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Addressable`](#field-addressable) | 引用安全：场景在 Addressables 组中（地址数据经编辑器同步缓存，运行时纯数据判定）。 |
| [`Regular`](#field-regular) | 引用安全：场景已加入 BuildSettings 并启用。 |
| [`Unsafe`](#field-unsafe) | 引用不安全：空引用，或场景未加入 BuildSettings（或被禁用）且不可 Addressable。 |

</div>

### Addressable {#field-addressable}

引用安全：场景在 Addressables 组中（地址数据经编辑器同步缓存，运行时纯数据判定）。

``` csharp
public const SceneAssetWrapperState Addressable;
```

### Regular {#field-regular}

引用安全：场景已加入 BuildSettings 并启用。

``` csharp
public const SceneAssetWrapperState Regular;
```

### Unsafe {#field-unsafe}

引用不安全：空引用，或场景未加入 BuildSettings（或被禁用）且不可 Addressable。

``` csharp
public const SceneAssetWrapperState Unsafe;
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
