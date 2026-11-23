---
title: IDerivedMemberData
description: "Runestone.AesirModules.ScriptDocGenerator.IDerivedMemberData 的 API 文档"
---

# `IDerivedMemberData`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

## 声明

``` csharp
public interface IDerivedMemberData
```

派生成员数据接口，不同的派生类有不同的表现形式，MemberData 无法直接准确获取的信息，需要派生类自己实现

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | — |
| [`MemberType`](#property-membertype) | — |
| [`IsStatic`](#property-isstatic) | — |
| [`AccessModifierName`](#property-accessmodifiername) | — |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | — |
| [`MemberTypeName`](#property-membertypename) | — |
| [`Signature`](#property-signature) | — |

</div>

### AccessModifier {#property-accessmodifier}

``` csharp
public AccessModifierType AccessModifier { get; }
```

### MemberType {#property-membertype}

``` csharp
public MemberTypes MemberType { get; }
```

### IsStatic {#property-isstatic}

``` csharp
public bool IsStatic { get; }
```

### AccessModifierName {#property-accessmodifiername}

``` csharp
public string AccessModifierName { get; }
```

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

``` csharp
public string FullDeclarationWithAttributes { get; }
```

### MemberTypeName {#property-membertypename}

``` csharp
public string MemberTypeName { get; }
```

### Signature {#property-signature}

``` csharp
public string Signature { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
