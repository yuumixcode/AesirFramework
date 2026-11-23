---
title: ICustomOnApplicationQuit
description: "Runestone.AesirArchitecture.ICustomOnApplicationQuit 的 API 文档"
---

# `ICustomOnApplicationQuit`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

## 声明

``` csharp
public interface ICustomOnApplicationQuit
```

自定义 OnApplicationQuit 生命周期。对应 OnApplicationQuit。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnCustomApplicationQuit()`](#method-oncustomapplicationquit) | 在应用退出时执行的自定义逻辑 |

</div>

### OnCustomApplicationQuit() {#method-oncustomapplicationquit}

在应用退出时执行的自定义逻辑

``` csharp
public abstract void OnCustomApplicationQuit()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
