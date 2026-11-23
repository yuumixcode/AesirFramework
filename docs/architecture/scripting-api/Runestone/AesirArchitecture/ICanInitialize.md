---
title: ICanInitialize
description: "Runestone.AesirArchitecture.ICanInitialize 的 API 文档"
---

# `ICanInitialize`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `System.IDisposable`

## 声明

``` csharp
public interface ICanInitialize : System.IDisposable
```

可初始化接口。提供初始化与初始化状态标记。
被 IModel 和 IService 继承。

**备注**

框架的初始化遵循严格的两阶段流程：
1. Context 先调用 Configure()，此时所有 Model 和 Service 通过 RegisterModel / RegisterService 注册到容器中，但尚未初始化。

2. 注册完成后，按注册顺序依次调用各模块的 Initialize()， 先初始化全部 Model，再初始化全部 Service。

这种两阶段设计确保了模块在被初始化时，其所依赖的其他模块已经全部注册完毕， 从而可以在 Initialize() 中安全地获取对端模块的引用。

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
| [`Initialize()`](#method-initialize) | 初始化 |

</div>

### Initialize() {#method-initialize}

初始化

**备注**

在此方法中可以安全地访问 Context 中已注册的其他模块。 注意：初始化顺序由 Context 控制，模块之间不应在初始化阶段产生循环依赖。

``` csharp
public abstract void Initialize()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
