---
title: AesirTimerDebuggerSettings
description: "Runestone.AesirArchitecture.Editor.AesirTimerDebuggerSettings 的 API 文档"
---

# `AesirTimerDebuggerSettings`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirTimerDebuggerSettings`

## 声明

``` csharp
internal static class AesirTimerDebuggerSettings
```

计时器调试面板的持久化设置：拉取模式与频率、是否自动刷新。

**备注**

调试面板只在"拉取"时枚举注册表并重建显示文本，两次拉取之间只绘制缓存行； 因此这里配置的是状态拉取频率而不是重绘频率——它决定了打开面板时的调试开销。 默认按 0.5 秒拉取：肉眼看不出延迟，开销约为每帧拉取的 1/30。

值经 EditorPrefs 持久化，并缓存在静态字段里（域重载后首次访问重新读取）， 避免 60–100 Hz 的 update 路径反复调用原生 EditorPrefs。 写入时触发 Changed，已打开的两个窗口据此重置节奏并立即刷新。

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
