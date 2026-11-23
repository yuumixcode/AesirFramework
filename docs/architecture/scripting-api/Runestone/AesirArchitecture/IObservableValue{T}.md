---
title: IObservableValue<T>
description: "Runestone.AesirArchitecture.IObservableValue<T> 的 API 文档"
---

# `IObservableValue<T>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `Runestone.AesirArchitecture.IReadOnlyObservableValue<T>`

**类型参数**

- `T` — 属性值类型

## 声明

``` csharp
public interface IObservableValue<T> : Runestone.AesirArchitecture.IReadOnlyObservableValue<T> 
```

完整可观察属性接口。
Presenter 层通过此接口读写数据。

**备注**

Presenter 层通过此接口读写数据，View 层通过 IReadOnlyObservableValue{T} 只读订阅。
Value 的 setter 使用 EqualityComparer{T}.Default 比较新旧值，仅在值变化时触发通知。

SetValueSilently 用于反序列化或批量更新场景——先静默设值再统一调用 InvokeEvent 触发通知，避免中间状态触发多次回调。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Value`](#property-value) | — |

</div>

### Value {#property-value}

``` csharp
public T Value { get; set; }
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SetValueSilently(T)`](#method-setvaluesilently-t) | 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。 |

</div>

### SetValueSilently(T) {#method-setvaluesilently-t}

静默设置值，不触发通知。用于反序列化或批量更新后统一触发。

**备注**

不触发任何变更通知。适用于反序列化或批量更新场景——先静默设值，再统一调用 InvokeEvent 触发通知，避免中间状态触发多次回调。

``` csharp
public abstract void SetValueSilently(T value)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `value` | `T` | 要设置的新值。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
