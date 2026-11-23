---
title: AesirUpdateService.ReleaseRouteKind
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.ReleaseRouteKind 的 API 文档"
---

# `AesirUpdateService.ReleaseRouteKind`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `AesirUpdateService.ReleaseRouteKind`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum AesirUpdateService.ReleaseRouteKind : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

检测线路类别——决定界面上的实时性提示（越靠前越实时）。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CdnRelay`](#field-cdnrelay) | 免费 CDN 中转（jsDelivr 等）：分支内容有缓存，可能有数小时延迟。 |
| [`GitHubDirect`](#field-githubdirect) | 直连 GitHub（api.github.com / github.com / raw.githubusercontent.com）： 发布即刻可见，版本信息 100% 实时。 |
| [`GitHubMirror`](#field-githubmirror) | GitHub 镜像站（第三方代理 GitHub 内容）：内容实时，可用性取决于镜像站自身。 |

</div>

### CdnRelay {#field-cdnrelay}

免费 CDN 中转（jsDelivr 等）：分支内容有缓存，可能有数小时延迟。

``` csharp
public const AesirUpdateService.ReleaseRouteKind CdnRelay;
```

### GitHubDirect {#field-githubdirect}

直连 GitHub（api.github.com / github.com / raw.githubusercontent.com）： 发布即刻可见，版本信息 100% 实时。

``` csharp
public const AesirUpdateService.ReleaseRouteKind GitHubDirect;
```

### GitHubMirror {#field-githubmirror}

GitHub 镜像站（第三方代理 GitHub 内容）：内容实时，可用性取决于镜像站自身。

``` csharp
public const AesirUpdateService.ReleaseRouteKind GitHubMirror;
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `HasFlag(Enum)` | — | `Enum` |
| `Equals(object)` | — | `Enum` |
| `GetHashCode()` | — | `Enum` |
| `ToString()` | — | `Enum` |
| `ToString(string)` | — | `Enum` |
| `GetTypeCode()` | — | `Enum` |
| `CompareTo(object)` | — | `Enum` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `ToString(IFormatProvider)` | — | `Enum` |
| `ToString(string, IFormatProvider)` | — | `Enum` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
