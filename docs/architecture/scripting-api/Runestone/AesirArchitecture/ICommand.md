---
title: ICommand
description: "Runestone.AesirArchitecture.ICommand 的 API 文档"
---

# `ICommand`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`，`Runestone.AesirArchitecture.ICanExecuteCommand`

## 声明

``` csharp
public interface ICommand : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService, 
Runestone.AesirArchitecture.ICanExecuteCommand
```

同步命令接口。通过 Command 修改 Model 状态，只写无返回值。
能力：GetModel, GetService, ExecuteCommand

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Execute()`](#method-execute) | 执行命令 |

</div>

### Execute() {#method-execute}

执行命令

**备注**

执行前由 CapabilityExtensions.ExecuteCommand 自动注入上下文， 使命令在执行时具备 GetModel<T> / GetService<T> 能力。 实现者不应手动调用此方法，应统一经由能力扩展方法触发。

``` csharp
public abstract void Execute()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
