---
title: ICustomAfterUpdate
description: "Runestone.AesirArchitecture.ICustomAfterUpdate 的 API 文档"
---

# `ICustomAfterUpdate`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

## 声明

``` csharp
public interface ICustomAfterUpdate
```

自定义 AfterUpdate 生命周期。对应 AfterUpdate。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnCustomAfterUpdate()`](#method-oncustomafterupdate) | 在 AfterUpdate 阶段执行的自定义逻辑（PlayerLoop 注入，在 PostLateUpdate 之后） |

</div>

### OnCustomAfterUpdate() {#method-oncustomafterupdate}

在 AfterUpdate 阶段执行的自定义逻辑（PlayerLoop 注入，在 PostLateUpdate 之后）

``` csharp
public abstract void OnCustomAfterUpdate()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
