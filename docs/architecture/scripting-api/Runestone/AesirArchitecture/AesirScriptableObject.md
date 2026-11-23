---
title: AesirScriptableObject
description: "Runestone.AesirArchitecture.AesirScriptableObject 的 API 文档"
---

# `AesirScriptableObject`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `Sirenix.OdinInspector.SerializedScriptableObject` → `AesirScriptableObject`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
public abstract class AesirScriptableObject : Sirenix.OdinInspector.SerializedScriptableObject, 
UnityEngine.ISerializationCallbackReceiver
```

RAA 架构标准 ScriptableObject 基类，根据运行环境自动选择序列化方式。

**备注**

通过条件编译在编译期决定基类，与 AesirMonoBehaviour 采用相同的策略： 编辑器 + 定义了 ODIN_INSPECTOR：继承 SerializedScriptableObject，获得 Odin 序列化能力。 运行时 + 定义了 ODIN_INSPECTOR 且未定义 ODIN_INSPECTOR_EDITOR_ONLY：继承 SerializedScriptableObject。 其他情况：继承 ScriptableObject，使用 Unity 默认序列化。
ODIN_INSPECTOR_EDITOR_ONLY 宏用于在运行时剔除 Odin 序列化（减小包体）， 同时保留编辑器内的 Odin Inspector 体验。 警告：定义该宏后，编辑器分支经 Odin 序列化后端持久化、构建分支回退为裸 Unity 序列化后端—— 编辑器内经 Odin 录入而 Unity 无法原生序列化的数据（字典、私有字段等）在玩家构建中会静默丢失。 使用该宏前请确认序列化字段均为 Unity 原生可序列化形态。

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
