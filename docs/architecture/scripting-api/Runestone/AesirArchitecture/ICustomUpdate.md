---
title: ICustomUpdate
description: "Runestone.AesirArchitecture.ICustomUpdate 的 API 文档"
---

# `ICustomUpdate`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

## 声明

``` csharp
public interface ICustomUpdate
```

自定义 Update 生命周期。对应 Update。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnCustomUpdate()`](#method-oncustomupdate) | 在 Update 阶段执行的自定义逻辑 |

</div>

### OnCustomUpdate() {#method-oncustomupdate}

在 Update 阶段执行的自定义逻辑

``` csharp
public abstract void OnCustomUpdate()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
