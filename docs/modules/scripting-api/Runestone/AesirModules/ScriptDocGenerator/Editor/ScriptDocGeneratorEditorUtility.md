---
title: ScriptDocGeneratorEditorUtility
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorEditorUtility 的 API 文档"
---

# `ScriptDocGeneratorEditorUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `ScriptDocGeneratorEditorUtility`

## 声明

``` csharp
public static class ScriptDocGeneratorEditorUtility
```

Script Doc Generator 的编辑器工具方法，仅供编辑器程序集内部使用。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetOrCreateEditorScriptableObject(string, string, string)`](#method-getorcreateeditorscriptableobject-string-string-string) | 根据配置名称获取或创建编辑器 ScriptableObject 资源。 如果资源不存在则自动创建并保存到指定路径，同时将资源注册到 EditorBuildSettings 中。 |
| [`EnsureDirectoryExists(string)`](#method-ensuredirectoryexists-string) | 确保 Assets 目录下的相对路径的文件夹存在，如果不存在则递归创建。 |
| [`PingAndSelectAsset(string)`](#method-pingandselectasset-string) | Ping 项目中的任何资源，可以是文件夹路径。传入相对路径。 |

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

### GetOrCreateEditorScriptableObject(string, string, string) {#method-getorcreateeditorscriptableobject-string-string-string}

根据配置名称获取或创建编辑器 ScriptableObject 资源。 如果资源不存在则自动创建并保存到指定路径，同时将资源注册到 EditorBuildSettings 中。

``` csharp
public static T GetOrCreateEditorScriptableObject<T>(string configName, string folderPath, string assetName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `configName` | `string` | — |
| `folderPath` | `string` | — |
| `assetName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | — |

</div>

### EnsureDirectoryExists(string) {#method-ensuredirectoryexists-string}

确保 Assets 目录下的相对路径的文件夹存在，如果不存在则递归创建。

``` csharp
public static void EnsureDirectoryExists(string relativePath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `relativePath` | `string` | — |

</div>

### PingAndSelectAsset(string) {#method-pingandselectasset-string}

Ping 项目中的任何资源，可以是文件夹路径。传入相对路径。

``` csharp
public static void PingAndSelectAsset(string relativePath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `relativePath` | `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
