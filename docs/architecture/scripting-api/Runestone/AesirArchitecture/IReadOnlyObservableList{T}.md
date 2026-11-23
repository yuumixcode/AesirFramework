---
title: IReadOnlyObservableList<T>
description: "Runestone.AesirArchitecture.IReadOnlyObservableList<T> 的 API 文档"
---

# `IReadOnlyObservableList<T>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `System.Collections.Generic.IReadOnlyList<T>`，`System.Collections.Generic.IEnumerable<T>`，`System.Collections.IEnumerable`，`Runestone.AesirArchitecture.IObservableCollection<T>`，`System.Collections.Generic.IReadOnlyCollection<T>`

**类型参数**

- `T` — 元素类型

## 声明

``` csharp
public interface IReadOnlyObservableList<T> : System.Collections.Generic.IReadOnlyList<T>, 
System.Collections.Generic.IEnumerable<T>, 
System.Collections.IEnumerable, 
Runestone.AesirArchitecture.IObservableCollection<T>, 
System.Collections.Generic.IReadOnlyCollection<T> 
```

只读可观察列表接口。
View 层通过此接口枚举元素并订阅变更，不能修改集合。

**备注**

事件语义与 MiniEvent{T} 一致：回调触发时集合已处于变更后的状态； 监听者抛异常按原生 C# 事件 fail-fast 向上传播，监听回调不应抛异常属框架约定。
变更通知为单一事件（AddListener），载荷 CollectionChangedEventArgs{T}： 批量操作逐项通知、Sort / Reverse / Clear 以 Reset 通知、无变更的写操作不通知。

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
