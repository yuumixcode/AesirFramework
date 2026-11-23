---
title: AesirGetStartedWindowOdin.PackageCard
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedWindowOdin.PackageCard 的 API 文档"
---

# `AesirGetStartedWindowOdin.PackageCard`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `AesirGetStartedWindowOdin.PackageCard`

## 声明

``` csharp
private class AesirGetStartedWindowOdin.PackageCard
```

概览卡片视图模型（扫描时重建；显示文本预计算，OnGUI 零拼接）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirGetStartedWindowOdin.PackageCard()`](#constructor-aesirgetstartedwindowodin-packagecard) | — |

</div>

### AesirGetStartedWindowOdin.PackageCard() {#constructor-aesirgetstartedwindowodin-packagecard}

``` csharp
public AesirGetStartedWindowOdin.PackageCard()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Installed`](#field-installed) | — |
| [`Description`](#field-description) | — |
| [`DisplayName`](#field-displayname) | — |
| [`StatusText`](#field-statustext) | 状态行文本（扫描时预计算：已安装 · v0.23.0 / 未安装）。 |

</div>

### Installed {#field-installed}

``` csharp
public AesirGetStartedService.AesirPackageInfo Installed;
```

### Description {#field-description}

``` csharp
public string Description;
```

### DisplayName {#field-displayname}

``` csharp
public string DisplayName;
```

### StatusText {#field-statustext}

状态行文本（扫描时预计算：已安装 · v0.23.0 / 未安装）。

``` csharp
public string StatusText;
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
