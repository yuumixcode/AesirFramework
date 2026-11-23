---
title: BinderBaseTypeAttribute
description: "Runestone.AesirModules.BinderBaseTypeAttribute 的 API 文档"
---

# `BinderBaseTypeAttribute`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `System.Attribute` → `BinderBaseTypeAttribute`

**实现接口:** `System.Runtime.InteropServices._Attribute`

## 声明

``` csharp
[AttributeUsage]
public sealed class BinderBaseTypeAttribute : System.Attribute, 
System.Runtime.InteropServices._Attribute
```

标记一个 MonoBehaviour 派生类可作为 Binder 生成脚本的基类（用户自定义基类的扩展入口）。
Aesir 面板家族（AesirBasePanel、AesirBasePanelView<T>、 AesirBasePanelViewController<T>）由 BinderAssistant 内置预选，无需标注。

本特性与整个 Binder 功能一同收录于 Odin 程序集（Runtime/UI/OdinInspector/Binder/）： Binder 的类型选择器（组件/基类的 ValueDropdown）强依赖 Odin Inspector。 用户程序集引用 Runestone.AesirModules.OdinInspector 后即可标注 （Assembly-CSharp 经 autoReferenced 自动引用；自定义 asmdef 需显式引用； 卸载 Odin Inspector 后相关标注代码需自行条件编译）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`BinderBaseTypeAttribute()`](#constructor-binderbasetypeattribute) | — |

</div>

### BinderBaseTypeAttribute() {#constructor-binderbasetypeattribute}

``` csharp
public BinderBaseTypeAttribute()
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
