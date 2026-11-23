---
title: ICanSetContext
description: "Runestone.AesirArchitecture.ICanSetContext 的 API 文档"
---

# `ICanSetContext`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

## 声明

``` csharp
public interface ICanSetContext
```

可设置上下文引用接口

**备注**

该接口定义了上下文注入的入口。框架在注册模块时由 AbstractContext<T> 自动调用 SetContext(IContext) 将自身引用注入到被注册的对象中， 因此子类无需也不应手动调用此方法。
通过将注入逻辑收口在注册流程中，保证了所有模块在进入业务逻辑之前 一定持有有效的上下文引用，避免了空引用和时序问题。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SetContext(IContext)`](#method-setcontext-icontext) | 设置上下文引用 |

</div>

### SetContext(IContext) {#method-setcontext-icontext}

设置上下文引用

``` csharp
public abstract void SetContext(IContext context)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `context` | `IContext` | 要注入的模块上下文 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
