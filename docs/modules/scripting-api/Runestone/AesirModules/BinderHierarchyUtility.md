---
title: BinderHierarchyUtility
description: "Runestone.AesirModules.BinderHierarchyUtility 的 API 文档"
---

# `BinderHierarchyUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `BinderHierarchyUtility`

## 声明

``` csharp
internal static class BinderHierarchyUtility
```

场景层级路径工具类，用于 Object Binder 计算物体在层级中的路径。
提供绝对路径和相对路径两种计算方式： - 绝对路径：从场景根物体到目标的完整路径，用于 BinderAssistant 和 BinderTag 的路径标识。 - 相对路径：子物体相对于父物体的路径，用于生成脚本中 transform.Find() 的参数。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetAbsolutePath(Transform)`](#method-getabsolutepath-transform) | 获取物体在场景层级中的绝对路径（从根物体到目标，以 / 分隔）。 |
| [`GetRelativePath(string, string)`](#method-getrelativepath-string-string) | 获取子物体相对于父物体的路径。 通过对比两条绝对路径的公共前缀，截取子路径部分。 若子路径不以父路径为前缀（含子路径比父路径更短）则返回 null（说明二者不是父子关系）。 二者为同一物体时返回空字符串（生成代码中用于表示绑定自身）。 |

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

### GetAbsolutePath(Transform) {#method-getabsolutepath-transform}

获取物体在场景层级中的绝对路径（从根物体到目标，以 / 分隔）。

``` csharp
public static string GetAbsolutePath(Transform trans)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `trans` | `Transform` | 目标 Transform。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | 绝对路径字符串，如 Canvas/Panel/Button。 |

</div>

### GetRelativePath(string, string) {#method-getrelativepath-string-string}

获取子物体相对于父物体的路径。
通过对比两条绝对路径的公共前缀，截取子路径部分。 若子路径不以父路径为前缀（含子路径比父路径更短）则返回 null（说明二者不是父子关系）。 二者为同一物体时返回空字符串（生成代码中用于表示绑定自身）。

``` csharp
public static string GetRelativePath(string parentPath, string childPath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `parentPath` | `string` | 父物体的绝对路径。 |
| `childPath` | `string` | 子物体的绝对路径。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | 相对路径字符串（如 Panel/Button）；同一物体返回空字符串；不是父子关系则返回 null。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
