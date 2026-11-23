---
title: GenericLocator<T>
description: "Runestone.AesirArchitecture.GenericLocator<T> 的 API 文档"
---

# `GenericLocator<T>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `GenericLocator<T>`

**实现接口:** `Runestone.AesirArchitecture.IGenericLocator<T>`，`System.IDisposable`

## 声明

``` csharp
[Serializable]
public sealed class GenericLocator<T> : Runestone.AesirArchitecture.IGenericLocator<T>, 
System.IDisposable where T : class
```

泛型对象定位器。按类型注册、查询与获取以 T 为基类的对象实例。

**备注**

内部以 Type 为键、T 为值的 Dictionary{TKey, TValue} 作为容器， 支持按类型注册和获取实例。注册时以 typeof(TItem) 作为键，查询时须使用相同的类型参数。

AbstractContext{T} 内部使用两个 GenericLocator{T} 实例分别管理 IModel 和 IService，实现 Model / Service 的注册与查询。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GenericLocator()`](#constructor-genericlocator) | — |

</div>

### GenericLocator() {#constructor-genericlocator}

``` csharp
public GenericLocator<T>()
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetAllEntries()`](#method-getallentries) | 获取所有已注册键值对（诊断用途，近失识别专用）。 |
| [`GetAll()`](#method-getall) | 按注册顺序获取所有已注册的实例集合 |
| [`Get()`](#method-get) | 获取指定类型的实例。如果不存在，返回 null。 |
| [`TryGet(ref TItem)`](#method-tryget-ref-titem) | 尝试获取指定类型的实例 |
| [`Dispose()`](#method-dispose) | 释放资源，清空所有注册。 |
| [`Register(Type, T)`](#method-register-type-t) | 按显式指定的类型注册一个实例。如果类型已存在，则覆盖原有注册（不改变其插入顺序位置）。 |
| [`Register(TItem)`](#method-register-titem) | 注册一个实例。如果类型已存在，则覆盖原有注册（不改变其插入顺序位置）。 |
| [`Unregister()`](#method-unregister) | 注销指定类型的实例 |

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

### GetAllEntries() {#method-getallentries}

获取所有已注册键值对（诊断用途，近失识别专用）。

**备注**

正常查询请使用 Get{TItem} / TryGet{TItem}。 此成员仅供诊断路径遍历已注册条目（如 AbstractContext{T} 在"未注册"异常中 识别"已注册实例可赋值给查询类型"的近失情况）；正常路径不产生开销。
与 GetAll 一致地返回调用时刻的物化快照：枚举期间修改定位器不会抛"集合已修改"异常， 期间发生的注册/注销不影响已返回的枚举。物化分配仅发生在调用时（异常诊断等冷路径）。

``` csharp
public IEnumerable<KeyValuePair<Type, T>> GetAllEntries()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<KeyValuePair<Type, T>>` | — |

</div>

### GetAll() {#method-getall}

按注册顺序获取所有已注册的实例集合

**备注**

返回调用时刻的完整快照（同步物化）：消费端可在枚举期间修改定位器而不抛"集合已修改"异常， 期间发生的注册/注销不影响已返回的枚举。物化分配仅发生在调用时（初始化/关停等冷路径）。 与诊断成员 GetAllEntries 取同一份快照语义，两者修改安全性契约一致。

``` csharp
public IEnumerable<T> GetAll()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<T>` | 所有已注册实例的 IEnumerable{T} 集合，不含类型键，按注册顺序排列。 |

</div>

### Get() {#method-get}

获取指定类型的实例。如果不存在，返回 null。

``` csharp
public TItem Get<TItem>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TItem` | 已注册的实例；若未注册则返回 null。 |

</div>

### TryGet(ref TItem) {#method-tryget-ref-titem}

尝试获取指定类型的实例

``` csharp
public bool TryGet<TItem>(out ref TItem instance)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `instance` | `ref TItem` | 找到时输出已注册的实例；未找到时输出 null。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 成功找到则返回 true；未注册则返回 false。 |

</div>

### Dispose() {#method-dispose}

释放资源，清空所有注册。

``` csharp
public void Dispose()
```

### Register(Type, T) {#method-register-type-t}

按显式指定的类型注册一个实例。如果类型已存在，则覆盖原有注册（不改变其插入顺序位置）。

``` csharp
public void Register(Type type, T instance)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | 注册时使用的键类型，实例必须可赋值给该类型。 |
| `instance` | `T` | 要注册的实例。 |

</div>

### Register(TItem) {#method-register-titem}

注册一个实例。如果类型已存在，则覆盖原有注册（不改变其插入顺序位置）。

``` csharp
public void Register<TItem>(TItem instance)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `instance` | `TItem` | 要注册的实例。 |

</div>

### Unregister() {#method-unregister}

注销指定类型的实例

``` csharp
public void Unregister<TItem>()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
