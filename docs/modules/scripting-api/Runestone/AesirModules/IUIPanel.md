---
title: IUIPanel
description: "Runestone.AesirModules.IUIPanel 的 API 文档"
---

# `IUIPanel`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

## 声明

``` csharp
public interface IUIPanel
```

UI 面板契约。生命周期：Initialize → Show → Hide → DestroyPanel。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Layer`](#property-layer) | — |
| [`DestroyOnHide`](#property-destroyonhide) | — |
| [`IsOpen`](#property-isopen) | — |

</div>

### Layer {#property-layer}

``` csharp
public UILayer Layer { get; }
```

### DestroyOnHide {#property-destroyonhide}

``` csharp
public bool DestroyOnHide { get; }
```

### IsOpen {#property-isopen}

``` csharp
public bool IsOpen { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DestroyPanel()`](#method-destroypanel) | 销毁面板实例，释放资源。 |
| [`Hide()`](#method-hide) | 隐藏面板（不销毁实例）。 |
| [`Initialize()`](#method-initialize) | 首次创建后由 UIModule 调用一次。 |
| [`Show(object)`](#method-show-object) | — |

</div>

### DestroyPanel() {#method-destroypanel}

销毁面板实例，释放资源。

``` csharp
public abstract void DestroyPanel()
```

### Hide() {#method-hide}

隐藏面板（不销毁实例）。

``` csharp
public abstract void Hide()
```

### Initialize() {#method-initialize}

首次创建后由 UIModule 调用一次。

``` csharp
public abstract void Initialize()
```

### Show(object) {#method-show-object}

``` csharp
public abstract void Show(object payload = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `object` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
