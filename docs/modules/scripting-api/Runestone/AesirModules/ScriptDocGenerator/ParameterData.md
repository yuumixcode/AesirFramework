---
title: ParameterData
description: "Runestone.AesirModules.ScriptDocGenerator.ParameterData 的 API 文档"
---

# `ParameterData`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `ParameterData`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IParameterData`

## 声明

``` csharp
[Serializable]
public class ParameterData : Runestone.AesirModules.ScriptDocGenerator.IParameterData
```

参数信息解析数据

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ParameterData(ParameterInfo)`](#constructor-parameterdata-parameterinfo) | 创建参数信息解析数据实例 |

</div>

### ParameterData(ParameterInfo) {#constructor-parameterdata-parameterinfo}

创建参数信息解析数据实例

``` csharp
public ParameterData(ParameterInfo parameterInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `parameterInfo` | `ParameterInfo` | — |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Direction`](#property-direction) | 参数方向（in/out/ref） |
| [`ParameterType`](#property-parametertype) | 参数类型 |
| [`HasDefaultValue`](#property-hasdefaultvalue) | 是否有默认值 |
| [`IsParams`](#property-isparams) | 是否为 params 参数 |
| [`DefaultValue`](#property-defaultvalue) | 默认值 |
| [`Name`](#property-name) | 参数名称 |

</div>

### Direction {#property-direction}

参数方向（in/out/ref）

``` csharp
public ParameterDirection Direction { get; }
```

### ParameterType {#property-parametertype}

参数类型

``` csharp
public Type ParameterType { get; }
```

### HasDefaultValue {#property-hasdefaultvalue}

是否有默认值

``` csharp
public bool HasDefaultValue { get; }
```

### IsParams {#property-isparams}

是否为 params 参数

``` csharp
public bool IsParams { get; }
```

### DefaultValue {#property-defaultvalue}

默认值

``` csharp
public object DefaultValue { get; }
```

### Name {#property-name}

参数名称

``` csharp
public string Name { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetFormattedString()`](#method-getformattedstring) | 生成格式化的参数字符串 |

</div>

**继承的方法**

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

### GetFormattedString() {#method-getformattedstring}

生成格式化的参数字符串

``` csharp
public string GetFormattedString()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
