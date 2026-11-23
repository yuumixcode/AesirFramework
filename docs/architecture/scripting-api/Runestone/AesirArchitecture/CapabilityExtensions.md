---
title: CapabilityExtensions
description: "Runestone.AesirArchitecture.CapabilityExtensions 的 API 文档"
---

# `CapabilityExtensions`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `CapabilityExtensions`

## 声明

``` csharp
[Extension]
public static class CapabilityExtensions
```

能力扩展方法集合

**备注**

能力接口组合模式是本架构角色系统的核心设计。
角色类型（如 Model、Service、Command、Query 等）通过组合不同的细粒度能力接口 （ICanGetModel、ICanGetService、ICanExecuteCommand、 ICanExecuteQuery）来声明自己可以执行的操作，而非通过继承庞大的基类获得全部权限。

本类中的扩展方法通过接口约束（where T : ICanXxx）确保类型安全—— 只有显式声明了对应能力接口的类型才能调用相应的扩展方法，将访问控制前置到编译期。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetModel(ICanGetModel)`](#method-getmodel-icangetmodel) | 获取已注册的 Model。未注册时由 GetModel{T} 抛出异常； 已注册但尚未初始化时，抛出注册顺序错误或循环依赖异常。 |
| [`GetService(ICanGetService)`](#method-getservice-icangetservice) | 获取已注册的 Service。未注册时由 GetService{T} 抛出异常； 已注册但尚未初始化时抛出——Service 间依赖为注册顺序问题， 手动实现 ICanGetService 的 Model 在初始化阶段调用则属两阶段初始化的必然约束。 |
| [`ExecuteQuery(ICanExecuteQuery)`](#method-executequery-icanexecutequery) | 执行无参查询 |
| [`ExecuteQuery(ICanExecuteQuery, IQuery<TResult>)`](#method-executequery-icanexecutequery-iquery-tresult) | 执行带参查询 |
| [`ExecuteCommand(ICanExecuteCommand)`](#method-executecommand-icanexecutecommand) | 执行无参命令 |
| [`ExecuteCommand(ICanExecuteCommand, T)`](#method-executecommand-icanexecutecommand-t) | 执行带参命令 |

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

### GetModel(ICanGetModel) {#method-getmodel-icangetmodel}

获取已注册的 Model。未注册时由 GetModel{T} 抛出异常； 已注册但尚未初始化时，抛出注册顺序错误或循环依赖异常。

``` csharp
[Extension]
[Ext] public static T GetModel<T>(this ICanGetModel self)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `self` | `ICanGetModel` | 调用方实例，必须已持有有效的上下文引用 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 已注册且已初始化完成的 Model 实例 |

</div>

### GetService(ICanGetService) {#method-getservice-icangetservice}

获取已注册的 Service。未注册时由 GetService{T} 抛出异常； 已注册但尚未初始化时抛出——Service 间依赖为注册顺序问题， 手动实现 ICanGetService 的 Model 在初始化阶段调用则属两阶段初始化的必然约束。

``` csharp
[Extension]
[Ext] public static T GetService<T>(this ICanGetService self)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `self` | `ICanGetService` | 调用方实例，必须已持有有效的上下文引用 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 已注册且已初始化完成的 Service 实例 |

</div>

### ExecuteQuery(ICanExecuteQuery) {#method-executequery-icanexecutequery}

执行无参查询

**备注**

执行前经 SetContext 注入当前上下文， 使查询在 Execute 内部具备 GetModel / GetService 能力。

``` csharp
[Extension]
[Ext] public static TResult ExecuteQuery<TQuery, TResult>(this ICanExecuteQuery self)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `self` | `ICanExecuteQuery` | 调用方实例，必须已持有有效的上下文引用 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TResult` | 查询执行结果 |

</div>

### ExecuteQuery(ICanExecuteQuery, IQuery<TResult>) {#method-executequery-icanexecutequery-iquery-tresult}

执行带参查询

**备注**

执行前经 SetContext 注入当前上下文， 使查询在 Execute 内部具备 GetModel / GetService 能力。

``` csharp
[Extension]
[Ext] public static TResult ExecuteQuery<TResult>(this ICanExecuteQuery self, IQuery<TResult> query)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `self` | `ICanExecuteQuery` | 调用方实例，必须已持有有效的上下文引用 |
| `query` | `IQuery<TResult>` | 要执行的查询实例 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TResult` | 查询执行结果 |

</div>

### ExecuteCommand(ICanExecuteCommand) {#method-executecommand-icanexecutecommand}

执行无参命令

**备注**

执行前经 SetContext 注入当前上下文， 使命令在 Execute 内部具备 GetModel / GetService 能力。

``` csharp
[Extension]
[Ext] public static void ExecuteCommand<T>(this ICanExecuteCommand self)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `self` | `ICanExecuteCommand` | 调用方实例，必须已持有有效的上下文引用 |

</div>

### ExecuteCommand(ICanExecuteCommand, T) {#method-executecommand-icanexecutecommand-t}

执行带参命令

**备注**

执行前经 SetContext 注入当前上下文， 使命令在 Execute 内部具备 GetModel / GetService 能力。

``` csharp
[Extension]
[Ext] public static void ExecuteCommand<T>(this ICanExecuteCommand self, T command)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `self` | `ICanExecuteCommand` | 调用方实例，必须已持有有效的上下文引用 |
| `command` | `T` | 要执行的命令实例 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
