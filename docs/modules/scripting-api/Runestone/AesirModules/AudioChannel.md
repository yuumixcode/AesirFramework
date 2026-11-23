---
title: AudioChannel
description: "Runestone.AesirModules.AudioChannel 的 API 文档"
---

# `AudioChannel`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `AudioChannel`

## 声明

``` csharp
[IsReadOnly]
public struct AudioChannel : System.ValueType
```

音频通道描述符（不可变值对象）。音量/静音的存取、PlayerPrefs 持久化与配置默认值载入 均以通道列表（Channels）为唯一数据源遍历。

**备注**

意图：把"通道"从散落在字段声明、键名属性、静态属性、配置载入与 ApplyVolumes / ApplyMutes 分支中的硬编码，收敛为一次性声明的数据。此前新增第 4 个通道需要改动约 20 处语句， 且容易漏改持久化键；现在新增通道只需在 Channels 增加一项 （含 Index，须与列表位置一致）并在 GetChannelVolumes 追加对应默认值。
不含音源持有信息：音源拓扑（BGM 专用源 / SFX 独占源组）与通道列表不同构， 强行并入会让值对象反过来承担音源引用职责，故此处只描述"可被统一遍历的通道维度"。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AudioChannel(string, string, string, int)`](#constructor-audiochannel-string-string-string-int) | 构造通道描述符。 |

</div>

### AudioChannel(string, string, string, int) {#constructor-audiochannel-string-string-string-int}

构造通道描述符。

``` csharp
public AudioChannel(string id, string volumeKeySuffix, string muteKeySuffix, int index)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `id` | `string` | 通道标识（日志与调试用）。 |
| `volumeKeySuffix` | `string` | PlayerPrefs 音量键后缀（不含前缀与分隔符）。 |
| `muteKeySuffix` | `string` | PlayerPrefs 静音键后缀（不含前缀与分隔符）。 |
| `index` | `int` | 在通道列表中的下标。 |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Index`](#property-index) | 本通道在音量/静音数组与通道列表中的下标，须与列表位置一致。 |
| [`Id`](#property-id) | 通道标识（日志与调试用）。 |
| [`MuteKeySuffix`](#property-mutekeysuffix) | PlayerPrefs 静音键后缀，实际键为 前缀 + "." + 该后缀。 |
| [`VolumeKeySuffix`](#property-volumekeysuffix) | PlayerPrefs 音量键后缀，实际键为 前缀 + "." + 该后缀。 |

</div>

### Index {#property-index}

本通道在音量/静音数组与通道列表中的下标，须与列表位置一致。

``` csharp
public int Index { get; }
```

### Id {#property-id}

通道标识（日志与调试用）。

``` csharp
public string Id { get; }
```

### MuteKeySuffix {#property-mutekeysuffix}

PlayerPrefs 静音键后缀，实际键为 前缀 + "." + 该后缀。

``` csharp
public string MuteKeySuffix { get; }
```

### VolumeKeySuffix {#property-volumekeysuffix}

PlayerPrefs 音量键后缀，实际键为 前缀 + "." + 该后缀。

``` csharp
public string VolumeKeySuffix { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `ValueType` |
| `GetHashCode()` | — | `ValueType` |
| `ToString()` | — | `ValueType` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
