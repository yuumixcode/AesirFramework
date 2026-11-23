---
title: DefaultScriptingAPISettingsSO
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.DefaultScriptingAPISettingsSO 的 API 文档"
---

# `DefaultScriptingAPISettingsSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `Runestone.AesirModules.ScriptDocGenerator.Editor.DocGeneratorSettingsSO` → `DefaultScriptingAPISettingsSO`

## 声明

``` csharp
public class DefaultScriptingAPISettingsSO : Runestone.AesirModules.ScriptDocGenerator.Editor.DocGeneratorSettingsSO
```

默认中文 API 文档生成设置

**备注**

纯 Markdown 输出（无 Front Matter / 无 div 包裹 / 无锚点详情）。 成员章节的分组核心（API 过滤 → 常量/声明/继承/运算符分组 → 固定顺序）与 Zensical 生成器共享 MemberGrouper，本类只声明各章节的表头、 列形与名称选择器（每节约 10 行配置），不再手写三旗标探测循环。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultScriptingAPISettingsSO()`](#constructor-defaultscriptingapisettingsso) | — |

</div>

### DefaultScriptingAPISettingsSO() {#constructor-defaultscriptingapisettingsso}

``` csharp
public DefaultScriptingAPISettingsSO()
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
| [`Instance`](#property-instance) | 默认生成设置单例访问。解析结果按域缓存：缺失资产的解析会执行 CreateAsset 与 AssetDatabase.Refresh，每次都重新解析会把这类重操作带进高频路径。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

### Instance {#property-instance}

默认生成设置单例访问。解析结果按域缓存：缺失资产的解析会执行 CreateAsset 与 AssetDatabase.Refresh，每次都重新解析会把这类重操作带进高频路径。

``` csharp
public static DefaultScriptingAPISettingsSO Instance { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `GetGeneratedDocumentation(ITypeData)` | — | `DefaultScriptingAPISettingsSO` |
| `ToString()` | — | `Object` |
| `ResetToDefault()` | 重置文档生成器设置 | `DocGeneratorSettingsSO` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
