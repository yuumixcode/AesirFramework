---
title: AbstractModel
description: "Runestone.AesirArchitecture.AbstractModel 的 API 文档"
---

# `AbstractModel`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `Runestone.AesirArchitecture.AbstractSubmodule` → `AbstractModel`

**实现接口:** `Runestone.AesirArchitecture.IModel`，`Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.ICanInitialize`，`Runestone.AesirArchitecture.ICanGetModel`，`System.IDisposable`

## 声明

``` csharp
[Serializable]
public abstract class AbstractModel : Runestone.AesirArchitecture.AbstractSubmodule, 
Runestone.AesirArchitecture.IModel, 
Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanInitialize, 
Runestone.AesirArchitecture.ICanGetModel, 
System.IDisposable
```

Model 基类。继承 AbstractSubmodule 获得生命周期管理，实现 IModel 标记数据层角色。

**备注**

Model 是数据层，持有状态（通常使用 ObservableValue{T}）。 状态仅通过 Command 写入，View 不直接调用 Model 的写入方法； Model 通过 IReadOnlyObservableValue<T> 向 View 暴露只读订阅， 确保数据流向单向可控——View 只能观察变化，不能回写。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `Initialized` | 是否已初始化（只读） | `AbstractSubmodule` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `Dispose()` | 释放资源，触发 OnDispose | `AbstractSubmodule` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `OnDispose()` | 释放时的清理逻辑，子类可覆写 | `AbstractSubmodule` |
| `OnInitialize()` | 初始化逻辑，子类可选覆写（默认空实现）。 | `AbstractSubmodule` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
