---
title: AbstractService
description: "Runestone.AesirArchitecture.AbstractService 的 API 文档"
---

# `AbstractService`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `Runestone.AesirArchitecture.AbstractSubmodule` → `AbstractService`

**实现接口:** `Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.IService`，`Runestone.AesirArchitecture.ICanInitialize`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`，`System.IDisposable`

## 声明

``` csharp
[Serializable]
public abstract class AbstractService : Runestone.AesirArchitecture.AbstractSubmodule, 
Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.IService, 
Runestone.AesirArchitecture.ICanInitialize, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService, 
System.IDisposable
```

Service 基类。继承 AbstractSubmodule 获得生命周期管理，实现 IService 标记服务层角色。

**备注**

Service 是跨模块协调层，可读写 Model、调用其他 Service 以封装跨模块业务逻辑， 但不包含 Command / Query 执行能力——Command/Query 的执行入口应由 Controller / Presenter 触发，避免 Service 成为逻辑黑洞。

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
