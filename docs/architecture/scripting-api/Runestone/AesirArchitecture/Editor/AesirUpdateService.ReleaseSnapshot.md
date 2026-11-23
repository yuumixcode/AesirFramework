---
title: AesirUpdateService.ReleaseSnapshot
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.ReleaseSnapshot 的 API 文档"
---

# `AesirUpdateService.ReleaseSnapshot`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.ReleaseSnapshot`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.ReleaseSnapshot
```

一次成功检测的结果快照：来源 + tag +（可能缺失的）清单。 unitypackage 下载地址按命名约定从 tag 构造，不依赖 API 的资产列表。
标记 SerializableAttribute — 编辑器窗口字段持有快照时可跨域重载保留。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.ReleaseSnapshot()`](#constructor-aesirupdateservice-releasesnapshot) | — |

</div>

### AesirUpdateService.ReleaseSnapshot() {#constructor-aesirupdateservice-releasesnapshot}

``` csharp
public AesirUpdateService.ReleaseSnapshot()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Kind`](#field-kind) | 来源所属线路类别（直连 GitHub / 镜像站 / CDN 中转）。 |
| [`Info`](#field-info) | 版本与清单信息；302 重定向路径只有 tag，此字段为 null（更新时跳过残留清理）。 |
| [`Source`](#field-source) | 来源描述（如 "GitHub API" / "镜像 ghproxy.net" / "jsDelivr (cdn.jsdelivr.net)"）。 |
| [`Tag`](#field-tag) | Release 标签名（如 v0.15.0）。 |

</div>

### Kind {#field-kind}

来源所属线路类别（直连 GitHub / 镜像站 / CDN 中转）。

``` csharp
public AesirUpdateService.ReleaseRouteKind Kind;
```

### Info {#field-info}

版本与清单信息；302 重定向路径只有 tag，此字段为 null（更新时跳过残留清理）。

``` csharp
public AesirUpdateService.UpdateInfo Info;
```

### Source {#field-source}

来源描述（如 "GitHub API" / "镜像 ghproxy.net" / "jsDelivr (cdn.jsdelivr.net)"）。

``` csharp
public string Source;
```

### Tag {#field-tag}

Release 标签名（如 v0.15.0）。

``` csharp
public string Tag;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetUnityPackageUrl(string)`](#method-getunitypackageurl-string) | 指定包目录的 unitypackage 下载地址。 命名约定由 CI 保证：<包目录名>-v<版本>.unitypackage。 |

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

### GetUnityPackageUrl(string) {#method-getunitypackageurl-string}

指定包目录的 unitypackage 下载地址。 命名约定由 CI 保证：<包目录名>-v<版本>.unitypackage。

``` csharp
public string GetUnityPackageUrl(string dirName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `dirName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
