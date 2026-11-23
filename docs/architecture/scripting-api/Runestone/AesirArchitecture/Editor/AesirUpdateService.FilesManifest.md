---
title: AesirUpdateService.FilesManifest
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.FilesManifest 的 API 文档"
---

# `AesirUpdateService.FilesManifest`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.FilesManifest`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.FilesManifest
```

files-manifest 结构的本地安装清单（.aesir/installed-manifest.json）。 与 UpdateInfo 共用 PackageEntry。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.FilesManifest()`](#constructor-aesirupdateservice-filesmanifest) | — |

</div>

### AesirUpdateService.FilesManifest() {#constructor-aesirupdateservice-filesmanifest}

``` csharp
public AesirUpdateService.FilesManifest()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`packages`](#field-packages) | 各包清单。 |

</div>

### packages {#field-packages}

各包清单。

``` csharp
public AesirUpdateService.FilesManifest.PackageEntry[] packages;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetPackage(string)`](#method-getpackage-string) | 按包目录名查找条目；不存在返回 null。 |

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

### GetPackage(string) {#method-getpackage-string}

按包目录名查找条目；不存在返回 null。

``` csharp
public AesirUpdateService.FilesManifest.PackageEntry GetPackage(string dirName)
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
| `AesirUpdateService.FilesManifest.PackageEntry` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
