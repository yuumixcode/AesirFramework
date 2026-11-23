---
title: AesirPathLookup
description: "Runestone.AesirArchitecture.Editor.AesirPathLookup 的 API 文档"
---

# `AesirPathLookup`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `AesirPathLookup`

## 声明

``` csharp
public class AesirPathLookup : UnityEngine.ScriptableObject
```

Aesir 安装位置锚点资产 — 空壳标记资产，每包包根各放一份（AesirPathLookup.asset）。
机制参照 Odin Inspector 的 SirenixPathLookupScriptableObject（OdinPathLookup.asset）： 资产 .meta 里的 GUID 在文件夹移动后保持不变，路径定位器 AesirAssetPaths 经 GUID 查询拿到资产实际路径、再反推包的安装位置——由此 Assets 形态安装的 Runestone 目录可自由移动到项目任意文件夹，包内更新器、Getting Started 窗口与示例场景的构建剔除照常工作。

勿删除本资产；其 Inspector 由 AesirPathLookupAssetEditor 绘制说明。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirPathLookup()`](#constructor-aesirpathlookup) | — |

</div>

### AesirPathLookup() {#constructor-aesirpathlookup}

``` csharp
public AesirPathLookup()
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
| `SetDirty()` | — | `ScriptableObject` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
