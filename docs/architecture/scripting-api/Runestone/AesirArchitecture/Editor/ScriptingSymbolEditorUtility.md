---
title: ScriptingSymbolEditorUtility
description: "Runestone.AesirArchitecture.Editor.ScriptingSymbolEditorUtility 的 API 文档"
---

# `ScriptingSymbolEditorUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `ScriptingSymbolEditorUtility`

## 声明

``` csharp
public static class ScriptingSymbolEditorUtility
```

脚本宏定义工具，用于管理 PlayerSettings 中的 Scripting Define Symbols。
遍历所有构建目标（排除 Unknown 和 Dedicated Server），提供幂等的宏定义符号添加/移除能力。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`HasScriptingDefineSymbol(string)`](#method-hasscriptingdefinesymbol-string) | 检查指定的宏定义符号是否已存在于当前构建目标中。 |
| [`EnsureScriptingDefineSymbol(string)`](#method-ensurescriptingdefinesymbol-string) | 确保指定的宏定义符号存在于所有有效构建目标中（排除 Unknown 和 Dedicated Server）。若已存在则不重复添加。 |
| [`RemoveScriptingDefineSymbol(string)`](#method-removescriptingdefinesymbol-string) | 确保指定的宏定义符号不存在于所有有效构建目标中。若不存在则不做任何操作。 |

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

### HasScriptingDefineSymbol(string) {#method-hasscriptingdefinesymbol-string}

检查指定的宏定义符号是否已存在于当前构建目标中。

**备注**

仅检查当前在 Unity Editor 中选中的构建目标组（selectedBuildTargetGroup）， 不遍历所有构建目标。如需检查全部目标，请遍历调用各目标的查询方法。

``` csharp
public static bool HasScriptingDefineSymbol(string symbol)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `symbol` | `string` | 要检查的宏定义符号 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 若符号存在于当前选中的构建目标中则返回 true，否则返回 false |

</div>

### EnsureScriptingDefineSymbol(string) {#method-ensurescriptingdefinesymbol-string}

确保指定的宏定义符号存在于所有有效构建目标中（排除 Unknown 和 Dedicated Server）。若已存在则不重复添加。

**备注**

此方法是幂等的：若符号在某个构建目标中已存在，则跳过该目标不会重复添加， 避免产生重复的分号分隔条目。

``` csharp
public static void EnsureScriptingDefineSymbol(string symbol)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `symbol` | `string` | 要添加的宏定义符号（如 "AESIR_ARCHITECTURE"） |

</div>

### RemoveScriptingDefineSymbol(string) {#method-removescriptingdefinesymbol-string}

确保指定的宏定义符号不存在于所有有效构建目标中。若不存在则不做任何操作。

**备注**

此方法是幂等的：若符号在某个构建目标中不存在，则跳过该目标不会产生错误， 仅在实际移除了符号时才记录日志。

``` csharp
public static void RemoveScriptingDefineSymbol(string symbol)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `symbol` | `string` | 要移除的宏定义符号 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
