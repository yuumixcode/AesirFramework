---
title: AudioConfigSO
description: "Runestone.AesirModules.AudioConfigSO 的 API 文档"
---

# `AudioConfigSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `Sirenix.OdinInspector.SerializedScriptableObject` → `Runestone.AesirArchitecture.AesirScriptableObject` → `AudioConfigSO`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[CreateAssetMenu]
public class AudioConfigSO : Runestone.AesirArchitecture.AesirScriptableObject, 
UnityEngine.ISerializationCallbackReceiver
```

音频模块配置资产。创建路径：Assets → Create → Aesir Modules → Audio → AudioConfig。
配置默认音量、音量持久化开关与 PlayerPrefs 键前缀；留空键前缀时回退 AesirAudio。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AudioConfigSO()`](#constructor-audioconfigso) | — |

</div>

### AudioConfigSO() {#constructor-audioconfigso}

``` csharp
public AudioConfigSO()
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
