---
title: ZensicalScriptingAPISettingsSO
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ZensicalScriptingAPISettingsSO 的 API 文档"
---

# `ZensicalScriptingAPISettingsSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `Runestone.AesirModules.ScriptDocGenerator.Editor.DocGeneratorSettingsSO` → `ZensicalScriptingAPISettingsSO`

## 声明

``` csharp
public class ZensicalScriptingAPISettingsSO : Runestone.AesirModules.ScriptDocGenerator.Editor.DocGeneratorSettingsSO
```

Zensical 静态站点专用的 API 文档生成设置

**备注**

输出遵循 Zensical (Python-Markdown) 约定的 Markdown： YAML Front Matter、无标题 note 警示框元信息、md_in_html 表格样式包裹（.api-summary-table 等）、 显式标题锚点（{#api-xxx}）与 csharp 声明块， 配合站点侧 docs/stylesheets/api.css 呈现 DocFX 风格的 Scripting API 页面。 样式锚点约定：.api-summary-table / .api-params-table / .api-returns-table。 表格一律经 <div class="..." markdown="1"> 包裹注入 class —— attr_list 块级标记 {: .cls } 对表格无效（Zensical 实测：class 不应用且标记原文渲染），仅标题级 {#anchor} 行内标记可用。 成员详情统一"先注释后声明"：读者先看到用途说明，再看签名。 参数与返回值说明来自分析期的结构化数据（IParameterData 与 XML 文档注释解析链）， 不从格式化字符串反解析。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ZensicalScriptingAPISettingsSO()`](#constructor-zensicalscriptingapisettingsso) | — |

</div>

### ZensicalScriptingAPISettingsSO() {#constructor-zensicalscriptingapisettingsso}

``` csharp
public ZensicalScriptingAPISettingsSO()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `customizeDocFileExtensionName` | 是否自定义文档扩展名 | `DocGeneratorSettingsSO` |
| `generateIdentifier` | 是否生成增量标识符 | `DocGeneratorSettingsSO` |
| `generateNamespaceFolder` | 是否按命名空间生成文件夹 | `DocGeneratorSettingsSO` |
| `docFileExtensionName` | 设置的文档扩展名 | `DocGeneratorSettingsSO` |

</div>

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Instance`](#property-instance) | Zensical 文档生成设置单例。解析结果按域缓存：缺失资产的解析会执行 CreateAsset 与 AssetDatabase.Refresh，每次都重新解析会把这类重操作带进高频路径。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

### Instance {#property-instance}

Zensical 文档生成设置单例。解析结果按域缓存：缺失资产的解析会执行 CreateAsset 与 AssetDatabase.Refresh，每次都重新解析会把这类重操作带进高频路径。

``` csharp
public static ZensicalScriptingAPISettingsSO Instance { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `GetGeneratedDocumentation(ITypeData)` | — | `ZensicalScriptingAPISettingsSO` |
| `ToString()` | — | `Object` |
| `ResetToDefault()` | 重置文档生成器设置 | `DocGeneratorSettingsSO` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
