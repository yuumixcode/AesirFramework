---
title: ScriptBindingInfo<TEventArgs>
description: "Runestone.AesirModules.ScriptBindingInfo<TEventArgs> 的 API 文档"
---

# `ScriptBindingInfo<TEventArgs>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `Runestone.AesirModules.BindingInfo` → `ScriptBindingInfo<TEventArgs>`

## 声明

``` csharp
internal sealed class ScriptBindingInfo<TEventArgs> : Runestone.AesirModules.BindingInfo where TEventArgs : Runestone.AesirModules.AesirEventArgs
```

Script 订阅绑定。持有 Action{T} 委托，注册后直接调用—— 无反射、无表达式树、无参数数组，分发热路径只是一次虚调用加一次委托调用。

**备注**

非泛型订阅（按参数实例推断类型）复用 ScriptBindingInfo<AesirEventArgs> 并显式传入运行期事件类型作为注册表键——键已保证分发参数的实际类型， 基类回调可直接接收，无需反射适配。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ScriptBindingInfo(Action<TEventArgs>, SubscriberPriority, object, Type)`](#constructor-scriptbindinginfo-action-teventargs-subscriberpriority-object-type) | — |

</div>

### ScriptBindingInfo(Action<TEventArgs>, SubscriberPriority, object, Type) {#constructor-scriptbindinginfo-action-teventargs-subscriberpriority-object-type}

``` csharp
public ScriptBindingInfo<TEventArgs>(Action<TEventArgs> callback, SubscriberPriority priority, object subscriber, Type eventType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<TEventArgs>` | — |
| `priority` | `SubscriberPriority` | — |
| `subscriber` | `object` | — |
| `eventType` | `Type` | — |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `Priority` | 订阅优先级档位。 | `BindingInfo` |
| `EventType` | 本绑定监听的事件参数类型，同时是注册表键。 | `BindingInfo` |
| `Subscriber` | 订阅者对象。供死引用清理与按对象退订使用。 | `BindingInfo` |
| `MemberName` | 委托方法名，供监视面板展示（Lambda 通常显示为编译器生成的名字）。 | `BindingInfo` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Invoke(AesirEventArgs)` | — | `ScriptBindingInfo<TEventArgs>` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
