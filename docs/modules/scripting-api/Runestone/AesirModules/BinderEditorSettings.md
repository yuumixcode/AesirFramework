---
title: BinderEditorSettings
description: "Runestone.AesirModules.BinderEditorSettings 的 API 文档"
---

# `BinderEditorSettings`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `UnityEditor.ScriptableSingleton<BinderEditorSettings>` → `BinderEditorSettings`

## 声明

``` csharp
[FilePath]
public class BinderEditorSettings : UnityEditor.ScriptableSingleton<BinderEditorSettings>
```

Binder 编辑器持久化设置（ScriptableSingleton）：保存 partial 分部类模式的可选文件后缀列表、 默认后缀与最近使用的命名空间，供新建 BinderAssistant 的默认值与后缀下拉共用。

**备注**

落盘路径为项目根 ScriptableSingleton/AesirModules/BinderEditorSettings.asset（已被 .gitignore 覆盖）。 ScriptableSingleton 走 Unity 原生序列化，故除 FilePathAttribute 外， 每个持久字段还需 [SerializeField]，且写盘必须调用基类 Save(true)—— 仅 EditorUtility.SetDirty / AssetDatabase.SaveAssets 对本单例无效（它不是资产）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`BinderEditorSettings()`](#constructor-bindereditorsettings) | — |

</div>

### BinderEditorSettings() {#constructor-bindereditorsettings}

``` csharp
public BinderEditorSettings()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`PartialSuffixes`](#property-partialsuffixes) | partial 分部类模式下自动维护文件的可选后缀列表（含扩展名，如 .designer.cs）。 |
| [`DefaultPartialSuffix`](#property-defaultpartialsuffix) | 新建 BinderAssistant 时默认选中的自动维护文件后缀。 |
| [`LastNamespace`](#property-lastnamespace) | 最近一次成功生成脚本时使用的命名空间，作为新建 BinderAssistant 的命名空间默认值。 |
| [`Settings`](#property-settings) | 实例访问器。Unity 的 ScriptableSingleton<T> 暴露小写 instance 属性 （双引擎一致），此处仍按大写/小写双探测并缓存，以免依赖单一拼写。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

### PartialSuffixes {#property-partialsuffixes}

partial 分部类模式下自动维护文件的可选后缀列表（含扩展名，如 .designer.cs）。

``` csharp
public List<string> PartialSuffixes { get; }
```

### DefaultPartialSuffix {#property-defaultpartialsuffix}

新建 BinderAssistant 时默认选中的自动维护文件后缀。

``` csharp
public string DefaultPartialSuffix { get; }
```

### LastNamespace {#property-lastnamespace}

最近一次成功生成脚本时使用的命名空间，作为新建 BinderAssistant 的命名空间默认值。

``` csharp
public string LastNamespace { get; }
```

### Settings {#property-settings}

实例访问器。Unity 的 ScriptableSingleton<T> 暴露小写 instance 属性 （双引擎一致），此处仍按大写/小写双探测并缓存，以免依赖单一拼写。

``` csharp
public static BinderEditorSettings Settings { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Save()`](#method-save) | 立即落盘（后缀列表编辑为低频操作，直接写入设置文件保证持久化）。 |
| [`SetDefaultPartialSuffix(string)`](#method-setdefaultpartialsuffix-string) | 更新默认选中的自动维护文件后缀。 |
| [`SetLastNamespace(string)`](#method-setlastnamespace-string) | 更新最近使用的命名空间（由生成流程调用，随即写盘）。 |
| [`SetPartialSuffixes(List<string>)`](#method-setpartialsuffixes-list-string) | 用编辑器内编辑过的列表替换可选后缀列表并落盘。 |

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
| `Save(bool)` | — | `ScriptableSingleton<BinderEditorSettings>` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

### Save() {#method-save}

立即落盘（后缀列表编辑为低频操作，直接写入设置文件保证持久化）。

``` csharp
public void Save()
```

### SetDefaultPartialSuffix(string) {#method-setdefaultpartialsuffix-string}

更新默认选中的自动维护文件后缀。

``` csharp
public void SetDefaultPartialSuffix(string suffix)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `suffix` | `string` | — |

</div>

### SetLastNamespace(string) {#method-setlastnamespace-string}

更新最近使用的命名空间（由生成流程调用，随即写盘）。

``` csharp
public void SetLastNamespace(string targetNamespace)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `targetNamespace` | `string` | — |

</div>

### SetPartialSuffixes(List<string>) {#method-setpartialsuffixes-list-string}

用编辑器内编辑过的列表替换可选后缀列表并落盘。

``` csharp
public void SetPartialSuffixes(List<string> suffixes)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `suffixes` | `List<string>` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
