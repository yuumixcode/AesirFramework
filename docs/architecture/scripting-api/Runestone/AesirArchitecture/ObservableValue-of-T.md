---
title: ObservableValue<T>
description: "Runestone.AesirArchitecture.ObservableValue<T> 的 API 文档"
---

# `ObservableValue<T>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `ObservableValue<T>`

**实现接口:** `Runestone.AesirArchitecture.IObservableValue<T>`，`Runestone.AesirArchitecture.IReadOnlyObservableValue<T>`

**类型参数**

- `T` — 属性值类型

## 声明

``` csharp
[Serializable]
public sealed class ObservableValue<T> : Runestone.AesirArchitecture.IObservableValue<T>, 
Runestone.AesirArchitecture.IReadOnlyObservableValue<T> 
```

可观察属性实现。
Model 层持有可写实例，View 层通过 IReadOnlyObservableValue{T} 只读订阅。

**备注**

内部使用 MiniEvent{T} 管理监听者——Invoke 路径零分配（直接多播调用）。
[SerializeField] 标记 value 字段使其可在 Inspector 中编辑。

PrivateValueFieldName 和 InvokeMethodName 常量供 ObservableValueAttributeProcessor 反射引用，避免硬编码字符串导致的重构断裂。

值比较使用 EqualityComparer{T}.Default.Equals，支持值类型和引用类型的正确比较。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ObservableValue()`](#constructor-observablevalue) | 默认构造，使用类型 T 的默认值 |
| [`ObservableValue(T)`](#constructor-observablevalue-t) | 默认构造，使用类型 T 的默认值 |

</div>

### ObservableValue() {#constructor-observablevalue}

默认构造，使用类型 T 的默认值

``` csharp
public ObservableValue<T>()
```

### ObservableValue(T) {#constructor-observablevalue-t}

默认构造，使用类型 T 的默认值

``` csharp
public ObservableValue<T>(T initialValue)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `initialValue` | `T` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`InvokeMethodName`](#field-invokemethodname) | — |
| [`PrivateValueFieldName`](#field-privatevaluefieldname) | — |

</div>

### InvokeMethodName {#field-invokemethodname}

``` csharp
public const string InvokeMethodName;
```

### PrivateValueFieldName {#field-privatevaluefieldname}

``` csharp
public const string PrivateValueFieldName;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Value`](#property-value) | 读写属性值。设置新值时若与旧值不同，则触发变更通知。 |

</div>

### Value {#property-value}

读写属性值。设置新值时若与旧值不同，则触发变更通知。

**备注**

使用 EqualityComparer{T}.Default 判断值是否变化，仅在变化时触发 MiniEvent{T}.Invoke。
性能注记：EqualityComparer<T>.Default 对未实现 IEquatable{T} 的值类型会回退到装箱比较（每次 set 装箱旧/新值）。热点路径上的自定义结构体建议实现 IEquatable<T> 消除装箱；基元类型（int / float / bool 等）已内置实现，无此开销。

``` csharp
public T Value { get; set; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddListener(Action<T>)`](#method-addlistener-action-t) | 添加监听者。回调参数为新值。 |
| [`AddListenerAndInvoke(Action<T>)`](#method-addlistenerandinvoke-action-t) | 添加监听并立即触发一次当前值，用于初始化时同步监听方状态。 |
| [`ClearListeners()`](#method-clearlisteners) | 清除所有监听。 |
| [`InvokeEvent()`](#method-invokeevent) | 触发值变更通知，用于强制刷新订阅方状态。 |
| [`RemoveListener(Action<T>)`](#method-removelistener-action-t) | 移除监听者。 |
| [`SetValueSilently(T)`](#method-setvaluesilently-t) | 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。 |

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

### AddListener(Action<T>) {#method-addlistener-action-t}

添加监听者。回调参数为新值。

``` csharp
public AutoRemoveListenerHandle AddListener(Action<T> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<T>` | 值变更时调用的回调函数，参数为变更后的新值。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 返回一个 AutoRemoveListenerHandle，释放后自动移除监听，避免手动管理生命周期。 |

</div>

### AddListenerAndInvoke(Action<T>) {#method-addlistenerandinvoke-action-t}

添加监听并立即触发一次当前值，用于初始化时同步监听方状态。

**备注**

先通过 AddListener 添加监听，再立即用当前值触发一次回调，确保订阅方在注册瞬间即可同步初始状态。

``` csharp
public AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<T>` | 值变更时调用的回调函数，参数为变更后的新值。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 返回一个 AutoRemoveListenerHandle，释放后自动移除监听。 |

</div>

### ClearListeners() {#method-clearlisteners}

清除所有监听。

**备注**

清除所有监听引用，防止因监听者未释放导致的内存泄漏。 命名对齐可观察集合的 ClearListeners——清除的是监听者而非值本身。

``` csharp
public void ClearListeners()
```

### InvokeEvent() {#method-invokeevent}

触发值变更通知，用于强制刷新订阅方状态。

**备注**

强制触发通知，用于值未变但需要刷新订阅方状态的场景。

``` csharp
public void InvokeEvent()
```

### RemoveListener(Action<T>) {#method-removelistener-action-t}

移除监听者。

``` csharp
public void RemoveListener(Action<T> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `callback` | `Action<T>` | 先前通过 AddListener 注册的回调函数。 |

</div>

### SetValueSilently(T) {#method-setvaluesilently-t}

静默设置值，不触发通知。用于反序列化或批量更新后统一触发。

**备注**

不触发变更通知，但仍做值比较——值未变时跳过赋值，避免无意义的引用替换。

``` csharp
public void SetValueSilently(T v)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `v` | `T` | 要设置的新值。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
