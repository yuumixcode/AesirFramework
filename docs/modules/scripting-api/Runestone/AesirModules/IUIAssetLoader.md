---
title: IUIAssetLoader
description: "Runestone.AesirModules.IUIAssetLoader 的 API 文档"
---

# `IUIAssetLoader`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

## 声明

``` csharp
public interface IUIAssetLoader
```

面板加载器契约。加载语义为同步：适用于 Resources、同步缓存等管线； Addressables 等异步管线需自行预加载后同步返回，无法在接口内表达等待。

**备注**

预制体引用由 UIModule 的注册表持有，生命周期与模块一致，契约不设释放方法。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Load(string)`](#method-load-string) | 按路径加载面板预制体。 |

</div>

### Load(string) {#method-load-string}

按路径加载面板预制体。

``` csharp
public abstract GameObject Load(string path)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | 资源路径。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `GameObject` | 加载到的预制体，未找到返回 null。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
