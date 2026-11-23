---
title: AesirGetStartedService.AesirPackageInfo
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedService.AesirPackageInfo 的 API 文档"
---

# `AesirGetStartedService.AesirPackageInfo`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirGetStartedService.AesirPackageInfo`

## 声明

``` csharp
public sealed class AesirGetStartedService.AesirPackageInfo
```

已安装的 Aesir 包信息（扫描产物）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirGetStartedService.AesirPackageInfo()`](#constructor-aesirgetstartedservice-aesirpackageinfo) | — |

</div>

### AesirGetStartedService.AesirPackageInfo() {#constructor-aesirgetstartedservice-aesirpackageinfo}

``` csharp
public AesirGetStartedService.AesirPackageInfo()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`InstallType`](#field-installtype) | 安装形态。 |
| [`Samples`](#field-samples) | — |
| [`Description`](#field-description) | 包定位描述（package.json description）。 |
| [`DirName`](#field-dirname) | 包目录名（Assets 副本目录 / 嵌入式目录 / PackageCache 缓存目录）。 |
| [`DisplayName`](#field-displayname) | 包显示名（概览卡片标题 / UPM 示例导入目录名）。 |
| [`Id`](#field-id) | package.json name（包唯一标识）。 |
| [`PackageRootPath`](#field-packagerootpath) | 包根的项目相对路径（仅 AssetsCopy 形态有值； 经锚点资产定位，Runestone 可移动到项目任意文件夹——示例路径据此动态拼接， 不硬编码安装根。嵌入式 / UPM 形态为 null。 |
| [`Version`](#field-version) | 本地版本号。 |

</div>

### InstallType {#field-installtype}

安装形态。

``` csharp
public AesirGetStartedService.AesirInstallType InstallType;
```

### Samples {#field-samples}

``` csharp
public List<AesirGetStartedService.AesirSampleInfo> Samples;
```

### Description {#field-description}

包定位描述（package.json description）。

``` csharp
public string Description;
```

### DirName {#field-dirname}

包目录名（Assets 副本目录 / 嵌入式目录 / PackageCache 缓存目录）。

``` csharp
public string DirName;
```

### DisplayName {#field-displayname}

包显示名（概览卡片标题 / UPM 示例导入目录名）。

``` csharp
public string DisplayName;
```

### Id {#field-id}

package.json name（包唯一标识）。

``` csharp
public string Id;
```

### PackageRootPath {#field-packagerootpath}

包根的项目相对路径（仅 AssetsCopy 形态有值； 经锚点资产定位，Runestone 可移动到项目任意文件夹——示例路径据此动态拼接， 不硬编码安装根。嵌入式 / UPM 形态为 null。

``` csharp
public string PackageRootPath;
```

### Version {#field-version}

本地版本号。

``` csharp
public string Version;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ImportedWithSceneCount`](#property-importedwithscenecount) | 已导入且可直接打开场景的示例数。 |

</div>

### ImportedWithSceneCount {#property-importedwithscenecount}

已导入且可直接打开场景的示例数。

``` csharp
public int ImportedWithSceneCount { get; }
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
