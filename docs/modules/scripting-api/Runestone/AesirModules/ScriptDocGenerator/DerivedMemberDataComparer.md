---
title: DerivedMemberDataComparer
description: "Runestone.AesirModules.ScriptDocGenerator.DerivedMemberDataComparer 的 API 文档"
---

# `DerivedMemberDataComparer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `DerivedMemberDataComparer`

**实现接口:** `System.Collections.Generic.IComparer<IDerivedMemberData>`

## 声明

``` csharp
public class DerivedMemberDataComparer : System.Collections.Generic.IComparer<IDerivedMemberData>
```

IDerivedMemberData 比较类

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DerivedMemberDataComparer()`](#constructor-derivedmemberdatacomparer) | — |

</div>

### DerivedMemberDataComparer() {#constructor-derivedmemberdatacomparer}

``` csharp
public DerivedMemberDataComparer()
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Compare(IDerivedMemberData, IDerivedMemberData)`](#method-compare-iderivedmemberdata-iderivedmemberdata) | 比较两个继承 IDerivedMemberData 的数据类的实例，用于排序 |

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

### Compare(IDerivedMemberData, IDerivedMemberData) {#method-compare-iderivedmemberdata-iderivedmemberdata}

比较两个继承 IDerivedMemberData 的数据类的实例，用于排序

``` csharp
public int Compare(IDerivedMemberData x, IDerivedMemberData y)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `x` | `IDerivedMemberData` | — |
| `y` | `IDerivedMemberData` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `int` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
