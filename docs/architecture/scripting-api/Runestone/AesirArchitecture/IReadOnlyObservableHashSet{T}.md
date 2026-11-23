---
title: IReadOnlyObservableHashSet<T>
description: "Runestone.AesirArchitecture.IReadOnlyObservableHashSet<T> 的 API 文档"
---

# `IReadOnlyObservableHashSet<T>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `System.Collections.Generic.IEnumerable<T>`，`System.Collections.IEnumerable`，`Runestone.AesirArchitecture.IObservableCollection<T>`，`System.Collections.Generic.IReadOnlyCollection<T>`

**类型参数**

- `T` — 元素类型

## 声明

``` csharp
public interface IReadOnlyObservableHashSet<T> : System.Collections.Generic.IEnumerable<T>, 
System.Collections.IEnumerable, 
Runestone.AesirArchitecture.IObservableCollection<T>, 
System.Collections.Generic.IReadOnlyCollection<T> 
```

只读可观察集合接口。
View 层通过此接口读取元素并订阅变更，不能修改集合。

**备注**

事件语义与 MiniEvent{T} 一致：回调触发时集合已处于变更后的状态； 监听者抛异常按原生 C# 事件 fail-fast 向上传播，监听回调不应抛异常属框架约定。
变更通知为单一事件（AddListener），载荷 CollectionChangedEventArgs{T}： 批量操作（AddRange / RemoveRange）逐项通知实际变更的元素、Clear 以 Reset 通知、无变更的写操作不通知。

.NET Standard 2.1 无 IReadOnlySet<T>（.NET 5 才引入），只读侧无法继承只读集合契约， 因此本接口自行声明 Contains，其余读取能力继承自 IReadOnlyCollection{T}。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Contains(T)`](#method-contains-t) | 判断是否包含指定元素。 |

</div>

### Contains(T) {#method-contains-t}

判断是否包含指定元素。

``` csharp
public abstract bool Contains(T item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `T` | 要查找的元素。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 包含返回 true，否则返回 false。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
