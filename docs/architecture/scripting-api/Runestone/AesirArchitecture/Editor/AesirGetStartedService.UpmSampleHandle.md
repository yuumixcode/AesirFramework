---
title: AesirGetStartedService.UpmSampleHandle
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedService.UpmSampleHandle 的 API 文档"
---

# `AesirGetStartedService.UpmSampleHandle`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirGetStartedService.UpmSampleHandle`

## 声明

``` csharp
internal sealed class AesirGetStartedService.UpmSampleHandle
```

UnityEditor.PackageManager.UI.Sample 的最小投影（显示名 / 已导入态 / 导入委托）。 真实类型构造非公开、不可继承，测试经 ImportUpmSample 的 finder 参数注入伪造实现。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirGetStartedService.UpmSampleHandle()`](#constructor-aesirgetstartedservice-upmsamplehandle) | — |

</div>

### AesirGetStartedService.UpmSampleHandle() {#constructor-aesirgetstartedservice-upmsamplehandle}

``` csharp
public AesirGetStartedService.UpmSampleHandle()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Import`](#field-import) | 执行导入，返回是否成功（真实实现等价无参 Sample.Import()）。 |
| [`IsImported`](#field-isimported) | 是否已导入（导入目录已存在于 Assets/Samples）。 |
| [`DisplayName`](#field-displayname) | 示例显示名（package.json samples.displayName，与清单同源的匹配键）。 |

</div>

### Import {#field-import}

执行导入，返回是否成功（真实实现等价无参 Sample.Import()）。

``` csharp
public Func<bool> Import;
```

### IsImported {#field-isimported}

是否已导入（导入目录已存在于 Assets/Samples）。

``` csharp
public bool IsImported;
```

### DisplayName {#field-displayname}

示例显示名（package.json samples.displayName，与清单同源的匹配键）。

``` csharp
public string DisplayName;
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
