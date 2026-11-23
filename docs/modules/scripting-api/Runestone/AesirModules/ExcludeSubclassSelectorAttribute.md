---
title: ExcludeSubclassSelectorAttribute
description: "Runestone.AesirModules.ExcludeSubclassSelectorAttribute 的 API 文档"
---

# `ExcludeSubclassSelectorAttribute`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.Attribute` → `ExcludeSubclassSelectorAttribute`

**实现接口:** `System.Runtime.InteropServices._Attribute`

## 声明

``` csharp
[AttributeUsage]
public class ExcludeSubclassSelectorAttribute : System.Attribute, 
System.Runtime.InteropServices._Attribute
```

排除特性。标记不想出现在 SubclassSelectorAttribute 下拉中的类型 （如抽象中间层、仅供程序内部使用的事件参数）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ExcludeSubclassSelectorAttribute()`](#constructor-excludesubclassselectorattribute) | — |

</div>

### ExcludeSubclassSelectorAttribute() {#constructor-excludesubclassselectorattribute}

``` csharp
public ExcludeSubclassSelectorAttribute()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `TypeId` | — | `Attribute` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `Attribute` |
| `GetHashCode()` | — | `Attribute` |
| `IsDefaultAttribute()` | — | `Attribute` |
| `Match(object)` | — | `Attribute` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
