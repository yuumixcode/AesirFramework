---
title: IConstructorData
description: "Runestone.AesirModules.ScriptDocGenerator.IConstructorData 的 API 文档"
---

# `IConstructorData`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData`

## 声明

``` csharp
public interface IConstructorData : Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData
```

构造方法数据接口，继承自 IDerivedMemberData

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Parameters`](#property-parameters) | — |
| [`ParamSummaries`](#property-paramsummaries) | 参数级注释字典（XML <param> 标签），键为参数名。无参数注释时为 null |
| [`ParametersDeclaration`](#property-parametersdeclaration) | — |
| [`SignatureWithoutParameters`](#property-signaturewithoutparameters) | — |

</div>

### Parameters {#property-parameters}

``` csharp
public IParameterData[] Parameters { get; }
```

### ParamSummaries {#property-paramsummaries}

参数级注释字典（XML <param> 标签），键为参数名。无参数注释时为 null

``` csharp
public IReadOnlyDictionary<string, string> ParamSummaries { get; }
```

### ParametersDeclaration {#property-parametersdeclaration}

``` csharp
public string ParametersDeclaration { get; }
```

### SignatureWithoutParameters {#property-signaturewithoutparameters}

``` csharp
public string SignatureWithoutParameters { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
