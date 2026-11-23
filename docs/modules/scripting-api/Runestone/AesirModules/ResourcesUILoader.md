---
title: ResourcesUILoader
description: "Runestone.AesirModules.ResourcesUILoader 的 API 文档"
---

# `ResourcesUILoader`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `ResourcesUILoader`

**实现接口:** `Runestone.AesirModules.IUIAssetLoader`

## 声明

``` csharp
public sealed class ResourcesUILoader : Runestone.AesirModules.IUIAssetLoader
```

默认加载器：从 Resources 路径加载面板预制体。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ResourcesUILoader()`](#constructor-resourcesuiloader) | — |

</div>

### ResourcesUILoader() {#constructor-resourcesuiloader}

``` csharp
public ResourcesUILoader()
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Load(string)`](#method-load-string) | 从 Resources 路径加载面板预制体。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

### Load(string) {#method-load-string}

从 Resources 路径加载面板预制体。

``` csharp
public GameObject Load(string path)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | Resources 下的相对路径。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `GameObject` | 加载到的预制体，未找到时记录错误并返回 null。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
