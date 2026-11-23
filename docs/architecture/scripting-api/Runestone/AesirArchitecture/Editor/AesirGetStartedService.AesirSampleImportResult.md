---
title: AesirGetStartedService.AesirSampleImportResult
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedService.AesirSampleImportResult 的 API 文档"
---

# `AesirGetStartedService.AesirSampleImportResult`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `AesirGetStartedService.AesirSampleImportResult`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum AesirGetStartedService.AesirSampleImportResult : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

UPM / 嵌入式安装的示例导入结果。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Cancelled`](#field-cancelled) | 用户在确认框取消了导入。 |
| [`Failed`](#field-failed) | — |
| [`Imported`](#field-imported) | 导入成功（含示例目录已存在时的幂等跳过）。 |
| [`NotFound`](#field-notfound) | Package Manager 示例清单中未找到对应条目（按显示名匹配失败）。 |

</div>

### Cancelled {#field-cancelled}

用户在确认框取消了导入。

``` csharp
public const AesirGetStartedService.AesirSampleImportResult Cancelled;
```

### Failed {#field-failed}

``` csharp
public const AesirGetStartedService.AesirSampleImportResult Failed;
```

### Imported {#field-imported}

导入成功（含示例目录已存在时的幂等跳过）。

``` csharp
public const AesirGetStartedService.AesirSampleImportResult Imported;
```

### NotFound {#field-notfound}

Package Manager 示例清单中未找到对应条目（按显示名匹配失败）。

``` csharp
public const AesirGetStartedService.AesirSampleImportResult NotFound;
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `HasFlag(Enum)` | — | `Enum` |
| `Equals(object)` | — | `Enum` |
| `GetHashCode()` | — | `Enum` |
| `ToString()` | — | `Enum` |
| `ToString(string)` | — | `Enum` |
| `GetTypeCode()` | — | `Enum` |
| `CompareTo(object)` | — | `Enum` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `ToString(IFormatProvider)` | — | `Enum` |
| `ToString(string, IFormatProvider)` | — | `Enum` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
