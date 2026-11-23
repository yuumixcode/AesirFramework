---
title: ResetStaticsAssistant
description: "Runestone.AesirArchitecture.ResetStaticsAssistant 的 API 文档"
---

# `ResetStaticsAssistant`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `ResetStaticsAssistant`

## 声明

``` csharp
public static class ResetStaticsAssistant
```

静态变量重置助手（仅泛型类使用）。用于运行时阶段自动重置泛型类中的静态变量，兼容 Disable Domain Reload。 关闭 Domain Reload 时静态回调列表不会重置，所以每次启动时均可调用重置方法。

**备注**

适用范围：仅泛型类（如 AbstractContext<T>）需要本助手——泛型类中的 [RuntimeInitializeOnLoadMethod] 会被 Unity 静默跳过（不执行也不报错，Unity 2022.3 实测）， 无法在自身内部声明域重载重置入口，只能通过本助手在非泛型的中心位置注册重置回调。 非泛型类不要使用本助手，直接在类内声明 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] 重置方法即可。

背景：Unity 默认在进入 Play Mode 时进行域重置（Domain Reload）， 会清空所有静态字段的状态。但关闭 Domain Reload（即 "Enter Play Mode Options" 中的 "Reload Domain" 未勾选）后， 静态字段不会自动重置，上一次 Play Mode 的残留数据可能导致意外的行为或错误。

解决方案：此助手通过 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] 在域加载的子系统注册阶段自动触发 ResetStaticsAll， 遍历并执行所有通过 Register 注册的重置回调，手动将静态字段恢复到初始状态。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Register(Action)`](#method-register-action) | 注册静态变量重置回调，在 Domain Reload 时自动调用 |
| [`ResetForTests()`](#method-resetfortests) | 手动执行所有已注册的静态变量重置回调。仅供单元测试隔离静态单例状态使用。 |

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

### Register(Action) {#method-register-action}

注册静态变量重置回调，在 Domain Reload 时自动调用

**备注**

注册的回调会在每次域加载时由 ResetStaticsAll 自动执行，无需手动调用。 适用于重置任何在 Disable Domain Reload 模式下不会自动清空的静态字段。
约定：重置回调中禁止调用 Register—— 遍历回调列表时动态添加会导致 InvalidOperationException: Collection was modified。 当前全部注册均在静态构造函数中完成（一次性、非动态），不触发此问题。

``` csharp
public static void Register(Action callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action` | 静态变量重置回调，在域加载时自动执行，应将相关静态字段重置为初始值 |

</div>

### ResetForTests() {#method-resetfortests}

手动执行所有已注册的静态变量重置回调。仅供单元测试隔离静态单例状态使用。

**备注**

EditMode 测试在同一域内重复运行时不会触发域重载，已注册的静态单例（如 AbstractContext<T>._instance）会跨测试运行残留，导致依赖"首次访问创建"的用例失败。 测试夹具应在 SetUp 中调用此方法恢复静态字段初始状态。
以 [Conditional("UNITY_INCLUDE_TESTS")] 标记，非测试构建中所有调用点自动剔除。

``` csharp
[Conditional]
public static void ResetForTests()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
