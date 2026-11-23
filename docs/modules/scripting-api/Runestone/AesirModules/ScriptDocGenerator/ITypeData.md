---
title: ITypeData
description: "Runestone.AesirModules.ScriptDocGenerator.ITypeData 的 API 文档"
---

# `ITypeData`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`

## 声明

``` csharp
public interface ITypeData : Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData
```

类型解析数据接口，继承自 IDerivedMemberData 接口

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Assembly`](#property-assembly) | — |
| [`DataFactory`](#property-datafactory) | — |
| [`RuntimeReflectedConstructorsData`](#property-runtimereflectedconstructorsdata) | — |
| [`RuntimeReflectedEventsData`](#property-runtimereflectedeventsdata) | — |
| [`RuntimeReflectedFieldsData`](#property-runtimereflectedfieldsdata) | — |
| [`RuntimeReflectedMethodsData`](#property-runtimereflectedmethodsdata) | — |
| [`RuntimeReflectedPropertiesData`](#property-runtimereflectedpropertiesdata) | — |
| [`TypeParamSummaries`](#property-typeparamsummaries) | 泛型参数注释字典（XML <typeparam> 标签），键为参数名。无注释时为 null |
| [`TypeCategory`](#property-typecategory) | — |
| [`IsAbstract`](#property-isabstract) | — |
| [`IsGenericType`](#property-isgenerictype) | — |
| [`IsSealed`](#property-issealed) | — |
| [`AssemblyName`](#property-assemblyname) | — |
| [`NamespaceName`](#property-namespacename) | — |
| [`InheritanceChain`](#property-inheritancechain) | — |
| [`InterfaceArray`](#property-interfacearray) | — |
| [`ReferenceWebLinkArray`](#property-referenceweblinkarray) | — |

</div>

### Assembly {#property-assembly}

``` csharp
public Assembly Assembly { get; }
```

### DataFactory {#property-datafactory}

``` csharp
public IAnalysisDataFactory DataFactory { get; }
```

### RuntimeReflectedConstructorsData {#property-runtimereflectedconstructorsdata}

``` csharp
public IConstructorData[] RuntimeReflectedConstructorsData { get; }
```

### RuntimeReflectedEventsData {#property-runtimereflectedeventsdata}

``` csharp
public IEventData[] RuntimeReflectedEventsData { get; }
```

### RuntimeReflectedFieldsData {#property-runtimereflectedfieldsdata}

``` csharp
public IFieldData[] RuntimeReflectedFieldsData { get; }
```

### RuntimeReflectedMethodsData {#property-runtimereflectedmethodsdata}

``` csharp
public IMethodData[] RuntimeReflectedMethodsData { get; }
```

### RuntimeReflectedPropertiesData {#property-runtimereflectedpropertiesdata}

``` csharp
public IPropertyData[] RuntimeReflectedPropertiesData { get; }
```

### TypeParamSummaries {#property-typeparamsummaries}

泛型参数注释字典（XML <typeparam> 标签），键为参数名。无注释时为 null

``` csharp
public IReadOnlyDictionary<string, string> TypeParamSummaries { get; }
```

### TypeCategory {#property-typecategory}

``` csharp
public TypeCategory TypeCategory { get; }
```

### IsAbstract {#property-isabstract}

``` csharp
public bool IsAbstract { get; }
```

### IsGenericType {#property-isgenerictype}

``` csharp
public bool IsGenericType { get; }
```

### IsSealed {#property-issealed}

``` csharp
public bool IsSealed { get; }
```

### AssemblyName {#property-assemblyname}

``` csharp
public string AssemblyName { get; }
```

### NamespaceName {#property-namespacename}

``` csharp
public string NamespaceName { get; }
```

### InheritanceChain {#property-inheritancechain}

``` csharp
public string[] InheritanceChain { get; }
```

### InterfaceArray {#property-interfacearray}

``` csharp
public string[] InterfaceArray { get; }
```

### ReferenceWebLinkArray {#property-referenceweblinkarray}

``` csharp
public string[] ReferenceWebLinkArray { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
