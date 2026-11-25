---
title: CloneCollection<T>.EnumerableCollection<T>
description: "Runestone.AesirArchitecture.Internal.CloneCollection<T>.EnumerableCollection<T> 的 API 文档"
---

# `CloneCollection<T>.EnumerableCollection<T>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Internal`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `CloneCollection<T>.EnumerableCollection<T>`

**实现接口:** `System.Collections.Generic.IEnumerable<T>`，`System.Collections.IEnumerable`，`System.Collections.Generic.ICollection<T>`

**类型参数**

- `T` — 元素类型。

## 声明

``` csharp
[Nullable]
private class CloneCollection<T>.EnumerableCollection<T> : System.Collections.Generic.IEnumerable<T>, 
System.Collections.IEnumerable, 
System.Collections.Generic.ICollection<T> 
```

只读克隆集合：把源序列物化为租借数组的临时快照， 供批量操作在写入自身前先完成拷贝（如源序列传入集合自身时避免"枚举中修改"异常）。

**备注**

数组经 Shared 租借、Dispose 时归还； 上游 Cysharp.ObservableCollections 经 CollectionsMarshal 零拷贝取 List{T} 内部数组， Unity netstandard2.1 参考程序集无该 API，本项目以逐项物化作语义等价的降级实现。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CloneCollection(T[], int)`](#constructor-clonecollection-t-int) | 以可能为 null 的租借数组构造（null 视为空集合）。 |

</div>

### CloneCollection(T[], int) {#constructor-clonecollection-t-int}

以可能为 null 的租借数组构造（null 视为空集合）。

``` csharp
public CloneCollection<T>.EnumerableCollection<T>(T[] array, int count)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `array` | `T[]` | — |
| `count` | `int` | — |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IsReadOnly`](#property-isreadonly) | 固定为只读。 |
| [`Count`](#property-count) | 已物化的元素数。 |

</div>

### IsReadOnly {#property-isreadonly}

固定为只读。

``` csharp
public bool IsReadOnly { get; }
```

### Count {#property-count}

已物化的元素数。

``` csharp
public int Count { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetEnumerator()`](#method-getenumerator) | 按序枚举已物化区间。 |
| [`Contains(T)`](#method-contains-t) | 按值扫描已物化区间。实现 ICollection{T} 契约而非抛异常—— 调用方按接口约定使用 Contains 不应触雷。 |
| [`Remove(T)`](#method-remove-t) | 不支持——克隆为只读快照。 |
| [`Add(T)`](#method-add-t) | 不支持——克隆为只读快照。 |
| [`Clear()`](#method-clear) | 不支持——克隆为只读快照。 |
| [`CopyTo(T[], int)`](#method-copyto-t-int) | 拷贝已物化区间到目标数组。 |

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

### GetEnumerator() {#method-getenumerator}

按序枚举已物化区间。

``` csharp
[IteratorStateMachine]
public IEnumerator<T> GetEnumerator()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerator<T>` | — |

</div>

### Contains(T) {#method-contains-t}

按值扫描已物化区间。实现 ICollection{T} 契约而非抛异常—— 调用方按接口约定使用 Contains 不应触雷。

``` csharp
public bool Contains(T item)
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

### Remove(T) {#method-remove-t}

不支持——克隆为只读快照。

``` csharp
public bool Remove(T item)
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

### Add(T) {#method-add-t}

不支持——克隆为只读快照。

``` csharp
public void Add(T item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `T` | — |

</div>

### Clear() {#method-clear}

不支持——克隆为只读快照。

``` csharp
public void Clear()
```

### CopyTo(T[], int) {#method-copyto-t-int}

拷贝已物化区间到目标数组。

``` csharp
public void CopyTo(T[] dest, int destIndex)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `dest` | `T[]` | — |
| `destIndex` | `int` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
