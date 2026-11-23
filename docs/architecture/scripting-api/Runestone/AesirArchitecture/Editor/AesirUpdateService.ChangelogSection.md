---
title: AesirUpdateService.ChangelogSection
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.ChangelogSection 的 API 文档"
---

# `AesirUpdateService.ChangelogSection`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.ChangelogSection`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.ChangelogSection
```

CHANGELOG 中的一个版本段落（Keep a Changelog 格式的 ## [x.y.z] - 日期 小节）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.ChangelogSection()`](#constructor-aesirupdateservice-changelogsection) | — |

</div>

### AesirUpdateService.ChangelogSection() {#constructor-aesirupdateservice-changelogsection}

``` csharp
public AesirUpdateService.ChangelogSection()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Content`](#field-content) | 段落正文（不含标题行；已剔除首尾空行与 --- 分隔线）。 |
| [`Date`](#field-date) | 发布日期（标题行 - 后的文本，可为空）。 |
| [`Version`](#field-version) | 版本号（如 0.21.0，无 v 前缀）。 |

</div>

### Content {#field-content}

段落正文（不含标题行；已剔除首尾空行与 --- 分隔线）。

``` csharp
public string Content;
```

### Date {#field-date}

发布日期（标题行 - 后的文本，可为空）。

``` csharp
public string Date;
```

### Version {#field-version}

版本号（如 0.21.0，无 v 前缀）。

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
