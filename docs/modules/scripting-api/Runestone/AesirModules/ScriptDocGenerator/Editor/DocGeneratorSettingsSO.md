---
title: DocGeneratorSettingsSO
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.DocGeneratorSettingsSO 的 API 文档"
---

# `DocGeneratorSettingsSO`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `DocGeneratorSettingsSO`

## 声明

``` csharp
public abstract class DocGeneratorSettingsSO : UnityEngine.ScriptableObject
```

文档生成器设置抽象类

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`customizeDocFileExtensionName`](#field-customizedocfileextensionname) | 是否自定义文档扩展名 |
| [`generateIdentifier`](#field-generateidentifier) | 是否生成增量标识符 |
| [`generateNamespaceFolder`](#field-generatenamespacefolder) | 是否按命名空间生成文件夹 |
| [`docFileExtensionName`](#field-docfileextensionname) | 设置的文档扩展名 |

</div>

### customizeDocFileExtensionName {#field-customizedocfileextensionname}

是否自定义文档扩展名

``` csharp
[LabelText]
public bool customizeDocFileExtensionName;
```

### generateIdentifier {#field-generateidentifier}

是否生成增量标识符

``` csharp
[LabelText]
public bool generateIdentifier;
```

### generateNamespaceFolder {#field-generatenamespacefolder}

是否按命名空间生成文件夹

``` csharp
[LabelText]
public bool generateNamespaceFolder;
```

### docFileExtensionName {#field-docfileextensionname}

设置的文档扩展名

``` csharp
[EnableIf]
[LabelText]
public string docFileExtensionName;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetGeneratedDocumentation(ITypeData)`](#method-getgenerateddocumentation-itypedata) | 通过 TypeData 实例对象，生成文档内容。注意：不要在此方法中添加增量生成标识符 |
| [`ResetToDefault()`](#method-resettodefault) | 重置文档生成器设置 |

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

### GetGeneratedDocumentation(ITypeData) {#method-getgenerateddocumentation-itypedata}

通过 TypeData 实例对象，生成文档内容。注意：不要在此方法中添加增量生成标识符

``` csharp
public abstract string GetGeneratedDocumentation(ITypeData data)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `data` | `ITypeData` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### ResetToDefault() {#method-resettodefault}

重置文档生成器设置

``` csharp
[Button]
public void ResetToDefault()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
