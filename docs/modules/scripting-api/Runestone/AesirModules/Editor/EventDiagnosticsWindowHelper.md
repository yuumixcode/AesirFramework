---
title: EventDiagnosticsWindowHelper
description: "Runestone.AesirModules.Editor.EventDiagnosticsWindowHelper 的 API 文档"
---

# `EventDiagnosticsWindowHelper`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `EventDiagnosticsWindowHelper`

## 声明

``` csharp
internal static class EventDiagnosticsWindowHelper
```

事件诊断窗口的 UI 构件helper。集中标题、行文本、空态等零散样式， 供三个 UI Toolkit 窗口复用，避免各自复制样式代码。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`HeaderColor`](#field-headercolor) | — |
| [`MutedColor`](#field-mutedcolor) | — |
| [`OkColor`](#field-okcolor) | — |
| [`WarnColor`](#field-warncolor) | — |

</div>

### HeaderColor {#field-headercolor}

``` csharp
public static readonly Color HeaderColor = RGBA(0.220, 0.260, 0.320, 1.000);
```

### MutedColor {#field-mutedcolor}

``` csharp
public static readonly Color MutedColor = RGBA(0.620, 0.650, 0.700, 1.000);
```

### OkColor {#field-okcolor}

``` csharp
public static readonly Color OkColor = RGBA(0.400, 0.850, 0.550, 1.000);
```

### WarnColor {#field-warncolor}

``` csharp
public static readonly Color WarnColor = RGBA(0.950, 0.720, 0.300, 1.000);
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Empty(string)`](#method-empty-string) | 创建空态提示（居中灰字）。 |
| [`Header(string)`](#method-header-string) | 创建带样式的表头标签。 |
| [`Row(string, Color?)`](#method-row-string-color) | 创建普通信息行。 |

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

### Empty(string) {#method-empty-string}

创建空态提示（居中灰字）。

``` csharp
public static Label Empty(string text)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `text` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Label` | — |

</div>

### Header(string) {#method-header-string}

创建带样式的表头标签。

``` csharp
public static Label Header(string text)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `text` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Label` | — |

</div>

### Row(string, Color?) {#method-row-string-color}

创建普通信息行。

``` csharp
public static Label Row(string text, Color? color = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `text` | `string` | — |
| `color` | `Color?` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Label` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
