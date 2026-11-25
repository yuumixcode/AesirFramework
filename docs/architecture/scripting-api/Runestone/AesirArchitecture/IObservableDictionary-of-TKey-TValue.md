---
title: IObservableDictionary<TKey, TValue>
description: "Runestone.AesirArchitecture.IObservableDictionary<TKey, TValue> 的 API 文档"
---

# `IObservableDictionary<TKey, TValue>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>`，`System.Collections.Generic.IDictionary<TKey, TValue>`，`Runestone.AesirArchitecture.IReadOnlyObservableDictionary<TKey, TValue>`，`System.Collections.Generic.IEnumerable<KeyValuePair<TKey, TValue>>`，`System.Collections.IEnumerable`，`Runestone.AesirArchitecture.IObservableCollection<KeyValuePair<TKey, TValue>>`，`System.Collections.Generic.ICollection<KeyValuePair<TKey, TValue>>`，`System.Collections.Generic.IReadOnlyCollection<KeyValuePair<TKey, TValue>>`

**类型参数**

- `TKey` — 键类型
- `TValue` — 值类型

## 声明

``` csharp
[DefaultMember]
public interface IObservableDictionary<TKey, TValue> : System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>, 
System.Collections.Generic.IDictionary<TKey, TValue>, 
Runestone.AesirArchitecture.IReadOnlyObservableDictionary<TKey, TValue>, 
System.Collections.Generic.IEnumerable<KeyValuePair<TKey, TValue>>, 
System.Collections.IEnumerable, 
Runestone.AesirArchitecture.IObservableCollection<KeyValuePair<TKey, TValue>>, 
System.Collections.Generic.ICollection<KeyValuePair<TKey, TValue>>, 
System.Collections.Generic.IReadOnlyCollection<KeyValuePair<TKey, TValue>>  
```

完整可观察字典接口。
Model 层通过此接口读写集合；View 层使用 IReadOnlyObservableDictionary{TKey, TValue} 只读订阅。

**备注**

所有写操作完成后才触发对应事件，监听者回调中读取到的集合已是变更后的状态。 无变更的操作不触发事件：Remove 不存在的键、Clear 空字典、索引器赋相同值。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Keys`](#property-keys) | — |
| [`Values`](#property-values) | — |
| [`Item`](#property-item) | 读写指定键的值。重新声明以统一 IDictionary{TKey, TValue} 与 IReadOnlyDictionary{TKey, TValue} 的索引器。 |
| [`Count`](#property-count) | — |

</div>

### Keys {#property-keys}

``` csharp
public IEnumerable<TKey> Keys { get; }
```

### Values {#property-values}

``` csharp
public IEnumerable<TValue> Values { get; }
```

### Item {#property-item}

读写指定键的值。重新声明以统一 IDictionary{TKey, TValue} 与 IReadOnlyDictionary{TKey, TValue} 的索引器。

``` csharp
public TValue Item { get; set; }
```

### Count {#property-count}

``` csharp
public int Count { get; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ContainsKey(TKey)`](#method-containskey-tkey) | — |
| [`TryGetValue(TKey, ref TValue)`](#method-trygetvalue-tkey-ref-tvalue) | — |

</div>

### ContainsKey(TKey) {#method-containskey-tkey}

``` csharp
public abstract bool ContainsKey(TKey key)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `key` | `TKey` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetValue(TKey, ref TValue) {#method-trygetvalue-tkey-ref-tvalue}

``` csharp
public abstract bool TryGetValue(TKey key, out ref TValue value)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `key` | `TKey` | — |
| `value` | `ref TValue` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
