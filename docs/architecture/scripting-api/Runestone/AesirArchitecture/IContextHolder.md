---
title: IContextHolder
description: "Runestone.AesirArchitecture.IContextHolder 的 API 文档"
---

# `IContextHolder`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

## 声明

``` csharp
public interface IContextHolder
```

上下文持有者接口。

**备注**

该接口是整个能力接口体系的根基，所有需要访问上下文的角色类型均继承此接口。
继承此接口的角色包括：IModel、IService、 View、Controller、Presenter、Command 以及 Query。

通过持有 IContext 引用，角色类型可以在运行时获取已注册的 Model、Service， 或执行命令与查询，从而实现模块间的松耦合协作。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Context`](#property-context) | — |

</div>

### Context {#property-context}

``` csharp
public IContext Context { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
