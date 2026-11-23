---
title: AesirGetStartedService.AesirKnownPackage
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedService.AesirKnownPackage 的 API 文档"
---

# `AesirGetStartedService.AesirKnownPackage`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirGetStartedService.AesirKnownPackage`

## 声明

``` csharp
public sealed class AesirGetStartedService.AesirKnownPackage
```

框架已知包（概览页按此补全「未安装」占位卡片；新增公开包时在此登记）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirGetStartedService.AesirKnownPackage()`](#constructor-aesirgetstartedservice-aesirknownpackage) | — |

</div>

### AesirGetStartedService.AesirKnownPackage() {#constructor-aesirgetstartedservice-aesirknownpackage}

``` csharp
public AesirGetStartedService.AesirKnownPackage()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Description`](#field-description) | 包定位描述（概览卡片文案）。 |
| [`DirName`](#field-dirname) | 包目录名（Release 资产命名与默认安装目录名；更新器的补装与差集清理亦按此定位）。 |
| [`DisplayName`](#field-displayname) | 包显示名（概览卡片标题）。 |
| [`Id`](#field-id) | package.json name（包唯一标识）。 |

</div>

### Description {#field-description}

包定位描述（概览卡片文案）。

``` csharp
public string Description;
```

### DirName {#field-dirname}

包目录名（Release 资产命名与默认安装目录名；更新器的补装与差集清理亦按此定位）。

``` csharp
public string DirName;
```

### DisplayName {#field-displayname}

包显示名（概览卡片标题）。

``` csharp
public string DisplayName;
```

### Id {#field-id}

package.json name（包唯一标识）。

``` csharp
public string Id;
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
