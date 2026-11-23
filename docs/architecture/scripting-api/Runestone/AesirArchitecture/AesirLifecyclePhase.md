---
title: AesirLifecyclePhase
description: "Runestone.AesirArchitecture.AesirLifecyclePhase 的 API 文档"
---

# `AesirLifecyclePhase`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `AesirLifecyclePhase`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum AesirLifecyclePhase : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

游戏级生命周期阶段，对应 PlayerLoop 子系统插入点

**备注**

各阶段对应的 PlayerLoop 插入位置（按执行顺序）： BeforeUpdate：通过 PlayerLoopUtility.InsertSystemBefore<Update> 注入到 PlayerLoop.Update 子系统之前，确保架构逻辑在每帧 Update 阶段开始前执行。 AfterUpdate：通过 PlayerLoopUtility.InsertSystemAfter<PostLateUpdate> 注入到 PlayerLoop.PostLateUpdate 子系统之后，确保架构逻辑在每帧所有更新完成后执行，可读取当前帧的最终状态。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AfterUpdate`](#field-afterupdate) | 逻辑帧结束：在 PlayerLoop.PostLateUpdate 之后执行，读取当前帧所有状态 |
| [`BeforeUpdate`](#field-beforeupdate) | 逻辑帧开始：在 PlayerLoop.Update 之前执行，架构优先运算 |

</div>

### AfterUpdate {#field-afterupdate}

逻辑帧结束：在 PlayerLoop.PostLateUpdate 之后执行，读取当前帧所有状态

``` csharp
public const AesirLifecyclePhase AfterUpdate;
```

### BeforeUpdate {#field-beforeupdate}

逻辑帧开始：在 PlayerLoop.Update 之前执行，架构优先运算

``` csharp
public const AesirLifecyclePhase BeforeUpdate;
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `HasFlag(Enum)` | — | `Enum` |
| `Equals(object)` | — | `Enum` |
| `GetHashCode()` | — | `Enum` |
| `ToString()` | — | `Enum` |
| `ToString(string)` | — | `Enum` |
| `GetTypeCode()` | — | `Enum` |
| `CompareTo(object)` | — | `Enum` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `ToString(IFormatProvider)` | — | `Enum` |
| `ToString(string, IFormatProvider)` | — | `Enum` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
