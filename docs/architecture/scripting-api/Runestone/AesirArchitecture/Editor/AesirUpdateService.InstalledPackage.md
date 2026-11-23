---
title: AesirUpdateService.InstalledPackage
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.InstalledPackage 的 API 文档"
---

# `AesirUpdateService.InstalledPackage`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.InstalledPackage`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.InstalledPackage
```

扫描到的本地已安装包。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.InstalledPackage()`](#constructor-aesirupdateservice-installedpackage) | — |

</div>

### AesirUpdateService.InstalledPackage() {#constructor-aesirupdateservice-installedpackage}

``` csharp
public AesirUpdateService.InstalledPackage()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AssetsPath`](#field-assetspath) | 包目录的 Assets 相对路径（如 Assets/Runestone/AesirArchitecture）。 |
| [`DirName`](#field-dirname) | 包目录名（如 AesirArchitecture）。 |
| [`PackageId`](#field-packageid) | package.json 中的包 id（如 cn.runestone.aesir.architecture）。 |
| [`Version`](#field-version) | package.json 中的版本号。 |

</div>

### AssetsPath {#field-assetspath}

包目录的 Assets 相对路径（如 Assets/Runestone/AesirArchitecture）。

``` csharp
public string AssetsPath;
```

### DirName {#field-dirname}

包目录名（如 AesirArchitecture）。

``` csharp
public string DirName;
```

### PackageId {#field-packageid}

package.json 中的包 id（如 cn.runestone.aesir.architecture）。

``` csharp
public string PackageId;
```

### Version {#field-version}

package.json 中的版本号。

``` csharp
public string Version;
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
