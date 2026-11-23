---
title: ScriptDocGeneratorUtility
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorUtility 的 API 文档"
---

# `ScriptDocGeneratorUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `ScriptDocGeneratorUtility`

## 声明

``` csharp
public static class ScriptDocGeneratorUtility
```

脚本文档生成器逻辑控制类，负责处理文档生成的核心逻辑

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AnalyzeSingleType(Type)`](#method-analyzesingletype-type) | — |
| [`AnalyzeMultipleAssemblies(List<string>)`](#method-analyzemultipleassemblies-list-string) | 分析多个程序集中的所有类型 |
| [`AnalyzeMultipleTypes(List<Type>)`](#method-analyzemultipletypes-list-type) | — |
| [`AnalyzeMultipleTypes(TypesCacheSO)`](#method-analyzemultipletypes-typescacheso) | — |
| [`AnalyzeSingleAssembly(string)`](#method-analyzesingleassembly-string) | — |
| [`EnsureInitialized()`](#method-ensureinitialized) | 检查 Script Doc Generator 是否已初始化。未初始化时提示用户。 |
| [`GenerateMultipleTypeDocs(List<ITypeData>, DocGeneratorSettingsSO, string)`](#method-generatemultipletypedocs-list-itypedata-docgeneratorsettingsso-string) | — |
| [`GenerateSingleTypeDoc(ITypeData, DocGeneratorSettingsSO, string)`](#method-generatesingletypedoc-itypedata-docgeneratorsettingsso-string) | — |
| [`InitializeAssets()`](#method-initializeassets) | — |

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

### AnalyzeSingleType(Type) {#method-analyzesingletype-type}

``` csharp
public static ITypeData AnalyzeSingleType(Type targetType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `targetType` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ITypeData` | — |

</div>

### AnalyzeMultipleAssemblies(List<string>) {#method-analyzemultipleassemblies-list-string}

分析多个程序集中的所有类型

``` csharp
public static List<ITypeData> AnalyzeMultipleAssemblies(List<string> assemblyFullNames)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `assemblyFullNames` | `List<string>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<ITypeData>` | — |

</div>

### AnalyzeMultipleTypes(List<Type>) {#method-analyzemultipletypes-list-type}

``` csharp
public static List<ITypeData> AnalyzeMultipleTypes(List<Type> types)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `types` | `List<Type>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<ITypeData>` | — |

</div>

### AnalyzeMultipleTypes(TypesCacheSO) {#method-analyzemultipletypes-typescacheso}

``` csharp
public static List<ITypeData> AnalyzeMultipleTypes(TypesCacheSO typesCache)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `typesCache` | `TypesCacheSO` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<ITypeData>` | — |

</div>

### AnalyzeSingleAssembly(string) {#method-analyzesingleassembly-string}

``` csharp
public static List<ITypeData> AnalyzeSingleAssembly(string assemblyFullName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `assemblyFullName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<ITypeData>` | — |

</div>

### EnsureInitialized() {#method-ensureinitialized}

检查 Script Doc Generator 是否已初始化。未初始化时提示用户。

``` csharp
public static bool EnsureInitialized()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | true 表示已初始化（或用户刚确认初始化）；false 表示用户取消了初始化。 |

</div>

### GenerateMultipleTypeDocs(List<ITypeData>, DocGeneratorSettingsSO, string) {#method-generatemultipletypedocs-list-itypedata-docgeneratorsettingsso-string}

``` csharp
public static void GenerateMultipleTypeDocs(List<ITypeData> typeDataCollection, DocGeneratorSettingsSO generatorSettings, string targetFolderPath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `typeDataCollection` | `List<ITypeData>` | — |
| `generatorSettings` | `DocGeneratorSettingsSO` | — |
| `targetFolderPath` | `string` | — |

</div>

### GenerateSingleTypeDoc(ITypeData, DocGeneratorSettingsSO, string) {#method-generatesingletypedoc-itypedata-docgeneratorsettingsso-string}

``` csharp
public static void GenerateSingleTypeDoc(ITypeData typeData, DocGeneratorSettingsSO generatorSettings, string targetFolderPath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `typeData` | `ITypeData` | — |
| `generatorSettings` | `DocGeneratorSettingsSO` | — |
| `targetFolderPath` | `string` | — |

</div>

### InitializeAssets() {#method-initializeassets}

``` csharp
public static void InitializeAssets()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
