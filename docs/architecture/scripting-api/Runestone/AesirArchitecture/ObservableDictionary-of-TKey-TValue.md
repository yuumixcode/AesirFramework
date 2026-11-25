---
title: ObservableDictionary<TKey, TValue>
description: "Runestone.AesirArchitecture.ObservableDictionary<TKey, TValue> 的 API 文档"
---

# `ObservableDictionary<TKey, TValue>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `ObservableDictionary<TKey, TValue>`

**实现接口:** `System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>`，`System.Collections.Generic.IDictionary<TKey, TValue>`，`Runestone.AesirArchitecture.IReadOnlyObservableDictionary<TKey, TValue>`，`System.Collections.Generic.IEnumerable<KeyValuePair<TKey, TValue>>`，`System.Collections.IEnumerable`，`Runestone.AesirArchitecture.IObservableCollection<KeyValuePair<TKey, TValue>>`，`System.Collections.Generic.ICollection<KeyValuePair<TKey, TValue>>`，`System.Collections.Generic.IReadOnlyCollection<KeyValuePair<TKey, TValue>>`，`Runestone.AesirArchitecture.IObservableDictionary<TKey, TValue>`

**类型参数**

- `TKey` — 键类型
- `TValue` — 值类型

## 声明

``` csharp
[DefaultMember]
[Serializable]
public sealed class ObservableDictionary<TKey, TValue> : System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>, 
System.Collections.Generic.IDictionary<TKey, TValue>, 
Runestone.AesirArchitecture.IReadOnlyObservableDictionary<TKey, TValue>, 
System.Collections.Generic.IEnumerable<KeyValuePair<TKey, TValue>>, 
System.Collections.IEnumerable, 
Runestone.AesirArchitecture.IObservableCollection<KeyValuePair<TKey, TValue>>, 
System.Collections.Generic.ICollection<KeyValuePair<TKey, TValue>>, 
System.Collections.Generic.IReadOnlyCollection<KeyValuePair<TKey, TValue>>, 
Runestone.AesirArchitecture.IObservableDictionary<TKey, TValue>  
```

可观察字典实现。
Model 层持有可写实例，View 层通过 IReadOnlyObservableDictionary{TKey, TValue} 只读订阅。

**备注**

内部组合 Dictionary{TKey,TValue} 存储键值，变更通知经 MiniEvent{T} 分发——Invoke 路径零分配（直接多播调用）。注意：订阅路径（AddListener / 句柄创建）有与监听者数量成正比的委托分配， 勿在每帧订阅场景使用。
[SerializeField] 标记 dictionary 字段——Unity 原生不序列化 Dictionary{TKey, TValue} （该标记对 Unity 序列化无效果），安装 Odin Inspector 且宿主走 Odin 序列化（如 AesirMonoBehaviour 派生组件）时， 该字段可被 Odin 序列化管线接管，便于在 Inspector 中编辑初始键值。

变更通知为单一事件（AddListener），载荷为 CollectionChangedEventArgs{T} （T = KeyValuePair{TKey,TValue}）：写操作完成后才触发，监听者回调中读取到的集合已是变更后的状态； 索引器为不存在的键赋值触发 Add、为已有键赋新值触发 Replace（旧值见载荷 OldItem）、赋相同值不通知； Remove 不存在的键、Clear 空字典不通知。字典无索引概念，载荷索引固定 -1。

遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 IReadOnlyObservableDictionary{TKey, TValue} / IEnumerable{T} 接口遍历会装箱一次枚举器（与 BCL Dictionary{TKey, TValue} 行为一致）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ObservableDictionary()`](#constructor-observabledictionary) | 默认构造，创建空字典。 |
| [`ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>>)`](#constructor-observabledictionary-ienumerable-keyvaluepair-tkey-tvalue) | 指定初始键值构造。初始键值不触发变更通知（语义同反序列化填充）。 |
| [`ObservableDictionary(IEqualityComparer<TKey>)`](#constructor-observabledictionary-iequalitycomparer-tkey) | 指定键比较器构造。 |

</div>

### ObservableDictionary() {#constructor-observabledictionary}

默认构造，创建空字典。

``` csharp
public ObservableDictionary<TKey, TValue>()
```

### ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>>) {#constructor-observabledictionary-ienumerable-keyvaluepair-tkey-tvalue}

指定初始键值构造。初始键值不触发变更通知（语义同反序列化填充）。

``` csharp
public ObservableDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> initialItems)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `initialItems` | `IEnumerable<KeyValuePair<TKey, TValue>>` | 初始键值序列。 |

</div>

### ObservableDictionary(IEqualityComparer<TKey>) {#constructor-observabledictionary-iequalitycomparer-tkey}

指定键比较器构造。

``` csharp
public ObservableDictionary<TKey, TValue>(IEqualityComparer<TKey> comparer)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `comparer` | `IEqualityComparer<TKey>` | 键比较器；为 null 时使用 EqualityComparer{TKey}.Default。 |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Keys`](#property-keys) | 所有键的集合。 |
| [`Values`](#property-values) | 所有值的集合。 |
| [`Comparer`](#property-comparer) | 内部 Dictionary{TKey, TValue} 使用的键比较器。 |
| [`Item`](#property-item) | 读写指定键的值。 读取：键不存在时抛 KeyNotFoundException（fail-fast）。  写入：键不存在时添加并触发 Add；键已存在且新值不同时更新并触发 Replace（旧值见载荷 OldItem）；值相同不通知。 |
| [`IsReadOnly`](#property-isreadonly) | 固定返回 false，该集合可写。 |
| [`Count`](#property-count) | 键值对数量。 |

</div>

### Keys {#property-keys}

所有键的集合。

``` csharp
public IEnumerable<TKey> Keys { get; }
```

### Values {#property-values}

所有值的集合。

``` csharp
public IEnumerable<TValue> Values { get; }
```

### Comparer {#property-comparer}

内部 Dictionary{TKey, TValue} 使用的键比较器。

``` csharp
public IEqualityComparer<TKey> Comparer { get; }
```

### Item {#property-item}

读写指定键的值。
读取：键不存在时抛 KeyNotFoundException（fail-fast）。

写入：键不存在时添加并触发 Add；键已存在且新值不同时更新并触发 Replace（旧值见载荷 OldItem）；值相同不通知。

``` csharp
public TValue Item { get; set; }
```

### IsReadOnly {#property-isreadonly}

固定返回 false，该集合可写。

``` csharp
public bool IsReadOnly { get; }
```

### Count {#property-count}

键值对数量。

``` csharp
public int Count { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddListener(Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>>)`](#method-addlistener-action-collectionchangedeventargs-keyvaluepair-tkey-tvalue) | — |
| [`GetEnumerator()`](#method-getenumerator) | 返回遍历键值对的结构体枚举器，foreach 具体类型时零分配。 |
| [`Contains(KeyValuePair<TKey, TValue>)`](#method-contains-keyvaluepair-tkey-tvalue) | 判断是否包含指定键值对（键存在且值相等）。 |
| [`ContainsKey(TKey)`](#method-containskey-tkey) | 判断是否包含指定键。 |
| [`Remove(KeyValuePair<TKey, TValue>)`](#method-remove-keyvaluepair-tkey-tvalue) | 移除与指定键值对匹配的项（键存在且值相等），成功时触发 Remove 通知。 |
| [`Remove(TKey)`](#method-remove-tkey) | 移除指定键的键值对，成功时触发 Remove 通知（参数含被移除的键值对）。 |
| [`TryGetValue(TKey, ref TValue)`](#method-trygetvalue-tkey-ref-tvalue) | 获取与指定键关联的值。 |
| [`Add(KeyValuePair<TKey, TValue>)`](#method-add-keyvaluepair-tkey-tvalue) | 添加键值对，触发 Add 通知。键已存在时抛 ArgumentException（fail-fast）。 |
| [`Add(TKey, TValue)`](#method-add-tkey-tvalue) | 添加键值对，触发 Add 通知。键已存在时抛 ArgumentException（fail-fast）。 |
| [`Clear()`](#method-clear) | 清空字典。字典非空时以 Reset 通知；已为空时不通知。 |
| [`ClearListeners()`](#method-clearlisteners) | 清空所有变更监听。 |
| [`CopyTo(KeyValuePair<TKey, TValue>[], int)`](#method-copyto-keyvaluepair-tkey-tvalue-int) | 从指定数组索引开始复制键值对到目标数组。 |
| [`RemoveListener(Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>>)`](#method-removelistener-action-collectionchangedeventargs-keyvaluepair-tkey-tvalue) | — |

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

### AddListener(Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>>) {#method-addlistener-action-collectionchangedeventargs-keyvaluepair-tkey-tvalue}

``` csharp
public AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | — |

</div>

### GetEnumerator() {#method-getenumerator}

返回遍历键值对的结构体枚举器，foreach 具体类型时零分配。

``` csharp
public ObservableDictionary<TKey, TValue>.Enumerator<TKey, TValue> GetEnumerator()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ObservableDictionary<TKey, TValue>.Enumerator<TKey, TValue>` | 键值对枚举器。 |

</div>

### Contains(KeyValuePair<TKey, TValue>) {#method-contains-keyvaluepair-tkey-tvalue}

判断是否包含指定键值对（键存在且值相等）。

``` csharp
public bool Contains(KeyValuePair<TKey, TValue> item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `KeyValuePair<TKey, TValue>` | 要查找的键值对。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 包含返回 true，否则返回 false。 |

</div>

### ContainsKey(TKey) {#method-containskey-tkey}

判断是否包含指定键。

``` csharp
public bool ContainsKey(TKey key)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `key` | `TKey` | 要查找的键。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 包含返回 true，否则返回 false。 |

</div>

### Remove(KeyValuePair<TKey, TValue>) {#method-remove-keyvaluepair-tkey-tvalue}

移除与指定键值对匹配的项（键存在且值相等），成功时触发 Remove 通知。

**备注**

不复用 Remove(TKey)——其按键删除不校验值；此处先验证键值对完全匹配再移除，避免误删同键不同值。

``` csharp
public bool Remove(KeyValuePair<TKey, TValue> item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `KeyValuePair<TKey, TValue>` | 要移除的键值对。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 找到并移除返回 true；未匹配时不触发通知，返回 false。 |

</div>

### Remove(TKey) {#method-remove-tkey}

移除指定键的键值对，成功时触发 Remove 通知（参数含被移除的键值对）。

**备注**

使用 Remove(TKey, out TValue) 在移除的同时取回旧值，单次哈希查找。

``` csharp
public bool Remove(TKey key)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `key` | `TKey` | 要移除的键。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 找到并移除返回 true；键不存在时不触发通知，返回 false。 |

</div>

### TryGetValue(TKey, ref TValue) {#method-trygetvalue-tkey-ref-tvalue}

获取与指定键关联的值。

``` csharp
public bool TryGetValue(TKey key, out ref TValue value)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `key` | `TKey` | 要查找的键。 |
| `value` | `ref TValue` | 键存在时为关联的值，否则为 TValue 的默认值。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 键存在返回 true，否则返回 false。 |

</div>

### Add(KeyValuePair<TKey, TValue>) {#method-add-keyvaluepair-tkey-tvalue}

添加键值对，触发 Add 通知。键已存在时抛 ArgumentException（fail-fast）。

``` csharp
public void Add(KeyValuePair<TKey, TValue> item)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `item` | `KeyValuePair<TKey, TValue>` | 要添加的键值对。 |

</div>

### Add(TKey, TValue) {#method-add-tkey-tvalue}

添加键值对，触发 Add 通知。键已存在时抛 ArgumentException（fail-fast）。

``` csharp
public void Add(TKey key, TValue value)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `key` | `TKey` | 键。 |
| `value` | `TValue` | 值。 |

</div>

### Clear() {#method-clear}

清空字典。字典非空时以 Reset 通知；已为空时不通知。

``` csharp
public void Clear()
```

### ClearListeners() {#method-clearlisteners}

清空所有变更监听。

**备注**

清除全部监听引用，防止因监听者未释放导致的内存泄漏。 与 Clear 不同——后者清空的是字典键值。

``` csharp
public void ClearListeners()
```

### CopyTo(KeyValuePair<TKey, TValue>[], int) {#method-copyto-keyvaluepair-tkey-tvalue-int}

从指定数组索引开始复制键值对到目标数组。

``` csharp
public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `array` | `KeyValuePair<TKey, TValue>[]` | 目标数组。 |
| `arrayIndex` | `int` | 目标数组起始索引。 |

</div>

### RemoveListener(Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>>) {#method-removelistener-action-collectionchangedeventargs-keyvaluepair-tkey-tvalue}

``` csharp
public void RemoveListener(Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<CollectionChangedEventArgs<KeyValuePair<TKey, TValue>>>` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
