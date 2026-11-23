---
title: SourceScanner.Frame
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.SourceScanner.Frame 的 API 文档"
---

# `SourceScanner.Frame`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `SourceScanner.Frame`

## 声明

``` csharp
private sealed class SourceScanner.Frame
```

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SourceScanner.Frame(string, int)`](#constructor-sourcescanner-frame-string-int) | — |

</div>

### SourceScanner.Frame(string, int) {#constructor-sourcescanner-frame-string-int}

``` csharp
public SourceScanner.Frame(string name, int bodyDepth)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `name` | `string` | — |
| `bodyDepth` | `int` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Entered`](#field-entered) | — |
| [`BodyDepth`](#field-bodydepth) | — |
| [`Name`](#field-name) | — |

</div>

### Entered {#field-entered}

``` csharp
public bool Entered;
```

### BodyDepth {#field-bodydepth}

``` csharp
public readonly int BodyDepth;
```

### Name {#field-name}

``` csharp
public readonly string Name;
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
