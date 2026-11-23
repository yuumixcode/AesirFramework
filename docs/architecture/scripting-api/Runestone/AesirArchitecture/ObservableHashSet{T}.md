---
title: ObservableHashSet<T>
description: "Runestone.AesirArchitecture.ObservableHashSet<T> 的 API 文档"
---

# `ObservableHashSet<T>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `ObservableHashSet<T>`

**实现接口:** `Runestone.AesirArchitecture.IObservableHashSet<T>`，`Runestone.AesirArchitecture.IReadOnlyObservableHashSet<T>`，`System.Collections.Generic.IEnumerable<T>`，`System.Collections.IEnumerable`，`Runestone.AesirArchitecture.IObservableCollection<T>`，`System.Collections.Generic.ICollection<T>`，`System.Collections.Generic.IReadOnlyCollection<T>`

**类型参数**

- `T` — 元素类型

## 声明

``` csharp
[Serializable]
public sealed class ObservableHashSet<T> : Runestone.AesirArchitecture.IObservableHashSet<T>, 
Runestone.AesirArchitecture.IReadOnlyObservableHashSet<T>, 
System.Collections.Generic.IEnumerable<T>, 
System.Collections.IEnumerable, 
Runestone.AesirArchitecture.IObservableCollection<T>, 
System.Collections.Generic.ICollection<T>, 
System.Collections.Generic.IReadOnlyCollection<T> 
```

可观察哈希集合实现。
Model 层持有可写实例，View 层通过 IReadOnlyObservableHashSet{T} 只读订阅。

**备注**

内部组合 HashSet{T} 存储元素，变更通知经 MiniEvent{T} 分发——Invoke 路径零分配 （直接多播调用）。注意：订阅路径（AddListener / 句柄创建）有与监听者数量成正比的委托分配， 勿在每帧订阅场景使用。
[SerializeField] 标记 set 字段——Unity 原生不序列化 HashSet{T}（该标记对 Unity 序列化无效果）， 安装 Odin Inspector 且宿主走 Odin 序列化（如 AesirMonoBehaviour 派生组件）时， 该字段可被 Odin 序列化管线接管，便于在 Inspector 中编辑初始元素（与 ObservableDictionary{TKey, TValue} 行为一致）。

变更通知为单一事件（AddListener）：写操作完成后才触发，监听者回调中读取到的集合已是变更后的状态； 无变更的操作不通知（Add 重复元素、Remove 不存在的元素、Clear 空集合）； 批量操作（AddRange / RemoveRange）逐项通知实际变更的元素； Clear 以 Reset 通知。 集合无索引概念，载荷索引固定 -1。

遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 IReadOnlyObservableHashSet{T} / IEnumerable{T} 接口遍历会装箱一次枚举器（与 BCL HashSet{T} 行为一致）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ObservableHashSet()`](#constructor-observablehashset) | 默认构造，创建空集合。 |
| [`ObservableHashSet(IEnumerable<T>)`](#constructor-observablehashset-ienumerable-t) | 指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。 |
| [`ObservableHashSet(IEqualityComparer<T>)`](#constructor-observablehashset-iequalitycomparer-t) | 指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。 |
| [`ObservableHashSet(int)`](#constructor-observablehashset-int) | 指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。 |

</div>

### ObservableHashSet() {#constructor-observablehashset}

默认构造，创建空集合。

``` csharp
public ObservableHashSet<T>()
```

### ObservableHashSet(IEnumerable<T>) {#constructor-observablehashset-ienumerable-t}

指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。

``` csharp
public ObservableHashSet<T>(IEnumerable<T> initialItems)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `initialItems` | `IEnumerable<T>` | 初始元素序列。 |

</div>

### ObservableHashSet(IEqualityComparer<T>) {#constructor-observablehashset-iequalitycomparer-t}

指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。

``` csharp
public ObservableHashSet<T>(IEqualityComparer<T> comparer)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `comparer` | `IEqualityComparer<T>` | — |

</div>

### ObservableHashSet(int) {#constructor-observablehashset-int}

指定初始元素构造。初始元素不触发变更通知（语义同反序列化填充）。

``` csharp
public ObservableHashSet<T>(int capacity)
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
| [`Comparer`](#property-comparer) | 内部 HashSet{T} 使用的元素比较器。 |
| [`IsReadOnly`](#property-isreadonly) | 固定返回 false，该集合可写。 |
| [`Count`](#property-count) | 元素数量。 |

</div>

### Comparer {#property-comparer}

内部 HashSet{T} 使用的元素比较器。

``` csharp
public IEqualityComparer<T> Comparer { get; }
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
| [`Add(T)`](#method-add-t) | 添加元素，实际添加时触发 Add 通知（参数为该元素）。 |
| [`Contains(T)`](#method-contains-t) | 判断是否包含指定元素。 |
| [`Remove(T)`](#method-remove-t) | 移除指定元素，成功时触发 Remove 通知（参数为该元素）。 |
| [`TryGetValue(T, ref T)`](#method-trygetvalue-t-ref-t) | 按键取回集合中实际存储的等值元素（用于取回引用类型元素本身）。 |
| [`AddRange(IEnumerable<T>)`](#method-addrange-ienumerable-t) | 批量添加元素序列，逐项触发 Add 通知（仅实际新增的元素）。 |
| [`AddRange(T[])`](#method-addrange-t) | 批量添加元素数组，逐项触发 Add 通知（仅实际新增的元素）。 |
| [`Clear()`](#method-clear) | 清空集合。集合非空时以 Reset 通知；已为空时不通知。 |
| [`ClearListeners()`](#method-clearlisteners) | 清空所有变更监听。 |
| [`CopyTo(T[], int)`](#method-copyto-t-int) | 从指定数组索引开始复制元素到目标数组。 |
| [`RemoveListener(Action<CollectionChangedEventArgs<T>>)`](#method-removelistener-action-collectionchangedeventargs-t) | — |
| [`RemoveRange(IEnumerable<T>)`](#method-removerange-ienumerable-t) | 批量移除元素序列，逐项触发 Remove 通知（仅实际被移除的元素）。 |
| [`RemoveRange(T[])`](#method-removerange-t) | 批量移除元素数组，逐项触发 Remove 通知（仅实际被移除的元素）。 |

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
public ObservableHashSet<T>.Enumerator<T> GetEnumerator()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ObservableHashSet<T>.Enumerator<T>` | 元素枚举器。 |

</div>

### Add(T) {#method-add-t}

添加元素，实际添加时触发 Add 通知（参数为该元素）。

``` csharp
public bool Add(T item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `T` | 要添加的元素。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 新添加返回 true；元素已存在时不触发通知，返回 false。 |

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

移除指定元素，成功时触发 Remove 通知（参数为该元素）。

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

### TryGetValue(T, ref T) {#method-trygetvalue-t-ref-t}

按键取回集合中实际存储的等值元素（用于取回引用类型元素本身）。

``` csharp
public bool TryGetValue(T equalValue, out ref T actualValue)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `equalValue` | `T` | 用于比较的元素。 |
| `actualValue` | `ref T` | 集合中实际存储的等值元素。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 集合中存在等值元素返回 true，否则返回 false。 |

</div>

### AddRange(IEnumerable<T>) {#method-addrange-ienumerable-t}

批量添加元素序列，逐项触发 Add 通知（仅实际新增的元素）。

**备注**

先经 CloneCollection{T} 物化源序列再写入——源为集合自身时 直接枚举会在首次写入后触发内部 HashSet 的版本检查异常并留下部分变更，与 ObservableList 的批量操作统一"先克隆后写入"。

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

批量添加元素数组，逐项触发 Add 通知（仅实际新增的元素）。

**备注**

先经 CloneCollection{T} 物化源序列再写入——源为集合自身时 直接枚举会在首次写入后触发内部 HashSet 的版本检查异常并留下部分变更，与 ObservableList 的批量操作统一"先克隆后写入"。

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

清空集合。集合非空时以 Reset 通知；已为空时不通知。

``` csharp
public void Clear()
```

### ClearListeners() {#method-clearlisteners}

清空所有变更监听。

**备注**

清除全部监听引用，防止因监听者未释放导致的内存泄漏。 与 Clear 不同——后者清空的是集合元素。

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

### RemoveRange(IEnumerable<T>) {#method-removerange-ienumerable-t}

批量移除元素序列，逐项触发 Remove 通知（仅实际被移除的元素）。

**备注**

先物化源序列再写入（与 AddRange(IEnumerable{T}) 同因：源为集合自身时避免枚举中断）。

``` csharp
public void RemoveRange(IEnumerable<T> itemsToRemove)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `itemsToRemove` | `IEnumerable<T>` | 要移除的元素序列。 |

</div>

### RemoveRange(T[]) {#method-removerange-t}

批量移除元素数组，逐项触发 Remove 通知（仅实际被移除的元素）。

**备注**

先物化源序列再写入（与 AddRange(IEnumerable{T}) 同因：源为集合自身时避免枚举中断）。

``` csharp
public void RemoveRange(T[] itemsToRemove)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `itemsToRemove` | `T[]` | 要移除的元素数组。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
