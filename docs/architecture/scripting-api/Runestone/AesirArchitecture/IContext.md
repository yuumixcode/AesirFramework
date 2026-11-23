---
title: IContext
description: "Runestone.AesirArchitecture.IContext 的 API 文档"
---

# `IContext`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `System.IDisposable`

## 声明

``` csharp
public interface IContext : System.IDisposable
```

模块上下文接口。提供模块注册与获取。

**备注**

此接口定义了上下文的模块注册与获取契约。 AbstractContext{T} 是其默认实现，提供了懒加载单例、统一初始化和有序释放等完整功能。
实现类应在初始化阶段注册所有需要的 Model 和 Service，运行时通过 GetModel{T} / GetService{T} 获取模块实例——未注册时抛出 InvalidOperationException 而非返回 null。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Initialized`](#property-initialized) | — |

</div>

### Initialized {#property-initialized}

``` csharp
public bool Initialized { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetModel()`](#method-getmodel) | 获取已注册的 Model |
| [`GetService()`](#method-getservice) | 获取已注册的 Service |
| [`RegisterModel(T)`](#method-registermodel-t) | 注册 Model |
| [`RegisterService(T)`](#method-registerservice-t) | 注册 Service |

</div>

### GetModel() {#method-getmodel}

获取已注册的 Model

``` csharp
public abstract T GetModel<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 已注册的 Model 实例 |

</div>

### GetService() {#method-getservice}

获取已注册的 Service

``` csharp
public abstract T GetService<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 已注册的 Service 实例 |

</div>

### RegisterModel(T) {#method-registermodel-t}

注册 Model

``` csharp
public abstract void RegisterModel<T>(T model)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `model` | `T` | 要注册的 Model 实例 |

</div>

### RegisterService(T) {#method-registerservice-t}

注册 Service

``` csharp
public abstract void RegisterService<T>(T service)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `service` | `T` | 要注册的 Service 实例 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
