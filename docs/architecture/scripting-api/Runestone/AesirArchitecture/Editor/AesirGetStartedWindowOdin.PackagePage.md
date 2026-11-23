---
title: AesirGetStartedWindowOdin.PackagePage
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedWindowOdin.PackagePage 的 API 文档"
---

# `AesirGetStartedWindowOdin.PackagePage`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `Runestone.AesirArchitecture.Editor.AesirGetStartedWindowOdin.GetStartedPage` → `AesirGetStartedWindowOdin.PackagePage`

## 声明

``` csharp
private sealed class AesirGetStartedWindowOdin.PackagePage : Runestone.AesirArchitecture.Editor.AesirGetStartedWindowOdin.GetStartedPage
```

包示例页：按教学分组列出示例卡片，点击卡片打开示例场景。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirGetStartedWindowOdin.PackagePage()`](#constructor-aesirgetstartedwindowodin-packagepage) | — |

</div>

### AesirGetStartedWindowOdin.PackagePage() {#constructor-aesirgetstartedwindowodin-packagepage}

``` csharp
public AesirGetStartedWindowOdin.PackagePage()
```

## 字段

**声明的字段**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Package`](#field-package) | — |

</div>

**继承的字段**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `Window` | — | `AesirGetStartedWindowOdin.GetStartedPage` |
| `TitleIcon` | — | `AesirGetStartedWindowOdin.GetStartedPage` |
| `Title` | — | `AesirGetStartedWindowOdin.GetStartedPage` |
| `FooterSize` | — | `AesirGetStartedWindowOdin.GetStartedPage` |
| `ScrollPosition` | — | `AesirGetStartedWindowOdin.GetStartedPage` |

</div>

### Package {#field-package}

``` csharp
public AesirGetStartedService.AesirPackageInfo Package;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `EntranceT` | 入场进度（1 = 页面完全展开；概览收起时为 0，条目随其渐显增高）。 | `AesirGetStartedWindowOdin.GetStartedPage` |

</div>

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Rebind(AesirGetStartedService.AesirPackageInfo)`](#method-rebind-aesirgetstartedservice-aesirpackageinfo) | （重）绑定包实例并重建分组视图与页脚（导入示例后 RefreshAfterImport 重扫刷新复用）。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `DrawFooter(Rect)` | — | `AesirGetStartedWindowOdin.PackagePage` |
| `DrawPage(Rect)` | — | `AesirGetStartedWindowOdin.PackagePage` |
| `EnterPage()` | — | `AesirGetStartedWindowOdin.PackagePage` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `GoBack()` | — | `AesirGetStartedWindowOdin.GetStartedPage` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `BeginScrollableLayoutPage(Rect, int)` | — | `AesirGetStartedWindowOdin.GetStartedPage` |
| `EndScrollableLayoutPage()` | — | `AesirGetStartedWindowOdin.GetStartedPage` |

</div>

### Rebind(AesirGetStartedService.AesirPackageInfo) {#method-rebind-aesirgetstartedservice-aesirpackageinfo}

（重）绑定包实例并重建分组视图与页脚（导入示例后 RefreshAfterImport 重扫刷新复用）。

``` csharp
public void Rebind(AesirGetStartedService.AesirPackageInfo pkg)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `pkg` | `AesirGetStartedService.AesirPackageInfo` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
