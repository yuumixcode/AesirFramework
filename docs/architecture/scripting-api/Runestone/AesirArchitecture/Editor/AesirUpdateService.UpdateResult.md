---
title: AesirUpdateService.UpdateResult
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService.UpdateResult 的 API 文档"
---

# `AesirUpdateService.UpdateResult`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService.UpdateResult`

## 声明

``` csharp
[Serializable]
public sealed class AesirUpdateService.UpdateResult
```

一次更新流程的执行结果：成功 / 用户取消均以正常返回收尾（取消时 Cancelled 为 true， 已导入的包保持有效）；下载失败等异常仍向上抛出（由窗口层弹错误框 + 手动下载指引）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateService.UpdateResult()`](#constructor-aesirupdateservice-updateresult) | — |

</div>

### AesirUpdateService.UpdateResult() {#constructor-aesirupdateservice-updateresult}

``` csharp
public AesirUpdateService.UpdateResult()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CompletedDirNames`](#field-completeddirnames) | — |
| [`SkippedDirNames`](#field-skippeddirnames) | — |
| [`Cancelled`](#field-cancelled) | 用户是否在下载阶段取消了流程（导入是同步步骤，取消不会产生半导入的包）。 |

</div>

### CompletedDirNames {#field-completeddirnames}

``` csharp
public List<string> CompletedDirNames;
```

### SkippedDirNames {#field-skippeddirnames}

``` csharp
public List<string> SkippedDirNames;
```

### Cancelled {#field-cancelled}

用户是否在下载阶段取消了流程（导入是同步步骤，取消不会产生半导入的包）。

``` csharp
public bool Cancelled;
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
