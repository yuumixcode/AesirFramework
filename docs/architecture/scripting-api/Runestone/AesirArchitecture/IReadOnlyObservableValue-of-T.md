---
title: IReadOnlyObservableValue<T>
description: "Runestone.AesirArchitecture.IReadOnlyObservableValue<T> 的 API 文档"
---

# `IReadOnlyObservableValue<T>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**类型参数**

- `T` — 属性值类型

## 声明

``` csharp
public interface IReadOnlyObservableValue<T> 
```

只读可观察属性接口。
View 层通过此接口添加监听，不能修改值。

**备注**

这是 View 层使用的只读接口，只能订阅变更不能修改值。
AddListenerAndInvoke 在添加监听后立即触发一次当前值，适用于 View 初始化时同步显示。

InvokeEvent 强制触发通知，用于值未变但需要刷新的场景。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Value`](#property-value) | — |

</div>

### Value {#property-value}

``` csharp
public T Value { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddListener(Action<T>)`](#method-addlistener-action-t) | 添加监听者。回调参数为新值。 |
| [`AddListenerAndInvoke(Action<T>)`](#method-addlistenerandinvoke-action-t) | 添加监听并立即触发一次当前值，用于初始化时同步监听方状态。 |
| [`ClearListeners()`](#method-clearlisteners) | 清空所有监听。 |
| [`InvokeEvent()`](#method-invokeevent) | 触发值变更通知，用于强制刷新监听方状态。 |
| [`RemoveListener(Action<T>)`](#method-removelistener-action-t) | 移除监听者。 |

</div>

### AddListener(Action<T>) {#method-addlistener-action-t}

添加监听者。回调参数为新值。

``` csharp
public abstract AutoRemoveListenerHandle AddListener(Action<T> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<T>` | 值变更时调用的回调函数，参数为变更后的新值。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 返回一个 AutoRemoveListenerHandle，释放后自动移除监听，避免手动管理生命周期。 |

</div>

### AddListenerAndInvoke(Action<T>) {#method-addlistenerandinvoke-action-t}

添加监听并立即触发一次当前值，用于初始化时同步监听方状态。

``` csharp
public abstract AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<T>` | 值变更时调用的回调函数，参数为变更后的新值。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 返回一个 AutoRemoveListenerHandle，释放后自动移除监听。 |

</div>

### ClearListeners() {#method-clearlisteners}

清空所有监听。

**备注**

清除全部监听引用，防止因监听者未释放导致的内存泄漏。清除的是监听者而非值本身。
供持有只读接口的 View / Presenter 在对象池对象归还前统一摘除全部监听： 实现类型均为 sealed，无法经继承补出该能力，只读接口暴露本成员是"只持有接口"场景下的唯一入口。

``` csharp
public abstract void ClearListeners()
```

### InvokeEvent() {#method-invokeevent}

触发值变更通知，用于强制刷新监听方状态。

``` csharp
public abstract void InvokeEvent()
```

### RemoveListener(Action<T>) {#method-removelistener-action-t}

移除监听者。

``` csharp
public abstract void RemoveListener(Action<T> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<T>` | 先前通过 AddListener 注册的回调函数。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
