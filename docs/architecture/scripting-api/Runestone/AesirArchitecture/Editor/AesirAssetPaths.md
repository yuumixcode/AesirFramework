---
title: AesirAssetPaths
description: "Runestone.AesirArchitecture.Editor.AesirAssetPaths 的 API 文档"
---

# `AesirAssetPaths`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirAssetPaths`

## 声明

``` csharp
internal static class AesirAssetPaths
```

Aesir 本地安装路径定位器 — 解析 Assets 形态安装（复制 / unitypackage 导入）的 Runestone 安装根目录，确保 Runestone 可移动到项目的任意文件夹。
机制参照 Odin Inspector 的 SirenixAssetPaths（OdinPathLookup.asset 锚点）：每包包根放一个 AesirPathLookup 锚点资产，文件夹移动时 .meta GUID 保持不变，依次按 「默认安装根（磁盘存在性）→ 锚点 GUID 查询 → 锚点类型搜索」三级定位，逐级变慢且逐级兜底： 第二级**始终执行**（多安装根是常态，需要逐个锚点收集），仅第三级在第二级无果时才跑。

结果在静态构造期解析一次，域重载后自动重解析；移动含脚本的 Runestone 目录必然触发域重载， 各消费端（构建剔除 / 更新器 / Getting Started）下一次访问即拿到新位置。

边界：仅覆盖 Assets 形态安装——UPM（Git URL / 嵌入式）安装的包路径由 Package Manager 固定、 不可移动、无需锚点；Package Manager 导入的示例位于 Assets/Samples/（Unity 固定路径）， 由消费端直接按前缀匹配。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ArchitectureLookupAssetGuid`](#field-architecturelookupassetguid) | AesirArchitecture 包锚点资产的 .meta GUID（文件夹移动后 GUID 不变，定位靠它）。 |
| [`DefaultInstallRoot`](#field-defaultinstallroot) | 默认安装根（项目相对路径）——快路径首查位置。 |
| [`LookupAssetFileName`](#field-lookupassetfilename) | 锚点资产文件名（每包包根一份）。 |
| [`ModulesLookupAssetGuid`](#field-moduleslookupassetguid) | AesirModules 包锚点资产的 .meta GUID。 |

</div>

### ArchitectureLookupAssetGuid {#field-architecturelookupassetguid}

AesirArchitecture 包锚点资产的 .meta GUID（文件夹移动后 GUID 不变，定位靠它）。

``` csharp
public const string ArchitectureLookupAssetGuid = "d2965568e68354d3ca31d6085ac86865";
```

### DefaultInstallRoot {#field-defaultinstallroot}

默认安装根（项目相对路径）——快路径首查位置。

``` csharp
public const string DefaultInstallRoot = "Assets/Runestone";
```

### LookupAssetFileName {#field-lookupassetfilename}

锚点资产文件名（每包包根一份）。

``` csharp
public const string LookupAssetFileName = "AesirPathLookup.asset";
```

### ModulesLookupAssetGuid {#field-moduleslookupassetguid}

AesirModules 包锚点资产的 .meta GUID。

``` csharp
public const string ModulesLookupAssetGuid = "465d45b44c6cf426882248dceeea94a0";
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`InstallRoots`](#property-installroots) | 本地安装根列表（项目相对路径，按解析顺序去重；默认根优先）。 |
| [`PrimaryInstallRoot`](#property-primaryinstallroot) | 主安装根（列表首个；提示文案用）。无任何本地安装时回退默认根。 |

</div>

### InstallRoots {#property-installroots}

本地安装根列表（项目相对路径，按解析顺序去重；默认根优先）。

``` csharp
public static IReadOnlyList<string> InstallRoots { get; }
```

### PrimaryInstallRoot {#property-primaryinstallroot}

主安装根（列表首个；提示文案用）。无任何本地安装时回退默认根。

``` csharp
public static string PrimaryInstallRoot { get; } = "Assets/Runestone";
```

## 方法

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

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
