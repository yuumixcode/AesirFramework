---
title: AesirSamplesBuildFilter
description: "Runestone.AesirArchitecture.Editor.AesirSamplesBuildFilter 的 API 文档"
---

# `AesirSamplesBuildFilter`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirSamplesBuildFilter`

## 声明

``` csharp
internal static class AesirSamplesBuildFilter
```

Aesir 示例构建剔除钩子 — 构建发起时自动把本次构建场景列表中的 Aesir 示例场景剔除，并输出明细日志。
示例面向编辑器内学习，不应进入玩家构建：示例脚本已由整文件 #if UNITY_EDITOR 在编译期剔除， 本钩子补上场景侧——用户手动加进 Build Settings 的示例场景在构建时自动跳过， 且 Build Settings 窗口中的场景列表数据不被修改（剔除只作用于本次构建选项）。

实现：编辑器加载时经 RegisterBuildPlayerHandler 注册构建入口回调， 在回调中改写 scenes 后转交 BuildPlayer。 不用 IPreprocessBuildWithReport：其 OnPreprocessBuild 触发时场景列表已快照进构建选项， 回调内改 EditorBuildSettings 影响不到本次构建、还会污染持久数据。

**备注**

边界：只拦截 Build Settings 窗口发起的构建——自定义构建脚本 / CI 直接调用 BuildPipeline.BuildPlayer 时不经过本钩子，场景列表由调用方自行组织。

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
