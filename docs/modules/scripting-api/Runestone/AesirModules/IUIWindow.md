---
title: IUIWindow
description: "Runestone.AesirModules.IUIWindow 的 API 文档"
---

# `IUIWindow`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

## 声明

``` csharp
public interface IUIWindow
```

Canvas 根 UI 窗口契约。生命周期：Initialize → Show → Hide → DestroyWindow，与 IUIPanel 平行。
窗口预制体根节点自带 Canvas（独立渲染根）， 由 UIModule 实例化后直接挂载到 UIRoot 下（不经四层 Canvas）， 排序按 SortingOrder 自治（默认基准 500，恒在全部面板层之上）。

**备注**

窗口与面板是两种并列的 UI 形态（选型对比见包内 Documentation~/ui-module.md）； 一个类型不应同时实现 IUIPanel 与本接口。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DestroyOnHide`](#property-destroyonhide) | — |
| [`IsOpen`](#property-isopen) | — |
| [`SortingOrder`](#property-sortingorder) | — |

</div>

### DestroyOnHide {#property-destroyonhide}

``` csharp
public bool DestroyOnHide { get; }
```

### IsOpen {#property-isopen}

``` csharp
public bool IsOpen { get; }
```

### SortingOrder {#property-sortingorder}

``` csharp
public int SortingOrder { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DestroyWindow()`](#method-destroywindow) | 销毁窗口实例，释放资源。 |
| [`Hide()`](#method-hide) | 隐藏窗口（不销毁实例）。 |
| [`Initialize()`](#method-initialize) | 首次创建后由 UIModule 调用一次。 |
| [`SetMaskVisible(bool)`](#method-setmaskvisible-bool) | 设置蒙版子物体（约定名 Mask）的显隐，由 UIModule 按单遮/叠遮模式统一调度。 预制体无 Mask 子物体时为无操作。 |
| [`Show(object)`](#method-show-object) | — |

</div>

### DestroyWindow() {#method-destroywindow}

销毁窗口实例，释放资源。

``` csharp
public abstract void DestroyWindow()
```

### Hide() {#method-hide}

隐藏窗口（不销毁实例）。

``` csharp
public abstract void Hide()
```

### Initialize() {#method-initialize}

首次创建后由 UIModule 调用一次。

``` csharp
public abstract void Initialize()
```

### SetMaskVisible(bool) {#method-setmaskvisible-bool}

设置蒙版子物体（约定名 Mask）的显隐，由 UIModule 按单遮/叠遮模式统一调度。 预制体无 Mask 子物体时为无操作。

``` csharp
public abstract void SetMaskVisible(bool visible)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `visible` | `bool` | 蒙版是否可见（可见时拦截其下一切 UI 的射线）。 |

</div>

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
