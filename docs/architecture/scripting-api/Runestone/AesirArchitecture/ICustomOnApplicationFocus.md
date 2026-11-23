---
title: ICustomOnApplicationFocus
description: "Runestone.AesirArchitecture.ICustomOnApplicationFocus 的 API 文档"
---

# `ICustomOnApplicationFocus`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

## 声明

``` csharp
public interface ICustomOnApplicationFocus
```

自定义 OnApplicationFocus 生命周期。对应 OnApplicationFocus。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnCustomApplicationFocus()`](#method-oncustomapplicationfocus) | 在应用获得或失去焦点时执行的自定义逻辑 |

</div>

### OnCustomApplicationFocus() {#method-oncustomapplicationfocus}

在应用获得或失去焦点时执行的自定义逻辑

``` csharp
public abstract void OnCustomApplicationFocus()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
