---
title: AesirGetStartedWindowOdin.GetStartedPage
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedWindowOdin.GetStartedPage 的 API 文档"
---

# `AesirGetStartedWindowOdin.GetStartedPage`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `AesirGetStartedWindowOdin.GetStartedPage`

## 声明

``` csharp
private abstract class AesirGetStartedWindowOdin.GetStartedPage
```

Getting Started 页面基类（照 Odin GettingStartedPage：标题栏 / footer / 页面栈进出 / 滚动页包装）。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Window`](#field-window) | — |
| [`TitleIcon`](#field-titleicon) | — |
| [`Title`](#field-title) | — |
| [`FooterSize`](#field-footersize) | — |
| [`ScrollPosition`](#field-scrollposition) | — |

</div>

### Window {#field-window}

``` csharp
[NonSerialized]
public AesirGetStartedWindowOdin Window;
```

### TitleIcon {#field-titleicon}

``` csharp
public SdfIconType TitleIcon;
```

### Title {#field-title}

``` csharp
public string Title;
```

### FooterSize {#field-footersize}

``` csharp
public readonly float FooterSize;
```

### ScrollPosition {#field-scrollposition}

``` csharp
protected Vector2 ScrollPosition;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`EntranceT`](#property-entrancet) | 入场进度（1 = 页面完全展开；概览收起时为 0，条目随其渐显增高）。 |
| [`PagePaddingStyle`](#property-pagepaddingstyle) | — |

</div>

### EntranceT {#property-entrancet}

入场进度（1 = 页面完全展开；概览收起时为 0，条目随其渐显增高）。

``` csharp
public float EntranceT { get; }
```

### PagePaddingStyle {#property-pagepaddingstyle}

``` csharp
protected static GUIStyle PagePaddingStyle { protected get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DrawPage(Rect)`](#method-drawpage-rect) | — |
| [`DrawFooter(Rect)`](#method-drawfooter-rect) | — |
| [`EnterPage()`](#method-enterpage) | — |
| [`GoBack()`](#method-goback) | — |
| [`BeginScrollableLayoutPage(Rect, int)`](#method-beginscrollablelayoutpage-rect-int) | — |
| [`EndScrollableLayoutPage()`](#method-endscrollablelayoutpage) | — |

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

### DrawPage(Rect) {#method-drawpage-rect}

``` csharp
public abstract void DrawPage(Rect rect)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `rect` | `Rect` | — |

</div>

### DrawFooter(Rect) {#method-drawfooter-rect}

``` csharp
public virtual void DrawFooter(Rect rect)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `rect` | `Rect` | — |

</div>

### EnterPage() {#method-enterpage}

``` csharp
public virtual void EnterPage()
```

### GoBack() {#method-goback}

``` csharp
public virtual void GoBack()
```

### BeginScrollableLayoutPage(Rect, int) {#method-beginscrollablelayoutpage-rect-int}

``` csharp
protected void BeginScrollableLayoutPage(Rect rect, int paddingSize = 24)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `rect` | `Rect` | — |
| `paddingSize` | `int` | — |

</div>

### EndScrollableLayoutPage() {#method-endscrollablelayoutpage}

``` csharp
protected void EndScrollableLayoutPage()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
