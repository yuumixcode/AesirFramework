---
title: AbstractCommand
description: "Runestone.AesirArchitecture.AbstractCommand 的 API 文档"
---

# `AbstractCommand`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `AbstractCommand`

**实现接口:** `Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`，`Runestone.AesirArchitecture.ICanExecuteCommand`，`Runestone.AesirArchitecture.ICommand`

## 声明

``` csharp
[Serializable]
public abstract class AbstractCommand : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService, 
Runestone.AesirArchitecture.ICanExecuteCommand, 
Runestone.AesirArchitecture.ICommand
```

命令基类。持有上下文引用，通过 OnExecute 执行命令逻辑。

**备注**

Command 是只写操作，用于修改 Model 状态，无返回值。 通过显式接口实现 SetContext 接收上下文注入， 使命令在执行时具备 GetModel<T> / GetService<T> 能力。 子类应实现 OnExecute 编写命令逻辑，不应直接实现 Execute——后者已由本基类委托至 OnExecute。 标记 SerializableAttribute 以支持序列化场景。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnExecute()`](#method-onexecute) | 命令执行逻辑，子类必须实现 |

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

命令执行逻辑，子类必须实现

**备注**

子类在此实现命令逻辑，通过 this.GetModel<T>() / this.GetService<T>() 访问模块上下文中的 Model 和 Service，完成状态修改。

``` csharp
protected abstract void OnExecute()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
