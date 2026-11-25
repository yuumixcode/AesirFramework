---
title: MiniEvent<T>
description: "Runestone.AesirArchitecture.MiniEvent<T> 的 API 文档"
---

# `MiniEvent<T>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `MiniEvent<T>`

**实现接口:** `System.IDisposable`

**类型参数**

- `T` — 事件参数类型

## 声明

``` csharp
public sealed class MiniEvent<T> : System.IDisposable 
```

单参事件

**备注**

基于 Action{T} 委托的轻量级事件实现。不使用 List{T} 存储监听者， 而是直接通过 += / -= 操作委托，实现 Invoke 路径零分配（直接多播调用）。 注意：订阅与退订路径（+= / -=）有与当前监听者数量成正比的委托分配，仅适合低频订阅场景。
AddListener 返回 AutoRemoveListenerHandle， 支持使用 using 语句在作用域结束时自动移除监听，或通过 RemoveListenerExtensions 绑定到 Unity 生命周期事件。

与 C# event 关键字的区别：MiniEvent{T} 提供 Dispose 方法， 可主动清空所有委托引用，适合在响应式系统中随宿主对象一起释放资源， 而 C# event 没有内置的清空机制。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`MiniEvent()`](#constructor-minievent) | — |

</div>

### MiniEvent() {#constructor-minievent}

``` csharp
public MiniEvent<T>()
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddListener(Action<T>)`](#method-addlistener-action-t) | 添加监听者，返回可自动移除的监听句柄 |
| [`Dispose()`](#method-dispose) | 清空所有委托引用，释放内存 |
| [`Invoke(T)`](#method-invoke-t) | 调用事件，通知所有监听者 |
| [`RemoveListener(Action<T>)`](#method-removelistener-action-t) | 移除监听者 |

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

### AddListener(Action<T>) {#method-addlistener-action-t}

添加监听者，返回可自动移除的监听句柄

``` csharp
public AutoRemoveListenerHandle AddListener(Action<T> listener)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `listener` | `Action<T>` | 要添加的事件监听委托 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 用于后续自动移除该监听的句柄 |

</div>

### Dispose() {#method-dispose}

清空所有委托引用，释放内存

**备注**

将内部委托置空，断开对所有监听者的引用，防止因监听者长期存活而导致的内存泄漏。 调用后所有已注册的监听者将不再被通知，但不会触发各监听者的移除逻辑—— 如需逐个移除，应使用 RemoveListener 或通过 AutoRemoveListenerHandle。

``` csharp
public void Dispose()
```

### Invoke(T) {#method-invoke-t}

调用事件，通知所有监听者

**备注**

直接多播调用，零分配。异常语义与原生 C# 事件一致：某个监听者抛出异常会中断后续监听者的执行 并向上传播（fail-fast）——监听回调不应抛异常属框架约定，业务异常应在回调内部自行处理。

``` csharp
public void Invoke(T t)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `t` | `T` | 传递给监听者的事件参数 |

</div>

### RemoveListener(Action<T>) {#method-removelistener-action-t}

移除监听者

``` csharp
public void RemoveListener(Action<T> listener)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `listener` | `Action<T>` | 要移除的事件监听委托 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
