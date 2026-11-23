---
title: ScriptDocGeneratorAPI
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorAPI 的 API 文档"
---

# `ScriptDocGeneratorAPI`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `ScriptDocGeneratorAPI`

## 声明

``` csharp
public static class ScriptDocGeneratorAPI
```

脚本文档生成器静态 API —— 面板（Tools → Aesir → Modules → Script Doc Generator）的无 UI 等价入口， 面向自动化脚本与 AI 助手直接调用：不需要打开面板、不需要任何点击。

**备注**

与面板共用同一套分析与写入核心（ScriptDocGeneratorUtility）， 但全程无确认弹窗、不自动打开生成结果。覆盖语义与面板"多程序集模式"一致： 已存在的文档按增量规则合并（保留 Front Matter 与 ## Additional Notes 之后的手写内容），未存在则新建。

调用示例（AI / 自动化）： - 为程序集生成 Zensical 文档： ScriptDocGeneratorAPI.GenerateDocsForAssembly("Runestone.AesirModules", ScriptDocGeneratorAPI.ZensicalSettings) - 为文件夹内全部脚本生成默认文档： ScriptDocGeneratorAPI.GenerateDocsForFolder("Assets/Runestone/AesirModules/Runtime/Scene") - 为单个类型生成文档：ScriptDocGeneratorAPI.GenerateDocsForType(typeof(SomeClass))

settings 传 null 使用 DefaultSettings；outputFolder 传 null 使用 DefaultOutputFolder（项目根 ScriptDocGenerator/，Assets 外不产生 .meta）。 自定义生成器：继承 DocGeneratorSettingsSO 创建设置资产后传入， 或经 FindAllSettings 枚举项目内已有设置资产。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultSettings`](#property-defaultsettings) | 内置中文 API 文档生成设置（与面板同款预设资产）。 |
| [`ZensicalSettings`](#property-zensicalsettings) | 内置 Zensical 静态站点文档生成设置（与面板同款预设资产）。 |
| [`DefaultOutputFolder`](#property-defaultoutputfolder) | 默认输出根目录：项目根 ScriptDocGenerator/（Assets 外，不为生成文档产生 .meta）。 |

</div>

### DefaultSettings {#property-defaultsettings}

内置中文 API 文档生成设置（与面板同款预设资产）。

``` csharp
public static DocGeneratorSettingsSO DefaultSettings { get; }
```

### ZensicalSettings {#property-zensicalsettings}

内置 Zensical 静态站点文档生成设置（与面板同款预设资产）。

``` csharp
public static DocGeneratorSettingsSO ZensicalSettings { get; }
```

### DefaultOutputFolder {#property-defaultoutputfolder}

默认输出根目录：项目根 ScriptDocGenerator/（Assets 外，不为生成文档产生 .meta）。

``` csharp
public static string DefaultOutputFolder { get; } = "/Users/yuumix/Projects/Unity/AesirFramework/ScriptDocGenerator";
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`FindAllSettings()`](#method-findallsettings) | 枚举项目内全部文档生成器设置资产：含两个内置预设（Default / Zensical）与 用户自建的 DocGeneratorSettingsSO 派生资产，按资产名排序。 |
| [`GenerateDocsForAssembly(string, DocGeneratorSettingsSO, string)`](#method-generatedocsforassembly-string-docgeneratorsettingsso-string) | 为一个程序集内的全部类型生成脚本文档（等价面板"单程序集模式"的静默版）。 |
| [`GenerateDocsForFolder(string, DocGeneratorSettingsSO, string)`](#method-generatedocsforfolder-string-docgeneratorsettingsso-string) | 为一个文件夹（含子文件夹）内全部脚本声明的类型生成脚本文档。 源文件经 SourceScanner 解析出类型与命名空间声明后映射到当前编译域内的类型， 支持普通 C# 类（不限 MonoBehaviour / ScriptableObject）；解析不到的类型名记入 UnresolvedTypeNames（典型成因：被条件编译剔除）。 |
| [`GenerateDocsForType(Type, DocGeneratorSettingsSO, string)`](#method-generatedocsfortype-type-docgeneratorsettingsso-string) | 为单个类型生成脚本文档。 |
| [`GenerateDocsForTypes(IEnumerable<Type>, DocGeneratorSettingsSO, string)`](#method-generatedocsfortypes-ienumerable-type-docgeneratorsettingsso-string) | 为一组类型生成脚本文档（等价面板"多类型模式"的静默版）。 |

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

### FindAllSettings() {#method-findallsettings}

枚举项目内全部文档生成器设置资产：含两个内置预设（Default / Zensical）与 用户自建的 DocGeneratorSettingsSO 派生资产，按资产名排序。

``` csharp
public static List<DocGeneratorSettingsSO> FindAllSettings()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<DocGeneratorSettingsSO>` | — |

</div>

### GenerateDocsForAssembly(string, DocGeneratorSettingsSO, string) {#method-generatedocsforassembly-string-docgeneratorsettingsso-string}

为一个程序集内的全部类型生成脚本文档（等价面板"单程序集模式"的静默版）。

``` csharp
public static ScriptDocGenerationResult GenerateDocsForAssembly(string assemblyName, DocGeneratorSettingsSO settings = null, string outputFolder = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `assemblyName` | `string` | 程序集名：支持短名（如 Runestone.AesirModules，面板下拉同款）或 FullName（如 Runestone.AesirModules, Version=0.0.0.0, ...）。 |
| `settings` | `DocGeneratorSettingsSO` | 文档生成器设置；null 使用 DefaultSettings。 |
| `outputFolder` | `string` | 输出根目录；null 使用 DefaultOutputFolder。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ScriptDocGenerationResult` | — |

</div>

### GenerateDocsForFolder(string, DocGeneratorSettingsSO, string) {#method-generatedocsforfolder-string-docgeneratorsettingsso-string}

为一个文件夹（含子文件夹）内全部脚本声明的类型生成脚本文档。 源文件经 SourceScanner 解析出类型与命名空间声明后映射到当前编译域内的类型， 支持普通 C# 类（不限 MonoBehaviour / ScriptableObject）；解析不到的类型名记入 UnresolvedTypeNames（典型成因：被条件编译剔除）。

``` csharp
public static ScriptDocGenerationResult GenerateDocsForFolder(string folderPath, DocGeneratorSettingsSO settings = null, string outputFolder = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `folderPath` | `string` | 项目内文件夹路径：Assets / Packages 下的相对路径（如 Assets/Runestone/AesirModules/Runtime/Scene） 或项目内绝对路径。项目外路径 AssetDatabase 索引不到，会被拒绝。 |
| `settings` | `DocGeneratorSettingsSO` | 文档生成器设置；null 使用 DefaultSettings。 |
| `outputFolder` | `string` | 输出根目录；null 使用 DefaultOutputFolder。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ScriptDocGenerationResult` | — |

</div>

### GenerateDocsForType(Type, DocGeneratorSettingsSO, string) {#method-generatedocsfortype-type-docgeneratorsettingsso-string}

为单个类型生成脚本文档。

``` csharp
public static ScriptDocGenerationResult GenerateDocsForType(Type type, DocGeneratorSettingsSO settings = null, string outputFolder = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | 目标类型；null 或编译器/引擎生成内部类型时记录错误并返回空结果。 |
| `settings` | `DocGeneratorSettingsSO` | 文档生成器设置；null 使用 DefaultSettings。 |
| `outputFolder` | `string` | 输出根目录（绝对路径或项目内相对路径均可）；null 使用 DefaultOutputFolder。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ScriptDocGenerationResult` | — |

</div>

### GenerateDocsForTypes(IEnumerable<Type>, DocGeneratorSettingsSO, string) {#method-generatedocsfortypes-ienumerable-type-docgeneratorsettingsso-string}

为一组类型生成脚本文档（等价面板"多类型模式"的静默版）。

``` csharp
public static ScriptDocGenerationResult GenerateDocsForTypes(IEnumerable<Type> types, DocGeneratorSettingsSO settings = null, string outputFolder = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `types` | `IEnumerable<Type>` | 目标类型集合；空集合记录错误并返回空结果。 |
| `settings` | `DocGeneratorSettingsSO` | 文档生成器设置；null 使用 DefaultSettings。 |
| `outputFolder` | `string` | 输出根目录；null 使用 DefaultOutputFolder。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ScriptDocGenerationResult` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
