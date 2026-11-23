---
title: ICustomBeforeUpdate
description: "Runestone.AesirArchitecture.ICustomBeforeUpdate 的 API 文档"
---

# `ICustomBeforeUpdate`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

## 声明

``` csharp
public interface ICustomBeforeUpdate
```

自定义 BeforeUpdate 生命周期。对应 BeforeUpdate。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnCustomBeforeUpdate()`](#method-oncustombeforeupdate) | 在 BeforeUpdate 阶段执行的自定义逻辑（PlayerLoop 注入，在 Update 之前） |

</div>

### OnCustomBeforeUpdate() {#method-oncustombeforeupdate}

在 BeforeUpdate 阶段执行的自定义逻辑（PlayerLoop 注入，在 Update 之前）

``` csharp
public abstract void OnCustomBeforeUpdate()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
