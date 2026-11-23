---
title: AesirUpdateService.FilesManifest.PackageEntry
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.FilesManifest.PackageEntry 的 API 文档"
---

# `AesirUpdateService.FilesManifest.PackageEntry`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.FilesManifest.PackageEntry`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.FilesManifest.PackageEntry
```

单个包的安装清单。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.FilesManifest.PackageEntry()`](#constructor-aesirupdateservice-filesmanifest-packageentry) | — |

</div>

### AesirUpdateService.FilesManifest.PackageEntry() {#constructor-aesirupdateservice-filesmanifest-packageentry}

``` csharp
public AesirUpdateService.FilesManifest.PackageEntry()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`name`](#field-name) | 包目录名（如 AesirArchitecture），同时是主键。 |
| [`version`](#field-version) | 该清单对应的包版本。 |
| [`files`](#field-files) | 包内全部条目的项目相对路径（含目录条目，与 unitypackage 内 pathname 同源）。 |

</div>

### name {#field-name}

包目录名（如 AesirArchitecture），同时是主键。

``` csharp
public string name;
```

### version {#field-version}

该清单对应的包版本。

``` csharp
public string version;
```

### files {#field-files}

包内全部条目的项目相对路径（含目录条目，与 unitypackage 内 pathname 同源）。

``` csharp
public string[] files;
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
