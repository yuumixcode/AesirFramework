---
title: InternalContextAttribute
description: "Runestone.AesirArchitecture.InternalContextAttribute 的 API 文档"
---

# `InternalContextAttribute`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.Attribute` → `InternalContextAttribute`

**实现接口:** `System.Runtime.InteropServices._Attribute`

## 声明

``` csharp
[AttributeUsage]
public sealed class InternalContextAttribute : System.Attribute, 
System.Runtime.InteropServices._Attribute
```

标记一个 AbstractContext{T} 派生类为框架内部 Context（示例 / 测试等非用户工作流用途）。
被标记的 Context 不会出现在用户工作流的 Context 选择器中 （如 AesirModules Binder 的「Context 类型」下拉会跳过被标记的类型）。 框架自带的示例与测试 Context 均已标注。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`InternalContextAttribute()`](#constructor-internalcontextattribute) | — |

</div>

### InternalContextAttribute() {#constructor-internalcontextattribute}

``` csharp
public InternalContextAttribute()
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
