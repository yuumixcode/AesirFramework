---
title: WorldSpaceUICanvas.WorldSpaceBillboardMode
description: "Runestone.AesirModules.WorldSpaceUICanvas.WorldSpaceBillboardMode 的 API 文档"
---

# `WorldSpaceUICanvas.WorldSpaceBillboardMode`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `WorldSpaceUICanvas.WorldSpaceBillboardMode`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum WorldSpaceUICanvas.WorldSpaceBillboardMode : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

世界空间 UI 的朝向模式。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CameraFacingFull`](#field-camerafacingfull) | 完整面向 UI 相机（平面广告牌，适合始终正对屏幕的标记）。 |
| [`CameraFacingYLocked`](#field-camerafacingylocked) | 面向 UI 相机，忽略 X / Y 轴旋转，保持 Y 轴竖直（适合头顶信息牌）。 |
| [`None`](#field-none) | 不自动调整朝向，完全尊重 Canvas 父级的旋转。 |

</div>

### CameraFacingFull {#field-camerafacingfull}

完整面向 UI 相机（平面广告牌，适合始终正对屏幕的标记）。

``` csharp
public const WorldSpaceUICanvas.WorldSpaceBillboardMode CameraFacingFull;
```

### CameraFacingYLocked {#field-camerafacingylocked}

面向 UI 相机，忽略 X / Y 轴旋转，保持 Y 轴竖直（适合头顶信息牌）。

``` csharp
public const WorldSpaceUICanvas.WorldSpaceBillboardMode CameraFacingYLocked;
```

### None {#field-none}

不自动调整朝向，完全尊重 Canvas 父级的旋转。

``` csharp
public const WorldSpaceUICanvas.WorldSpaceBillboardMode None;
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
