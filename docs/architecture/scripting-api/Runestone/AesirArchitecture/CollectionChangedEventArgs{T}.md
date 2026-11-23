---
title: CollectionChangedEventArgs<T>
description: "Runestone.AesirArchitecture.CollectionChangedEventArgs<T> 的 API 文档"
---

# `CollectionChangedEventArgs<T>`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `CollectionChangedEventArgs<T>`

**类型参数**

- `T` — 元素类型（字典集合为 KeyValuePair{TKey,TValue}）。

## 声明

``` csharp
[IsReadOnly]
public struct CollectionChangedEventArgs<T> : System.ValueType 
```

集合变更事件参数。每次变更携带单个变更项；批量操作（AddRange / RemoveRange 等）由集合逐项触发事件。

**备注**

普通（非 ref）只读结构体，可自由存入集合与闭包；事件经 MiniEvent{T} 分发， 回调以值传递接收（结构体按字段拷贝，无堆分配）。
各 NotifyCollectionChangedAction 携带的字段： Add → NewItem / NewStartingIndex； Remove → OldItem / OldStartingIndex； Replace → NewItem / OldItem / NewStartingIndex（等于 OldStartingIndex）； Move → NewItem（等于 OldItem， 即被移动元素）/ 两个索引； Reset → 无附加字段（Clear / Sort / Reverse 共用， 监听方按"重建视图"处理）。

无索引概念的集合（字典 / HashSet）所有索引固定为 -1。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Action`](#field-action) | 变更类型。 |
| [`NewItem`](#field-newitem) | 变更后的元素（Add / Replace / Move 有效，其余为 default）。 |
| [`OldItem`](#field-olditem) | 变更前的元素（Remove / Replace / Move 有效，其余为 default）。 |
| [`NewStartingIndex`](#field-newstartingindex) | 新位置索引（Add / Replace / Move 有效）；无索引概念的集合为 -1。 |
| [`OldStartingIndex`](#field-oldstartingindex) | 原位置索引（Remove / Replace / Move 有效）；无索引概念的集合为 -1。 |

</div>

### Action {#field-action}

变更类型。

``` csharp
public readonly NotifyCollectionChangedAction Action;
```

### NewItem {#field-newitem}

变更后的元素（Add / Replace / Move 有效，其余为 default）。

``` csharp
public readonly T NewItem;
```

### OldItem {#field-olditem}

变更前的元素（Remove / Replace / Move 有效，其余为 default）。

``` csharp
public readonly T OldItem;
```

### NewStartingIndex {#field-newstartingindex}

新位置索引（Add / Replace / Move 有效）；无索引概念的集合为 -1。

``` csharp
public readonly int NewStartingIndex;
```

### OldStartingIndex {#field-oldstartingindex}

原位置索引（Remove / Replace / Move 有效）；无索引概念的集合为 -1。

``` csharp
public readonly int OldStartingIndex;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Add(T, int)`](#method-add-t-int) | 构造添加变更。 |
| [`Move(T, int, int)`](#method-move-t-int-int) | 构造移动变更。 |
| [`Remove(T, int)`](#method-remove-t-int) | 构造移除变更。 |
| [`Replace(T, T, int)`](#method-replace-t-t-int) | 构造替换变更。 |
| [`Reset()`](#method-reset) | 构造重置变更（Clear / Sort / Reverse 共用，无附加字段）。 |

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

### Add(T, int) {#method-add-t-int}

构造添加变更。

``` csharp
public static CollectionChangedEventArgs<T> Add(T newItem, int newStartingIndex)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `newItem` | `T` | 被添加的元素。 |
| `newStartingIndex` | `int` | 被添加到的索引；无索引概念的集合传 -1。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `CollectionChangedEventArgs<T>` | 添加变更参数。 |

</div>

### Move(T, int, int) {#method-move-t-int-int}

构造移动变更。

``` csharp
public static CollectionChangedEventArgs<T> Move(T movedItem, int newStartingIndex, int oldStartingIndex)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `movedItem` | `T` | 被移动的元素（同时写入 NewItem 与 OldItem）。 |
| `newStartingIndex` | `int` | 移动后的索引。 |
| `oldStartingIndex` | `int` | 移动前的索引。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `CollectionChangedEventArgs<T>` | 移动变更参数。 |

</div>

### Remove(T, int) {#method-remove-t-int}

构造移除变更。

``` csharp
public static CollectionChangedEventArgs<T> Remove(T oldItem, int oldStartingIndex)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `oldItem` | `T` | 被移除的元素。 |
| `oldStartingIndex` | `int` | 移除前所在索引；无索引概念的集合传 -1。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `CollectionChangedEventArgs<T>` | 移除变更参数。 |

</div>

### Replace(T, T, int) {#method-replace-t-t-int}

构造替换变更。

``` csharp
public static CollectionChangedEventArgs<T> Replace(T newItem, T oldItem, int index)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `newItem` | `T` | 替换后的元素。 |
| `oldItem` | `T` | 替换前的元素。 |
| `index` | `int` | 替换位置索引；无索引概念的集合传 -1。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `CollectionChangedEventArgs<T>` | 替换变更参数。 |

</div>

### Reset() {#method-reset}

构造重置变更（Clear / Sort / Reverse 共用，无附加字段）。

``` csharp
public static CollectionChangedEventArgs<T> Reset()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `CollectionChangedEventArgs<T>` | 重置变更参数。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
