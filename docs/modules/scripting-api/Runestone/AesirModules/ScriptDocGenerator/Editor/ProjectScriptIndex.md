---
title: ProjectScriptIndex
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.ProjectScriptIndex 的 API 文档"
---

# `ProjectScriptIndex`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `ProjectScriptIndex`

## 声明

``` csharp
internal static class ProjectScriptIndex
```

项目级类型声明索引：类型名 → 声明所在文件路径列表，附带 文件 → 命名空间集合。 惰性构建，每个域重载周期至多一次全项目扫描（历史实现按"每个找不到源文件的类型" 全项目扫描一次，N 个外部类型即 N 次全扫，此索引将其收敛为 1 次）。 类型声明检测复用 SourceScanner 的字符串/注释感知净化， 注释与字符串里的假类型声明不会进入索引。索引只存路径与名称，不驻留文件内容。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`FileDeclaresNamespace(string, string)`](#method-filedeclaresnamespace-string-string) | 文件是否声明了指定命名空间。用于按期望命名空间过滤候选文件， 排除其他命名空间中的同名类型。 |
| [`TryGetTypePaths(string, ref List<string>)`](#method-trygettypepaths-string-ref-list-string) | 查询声明了指定类型名的文件路径列表。索引未构建时先构建（全项目一次）。 |

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

### FileDeclaresNamespace(string, string) {#method-filedeclaresnamespace-string-string}

文件是否声明了指定命名空间。用于按期望命名空间过滤候选文件， 排除其他命名空间中的同名类型。

``` csharp
public static bool FileDeclaresNamespace(string path, string namespaceName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | — |
| `namespaceName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetTypePaths(string, ref List<string>) {#method-trygettypepaths-string-ref-list-string}

查询声明了指定类型名的文件路径列表。索引未构建时先构建（全项目一次）。

``` csharp
public static bool TryGetTypePaths(string typeName, out ref List<string> paths)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `typeName` | `string` | — |
| `paths` | `ref List<string>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
