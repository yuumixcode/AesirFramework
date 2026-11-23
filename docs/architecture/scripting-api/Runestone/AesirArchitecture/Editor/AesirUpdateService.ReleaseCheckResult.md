---
title: AesirUpdateService.ReleaseCheckResult
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.ReleaseCheckResult 的 API 文档"
---

# `AesirUpdateService.ReleaseCheckResult`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.ReleaseCheckResult`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.ReleaseCheckResult
```

一次版本检测的完整结果：快照 + 线路信息 + 各层尝试记录。
标记 SerializableAttribute — 窗口状态字段持有后可跨域重载保留。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.ReleaseCheckResult()`](#constructor-aesirupdateservice-releasecheckresult) | — |

</div>

### AesirUpdateService.ReleaseCheckResult() {#constructor-aesirupdateservice-releasecheckresult}

``` csharp
public AesirUpdateService.ReleaseCheckResult()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Attempts`](#field-attempts) | 各层尝试记录（按实际尝试顺序）。 |
| [`RouteKind`](#field-routekind) | 最终返回结果的线路类别。 |
| [`Snapshot`](#field-snapshot) | 检测结果快照（tag + 可能的清单）。 |
| [`GitHubDirectAvailable`](#field-githubdirectavailable) | 本次检测中直连 GitHub 是否可用（可用即结果 100% 实时，无延迟顾虑）。 |
| [`RouteName`](#field-routename) | 最终返回结果的源展示名。 |

</div>

### Attempts {#field-attempts}

各层尝试记录（按实际尝试顺序）。

``` csharp
public AesirUpdateService.DetectionAttempt[] Attempts;
```

### RouteKind {#field-routekind}

最终返回结果的线路类别。

``` csharp
public AesirUpdateService.ReleaseRouteKind RouteKind;
```

### Snapshot {#field-snapshot}

检测结果快照（tag + 可能的清单）。

``` csharp
public AesirUpdateService.ReleaseSnapshot Snapshot;
```

### GitHubDirectAvailable {#field-githubdirectavailable}

本次检测中直连 GitHub 是否可用（可用即结果 100% 实时，无延迟顾虑）。

``` csharp
public bool GitHubDirectAvailable;
```

### RouteName {#field-routename}

最终返回结果的源展示名。

``` csharp
public string RouteName;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`BuildAttemptsLog()`](#method-buildattemptslog) | 各层尝试的可读摘要（一行一次尝试，成功在前标注 ✓）。 |

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

### BuildAttemptsLog() {#method-buildattemptslog}

各层尝试的可读摘要（一行一次尝试，成功在前标注 ✓）。

``` csharp
public string BuildAttemptsLog()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
