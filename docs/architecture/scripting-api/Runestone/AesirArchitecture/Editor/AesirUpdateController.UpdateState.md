---
title: AesirUpdateController.UpdateState
description: "Runestone.AesirArchitecture.Editor.AesirUpdateController.UpdateState 的 API 文档"
---

# `AesirUpdateController.UpdateState`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateController.UpdateState`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateController.UpdateState
```

更新器状态。窗口以 [SerializeField] 持有以跨域重载保留远程检测结果与更新日志； AesirUpdateController 是唯一写入者，窗口层只读。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateController.UpdateState()`](#constructor-aesirupdatecontroller-updatestate) | — |

</div>

### AesirUpdateController.UpdateState() {#constructor-aesirupdatecontroller-updatestate}

``` csharp
public AesirUpdateController.UpdateState()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`RemoteRouteKind`](#field-remoteroutekind) | 检测来源所属线路类别（直连 GitHub / 镜像站 / CDN 中转）。 |
| [`Snapshot`](#field-snapshot) | 远程 Release 快照（检测成功后有值）。 |
| [`Packages`](#field-packages) | 本地扫描到的安装包（Rescan 时重建）。 |
| [`Busy`](#field-busy) | 忙碌标志（域重载后重置，见类注释）。 |
| [`GitHubDirectAvailable`](#field-githubdirectavailable) | 本次检测中直连 GitHub 是否可用（可用即结果 100% 实时）。 |
| [`IsGitRepository`](#field-isgitrepository) | 项目根是否存在 .git 目录（每次 Initialize 重跑）。 |
| [`ChangelogText`](#field-changelogtext) | 「本地 → 远程」更新日志摘要。 |
| [`DetectionDetail`](#field-detectiondetail) | 本次检测各层尝试的可读记录（界面「检测详情」与故障定位用）。 |
| [`RemoteSource`](#field-remotesource) | 远程版本的检测来源（源展示名，如 "GitHub API" / "jsDelivr (cdn.jsdelivr.net)"）。 |
| [`RemoteVersion`](#field-remoteversion) | 远程最新版本号。 |
| [`Status`](#field-status) | 状态栏文本。 |

</div>

### RemoteRouteKind {#field-remoteroutekind}

检测来源所属线路类别（直连 GitHub / 镜像站 / CDN 中转）。

``` csharp
public AesirUpdateService.ReleaseRouteKind RemoteRouteKind;
```

### Snapshot {#field-snapshot}

远程 Release 快照（检测成功后有值）。

``` csharp
public AesirUpdateService.ReleaseSnapshot Snapshot;
```

### Packages {#field-packages}

本地扫描到的安装包（Rescan 时重建）。

``` csharp
public List<AesirUpdateService.InstalledPackage> Packages;
```

### Busy {#field-busy}

忙碌标志（域重载后重置，见类注释）。

``` csharp
[NonSerialized]
public bool Busy;
```

### GitHubDirectAvailable {#field-githubdirectavailable}

本次检测中直连 GitHub 是否可用（可用即结果 100% 实时）。

``` csharp
public bool GitHubDirectAvailable;
```

### IsGitRepository {#field-isgitrepository}

项目根是否存在 .git 目录（每次 Initialize 重跑）。

``` csharp
[NonSerialized]
public bool IsGitRepository;
```

### ChangelogText {#field-changelogtext}

「本地 → 远程」更新日志摘要。

``` csharp
public string ChangelogText;
```

### DetectionDetail {#field-detectiondetail}

本次检测各层尝试的可读记录（界面「检测详情」与故障定位用）。

``` csharp
public string DetectionDetail;
```

### RemoteSource {#field-remotesource}

远程版本的检测来源（源展示名，如 "GitHub API" / "jsDelivr (cdn.jsdelivr.net)"）。

``` csharp
public string RemoteSource;
```

### RemoteVersion {#field-remoteversion}

远程最新版本号。

``` csharp
public string RemoteVersion;
```

### Status {#field-status}

状态栏文本。

``` csharp
public string Status;
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
