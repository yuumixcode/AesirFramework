---
title: ScriptDocGeneratorMenuPaths
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorMenuPaths 的 API 文档"
---

# `ScriptDocGeneratorMenuPaths`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `ScriptDocGeneratorMenuPaths`

## 声明

``` csharp
public static class ScriptDocGeneratorMenuPaths
```

Script Doc Generator 所有 MenuItem 菜单路径和优先级的统一管理。 Unity 中 MenuItem 的顺序由 priority 参数（一个整数）决定，核心规则是：数字越小，位置越靠上。若不设置，默认值为 1000。 父菜单的 priority 由其子菜单项 priority 的最小值决定。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ProcessSummaryRemoveOrder`](#field-processsummaryremoveorder) | Process Summary Remove 菜单项优先级。 |
| [`ProcessSummaryReplaceOrder`](#field-processsummaryreplaceorder) | Process Summary Replace 菜单项优先级。 |
| [`ProcessSummarySyncOrder`](#field-processsummarysyncorder) | Process Summary Sync 菜单项优先级。 Script Doc Generator 末尾 124，+11 产生分割线。 |
| [`ScriptDocGeneratorOrder`](#field-scriptdocgeneratororder) | Script Doc Generator 菜单项优先级。 决定 Modules 组的组级排序（父菜单 priority 由子项最小值决定）： 999 位于 Architecture 组（995）之后（Check for Updates 已置底 1100）； 与相邻组差值 ≤ 10 不产生分割线，同属工具组且为组内第一项。 |
| [`AssetsProcessSummaryRoot`](#field-assetsprocesssummaryroot) | Assets 上下文菜单中 Process Summary 的根路径。 |
| [`AssetsScriptDocGeneratorRoot`](#field-assetsscriptdocgeneratorroot) | Assets 上下文菜单中 Script Doc Generator 的根路径。 |
| [`ProcessSummaryRemove`](#field-processsummaryremove) | 移除所有 SummaryAttribute 的菜单路径。 |
| [`ProcessSummaryReplace`](#field-processsummaryreplace) | 用 SummaryAttribute 替换 XML Summary 注释的菜单路径。 |
| [`ProcessSummarySync`](#field-processsummarysync) | 同步 XML Summary 注释到 SummaryAttribute 的菜单路径。 |
| [`ScriptDocGenerator`](#field-scriptdocgenerator) | 打开 Script Doc Generator 窗口的菜单路径（Aesir Modules 包专属工具，归入 Modules 组）。 |

</div>

### ProcessSummaryRemoveOrder {#field-processsummaryremoveorder}

Process Summary Remove 菜单项优先级。

``` csharp
public const int ProcessSummaryRemoveOrder = -23;
```

### ProcessSummaryReplaceOrder {#field-processsummaryreplaceorder}

Process Summary Replace 菜单项优先级。

``` csharp
public const int ProcessSummaryReplaceOrder = -25;
```

### ProcessSummarySyncOrder {#field-processsummarysyncorder}

Process Summary Sync 菜单项优先级。 Script Doc Generator 末尾 124，+11 产生分割线。

``` csharp
public const int ProcessSummarySyncOrder = -28;
```

### ScriptDocGeneratorOrder {#field-scriptdocgeneratororder}

Script Doc Generator 菜单项优先级。 决定 Modules 组的组级排序（父菜单 priority 由子项最小值决定）： 999 位于 Architecture 组（995）之后（Check for Updates 已置底 1100）； 与相邻组差值 ≤ 10 不产生分割线，同属工具组且为组内第一项。

``` csharp
public const int ScriptDocGeneratorOrder = 999;
```

### AssetsProcessSummaryRoot {#field-assetsprocesssummaryroot}

Assets 上下文菜单中 Process Summary 的根路径。

``` csharp
public const string AssetsProcessSummaryRoot = "Assets/Script Doc Generator/Process Summary";
```

### AssetsScriptDocGeneratorRoot {#field-assetsscriptdocgeneratorroot}

Assets 上下文菜单中 Script Doc Generator 的根路径。

``` csharp
public const string AssetsScriptDocGeneratorRoot = "Assets/Script Doc Generator";
```

### ProcessSummaryRemove {#field-processsummaryremove}

移除所有 SummaryAttribute 的菜单路径。

``` csharp
public const string ProcessSummaryRemove = "Assets/Script Doc Generator/Process Summary/Remove";
```

### ProcessSummaryReplace {#field-processsummaryreplace}

用 SummaryAttribute 替换 XML Summary 注释的菜单路径。

``` csharp
public const string ProcessSummaryReplace = "Assets/Script Doc Generator/Process Summary/Replace";
```

### ProcessSummarySync {#field-processsummarysync}

同步 XML Summary 注释到 SummaryAttribute 的菜单路径。

``` csharp
public const string ProcessSummarySync = "Assets/Script Doc Generator/Process Summary/Sync";
```

### ScriptDocGenerator {#field-scriptdocgenerator}

打开 Script Doc Generator 窗口的菜单路径（Aesir Modules 包专属工具，归入 Modules 组）。

``` csharp
public const string ScriptDocGenerator = "Tools/Aesir/Modules/Script Doc Generator";
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
