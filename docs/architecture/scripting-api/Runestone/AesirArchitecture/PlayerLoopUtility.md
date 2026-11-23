---
title: PlayerLoopUtility
description: "Runestone.AesirArchitecture.PlayerLoopUtility 的 API 文档"
---

# `PlayerLoopUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `PlayerLoopUtility`

## 声明

``` csharp
public static class PlayerLoopUtility
```

PlayerLoop 操作的静态工具类，提供子系统的插入、查询与描述功能。
供框架内部和外部用户扩展 PlayerLoop，不局限于 AesirLifecyclePhase 预定义阶段。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ContainsSystem()`](#method-containssystem) | 检测 PlayerLoop 中是否包含指定类型的子系统 |
| [`InsertSystemAfter(PlayerLoopSystem)`](#method-insertsystemafter-playerloopsystem) | 在 PlayerLoop 中指定子系统后插入自定义系统 |
| [`InsertSystemBefore(PlayerLoopSystem)`](#method-insertsystembefore-playerloopsystem) | 在 PlayerLoop 中指定子系统前插入自定义系统 |

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

### ContainsSystem() {#method-containssystem}

检测 PlayerLoop 中是否包含指定类型的子系统

``` csharp
public static bool ContainsSystem<TTarget>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 是否包含目标子系统 |

</div>

### InsertSystemAfter(PlayerLoopSystem) {#method-insertsystemafter-playerloopsystem}

在 PlayerLoop 中指定子系统后插入自定义系统

``` csharp
public static bool InsertSystemAfter<TTarget>(PlayerLoopSystem system)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `system` | `PlayerLoopSystem` | 要插入的自定义子系统 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 是否成功找到目标并插入 |

</div>

### InsertSystemBefore(PlayerLoopSystem) {#method-insertsystembefore-playerloopsystem}

在 PlayerLoop 中指定子系统前插入自定义系统

``` csharp
public static bool InsertSystemBefore<TTarget>(PlayerLoopSystem system)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `system` | `PlayerLoopSystem` | 要插入的自定义子系统 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 是否成功找到目标并插入 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
