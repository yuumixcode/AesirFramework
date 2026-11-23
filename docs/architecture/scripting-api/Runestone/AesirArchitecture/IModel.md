---
title: IModel
description: "Runestone.AesirArchitecture.IModel 的 API 文档"
---

# `IModel`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.ICanInitialize`，`Runestone.AesirArchitecture.ICanGetModel`，`System.IDisposable`

## 声明

``` csharp
public interface IModel : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanInitialize, 
Runestone.AesirArchitecture.ICanGetModel, 
System.IDisposable
```

数据层接口。持有状态（通常使用 ObservableValue{T}）。
能力：GetModel, Initialize, Dispose

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
