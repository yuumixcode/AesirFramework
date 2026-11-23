---
title: AesirGetStartedService.AesirInstallType
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedService.AesirInstallType 的 API 文档"
---

# `AesirGetStartedService.AesirInstallType`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `AesirGetStartedService.AesirInstallType`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum AesirGetStartedService.AesirInstallType : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

包安装形态（决定示例根目录的解析方式）。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AssetsCopy`](#field-assetscopy) | 代码导入 Assets/Runestone（复制 / unitypackage），示例位于包内 Samples/。 |
| [`Embedded`](#field-embedded) | 嵌入式包（Packages/ 目录），示例经 Package Manager 导入到 Assets/Samples/。 |
| [`Upm`](#field-upm) | — |

</div>

### AssetsCopy {#field-assetscopy}

代码导入 Assets/Runestone（复制 / unitypackage），示例位于包内 Samples/。

``` csharp
public const AesirGetStartedService.AesirInstallType AssetsCopy;
```

### Embedded {#field-embedded}

嵌入式包（Packages/ 目录），示例经 Package Manager 导入到 Assets/Samples/。

``` csharp
public const AesirGetStartedService.AesirInstallType Embedded;
```

### Upm {#field-upm}

``` csharp
public const AesirGetStartedService.AesirInstallType Upm;
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
