---
title: AesirUpdateService.TimeoutBudget
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.TimeoutBudget 的 API 文档"
---

# `AesirUpdateService.TimeoutBudget`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `System.ValueType` → `AesirUpdateService.TimeoutBudget`

## 声明

``` csharp
[IsReadOnly]
public struct AesirUpdateService.TimeoutBudget : System.ValueType
```

墙钟超时预算。用于给"整轮检测""单次请求""下载"设硬上限—— UnityWebRequest.timeout 只覆盖"完全无数据"的情形，服务端慢速滴水时不会触发， 会把进度条长时间挂在屏幕上（用户实测反馈）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.TimeoutBudget(double, double)`](#constructor-aesirupdateservice-timeoutbudget-double-double) | 以 limitSeconds 为上限、自 now 起算建立预算。 |

</div>

### AesirUpdateService.TimeoutBudget(double, double) {#constructor-aesirupdateservice-timeoutbudget-double-double}

以 limitSeconds 为上限、自 now 起算建立预算。

``` csharp
public AesirUpdateService.TimeoutBudget(double limitSeconds, double now)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `limitSeconds` | `double` | — |
| `now` | `double` | — |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`LimitSeconds`](#property-limitseconds) | 预算时长（秒）。 |

</div>

### LimitSeconds {#property-limitseconds}

预算时长（秒）。

``` csharp
public double LimitSeconds { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IsExpired(double)`](#method-isexpired-double) | 当前时刻是否已超时。 |
| [`Remaining(double)`](#method-remaining-double) | 剩余时间（秒，最小 0）。 |

</div>

**继承的方法**

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

### IsExpired(double) {#method-isexpired-double}

当前时刻是否已超时。

``` csharp
public bool IsExpired(double now)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `now` | `double` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### Remaining(double) {#method-remaining-double}

剩余时间（秒，最小 0）。

``` csharp
public double Remaining(double now)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `now` | `double` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `double` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
