---
title: AesirUpdateWindowOdin.PackageRow
description: "Runestone.AesirArchitecture.Editor.AesirUpdateWindowOdin.PackageRow 的 API 文档"
---

# `AesirUpdateWindowOdin.PackageRow`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `AesirUpdateWindowOdin.PackageRow`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateWindowOdin.PackageRow
```

单个本地安装包的行视图模型。显示文本 / 颜色 / 可更新标记在 RebuildRows 时一次性算好，绘制期只读。

**备注**

每个显示字段都要标 [EnableGUI]：列表本身是只读属性，Odin 会把它的子项也当作不可编辑， 逐个推入 GUI.enabled = false 绘制（父级 [EnableGUI] 管不到子属性各自的绘制作用域）， 于是行文本被渲染成"禁用灰"。[EnableGUI] 让各字段按可用状态绘制，不可编辑的语义不变。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateWindowOdin.PackageRow()`](#constructor-aesirupdatewindowodin-packagerow) | — |

</div>

### AesirUpdateWindowOdin.PackageRow() {#constructor-aesirupdatewindowodin-packagerow}

``` csharp
public AesirUpdateWindowOdin.PackageRow()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Model`](#field-model) | 对应的本地安装包数据。 |
| [`Owner`](#field-owner) | 所属窗口（行内按钮回调用）。非序列化：Odin 序列化不保存、域重载后由 RebuildRows 重新注入（OnEnable → Initialize → Rescan → RebuildRows 必经）。 |
| [`StatusColor`](#field-statuscolor) | 状态文本着色。 |
| [`Outdated`](#field-outdated) | 本地版本落后于远程版本。 |
| [`UpdateEnabled`](#field-updateenabled) | 行内「更新」按钮的可用状态（忙碌期间禁用；Owner 缺失时保持可用以避免行渲染异常， 点击经窗口侧的忙碌门禁兜底）。 |
| [`Local`](#field-local) | — |
| [`Name`](#field-name) | — |
| [`Remote`](#field-remote) | — |
| [`RemoteVersion`](#field-remoteversion) | 检测到的远程版本（仅待更新时有值）。 |

</div>

### Model {#field-model}

对应的本地安装包数据。

``` csharp
[HideInInspector]
public AesirUpdateService.InstalledPackage Model;
```

### Owner {#field-owner}

所属窗口（行内按钮回调用）。非序列化：Odin 序列化不保存、域重载后由 RebuildRows 重新注入（OnEnable → Initialize → Rescan → RebuildRows 必经）。

``` csharp
[NonSerialized]
public AesirUpdateWindowOdin Owner;
```

### StatusColor {#field-statuscolor}

状态文本着色。

``` csharp
[HideInInspector]
public Color StatusColor;
```

### Outdated {#field-outdated}

本地版本落后于远程版本。

``` csharp
[HideInInspector]
public bool Outdated;
```

### UpdateEnabled {#field-updateenabled}

行内「更新」按钮的可用状态（忙碌期间禁用；Owner 缺失时保持可用以避免行渲染异常， 点击经窗口侧的忙碌门禁兜底）。

``` csharp
[HideInInspector]
public bool UpdateEnabled;
```

### Local {#field-local}

``` csharp
[HorizontalGroup]
[EnableGUI]
[DisplayAsString]
[HideLabel]
public string Local;
```

### Name {#field-name}

``` csharp
[HorizontalGroup]
[EnableGUI]
[DisplayAsString]
[HideLabel]
public string Name;
```

### Remote {#field-remote}

``` csharp
[HorizontalGroup]
[EnableGUI]
[DisplayAsString]
[HideLabel]
[GUIColor]
public string Remote;
```

### RemoteVersion {#field-remoteversion}

检测到的远程版本（仅待更新时有值）。

``` csharp
[HideInInspector]
public string RemoteVersion;
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
