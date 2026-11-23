---
title: MemberGrouper
description: "Runestone.AesirModules.ScriptDocGenerator.Editor.MemberGrouper 的 API 文档"
---

# `MemberGrouper`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator.Editor`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `MemberGrouper`

## 声明

``` csharp
internal static class MemberGrouper
```

成员分组引擎：Default 与 Zensical 两生成器共享的分组核心—— API 成员过滤 → 按选择器分组 → 按固定顺序（常量 → 声明 → 继承 → 运算符）输出非空分组。 替代各生成器手写的"三旗标探测循环"（阈值与守卫类的分组错误曾集中于此）。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GroupOrder`](#field-grouporder) | 分组的固定输出顺序。 |

</div>

### GroupOrder {#field-grouporder}

分组的固定输出顺序。

``` csharp
public static readonly MemberGroup[] GroupOrder;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GroupApiMembers(IEnumerable<IDerivedMemberData>, Func<IDerivedMemberData, MemberGroup>)`](#method-groupapimembers-ienumerable-iderivedmemberdata-func-iderivedmemberdata-membergroup) | 过滤 API 成员并按选择器分组（按 GroupOrder 顺序输出，空分组不输出）。 |
| [`GetGroupLabel(MemberGroup)`](#method-getgrouplabel-membergroup) | 分组标签文本（如"常量"/"声明的"/"继承的"/"运算符"）。 |

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

### GroupApiMembers(IEnumerable<IDerivedMemberData>, Func<IDerivedMemberData, MemberGroup>) {#method-groupapimembers-ienumerable-iderivedmemberdata-func-iderivedmemberdata-membergroup}

过滤 API 成员并按选择器分组（按 GroupOrder 顺序输出，空分组不输出）。

``` csharp
public static List<ValueTuple<MemberGroup, List<IDerivedMemberData>>> GroupApiMembers(IEnumerable<IDerivedMemberData> members, Func<IDerivedMemberData, MemberGroup> groupSelector)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `members` | `IEnumerable<IDerivedMemberData>` | — |
| `groupSelector` | `Func<IDerivedMemberData, MemberGroup>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<ValueTuple<MemberGroup, List<IDerivedMemberData>>>` | — |

</div>

### GetGroupLabel(MemberGroup) {#method-getgrouplabel-membergroup}

分组标签文本（如"常量"/"声明的"/"继承的"/"运算符"）。

``` csharp
public static string GetGroupLabel(MemberGroup group)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `group` | `MemberGroup` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
