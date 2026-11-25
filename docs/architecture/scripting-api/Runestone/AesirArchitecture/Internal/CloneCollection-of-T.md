---
title: CloneCollection<T>
description: "Runestone.AesirArchitecture.Internal.CloneCollection<T> 的 API 文档"
---

# `CloneCollection<T>`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture.Internal`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `CloneCollection<T>`

**实现接口:** `System.IDisposable`

**类型参数**

- `T` — 元素类型。

## 声明

``` csharp
[NullableContext]
[Nullable]
internal struct CloneCollection<T> : System.ValueType, 
System.IDisposable 
```

只读克隆集合：把源序列物化为租借数组的临时快照， 供批量操作在写入自身前先完成拷贝（如源序列传入集合自身时避免"枚举中修改"异常）。

**备注**

数组经 Shared 租借、Dispose 时归还； 上游 Cysharp.ObservableCollections 经 CollectionsMarshal 零拷贝取 List{T} 内部数组， Unity netstandard2.1 参考程序集无该 API，本项目以逐项物化作语义等价的降级实现。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CloneCollection(IEnumerable<T>)`](#constructor-clonecollection-ienumerable-t) | 从可枚举源物化克隆。源可提供非枚举计数（TryGetNonEnumeratedCount）时按计数一次性租借， 否则从 16 起步倍增扩容。 |
| [`CloneCollection(List<T>, int, int)`](#constructor-clonecollection-list-t-int-int) | 从 List{T} 区间物化克隆（Unity 无 CollectionsMarshal，逐项拷贝替代零拷贝）。 |

</div>

### CloneCollection(IEnumerable<T>) {#constructor-clonecollection-ienumerable-t}

从可枚举源物化克隆。源可提供非枚举计数（TryGetNonEnumeratedCount）时按计数一次性租借， 否则从 16 起步倍增扩容。

``` csharp
public CloneCollection<T>(IEnumerable<T> source)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `IEnumerable<T>` | 要克隆的源序列。 |

</div>

### CloneCollection(List<T>, int, int) {#constructor-clonecollection-list-t-int-int}

从 List{T} 区间物化克隆（Unity 无 CollectionsMarshal，逐项拷贝替代零拷贝）。

``` csharp
public CloneCollection<T>(List<T> source, int index, int count)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `source` | `List<T>` | 源列表。 |
| `index` | `int` | 区间起始索引。 |
| `count` | `int` | 区间元素数。 |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Span`](#property-span) | — |

</div>

### Span {#property-span}

``` csharp
[Nullable]
public ReadOnlySpan<T> Span { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AsEnumerable()`](#method-asenumerable) | 以 IEnumerable{T} 形态暴露已物化元素（供按接口消费的批量方法使用）。 |
| [`Dispose()`](#method-dispose) | 归还租借数组；重复调用幂等。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `ValueType` |
| `GetHashCode()` | — | `ValueType` |
| `ToString()` | — | `ValueType` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

### AsEnumerable() {#method-asenumerable}

以 IEnumerable{T} 形态暴露已物化元素（供按接口消费的批量方法使用）。

``` csharp
public IEnumerable<T> AsEnumerable()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<T>` | — |

</div>

### Dispose() {#method-dispose}

归还租借数组；重复调用幂等。

``` csharp
public void Dispose()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
