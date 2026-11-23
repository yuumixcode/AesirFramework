---
title: IPropertyData
description: "Runestone.AesirModules.ScriptDocGenerator.IPropertyData 的 API 文档"
---

# `IPropertyData`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`

## 声明

``` csharp
public interface IPropertyData : Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData
```

属性数据接口，继承自 IDerivedMemberData，包含属性特有的数据信息和方法，派生类的通用数据信息和方法

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`PropertyType`](#property-propertytype) | — |
| [`DefaultValue`](#property-defaultvalue) | — |
| [`PropertyTypeFullName`](#property-propertytypefullname) | — |
| [`PropertyTypeName`](#property-propertytypename) | — |
| [`ValueSummary`](#property-valuesummary) | — |

</div>

### PropertyType {#property-propertytype}

``` csharp
public Type PropertyType { get; }
```

### DefaultValue {#property-defaultvalue}

``` csharp
public object DefaultValue { get; }
```

### PropertyTypeFullName {#property-propertytypefullname}

``` csharp
public string PropertyTypeFullName { get; }
```

### PropertyTypeName {#property-propertytypename}

``` csharp
public string PropertyTypeName { get; }
```

### ValueSummary {#property-valuesummary}

``` csharp
public string ValueSummary { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
