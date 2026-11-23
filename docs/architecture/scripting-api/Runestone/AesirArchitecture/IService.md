---
title: IService
description: "Runestone.AesirArchitecture.IService 的 API 文档"
---

# `IService`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.ICanInitialize`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`，`System.IDisposable`

## 声明

``` csharp
public interface IService : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanInitialize, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService, 
System.IDisposable
```

服务层接口。万能协调层，封装跨模块业务逻辑，协调模块间交互与通信。
Service 能读写 Model、调用其他 Service，完成跨模块协调。 不包含 ICanExecuteCommand 和 ICanExecuteQuery——Command/Query 的执行入口应由 Controller/Presenter 触发。

能力：GetModel, GetService, Initialize, Dispose

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
