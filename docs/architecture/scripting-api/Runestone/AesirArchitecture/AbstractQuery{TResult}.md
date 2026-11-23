---
title: AbstractQuery<TResult>
description: "Runestone.AesirArchitecture.AbstractQuery<TResult> 的 API 文档"
---

# `AbstractQuery<TResult>`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `AbstractQuery<TResult>`

**实现接口:** `Runestone.AesirArchitecture.ICanExecuteQuery`，`Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.IQuery<TResult>`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`

**类型参数**

- `TResult` — 查询结果类型，由子类的查询逻辑决定

## 声明

``` csharp
[Serializable]
public abstract class AbstractQuery<TResult> : Runestone.AesirArchitecture.ICanExecuteQuery, 
Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.IQuery<TResult>, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService 
```

查询基类。持有上下文引用，通过 OnExecute 执行查询逻辑并返回结果。

**备注**

Query 是只读操作，返回结果且无副作用——不修改任何 Model 状态。 与 AbstractCommand 的区别：Command 负责写操作且无返回值， Query 负责读操作并返回 TResult。 通过显式接口实现 SetContext 接收上下文注入， 使查询在执行时具备 GetModel<T> / GetService<T> 能力。 子类实现 OnExecute 返回查询结果，不应直接实现 Execute——后者已由本基类委托至 OnExecute。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnExecute()`](#method-onexecute) | 查询执行逻辑，子类必须实现 |

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

### OnExecute() {#method-onexecute}

查询执行逻辑，子类必须实现

**备注**

子类在此实现查询逻辑，通过 this.GetModel<T>() / this.GetService<T>() 读取模块状态并组装返回值。方法应为纯读操作，不产生任何副作用。

``` csharp
protected abstract TResult OnExecute()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TResult` | 查询结果，类型为 TResult。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
