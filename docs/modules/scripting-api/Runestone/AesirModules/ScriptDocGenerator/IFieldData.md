---
title: IFieldData
description: "Runestone.AesirModules.ScriptDocGenerator.IFieldData 的 API 文档"
---

# `IFieldData`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`

## 声明

``` csharp
public interface IFieldData : Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData
```

字段数据接口，继承自 IDerivedMemberData

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`FieldType`](#property-fieldtype) | — |
| [`IsConstant`](#property-isconstant) | — |
| [`IsDynamic`](#property-isdynamic) | — |
| [`IsReadOnly`](#property-isreadonly) | — |
| [`DefaultValue`](#property-defaultvalue) | — |
| [`FieldTypeFullName`](#property-fieldtypefullname) | — |
| [`FieldTypeName`](#property-fieldtypename) | — |

</div>

### FieldType {#property-fieldtype}

``` csharp
public Type FieldType { get; }
```

### IsConstant {#property-isconstant}

``` csharp
public bool IsConstant { get; }
```

### IsDynamic {#property-isdynamic}

``` csharp
public bool IsDynamic { get; }
```

### IsReadOnly {#property-isreadonly}

``` csharp
public bool IsReadOnly { get; }
```

### DefaultValue {#property-defaultvalue}

``` csharp
public object DefaultValue { get; }
```

### FieldTypeFullName {#property-fieldtypefullname}

``` csharp
public string FieldTypeFullName { get; }
```

### FieldTypeName {#property-fieldtypename}

``` csharp
public string FieldTypeName { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
