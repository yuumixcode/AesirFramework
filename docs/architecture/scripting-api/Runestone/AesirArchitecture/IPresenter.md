---
title: IPresenter
description: "Runestone.AesirArchitecture.IPresenter 的 API 文档"
---

# `IPresenter`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.ICanExecuteQuery`，`Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`，`System.IDisposable`，`Runestone.AesirArchitecture.ICanExecuteCommand`

**类型参数**

- `T` — 上下文类型，必须继承 AbstractContext{T} 并提供无参构造。

## 声明

``` csharp
public interface IPresenter : Runestone.AesirArchitecture.ICanExecuteQuery, 
Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService, 
System.IDisposable, 
Runestone.AesirArchitecture.ICanExecuteCommand
```

泛型 MVP 中介接口。绑定指定上下文类型，实现者自动获得 Context 绑定。

**备注**

通过显式接口实现 Context 自动绑定到 Instance 单例，无需手动注入上下文。 此设计使 Presenter 与具体上下文类型解耦——只需声明泛型参数即可获得对应模块的全局上下文访问权。

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
