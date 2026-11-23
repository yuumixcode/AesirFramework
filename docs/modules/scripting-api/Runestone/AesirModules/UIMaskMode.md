---
title: UIMaskMode
description: "Runestone.AesirModules.UIMaskMode 的 API 文档"
---

# `UIMaskMode`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `UIMaskMode`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum UIMaskMode : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

窗口蒙版调度模式，初始值配置于 UIModuleConfigSO（Resources 兜底资产， 可显式创建；缺资产时用内存默认配置），运行时可经 MaskMode 切换。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Single`](#field-single) | 单遮模式：全局仅最高层可见窗口的蒙版生效，多窗口叠加时透明度不叠加。 |
| [`Stacked`](#field-stacked) | 叠遮模式：每个窗口的蒙版独立生效，多窗口叠加时透明度逐层叠加。 |

</div>

### Single {#field-single}

单遮模式：全局仅最高层可见窗口的蒙版生效，多窗口叠加时透明度不叠加。

``` csharp
[InspectorName]
public const UIMaskMode Single;
```

### Stacked {#field-stacked}

叠遮模式：每个窗口的蒙版独立生效，多窗口叠加时透明度逐层叠加。

``` csharp
[InspectorName]
public const UIMaskMode Stacked;
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
