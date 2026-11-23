---
title: TypesCacheSO
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.TypesCacheSO 的 API 文档"
---

# `TypesCacheSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `Sirenix.OdinInspector.SerializedScriptableObject` → `TypesCacheSO`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
public class TypesCacheSO : Sirenix.OdinInspector.SerializedScriptableObject, 
UnityEngine.ISerializationCallbackReceiver
```

存储 Type 的资源文件，提供给脚本文档生成工具复用，用户无需每次重新选择 Type

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`TypesCacheSO()`](#constructor-typescacheso) | — |

</div>

### TypesCacheSO() {#constructor-typescacheso}

``` csharp
public TypesCacheSO()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Types`](#field-types) | 存储 Type 的列表 |

</div>

### Types {#field-types}

存储 Type 的列表

``` csharp
public List<Type> Types;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

## 方法

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

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
