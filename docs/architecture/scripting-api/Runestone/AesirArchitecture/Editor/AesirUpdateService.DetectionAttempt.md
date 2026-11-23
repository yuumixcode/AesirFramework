---
title: AesirUpdateService.DetectionAttempt
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.DetectionAttempt 的 API 文档"
---

# `AesirUpdateService.DetectionAttempt`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.DetectionAttempt`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.DetectionAttempt
```

单次检测尝试的记录（界面「检测详情」与故障定位用）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.DetectionAttempt()`](#constructor-aesirupdateservice-detectionattempt) | — |

</div>

### AesirUpdateService.DetectionAttempt() {#constructor-aesirupdateservice-detectionattempt}

``` csharp
public AesirUpdateService.DetectionAttempt()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Kind`](#field-kind) | 所属线路类别。 |
| [`Succeeded`](#field-succeeded) | 本次尝试是否成功。 |
| [`ElapsedMs`](#field-elapsedms) | 耗时（毫秒）。 |
| [`Detail`](#field-detail) | 成功说明（解析到的 tag）或失败原因。 |
| [`Source`](#field-source) | 源展示名（如 "GitHub API" / "镜像 ghproxy.net" / "jsDelivr (cdn.jsdelivr.net)"）。 |

</div>

### Kind {#field-kind}

所属线路类别。

``` csharp
public AesirUpdateService.ReleaseRouteKind Kind;
```

### Succeeded {#field-succeeded}

本次尝试是否成功。

``` csharp
public bool Succeeded;
```

### ElapsedMs {#field-elapsedms}

耗时（毫秒）。

``` csharp
public long ElapsedMs;
```

### Detail {#field-detail}

成功说明（解析到的 tag）或失败原因。

``` csharp
public string Detail;
```

### Source {#field-source}

源展示名（如 "GitHub API" / "镜像 ghproxy.net" / "jsDelivr (cdn.jsdelivr.net)"）。

``` csharp
public string Source;
```

## 方法

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

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
