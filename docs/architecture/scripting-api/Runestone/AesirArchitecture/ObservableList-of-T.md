---
title: ObservableList<T>
description: "Runestone.AesirArchitecture.ObservableList<T> 的 API 文档"
---

# `ObservableList<T>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `ObservableList<T>`

**实现接口:** `System.Collections.Generic.IReadOnlyList<T>`，`Runestone.AesirArchitecture.IObservableList<T>`，`Runestone.AesirArchitecture.IReadOnlyObservableList<T>`，`System.Collections.Generic.IEnumerable<T>`，`System.Collections.IEnumerable`，`System.Collections.Generic.IList<T>`，`Runestone.AesirArchitecture.IObservableCollection<T>`，`System.Collections.Generic.ICollection<T>`，`System.Collections.Generic.IReadOnlyCollection<T>`

**类型参数**

- `T` — 元素类型

## 声明

``` csharp
[DefaultMember]
[Serializable]
public sealed class ObservableList<T> : System.Collections.Generic.IReadOnlyList<T>, 
Runestone.AesirArchitecture.IObservableList<T>, 
Runestone.AesirArchitecture.IReadOnlyObservableList<T>, 
System.Collections.Generic.IEnumerable<T>, 
System.Collections.IEnumerable, 
System.Collections.Generic.IList<T>, 
Runestone.AesirArchitecture.IObservableCollection<T>, 
System.Collections.Generic.ICollection<T>, 
System.Collections.Generic.IReadOnlyCollection<T> 
```

可观察列表实现。
Model 层持有可写实例，View 层通过 IReadOnlyObservableList{T} 只读订阅。

**备注**

内部组合 List{T} 存储元素，变更通知经 MiniEvent{T} 分发——Invoke 路径零分配 （直接多播调用）。注意：订阅路径（AddListener / 句柄创建）有与监听者数量成正比的委托分配， 勿在每帧订阅场景使用。
[SerializeField] 标记 items 字段使其可在 Inspector 中编辑初始元素； 反序列化填充不触发任何事件（与 ObservableValue{T} 行为一致）。

变更通知为单一事件（AddListener）：写操作完成后才触发，监听者回调中读取到的集合已是变更后的状态； 无变更的操作不通知（Remove 不存在的元素、Clear 空列表、索引器赋相同值）； 批量操作（AddRange / InsertRange / RemoveRange）逐项通知； Sort() / Reverse() / Clear 以 Reset 通知（无附加字段，监听方按"重建视图"处理）。

遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 IReadOnlyObservableList{T} / IEnumerable{T} 接口遍历会装箱一次枚举器（与 BCL List{T} 行为一致）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ObservableList()`](#constructor-observablelist) | 默认构造，创建空列表。 |
| [`ObservableList(IEnumerable<T>)`](#constructor-observablelist-ienumerable-t) | 指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。 |
| [`ObservableList(int)`](#constructor-observablelist-int) | 指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。 |

</div>

### ObservableList() {#constructor-observablelist}

默认构造，创建空列表。

``` csharp
public ObservableList<T>()
```

### ObservableList(IEnumerable<T>) {#constructor-observablelist-ienumerable-t}

指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。

``` csharp
public ObservableList<T>(IEnumerable<T> initialItems)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `initialItems` | `IEnumerable<T>` | 初始元素序列。 |

</div>

### ObservableList(int) {#constructor-observablelist-int}

指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。

``` csharp
public ObservableList<T>(int capacity)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `capacity` | `int` | — |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Item`](#property-item) | 读写指定索引的元素。值变化时触发 Replace 通知，相同则不通知。 |
| [`IsReadOnly`](#property-isreadonly) | 固定返回 false，该集合可写。 |
| [`Count`](#property-count) | 元素数量。 |

</div>

### Item {#property-item}

读写指定索引的元素。值变化时触发 Replace 通知，相同则不通知。

**备注**

使用 EqualityComparer{T}.Default 判断值是否变化，仅在变化时触发通知。

``` csharp
public T Item { get; set; }
```

### IsReadOnly {#property-isreadonly}

固定返回 false，该集合可写。

``` csharp
public bool IsReadOnly { get; }
```

### Count {#property-count}

元素数量。

``` csharp
public int Count { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddListener(Action<CollectionChangedEventArgs<T>>)`](#method-addlistener-action-collectionchangedeventargs-t) | — |
| [`GetEnumerator()`](#method-getenumerator) | 返回遍历元素的结构体枚举器，foreach 具体类型时零分配。 |
| [`Contains(T)`](#method-contains-t) | 判断是否包含指定元素。 |
| [`Remove(T)`](#method-remove-t) | 移除第一个匹配元素，成功时触发 Remove 通知。 |
| [`IndexOf(T)`](#method-indexof-t) | 返回指定元素的索引；不存在时返回 -1。 |
| [`Add(T)`](#method-add-t) | 在末尾添加元素，触发 Add 通知（索引为 Count - 1）。 |
| [`AddRange(IEnumerable<T>)`](#method-addrange-ienumerable-t) | 批量添加元素。逐项添加并逐项触发 Add 通知。 |
| [`AddRange(T[])`](#method-addrange-t) | 批量添加数组元素，逐项触发 Add 通知。 |
| [`Clear()`](#method-clear) | 清空列表。列表非空时以 Reset 通知；已为空时不通知。 |
| [`ClearListeners()`](#method-clearlisteners) | 清空所有变更监听。 |
| [`CopyTo(T[], int)`](#method-copyto-t-int) | 从指定数组索引开始复制元素到目标数组。 |
| [`ForEach(Action<T>)`](#method-foreach-action-t) | 对每个元素执行指定操作。 |
| [`Insert(int, T)`](#method-insert-int-t) | 在指定索引插入元素，触发 Add 通知（索引为插入位置）。 |
| [`InsertRange(int, IEnumerable<T>)`](#method-insertrange-int-ienumerable-t) | 在指定索引插入元素序列，逐项触发 Add 通知。 |
| [`InsertRange(int, T[])`](#method-insertrange-int-t) | 在指定索引插入数组元素，逐项触发 Add 通知。 |
| [`Move(int, int)`](#method-move-int-int) | 把元素从 oldIndex 移动到 newIndex，触发单次 Move 通知。 |
| [`RemoveAt(int)`](#method-removeat-int) | 移除指定索引的元素，触发 Remove 通知（参数含移除前索引与被移除元素）。 |
| [`RemoveListener(Action<CollectionChangedEventArgs<T>>)`](#method-removelistener-action-collectionchangedeventargs-t) | — |
| [`RemoveRange(int, int)`](#method-removerange-int-int) | 从指定索引移除指定数量的元素，逐项触发 Remove 通知（按原始顺序，索引为移除前位置）。 |
| [`Reverse()`](#method-reverse) | 反转全表，以 Reset 通知（少于 2 个元素时反转无变化，不通知）。 |
| [`Sort()`](#method-sort) | 对全表排序，以 Reset 通知（少于 2 个元素时排序无变化，不通知）。 |
| [`Sort(IComparer<T>)`](#method-sort-icomparer-t) | 使用指定比较器对全表排序，以 Reset 通知（少于 2 个元素时排序无变化，不通知）。 |

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

### AddListener(Action<CollectionChangedEventArgs<T>>) {#method-addlistener-action-collectionchangedeventargs-t}

``` csharp
public AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<T>> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<CollectionChangedEventArgs<T>>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | — |

</div>

### GetEnumerator() {#method-getenumerator}

返回遍历元素的结构体枚举器，foreach 具体类型时零分配。

``` csharp
public ObservableList<T>.Enumerator<T> GetEnumerator()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ObservableList<T>.Enumerator<T>` | 元素枚举器。 |

</div>

### Contains(T) {#method-contains-t}

判断是否包含指定元素。

``` csharp
public bool Contains(T item)
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

### Remove(T) {#method-remove-t}

移除第一个匹配元素，成功时触发 Remove 通知。

``` csharp
public bool Remove(T item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `T` | 要移除的元素。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 找到并移除返回 true；元素不存在时不触发通知，返回 false。 |

</div>

### IndexOf(T) {#method-indexof-t}

返回指定元素的索引；不存在时返回 -1。

``` csharp
public int IndexOf(T item)
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
| `int` | 元素索引或 -1。 |

</div>

### Add(T) {#method-add-t}

在末尾添加元素，触发 Add 通知（索引为 Count - 1）。

``` csharp
public void Add(T item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `T` | 要添加的元素。 |

</div>

### AddRange(IEnumerable<T>) {#method-addrange-ienumerable-t}

批量添加元素。逐项添加并逐项触发 Add 通知。

``` csharp
public void AddRange(IEnumerable<T> itemsToAdd)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `itemsToAdd` | `IEnumerable<T>` | 要添加的元素序列。 |

</div>

### AddRange(T[]) {#method-addrange-t}

批量添加数组元素，逐项触发 Add 通知。

``` csharp
public void AddRange(T[] itemsToAdd)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `itemsToAdd` | `T[]` | 要添加的元素数组。 |

</div>

### Clear() {#method-clear}

清空列表。列表非空时以 Reset 通知；已为空时不通知。

``` csharp
public void Clear()
```

### ClearListeners() {#method-clearlisteners}

清空所有变更监听。

**备注**

清除全部监听引用，防止因监听者未释放导致的内存泄漏。 与 Clear 不同——后者清空的是列表元素。

``` csharp
public void ClearListeners()
```

### CopyTo(T[], int) {#method-copyto-t-int}

从指定数组索引开始复制元素到目标数组。

``` csharp
public void CopyTo(T[] array, int arrayIndex)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `array` | `T[]` | 目标数组。 |
| `arrayIndex` | `int` | 目标数组起始索引。 |

</div>

### ForEach(Action<T>) {#method-foreach-action-t}

对每个元素执行指定操作。

``` csharp
public void ForEach(Action<T> action)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `action` | `Action<T>` | 对每个元素执行的操作。 |

</div>

### Insert(int, T) {#method-insert-int-t}

在指定索引插入元素，触发 Add 通知（索引为插入位置）。

``` csharp
public void Insert(int index, T item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `index` | `int` | 插入位置索引。 |
| `item` | `T` | 要插入的元素。 |

</div>

### InsertRange(int, IEnumerable<T>) {#method-insertrange-int-ienumerable-t}

在指定索引插入元素序列，逐项触发 Add 通知。

``` csharp
public void InsertRange(int index, IEnumerable<T> itemsToInsert)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `index` | `int` | 插入位置索引。 |
| `itemsToInsert` | `IEnumerable<T>` | 要插入的元素序列。 |

</div>

### InsertRange(int, T[]) {#method-insertrange-int-t}

在指定索引插入数组元素，逐项触发 Add 通知。

``` csharp
public void InsertRange(int index, T[] itemsToInsert)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `index` | `int` | 插入位置索引。 |
| `itemsToInsert` | `T[]` | 要插入的元素数组。 |

</div>

### Move(int, int) {#method-move-int-int}

把元素从 oldIndex 移动到 newIndex，触发单次 Move 通知。

``` csharp
public void Move(int oldIndex, int newIndex)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `oldIndex` | `int` | 元素当前索引。 |
| `newIndex` | `int` | 目标索引。 |

</div>

### RemoveAt(int) {#method-removeat-int}

移除指定索引的元素，触发 Remove 通知（参数含移除前索引与被移除元素）。

``` csharp
public void RemoveAt(int index)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `index` | `int` | 要移除元素的索引。 |

</div>

### RemoveListener(Action<CollectionChangedEventArgs<T>>) {#method-removelistener-action-collectionchangedeventargs-t}

``` csharp
public void RemoveListener(Action<CollectionChangedEventArgs<T>> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<CollectionChangedEventArgs<T>>` | — |

</div>

### RemoveRange(int, int) {#method-removerange-int-int}

从指定索引移除指定数量的元素，逐项触发 Remove 通知（按原始顺序，索引为移除前位置）。

``` csharp
public void RemoveRange(int index, int count)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `index` | `int` | 起始索引。 |
| `count` | `int` | 移除数量。 |

</div>

### Reverse() {#method-reverse}

反转全表，以 Reset 通知（少于 2 个元素时反转无变化，不通知）。

``` csharp
public void Reverse()
```

### Sort() {#method-sort}

对全表排序，以 Reset 通知（少于 2 个元素时排序无变化，不通知）。

``` csharp
public void Sort()
```

### Sort(IComparer<T>) {#method-sort-icomparer-t}

使用指定比较器对全表排序，以 Reset 通知（少于 2 个元素时排序无变化，不通知）。

``` csharp
public void Sort(IComparer<T> comparer)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `comparer` | `IComparer<T>` | 元素比较器。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
