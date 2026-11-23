---
title: SourceSummaryInitializer
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.SourceSummaryInitializer 的 API 文档"
---

# `SourceSummaryInitializer`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `SourceSummaryInitializer`

## 声明

``` csharp
[InitializeOnLoad]
public static class SourceSummaryInitializer
```

在编辑器程序集（Runestone.AesirModules.ScriptDocGenerator.Editor）加载时注入 XML 文档注释解析器： summary / param / returns / remarks / value / typeparam 六种标签全部接入 MemberData 对应解析委托。 Summary 优先检查 [Summary] 特性；若无则从源代码的 XML /// <summary> 注释中读取。 使用全限定键（AssemblyName.Namespace.TypeName.MemberName）避免跨程序集同名类型冲突。 查询键链：嵌套类型规范键（FullName，+ 转 .）→ 扁平旧键（Namespace.最内层类型）； 方法/构造函数附参数后缀：参数类型键 → 参数计数键 → 无后缀键； 重载全部失配时按"该方法名下 distinct 文本恰好 1 个"回退（宁可缺不可错）。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ClearCache()`](#method-clearcache) | 清空所有缓存。每次类型分析前由 ScriptDocGeneratorUtility 调用， 保证分析总是读取磁盘上最新的 XML 注释（此前仅靠域重载失效，存在同域内旧缓存窗口）。 |

</div>

**继承的方法**

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

### ClearCache() {#method-clearcache}

清空所有缓存。每次类型分析前由 ScriptDocGeneratorUtility 调用， 保证分析总是读取磁盘上最新的 XML 注释（此前仅靠域重载失效，存在同域内旧缓存窗口）。

``` csharp
public static void ClearCache()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
