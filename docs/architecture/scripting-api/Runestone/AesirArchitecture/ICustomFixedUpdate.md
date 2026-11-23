---
title: ICustomFixedUpdate
description: "Runestone.AesirArchitecture.ICustomFixedUpdate 的 API 文档"
---

# `ICustomFixedUpdate`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

## 声明

``` csharp
public interface ICustomFixedUpdate
```

自定义 FixedUpdate 生命周期。对应 FixedUpdate。

**备注**

本文件中的接口集合（ICustomFixedUpdate 至 ICustomOnApplicationQuit）每个接口对应一个 MonoLifecycleEvent，方法名以 OnCustom 前缀区分 Unity 原生回调。 实现任意 ICustomXXX 接口的对象可通过 RegisterAuto(object) 自动注册到匹配的事件；MonoBehaviour 经 Register(MonoBehaviour) 注册时还会在所在 GameObject 销毁时自动取消订阅。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnCustomFixedUpdate()`](#method-oncustomfixedupdate) | 在 FixedUpdate 阶段执行的自定义逻辑 |

</div>

### OnCustomFixedUpdate() {#method-oncustomfixedupdate}

在 FixedUpdate 阶段执行的自定义逻辑

``` csharp
public abstract void OnCustomFixedUpdate()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
