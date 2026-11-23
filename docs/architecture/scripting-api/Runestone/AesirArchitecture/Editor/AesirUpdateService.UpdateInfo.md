---
title: AesirUpdateService.UpdateInfo
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.UpdateInfo 的 API 文档"
---

# `AesirUpdateService.UpdateInfo`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.UpdateInfo`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.UpdateInfo
```

update-info.json 结构：版本信息 + 各包文件清单（仓库内文件，jsDelivr / GitHub 均可拉取）。
数组而非 Dictionary — JsonUtility 不支持字典序列化。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.UpdateInfo()`](#constructor-aesirupdateservice-updateinfo) | — |

</div>

### AesirUpdateService.UpdateInfo() {#constructor-aesirupdateservice-updateinfo}

``` csharp
public AesirUpdateService.UpdateInfo()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`packages`](#field-packages) | 各包文件清单。 |
| [`tag`](#field-tag) | Release 标签名（如 v0.15.0）。 |
| [`version`](#field-version) | 版本号（如 0.15.0）。 |

</div>

### packages {#field-packages}

各包文件清单。

``` csharp
public AesirUpdateService.FilesManifest.PackageEntry[] packages;
```

### tag {#field-tag}

Release 标签名（如 v0.15.0）。

``` csharp
public string tag;
```

### version {#field-version}

版本号（如 0.15.0）。

``` csharp
public string version;
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
