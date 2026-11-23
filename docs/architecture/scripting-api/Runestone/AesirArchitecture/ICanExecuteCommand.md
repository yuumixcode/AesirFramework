---
title: ICanExecuteCommand
description: "Runestone.AesirArchitecture.ICanExecuteCommand 的 API 文档"
---

# `ICanExecuteCommand`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.IContextHolder`

## 声明

``` csharp
public interface ICanExecuteCommand : Runestone.AesirArchitecture.IContextHolder
```

执行命令的能力接口

**备注**

标记接口——ExecuteCommand 能力由 ExecuteCommand{T}(ICanExecuteCommand, T) 扩展方法提供， 仅声明此接口的类型可调用（编译期访问控制）。

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
