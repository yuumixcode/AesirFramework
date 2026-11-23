---
title: UIRoot.LayerCanvasEntry
description: "Runestone.AesirModules.UIRoot.LayerCanvasEntry 的 API 文档"
---

# `UIRoot.LayerCanvasEntry`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `UIRoot.LayerCanvasEntry`

## 声明

``` csharp
[Serializable]
private struct UIRoot.LayerCanvasEntry : System.ValueType
```

层级 Canvas 的序列化引用条目。Unity 无法序列化字典，改以列表存储（条目数恒等于层数，运行时线性查找即可）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`UIRoot.LayerCanvasEntry(UILayer, Canvas)`](#constructor-uiroot-layercanvasentry-uilayer-canvas) | — |

</div>

### UIRoot.LayerCanvasEntry(UILayer, Canvas) {#constructor-uiroot-layercanvasentry-uilayer-canvas}

``` csharp
public UIRoot.LayerCanvasEntry(UILayer layer, Canvas canvas)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `layer` | `UILayer` | — |
| `canvas` | `Canvas` | — |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `ValueType` |
| `GetHashCode()` | — | `ValueType` |
| `ToString()` | — | `ValueType` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
