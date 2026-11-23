---
title: IObservableHashSet<T>
description: "Runestone.AesirArchitecture.IObservableHashSet<T> 的 API 文档"
---

# `IObservableHashSet<T>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.IReadOnlyObservableHashSet<T>`，`System.Collections.Generic.IEnumerable<T>`，`System.Collections.IEnumerable`，`Runestone.AesirArchitecture.IObservableCollection<T>`，`System.Collections.Generic.ICollection<T>`，`System.Collections.Generic.IReadOnlyCollection<T>`

**类型参数**

- `T` — 元素类型

## 声明

``` csharp
public interface IObservableHashSet<T> : Runestone.AesirArchitecture.IReadOnlyObservableHashSet<T>, 
System.Collections.Generic.IEnumerable<T>, 
System.Collections.IEnumerable, 
Runestone.AesirArchitecture.IObservableCollection<T>, 
System.Collections.Generic.ICollection<T>, 
System.Collections.Generic.IReadOnlyCollection<T> 
```

完整可观察集合接口。
Model 层通过此接口读写集合；View 层使用 IReadOnlyObservableHashSet{T} 只读订阅。

**备注**

所有写操作完成后才触发对应通知，监听者回调中读取到的集合已是变更后的状态。 无变更的操作不触发通知：Add 重复元素、Remove 不存在的元素、Clear 空集合。
有意不继承 ISet{T}——集合代数（并/交/差/子集判定）对独立游戏属低频能力， 需要时直接使用内部 HashSet{T} 或上游 Cysharp.ObservableCollections； 保持 ICollection{T} 写侧契约（Add / Remove / Clear / Contains）不变。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Count`](#property-count) | — |

</div>

### Count {#property-count}

``` csharp
public int Count { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Contains(T)`](#method-contains-t) | — |

</div>

### Contains(T) {#method-contains-t}

``` csharp
public abstract bool Contains(T item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `T` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
