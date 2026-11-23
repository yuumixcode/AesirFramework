---
title: BindingInfo
description: "Runestone.AesirModules.BindingInfo 的 API 文档"
---

# `BindingInfo`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `BindingInfo`

## 声明

``` csharp
internal abstract class BindingInfo
```

绑定信息基类：Script 订阅的公共部分（订阅者、优先级、事件类型与诊断成员名）。
internal 实现细节：订阅/退订必须经 EventModule 公开 API 走同一套注册表与死引用清理， 包外不可见；包内测试经 InternalsVisibleTo 访问。 注册表是异构列表（不同事件类型的绑定共存），由本基类提供统一的 Invoke 入口。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Priority`](#property-priority) | 订阅优先级档位。 |
| [`EventType`](#property-eventtype) | 本绑定监听的事件参数类型，同时是注册表键。 |
| [`Subscriber`](#property-subscriber) | 订阅者对象。供死引用清理与按对象退订使用。 |
| [`MemberName`](#property-membername) | 委托方法名，供监视面板展示（Lambda 通常显示为编译器生成的名字）。 |

</div>

### Priority {#property-priority}

订阅优先级档位。

``` csharp
public SubscriberPriority Priority { get; }
```

### EventType {#property-eventtype}

本绑定监听的事件参数类型，同时是注册表键。

``` csharp
public Type EventType { get; }
```

### Subscriber {#property-subscriber}

订阅者对象。供死引用清理与按对象退订使用。

``` csharp
public object Subscriber { get; }
```

### MemberName {#property-membername}

委托方法名，供监视面板展示（Lambda 通常显示为编译器生成的名字）。

``` csharp
public string MemberName { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Invoke(AesirEventArgs)`](#method-invoke-aesireventargs) | 调用订阅委托。参数类型由注册表键保证（分发按运行期事件类型精确匹配），子类直接强转。 |

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

### Invoke(AesirEventArgs) {#method-invoke-aesireventargs}

调用订阅委托。参数类型由注册表键保证（分发按运行期事件类型精确匹配），子类直接强转。

``` csharp
public abstract void Invoke(AesirEventArgs args)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `args` | `AesirEventArgs` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
