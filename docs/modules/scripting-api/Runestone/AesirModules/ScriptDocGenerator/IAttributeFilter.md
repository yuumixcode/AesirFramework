---
title: IAttributeFilter
description: "Runestone.AesirModules.ScriptDocGenerator.IAttributeFilter 的 API 文档"
---

# `IAttributeFilter`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

## 声明

``` csharp
public interface IAttributeFilter
```

特性过滤器接口，用于过滤掉不需要的特性

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ExcludeTypes`](#property-excludetypes) | — |

</div>

### ExcludeTypes {#property-excludetypes}

``` csharp
public Type[] ExcludeTypes { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ShouldFilterOut(Type)`](#method-shouldfilterout-type) | 判断传入的特性类型是否应该被过滤掉 |

</div>

### ShouldFilterOut(Type) {#method-shouldfilterout-type}

判断传入的特性类型是否应该被过滤掉

``` csharp
public abstract bool ShouldFilterOut(Type type)
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

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
