---
title: IView<T>
description: "Runestone.AesirArchitecture.IView<T> 的 API 文档"
---

# `IView<T>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.IView`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`

**类型参数**

- `T` — 上下文类型，必须继承自 AbstractContext{T} 并提供无参构造。

## 声明

``` csharp
public interface IView<T> : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.IView, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService where T : new(), Runestone.AesirArchitecture.AbstractContext<T>
```

泛型表现层接口。绑定指定上下文类型，实现者自动获得 Context 绑定。

**备注**

通过默认接口实现（DIM）将 Context 自动绑定到 Instance 单例，无需手动注入上下文。 此设计使 View 与具体上下文类型解耦——只需声明泛型参数即可获得对应模块的全局只读访问权， 能力面仍由非泛型 IView 约束（不含命令执行入口）。

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
