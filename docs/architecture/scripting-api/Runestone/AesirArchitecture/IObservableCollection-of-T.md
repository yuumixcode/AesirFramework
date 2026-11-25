---
title: IObservableCollection<T>
description: "Runestone.AesirArchitecture.IObservableCollection<T> 的 API 文档"
---

# `IObservableCollection<T>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `System.Collections.Generic.IEnumerable<T>`，`System.Collections.IEnumerable`，`System.Collections.Generic.IReadOnlyCollection<T>`

**类型参数**

- `T` — 元素类型（字典集合为 KeyValuePair{TKey,TValue}）。

## 声明

``` csharp
public interface IObservableCollection<T> : System.Collections.Generic.IEnumerable<T>, 
System.Collections.IEnumerable, 
System.Collections.Generic.IReadOnlyCollection<T> 
```

可观察集合契约：单一变更事件的订阅与退订。

**备注**

由 ObservableList{T} / ObservableDictionary{TKey, TValue} / ObservableHashSet{T} 统一实现。
变更通知为单轨事件（内部由 MiniEvent{T} 承载）：无变更的写操作不通知 （索引器赋相同值、Remove 不存在的元素、Clear 空集合等）；批量操作逐项通知； Sort / Reverse / Clear 以 Reset 通知（无附加字段）。

AddListener 返回 AutoRemoveListenerHandle， 可用 using 语句在作用域结束时自动移除，或经 RemoveListenerExtensions 绑定到 Unity 生命周期（OnDestroy / OnDisable / 场景卸载）自动清理。

集合内部不加锁，不做任何线程同步——与框架整体边界一致，仅约定主线程使用。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddListener(Action<CollectionChangedEventArgs<T>>)`](#method-addlistener-action-collectionchangedeventargs-t) | 添加集合变更监听者。回调参数为本次变更（见 CollectionChangedEventArgs{T} 的字段约定）。 |
| [`ClearListeners()`](#method-clearlisteners) | 清空所有变更监听。 |
| [`RemoveListener(Action<CollectionChangedEventArgs<T>>)`](#method-removelistener-action-collectionchangedeventargs-t) | 移除集合变更监听者。 |

</div>

### AddListener(Action<CollectionChangedEventArgs<T>>) {#method-addlistener-action-collectionchangedeventargs-t}

添加集合变更监听者。回调参数为本次变更（见 CollectionChangedEventArgs{T} 的字段约定）。

``` csharp
public abstract AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<T>> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<CollectionChangedEventArgs<T>>` | 集合变更时调用的回调函数。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 返回一个 AutoRemoveListenerHandle，释放后自动移除监听，避免手动管理生命周期。 |

</div>

### ClearListeners() {#method-clearlisteners}

清空所有变更监听。

**备注**

清除全部监听引用，防止因监听者未释放导致的内存泄漏。 与清空元素的方法（Clear）不同——本方法清除的是监听者而非集合内容。
供持有只读接口的 View / Presenter 在对象池对象归还前统一摘除全部监听： 实现类型均为 sealed，无法经继承补出该能力，只读接口暴露本成员是"只持有接口"场景下的唯一入口。

``` csharp
public abstract void ClearListeners()
```

### RemoveListener(Action<CollectionChangedEventArgs<T>>) {#method-removelistener-action-collectionchangedeventargs-t}

移除集合变更监听者。

``` csharp
public abstract void RemoveListener(Action<CollectionChangedEventArgs<T>> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<CollectionChangedEventArgs<T>>` | 先前通过 AddListener 注册的回调函数。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
