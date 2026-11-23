---
title: IParameterData
description: "Runestone.AesirModules.ScriptDocGenerator.IParameterData 的 API 文档"
---

# `IParameterData`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

## 声明

``` csharp
public interface IParameterData
```

参数信息解析数据接口

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Direction`](#property-direction) | — |
| [`ParameterType`](#property-parametertype) | — |
| [`HasDefaultValue`](#property-hasdefaultvalue) | — |
| [`IsParams`](#property-isparams) | — |
| [`DefaultValue`](#property-defaultvalue) | — |
| [`Name`](#property-name) | — |

</div>

### Direction {#property-direction}

``` csharp
public ParameterDirection Direction { get; }
```

### ParameterType {#property-parametertype}

``` csharp
public Type ParameterType { get; }
```

### HasDefaultValue {#property-hasdefaultvalue}

``` csharp
public bool HasDefaultValue { get; }
```

### IsParams {#property-isparams}

``` csharp
public bool IsParams { get; }
```

### DefaultValue {#property-defaultvalue}

``` csharp
public object DefaultValue { get; }
```

### Name {#property-name}

``` csharp
public string Name { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetFormattedString()`](#method-getformattedstring) | 生成格式化的参数字符串 |

</div>

### GetFormattedString() {#method-getformattedstring}

生成格式化的参数字符串

``` csharp
public abstract string GetFormattedString()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
