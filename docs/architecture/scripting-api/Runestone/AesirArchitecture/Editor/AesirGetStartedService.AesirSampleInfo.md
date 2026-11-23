---
title: AesirGetStartedService.AesirSampleInfo
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedService.AesirSampleInfo 的 API 文档"
---

# `AesirGetStartedService.AesirSampleInfo`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirGetStartedService.AesirSampleInfo`

## 声明

``` csharp
public sealed class AesirGetStartedService.AesirSampleInfo
```

单个示例信息（元数据来自 package.json samples 清单，路径经扫描解析）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirGetStartedService.AesirSampleInfo()`](#constructor-aesirgetstartedservice-aesirsampleinfo) | — |

</div>

### AesirGetStartedService.AesirSampleInfo() {#constructor-aesirgetstartedservice-aesirsampleinfo}

``` csharp
public AesirGetStartedService.AesirSampleInfo()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Package`](#field-package) | 所属包。 |
| [`Description`](#field-description) | 示例描述（package.json samples.description）。 |
| [`DisplayName`](#field-displayname) | 示例显示名（package.json samples.displayName）。 |
| [`RelativeDir`](#field-relativedir) | 包内相对目录（samples.path 去掉 Samples~/ 前缀，如 Counter-Mvc-Quick、Events/02_Filters）。 |
| [`RootPath`](#field-rootpath) | 示例根目录（项目 Assets 相对路径；未导入为 null）。 |
| [`ScenePath`](#field-scenepath) | 示例场景（Assets 相对路径；无场景示例为 null）。 |

</div>

### Package {#field-package}

所属包。

``` csharp
public AesirGetStartedService.AesirPackageInfo Package;
```

### Description {#field-description}

示例描述（package.json samples.description）。

``` csharp
public string Description;
```

### DisplayName {#field-displayname}

示例显示名（package.json samples.displayName）。

``` csharp
public string DisplayName;
```

### RelativeDir {#field-relativedir}

包内相对目录（samples.path 去掉 Samples~/ 前缀，如 Counter-Mvc-Quick、Events/02_Filters）。

``` csharp
public string RelativeDir;
```

### RootPath {#field-rootpath}

示例根目录（项目 Assets 相对路径；未导入为 null）。

``` csharp
public string RootPath;
```

### ScenePath {#field-scenepath}

示例场景（Assets 相对路径；无场景示例为 null）。

``` csharp
public string ScenePath;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`HasScene`](#property-hasscene) | 示例含可直接打开的场景。 |
| [`IsImported`](#property-isimported) | 示例已导入（根目录存在于 Assets 中）。 |

</div>

### HasScene {#property-hasscene}

示例含可直接打开的场景。

``` csharp
public bool HasScene { get; }
```

### IsImported {#property-isimported}

示例已导入（根目录存在于 Assets 中）。

``` csharp
public bool IsImported { get; }
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
