---
title: ScriptDocGeneratorAssetMarkerSO
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorAssetMarkerSO 的 API 文档"
---

# `ScriptDocGeneratorAssetMarkerSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `ScriptDocGeneratorAssetMarkerSO`

## 声明

``` csharp
public class ScriptDocGeneratorAssetMarkerSO : UnityEngine.ScriptableObject
```

Script Doc Generator 模块资产初始化完成标识。首次初始化后创建标识资产， 后续打开工具时通过检查标识是否存在来判断是否已完成初始化。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ScriptDocGeneratorAssetMarkerSO()`](#constructor-scriptdocgeneratorassetmarkerso) | — |

</div>

### ScriptDocGeneratorAssetMarkerSO() {#constructor-scriptdocgeneratorassetmarkerso}

``` csharp
public ScriptDocGeneratorAssetMarkerSO()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Description`](#property-description) | — |
| [`ToolName`](#property-toolname) | 标识资产写入的工具名（Inspector 只读展示，避免字段赋值无消费告警） |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

### Description {#property-description}

``` csharp
[DisplayAsString]
public string Description { get; }
```

### ToolName {#property-toolname}

标识资产写入的工具名（Inspector 只读展示，避免字段赋值无消费告警）

``` csharp
public string ToolName { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IsAssetsInitialized()`](#method-isassetsinitialized) | 检查 Script Doc Generator 模块的标识资产是否已初始化。 用于判断模块相关资源是否已创建并准备就绪。 |
| [`CreateMarkerAsset()`](#method-createmarkerasset) | 创建标识资产。应在工具的所有资产初始化完成后调用。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

### IsAssetsInitialized() {#method-isassetsinitialized}

检查 Script Doc Generator 模块的标识资产是否已初始化。 用于判断模块相关资源是否已创建并准备就绪。

``` csharp
public static bool IsAssetsInitialized()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 如果标识资产已存在，则返回 true；否则返回 false。 |

</div>

### CreateMarkerAsset() {#method-createmarkerasset}

创建标识资产。应在工具的所有资产初始化完成后调用。

``` csharp
public static void CreateMarkerAsset()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
