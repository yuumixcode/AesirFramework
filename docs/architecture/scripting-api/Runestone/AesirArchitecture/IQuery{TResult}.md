---
title: IQuery<TResult>
description: "Runestone.AesirArchitecture.IQuery<TResult> 的 API 文档"
---

# `IQuery<TResult>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.ICanExecuteQuery`，`Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`

**类型参数**

- `TResult` — 查询结果类型

## 声明

``` csharp
public interface IQuery<TResult> : Runestone.AesirArchitecture.ICanExecuteQuery, 
Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService 
```

查询接口。通过 Query 执行读操作并返回结果，无副作用。
与 ICommand 的区别：Command 负责写操作（无返回值），Query 负责读操作（返回 TResult）。

能力：GetModel, GetService, ExecuteQuery

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Execute()`](#method-execute) | 执行查询并返回结果 |

</div>

### Execute() {#method-execute}

执行查询并返回结果

``` csharp
public abstract TResult Execute()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TResult` | 查询结果，类型为 TResult。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
