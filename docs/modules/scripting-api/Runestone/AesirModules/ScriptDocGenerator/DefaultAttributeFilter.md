---
title: DefaultAttributeFilter
description: "Runestone.AesirModules.ScriptDocGenerator.DefaultAttributeFilter 的 API 文档"
---

# `DefaultAttributeFilter`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `DefaultAttributeFilter`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IAttributeFilter`

## 声明

``` csharp
public class DefaultAttributeFilter : Runestone.AesirModules.ScriptDocGenerator.IAttributeFilter
```

默认特性过滤器，构造函数中传入需要排除的 Attribute 类型

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultAttributeFilter(Type[])`](#constructor-defaultattributefilter-type) | 创建默认特性过滤器 |

</div>

### DefaultAttributeFilter(Type[]) {#constructor-defaultattributefilter-type}

创建默认特性过滤器

``` csharp
public DefaultAttributeFilter(Type[] excludeTypes)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `excludeTypes` | `Type[]` | — |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ExcludeTypes`](#property-excludetypes) | 排除的特性类型 |

</div>

### ExcludeTypes {#property-excludetypes}

排除的特性类型

``` csharp
public Type[] ExcludeTypes { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ShouldFilterOut(Type)`](#method-shouldfilterout-type) | 判断传入的特性类型是否应该被过滤掉 |

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

### ShouldFilterOut(Type) {#method-shouldfilterout-type}

判断传入的特性类型是否应该被过滤掉

``` csharp
public bool ShouldFilterOut(Type type)
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
