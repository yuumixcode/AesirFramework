---
title: IReadOnlyObservableDictionary<TKey, TValue>
description: "Runestone.AesirArchitecture.IReadOnlyObservableDictionary<TKey, TValue> 的 API 文档"
---

# `IReadOnlyObservableDictionary<TKey, TValue>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>`，`System.Collections.Generic.IEnumerable<KeyValuePair<TKey, TValue>>`，`System.Collections.IEnumerable`，`Runestone.AesirArchitecture.IObservableCollection<KeyValuePair<TKey, TValue>>`，`System.Collections.Generic.IReadOnlyCollection<KeyValuePair<TKey, TValue>>`

**类型参数**

- `TKey` — 键类型
- `TValue` — 值类型

## 声明

``` csharp
public interface IReadOnlyObservableDictionary<TKey, TValue> : System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>, 
System.Collections.Generic.IEnumerable<KeyValuePair<TKey, TValue>>, 
System.Collections.IEnumerable, 
Runestone.AesirArchitecture.IObservableCollection<KeyValuePair<TKey, TValue>>, 
System.Collections.Generic.IReadOnlyCollection<KeyValuePair<TKey, TValue>>  
```

只读可观察字典接口。
View 层通过此接口读取键值并订阅变更，不能修改集合。

**备注**

事件语义与 MiniEvent{T} 一致：回调触发时集合已处于变更后的状态； 监听者抛异常按原生 C# 事件 fail-fast 向上传播，监听回调不应抛异常属框架约定。
变更通知为单一事件（AddListener），载荷为 CollectionChangedEventArgs{T}（T = KeyValuePair{TKey,TValue}）： 新增键 → Add、移除键 → Remove、已有键赋新值 → Replace（旧值见 OldItem）、 Clear → Reset；字典无索引概念，事件索引固定 -1。

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
