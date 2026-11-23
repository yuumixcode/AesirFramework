---
title: ScriptDocGeneratorPanelSO
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorPanelSO 的 API 文档"
---

# `ScriptDocGeneratorPanelSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `Sirenix.OdinInspector.SerializedScriptableObject` → `ScriptDocGeneratorPanelSO`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
public class ScriptDocGeneratorPanelSO : Sirenix.OdinInspector.SerializedScriptableObject, 
UnityEngine.ISerializationCallbackReceiver
```

ScriptDocGenerator 可视化操作面板类

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ScriptDocGeneratorPanelSO()`](#constructor-scriptdocgeneratorpanelso) | — |

</div>

### ScriptDocGeneratorPanelSO() {#constructor-scriptdocgeneratorpanelso}

``` csharp
public ScriptDocGeneratorPanelSO()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`TemporaryTypes`](#property-temporarytypes) | — |
| [`TypeSourceProperty`](#property-typesourceproperty) | — |
| [`TargetType`](#property-targettype) | — |
| [`Instance`](#property-instance) | 面板单例访问。解析结果按域缓存：缺失资产的解析会执行 CreateAsset 与 AssetDatabase.Refresh， 每次都重新解析会把这类重操作带进绘制回调等高频路径。 |
| [`DefaultDocFolderPath`](#property-defaultdocfolderpath) | — |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

### TemporaryTypes {#property-temporarytypes}

``` csharp
public List<Type> TemporaryTypes { get; set; }
```

### TypeSourceProperty {#property-typesourceproperty}

``` csharp
public ScriptDocGeneratorPanelSO.TypeSource TypeSourceProperty { get; set; }
```

### TargetType {#property-targettype}

``` csharp
public Type TargetType { get; set; }
```

### Instance {#property-instance}

面板单例访问。解析结果按域缓存：缺失资产的解析会执行 CreateAsset 与 AssetDatabase.Refresh， 每次都重新解析会把这类重操作带进绘制回调等高频路径。

``` csharp
public static ScriptDocGeneratorPanelSO Instance { get; }
```

### DefaultDocFolderPath {#property-defaultdocfolderpath}

``` csharp
public static string DefaultDocFolderPath { get; } = "/Users/yuumix/Projects/Unity/AesirFramework/ScriptDocGenerator";
```

## 事件

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ToastRequested`](#event-toastrequested) | — |

</div>

### ToastRequested {#event-toastrequested}

``` csharp
public static event Action<ToastPosition, SdfIconType, string, Color, float> ToastRequested;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AnalyzeType()`](#method-analyzetype) | — |
| [`GenerateDoc()`](#method-generatedoc) | — |
| [`ResetToDefault()`](#method-resettodefault) | — |

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
| `OnAfterDeserialize()` | — | `SerializedScriptableObject` |
| `OnBeforeSerialize()` | — | `SerializedScriptableObject` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

### AnalyzeType() {#method-analyzetype}

``` csharp
[PropertyOrder]
[Title]
[Button]
public void AnalyzeType()
```

### GenerateDoc() {#method-generatedoc}

``` csharp
[PropertyOrder]
[ShowIf]
[Title]
[Button]
public void GenerateDoc()
```

### ResetToDefault() {#method-resettodefault}

``` csharp
[PropertyOrder]
[Title]
[InfoBox]
[Button]
public void ResetToDefault()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
