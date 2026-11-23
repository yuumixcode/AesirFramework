---
title: BinderCodeGenerator.BindUnit
description: "Runestone.AesirModules.BinderCodeGenerator.BindUnit 的 API 文档"
---

# `BinderCodeGenerator.BindUnit`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `System.ValueType` → `BinderCodeGenerator.BindUnit`

## 声明

``` csharp
[IsReadOnly]
internal struct BinderCodeGenerator.BindUnit : System.ValueType
```

单个绑定单元在代码生成阶段的只读描述。
HierarchyPath 为空字符串表示绑定 BinderAssistant 所在物体自身， 生成代码将直接调用 GetComponent 而不经过 transform.Find。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `ValueType` |
| `GetHashCode()` | — | `ValueType` |
| `ToString()` | — | `ValueType` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
